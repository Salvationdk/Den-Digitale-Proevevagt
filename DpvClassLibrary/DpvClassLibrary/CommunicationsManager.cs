using DpvClassLibrary.Configuration;
using DpvClassLibrary.Logging;
using DpvClassLibrary.Receivers;
using DpvClassLibrary.Tools;
using DpvClassLibrary.Workers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace DpvClassLibrary
{
	public class CommunicationsManager
	{
		private class TimerAndWorker
		{
			public DataPackageEnvelopeWorkerBase Worker
			{
				get;
				set;
			}

			public Timer Timer
			{
				get;
				set;
			}

			public TimerAndWorker(DataPackageEnvelopeWorkerBase worker, Timer timer)
			{
				Worker = worker;
				Timer = timer;
			}
		}

		private List<TimerAndWorker> _timersAndWorkers = new List<TimerAndWorker>();

		private TimersConfigSection _config;

		private TimerAndWorker _heartbeatTimerAndWorker;

		private Timer _timeToStartTimer;

		private Timer _timeToStopTimer;

		private ScreenshotWorker _screenshotWorker;

		public DataPackageEnvelopeAwsReceiver Receiver
		{
			get;
			set;
		}

		public WorkflowStatus CurrentStatus
		{
			get;
			private set;
		}

		public string StudentId
		{
			get
			{
				return DataPackageEnvelopeWorkerBase.UserToken;
			}
			set
			{
				DataPackageEnvelopeWorkerBase.UserToken = value;
			}
		}

		public event ResponseFromServerDelegate EventFromServer;

		public event EventHandler TimeToStop;

		public event EventHandler TimeToStart;

		public CommunicationsManager()
		{
			Receiver = new DataPackageEnvelopeAwsReceiver();
			_config = (TimersConfigSection)ConfigurationManager.GetSection("dpvSettings/timersConfig");
			Receiver.ResponseReceived += Receiver_ResponseReceived;
			_timeToStartTimer = new Timer
			{
				AutoReset = false
			};
			_timeToStopTimer = new Timer
			{
				AutoReset = false
			};
			_timeToStartTimer.Elapsed += _timeToStartTimer_Elapsed;
			_timeToStopTimer.Elapsed += _timeToStopTimer_Elapsed;
			CreateTimers();
			CurrentStatus = WorkflowStatus.StartedAndReadyToSignIn;
		}

		private void _timeToStartTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			StaticFileLogger.Current.LogEvent(GetType().Name, "_timeToStartTimer_Elapsed", "Starting worker timers", EventLogEntryType.Information);
			_timeToStartTimer.Enabled = false;
			if (CurrentStatus != WorkflowStatus.CollectingAndSendingData)
			{
				StartWorkerTimers();
				CurrentStatus = WorkflowStatus.CollectingAndSendingData;
			}
			this.TimeToStart?.Invoke(this, EventArgs.Empty);
		}

		private void _timeToStopTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			StaticFileLogger.Current.LogEvent(GetType().Name, "_timeToStopTimer_Elapsed", "Signaling TimeToStop event, stopping workers and disposing resources.", EventLogEntryType.Information);
			if (CurrentStatus != WorkflowStatus.StoppedAndPreparingToExit)
			{
				CurrentStatus = WorkflowStatus.StoppedAndPreparingToExit;
				StopWorkerTimers();
				this.TimeToStop?.Invoke(this, EventArgs.Empty);
				DisposeResources();
			}
			else
			{
				StaticFileLogger.Current.LogEvent(GetType().Name, "_timeToStopTimer_Elapsed", "CurrentStatus is already 'WorkflowStatus.StoppedAndPreparingToExit' - not performing anything further.", EventLogEntryType.Error);
			}
		}

		private void Receiver_ResponseReceived(object sender, ResponseFromServerEventArgs e)
		{
			StaticFileLogger.Current.LogEvent(GetType().Name, "Receiver_ResponseReceived", $"Response received: {((object)e.Receipt).ToString()}", EventLogEntryType.Information);
			if (e.Receipt.get_Status().HasValue)
			{
				switch (e.Receipt.get_Status().Value)
				{
				case 1:
					CurrentStatus = WorkflowStatus.SignedInButNotCollecting;
					break;
				case 10:
				case 11:
					CurrentStatus = WorkflowStatus.StoppedAndPreparingToExit;
					_heartbeatTimerAndWorker.Timer.Stop();
					break;
				}
				OnEventFromServer(e);
			}
			if (e.Receipt.get_SecToStart().HasValue)
			{
				ChangeSecondsToStart(e.Receipt.get_SecToStart().Value);
			}
			if (e.Receipt.get_SecToStop().HasValue)
			{
				ChangeSecondsToStop(e.Receipt.get_SecToStop().Value);
			}
		}

		private void ChangeSecondsToStop(double secondsToStop)
		{
			double num = secondsToStop * 1000.0 + 1000.0;
			StaticFileLogger.Current.LogEvent(GetType().Name, "ChangeSecondsToStop", $"Received {secondsToStop} from server. Setting SecondsToStop to {num}, to allow for timer to tick.", EventLogEntryType.Information);
			if (secondsToStop < 0.0)
			{
				secondsToStop = 0.0;
			}
			_timeToStopTimer.Interval = num;
			_timeToStopTimer.Enabled = true;
		}

		private void ChangeSecondsToStart(double secondsToStart)
		{
			StaticFileLogger.Current.LogEvent(GetType().Name, "ChangeSecondsToStart", $"Setting SecondsToStart to {secondsToStart}", EventLogEntryType.Information);
			if (secondsToStart > 0.0)
			{
				StopWorkerTimers();
			}
			_timeToStartTimer.Interval = secondsToStart * 1000.0 + 1000.0;
			_timeToStartTimer.Enabled = true;
		}

		private void RaiseEventOnUIThread(Delegate theEvent, object[] args)
		{
			Delegate[] invocationList = theEvent.GetInvocationList();
			foreach (Delegate @delegate in invocationList)
			{
				ISynchronizeInvoke synchronizeInvoke = @delegate.Target as ISynchronizeInvoke;
				if (synchronizeInvoke == null)
				{
					@delegate.DynamicInvoke(args);
				}
				else
				{
					synchronizeInvoke.BeginInvoke(@delegate, args);
				}
			}
		}

		protected void OnEventFromServer(ResponseFromServerEventArgs e)
		{
			this.EventFromServer?.Invoke(this, e);
		}

		private void CreateTimers()
		{
			StaticFileLogger.Current.LogEvent(GetType().Name, "CreateTimers", "Setting up", EventLogEntryType.Information);
			CreateHeartbeatWorkerAndTimer();
			CreateScreenshotworker();
			try
			{
				foreach (object timer2 in _config.Timers)
				{
					TimerInstantiation timerInstantiation = (TimerInstantiation)timer2;
					Type type = Type.GetType(timerInstantiation.WorkerToInstantiate);
					DataPackageEnvelopeWorkerBase newWorker = (DataPackageEnvelopeWorkerBase)Activator.CreateInstance(type, Receiver);
					newWorker.PackageDispatchNeeded += delegate
					{
						newWorker.DoWork();
					};
					Timer timer = new Timer(timerInstantiation.SecondsBetweenWork * 1000);
					timer.Elapsed += delegate
					{
						newWorker.DoWork();
					};
					TimerAndWorker item = new TimerAndWorker(newWorker, timer);
					_timersAndWorkers.Add(item);
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error creating timers from config file. The error was: '{ex.ToString()}'", ex);
			}
		}

		private void CreateScreenshotworker()
		{
			_screenshotWorker = new ScreenshotWorker(Receiver);
		}

		private void CreateHeartbeatWorkerAndTimer()
		{
			HeartbeatWorker heartbeatWorker = new HeartbeatWorker(Receiver);
			Timer timer = new Timer(10000.0);
			timer.Elapsed += delegate
			{
				heartbeatWorker.DoWork();
			};
			_heartbeatTimerAndWorker = new TimerAndWorker(heartbeatWorker, timer);
		}

		public void StartWorkerTimers()
		{
			StaticFileLogger.Current.LogEvent(GetType().Name, "StartWorkerTimers", null, EventLogEntryType.Information);
			_timersAndWorkers.ForEach(delegate(TimerAndWorker t)
			{
				t.Timer.Start();
			});
			CurrentBrowserUrlsTool.Start();
		}

		public void StopWorkerTimers()
		{
			StaticFileLogger.Current.LogEvent(GetType().Name, "StopWorkerTimers", null, EventLogEntryType.Information);
			foreach (TimerAndWorker timersAndWorker in _timersAndWorkers)
			{
				timersAndWorker.Timer.Stop();
			}
			_heartbeatTimerAndWorker.Timer.Enabled = false;
			CurrentBrowserUrlsTool.Stop();
		}

		public void DisposeResources()
		{
			foreach (TimerAndWorker timersAndWorker in _timersAndWorkers)
			{
				timersAndWorker.Worker.Dispose();
			}
			_timeToStopTimer.Enabled = false;
			_timeToStartTimer.Enabled = false;
			_heartbeatTimerAndWorker.Timer.Stop();
		}

		public void TrySignIn()
		{
			StaticFileLogger.Current.LogEvent(GetType().Name, "TrySignIn", $"Trying to sign in. CurrentStatus is {CurrentStatus}", EventLogEntryType.Error);
			if (CurrentStatus != WorkflowStatus.StartedAndReadyToSignIn)
			{
				StaticFileLogger.Current.LogEvent(GetType().Name, "TrySignIn", $"Tried to sign in, but CurrentStatus is {CurrentStatus}", EventLogEntryType.Error);
				throw new Exception("Not ready to sign in");
			}
			((HeartbeatWorker)_heartbeatTimerAndWorker.Worker).TryPerformSignIn();
			_heartbeatTimerAndWorker.Timer.Start();
		}

		public void SignOut()
		{
			if (CurrentStatus == WorkflowStatus.SignedInButNotCollecting || CurrentStatus == WorkflowStatus.CollectingAndSendingData || CurrentStatus == WorkflowStatus.StoppedAndPreparingToExit)
			{
				CurrentStatus = WorkflowStatus.SignOutSent;
				((HeartbeatWorker)_heartbeatTimerAndWorker.Worker).TryPerformSignOut();
			}
		}

		public async Task TakeAndSendScreenshot()
		{
			StaticFileLogger.Current.LogEvent(GetType().Name, "TakeAndSendScreenshot()", $"Asked to take screenshot. CurrentStatus is {CurrentStatus}", EventLogEntryType.Information);
			if (CurrentStatus == WorkflowStatus.CollectingAndSendingData)
			{
				StaticFileLogger.Current.LogEvent(GetType().Name, "TakeAndSendScreenshot()", "Taking screenshot.", EventLogEntryType.Information);
				await Task.Delay(500);
				_screenshotWorker.TakeAndSendScreenShot();
			}
			else
			{
				StaticFileLogger.Current.LogEvent(GetType().Name, "TakeAndSendScreenshot()", "Not taking screenshot.", EventLogEntryType.Information);
			}
		}
	}
}
