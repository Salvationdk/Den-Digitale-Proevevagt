using DpvClassLibrary.Logging;
using System;
using System.Diagnostics;

namespace DpvClassLibrary.Workers
{
	public abstract class WorkerBase
	{
		public class PackageForDispatchEventArgs
		{
			public string ReasonForDispatchNeed
			{
				get;
				private set;
			}

			public PackageForDispatchEventArgs(string reasonForDispatch)
			{
				ReasonForDispatchNeed = reasonForDispatch;
			}
		}

		private object _lockObject = new object();

		public string Name
		{
			get;
			set;
		}

		public bool IsReady
		{
			protected get;
			set;
		} = true;


		public event EventHandler<PackageForDispatchEventArgs> PackageDispatchNeeded;

		public void DoWork()
		{
			lock (_lockObject)
			{
				if (IsReady)
				{
					IsReady = false;
					try
					{
						DoConcreteWork();
					}
					catch (Exception ex)
					{
						if (!ex.ToString().ToLower().Contains("0x80004005"))
						{
							throw new Exception("Exception while trying to perform DoConcreteWork. Error is: " + ex.ToString(), ex);
						}
						IsReady = true;
					}
					finally
					{
						IsReady = true;
					}
				}
			}
		}

		protected abstract void DoConcreteWork();

		protected void OnPackageForDispatch(string message)
		{
			StaticFileLogger.Current.LogEvent(GetType().Name, "PackageForDispatch", $"message:{message}", EventLogEntryType.Information);
			this.PackageDispatchNeeded?.Invoke(this, new PackageForDispatchEventArgs(message));
		}
	}
}
