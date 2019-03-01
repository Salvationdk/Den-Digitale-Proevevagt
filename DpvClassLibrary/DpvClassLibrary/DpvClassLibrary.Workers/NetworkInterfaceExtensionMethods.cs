using System.Net.NetworkInformation;

namespace DpvClassLibrary.Workers
{
	public static class NetworkInterfaceExtensionMethods
	{
		public static string GetStateAsString(this NetworkInterface networkInterface)
		{
			return $"Interface:{networkInterface.Name}, description:{networkInterface.Description}, status:{networkInterface.OperationalStatus}, id:{networkInterface.Id},type:{networkInterface.NetworkInterfaceType}";
		}
	}
}
