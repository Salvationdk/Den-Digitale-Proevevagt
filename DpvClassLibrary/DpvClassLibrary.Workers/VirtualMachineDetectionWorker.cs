using DpvClassLibrary.Receivers;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using static IO.Swagger.Model.DataPackage;
using static IO.Swagger.Model.DataPackageEnvelope;

namespace DpvClassLibrary.Workers
{
	public class VirtualMachineDetectionWorker : DataPackageEnvelopeWorkerBase
	{
		private static readonly List<string> _vmSubStrings = new List<string>
		{
			"vmware",
			"virtualpc",
			"virtualbox"
		};

		protected override ActiveColsEnum ActiveColType => (ActiveColsEnum)4;


        public bool CheatActive = false;



        public VirtualMachineDetectionWorker(IDataPackageEnvelopeReceiver receiver)
			: base(receiver)
		{
		}


		protected override List<DataPackage> GetDataPackages()
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			bool flag = AmIRunningInVirtualMachineOrHaveAVirtualMachineProcessRunning();
			return new List<DataPackage>
			{
				new DataPackage((ColTypeEnum)4, (bool?)false, Encoding.UTF8.GetBytes(flag.ToString()), (DateTime?)DataPackageEnvelopeAwsReceiver.ServerTime, (long?)GetAndIncrementWorkSequence())
			};
		}



        private bool IsVirtualMachineProcessRunning()
        {
            Process[] processes = Process.GetProcesses();
            return processes.Any((Process p) => p.ProcessName.ContainsOneOrMoreInList(_vmSubStrings));

        }

        public bool AmIRunningInsideAVirtualMachine()
		{
			bool result = false;
			try
			{
				ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
				foreach (ManagementObject item in managementObjectSearcher.Get())
				{
					result = (item["Manufacturer"].ToString().ToLower() == "microsoft corporation".ToLower());
				}
				return result;
			}
			catch
			{
				return result;
			}
		}

		protected override bool ShouldSendPackage()
		{
			return AmIRunningInVirtualMachineOrHaveAVirtualMachineProcessRunning();
		}







        #region Disable



        private bool AmIRunningInVirtualMachineOrHaveAVirtualMachineProcessRunning() 
        {
            if (!CheatActive)
            {
                return AmIRunningInsideAVirtualMachine() || IsVirtualMachineProcessRunning();
            }
            else return false;
        }



















        #endregion



















    }
}