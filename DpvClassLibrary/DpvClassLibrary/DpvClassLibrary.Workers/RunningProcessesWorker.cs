using DpvClassLibrary.Receivers;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DpvClassLibrary.Workers
{
	public class RunningProcessesWorker : DataPackageEnvelopeWorkerBase
	{
		private int _lastNumberOfRunningProcesses = 0;

		protected override ActiveColsEnum ActiveColType => 1;

		public RunningProcessesWorker(IDataPackageEnvelopeReceiver receiver)
			: base(receiver)
		{
		}

		protected override List<DataPackage> GetDataPackages()
		{
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Expected O, but got Unknown
			Process[] processes = Process.GetProcesses();
			_lastNumberOfRunningProcesses = processes.Count();
			string text = "";
			Process[] array = processes;
			foreach (Process process in array)
			{
				text = text + "Id=" + process.Id + ",Name =" + process.ProcessName + ",";
				try
				{
					text = text + "Description=" + process.MainModule.FileVersionInfo.FileDescription + ";";
				}
				catch (Exception ex)
				{
					if (ex.ToString() != "")
					{
						text += "Description=;";
					}
				}
			}
			return new List<DataPackage>
			{
				new DataPackage(5, (bool?)false, Encoding.UTF8.GetBytes(text), (DateTime?)DataPackageEnvelopeAwsReceiver.ServerTime, (long?)GetAndIncrementWorkSequence())
			};
		}

		protected override bool ShouldSendPackage()
		{
			return Process.GetProcesses().Count() != _lastNumberOfRunningProcesses;
		}
	}
}
