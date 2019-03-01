using DpvClassLibrary.Logging;
using DpvClassLibrary.Receivers;
using DpvClassLibrary.Tools;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using static IO.Swagger.Model.DataPackageEnvelope;

namespace DpvClassLibrary.Workers
{
	public abstract class DataPackageEnvelopeWorkerBase : WorkerBase, IDisposable
	{
		private int _workSequence = 0;

		private static int _packageSequence = 0;

		private static object _lockObject = new object();

		private static List<ActiveColsEnum> _activeCols = new List<ActiveColsEnum>();

		public static string UserToken
		{
			get;
			set;
		}

		protected bool ShouldPerformSignInOnNextDoWork
		{
			get;
			set;
		}

		protected bool ShouldPerformSignOutOnNextDoWork
		{
			get;
			set;
		}

		public IDataPackageEnvelopeReceiver Receiver
		{
			get;
			set;
		}

		protected abstract ActiveColsEnum ActiveColType
		{
			get;
		}

		public static IEnumerable<ActiveColsEnum> GetActiveCols()
		{
			foreach (ActiveColsEnum activeCol in _activeCols)
			{
				yield return activeCol;
			}
		}

		protected static void AddActiveCol(ActiveColsEnum activeColToAdd)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			lock (_lockObject)
			{
				if (!_activeCols.Contains(activeColToAdd))
				{
					_activeCols.Add(activeColToAdd);
				}
			}
		}

		protected int GetAndIncrementWorkSequence()
		{
			_workSequence++;
			return _workSequence;
		}

		private static int GetAndIncrementPackageSequence()
		{
			_packageSequence++;
			return _packageSequence;
		}

		private string GetVersion()
		{
			return Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		private string GetDeviceIdentification()
		{
			return MachineFingerprintGenerator.GetFingerPrint();
		}

		public DataPackageEnvelopeWorkerBase(IDataPackageEnvelopeReceiver receiver)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			AddActiveCol(ActiveColType);
			Receiver = receiver;
		}

		protected override void DoConcreteWork()
		{
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Expected O, but got Unknown
			bool flag = false;
			flag = true;
			if (ShouldSendPackage())
			{
				DataPackageEnvelope packageEnvelope = null;
				lock (_lockObject)
				{
					packageEnvelope = new DataPackageEnvelope(UserToken, (bool?)ShouldPerformSignInOnNextDoWork, (bool?)ShouldPerformSignOutOnNextDoWork, (DateTime?)DataPackageEnvelopeAwsReceiver.ServerTime, GetActiveCols().ToList(), new List<CollectorConfig>(), (long?)GetAndIncrementPackageSequence(), Environment.OSVersion.ToString(), GetVersion(), GetDeviceIdentification(), (Guid?)Guid.NewGuid(), (bool?)flag, GetDataPackages());
					if (ShouldPerformSignInOnNextDoWork)
					{
						ShouldPerformSignInOnNextDoWork = false;
					}
				}
				try
				{
					Receiver.AddDataPackageEnvelope(packageEnvelope);
				}
				catch (Exception ex)
				{
					StaticFileLogger.Current.LogEvent(GetType().Name + ".DoConcreteWork()", "Error performing concrete work", ex.Message, EventLogEntryType.Error);
				}
			}
		}

		protected virtual bool ShouldSendPackage()
		{
			return true;
		}

		protected abstract List<DataPackage> GetDataPackages();

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposeInternally();
			}
		}

		~DataPackageEnvelopeWorkerBase()
		{
			Dispose(disposing: false);
		}

		protected virtual void DisposeInternally()
		{
		}
	}
}
