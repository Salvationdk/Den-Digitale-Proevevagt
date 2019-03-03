using DpvClassLibrary.Logging;
using DpvClassLibrary.Tools;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DpvClassLibrary.Receivers
{
	public class DataPackageEnvelopeAwsReceiver : IDataPackageEnvelopeReceiver, IDataPackageReceiptReceiverNotifier
	{
		private bool _hasSentDummyDemoResponse = false;

		private List<long> _debugSentMessageIds = new List<long>();

		public static ICommunicationLog LatestLog;

		private DataPackageEnvelopeQueue _queue = new DataPackageEnvelopeQueue();

		private bool _debugDumpTraffic = false;

		private static DateTime _lastServerTimeReceived = DateTime.Now;

		private static long _ticksAtLastServerTimeReceived = DateTime.Now.Ticks;

		public IO.Swagger.Client.Configuration Configuration
		{
			get;
			set;
		}

		public bool IsStudentDemoMode
		{
			get;
			set;
		}

		public bool IsInternalCGITestMode
		{
			get;
			set;
		}

		public static DateTime ServerTime
		{
			get
			{
				return _lastServerTimeReceived.AddTicks(TicksSinceLastServerTimeReceived);
			}
			private set
			{
				StaticFileLogger.Current.LogEvent("DataPackageEnvelopeAwsReceiver", $"ServerTime being set to {value}", null, EventLogEntryType.Information);
				_lastServerTimeReceived = value;
				_ticksAtLastServerTimeReceived = DateTime.Now.Ticks;
			}
		}

		private static long TicksSinceLastServerTimeReceived => DateTime.Now.Ticks - _ticksAtLastServerTimeReceived;

		public event ResponseFromServerDelegate ResponseReceived;

		public DataPackageEnvelopeAwsReceiver()
		{
			ConfigureFromSettings();
			string valueFromAppSettings = AppSettingsHelper.GetValueFromAppSettings("debugdump");
			if (!string.IsNullOrEmpty(valueFromAppSettings) && valueFromAppSettings.ToLower() == "true")
			{
				_debugDumpTraffic = true;
			}
		}

		private void ConfigureFromSettings()
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Expected O, but got Unknown
			string valueFromAppSettings = AppSettingsHelper.GetValueFromAppSettings("DpvClassLibrary.Receivers.DataPackageEnvelopeAwsReceiver.BasePath");
			string valueFromAppSettings2 = AppSettingsHelper.GetValueFromAppSettings("apikey");
			Configuration = new IO.Swagger.Client.Configuration((IDictionary<string, string>)new Dictionary<string, string>
			{
				{
					"x-api-key",
					valueFromAppSettings2
				}
			}, (IDictionary<string, string>)new Dictionary<string, string>(), (IDictionary<string, string>)new Dictionary<string, string>(), valueFromAppSettings);
		}

		public DataPackageEnvelopeAwsReceiver(IO.Swagger.Client.Configuration configuration)
		{
			Configuration = configuration;
		}

		public void AddDataPackageEnvelope(DataPackageEnvelope dataPackageEnvelope)
		{
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Expected O, but got Unknown
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Expected O, but got Unknown
			StaticFileLogger.Current.LogEvent(GetType().Name + ".AddDataPackageEnvelope()", $"Adding message:{dataPackageEnvelope.ToJson()} to queue.", "", EventLogEntryType.Information);
			if (IsInternalCGITestMode)
			{
				if (!_hasSentDummyDemoResponse)
				{
					int num = 10;
					int num2 = 60;
					string text = $"The DPV client is now running in demo mode. It will start harvesting in {num} seconds, and end in {num2} seconds.";
					OnResponseReceived(new DataPackageReceipt((int?)1, text, (int?)num, (int?)num2, (DateTime?)null, (List<DataPackageReceipt.ActivateColsEnum>)null, (List<CollectorConfig>)null));
					_hasSentDummyDemoResponse = true;
				}
			}
			else if (IsStudentDemoMode)
			{
				if (!_hasSentDummyDemoResponse)
				{
					OnResponseReceived(new DataPackageReceipt((int?)1, (string)null, (int?)null, (int?)null, (DateTime?)null, (List<DataPackageReceipt.ActivateColsEnum>)null, (List<CollectorConfig>)null));
					_hasSentDummyDemoResponse = true;
				}
			}
			else
			{
				try
				{
					AddEnvelopeToQueue(dataPackageEnvelope);
					TrySendingAllQueuedMessages();
				}
				catch (Exception ex)
				{
					StaticFileLogger.Current.LogEvent(GetType().Name + ".AddDataPackageEnvelope() exception", "Exception occurred", $"Error message: {ex.Message}", EventLogEntryType.Error);
				}
			}
		}

		private void TrySendingAllQueuedMessages()
		{
			StaticFileLogger.Current.LogEvent(GetType().Name, "TrySendingAllQueuedMessages", $"Current envelopes in queue: {_queue.EnvelopesInQueue}", EventLogEntryType.Information);
			for (int i = 0; i < _queue.EnvelopesInQueue; i++)
			{
				lock (this)
				{
					DataPackageEnvelope nextMessage = _queue.PeekNextDataPackageEnvelope();
					if (!TrySendingMessage(nextMessage))
					{
						return;
					}
					DataPackageEnvelope val = _queue.DequeueNextDataPackageEnvelopeToSend();
					_debugSentMessageIds.Add(val.Sequence.Value);
				}
			}
		}

		private bool TrySendingMessage(DataPackageEnvelope nextMessage)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Expected O, but got Unknown
			try
			{
				StaticFileLogger.Current.LogEvent(GetType().Name + ".TrySendingMessage()", $"Sending message #{nextMessage.Sequence} of type: " + nextMessage.Packages[0].ColType, "", EventLogEntryType.Information);
				ClientApi val = new ClientApi(Configuration);
				CommunicationLog communicationLog = new CommunicationLog();
				communicationLog.Request = Configuration.ApiClient.Serialize((object)nextMessage);
				DataPackageReceipt val2 = val.Push(nextMessage);
				if (val2.ServerTime.HasValue)
				{
					ServerTime = val2.ServerTime.Value;
				}
				OnResponseReceived(val2);
				communicationLog.Response = val2.ToJson();
				if (_debugDumpTraffic)
				{
					StaticFileLogger.Current.LogEvent(GetType().Name + ".TrySendingMessage()", $"    Data sent was: {communicationLog.ToString()}", "", EventLogEntryType.Information);
				}
				LatestLog = communicationLog;
				return true;
			}
			catch (Exception ex)
			{
				StaticFileLogger.Current.LogEvent(GetType().Name + ".TrySendingMessage()", $"    Error sending message to server. Message was : {ex.Message}. Exception is: {ex.ToString()}", "", EventLogEntryType.Error);
				return false;
			}
		}

		private void AddEnvelopeToQueue(DataPackageEnvelope dataPackageEnvelope)
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			StaticFileLogger.Current.LogEvent(GetType().Name + ".AddEnvelopeToQueue()", $"Adding message #{dataPackageEnvelope.Sequence} of type: " + dataPackageEnvelope.Packages[0].ColType + " to queue", "", EventLogEntryType.Information);
			_queue.AddDataPackageEnvelope(dataPackageEnvelope);
		}

		private void TrySendingQueuedPackages()
		{
			int envelopesInQueue = _queue.EnvelopesInQueue;
			if (envelopesInQueue > 0)
			{
				StaticFileLogger.Current.LogEvent(GetType().Name + ".TrySendingQueuedPackages()", $"Resending {envelopesInQueue} packages from queue", "", EventLogEntryType.Information);
				for (int i = 0; i < envelopesInQueue; i++)
				{
					AddDataPackageEnvelope(_queue.DequeueNextDataPackageEnvelopeToSend());
				}
			}
		}

		protected void OnResponseReceived(DataPackageReceipt receipt)
		{
			if (_debugDumpTraffic)
			{
				StaticFileLogger.Current.LogEvent(GetType().Name, "OnResponseReceived()", $"Receipt: {((object)receipt).ToString()}", EventLogEntryType.Information);
			}
			this.ResponseReceived?.Invoke(this, new ResponseFromServerEventArgs(receipt));
		}
	}
}
