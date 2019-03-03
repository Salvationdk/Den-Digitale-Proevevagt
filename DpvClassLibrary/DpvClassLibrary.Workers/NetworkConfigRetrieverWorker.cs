using DpvClassLibrary.Receivers;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace DpvClassLibrary.Workers
{
	public class NetworkConfigRetrieverWorker : DataPackageEnvelopeWorkerBase
	{
		private int _numberOfInterfacesAtLastCount;

		private StringBuilder _builder = new StringBuilder();

		protected override DataPackageEnvelope.ActiveColsEnum ActiveColType => (DataPackageEnvelope.ActiveColsEnum)2;

		public NetworkConfigRetrieverWorker(IDataPackageEnvelopeReceiver receiver)
			: base(receiver)
		{
		}

		protected override List<DataPackage> GetDataPackages()
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			byte[] bytes = Encoding.UTF8.GetBytes(GetNetworkConfigurationData());
			return new List<DataPackage>
			{
				new DataPackage((DataPackage.ColTypeEnum)2, (bool?)false, bytes, (DateTime?)DataPackageEnvelopeAwsReceiver.ServerTime, (long?)GetAndIncrementWorkSequence())
			};
		}

		internal string GetNetworkConfigurationData()
		{
			_builder.Clear();
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			_numberOfInterfacesAtLastCount = allNetworkInterfaces.Count();
			(from nwi in allNetworkInterfaces.ToList()
			orderby nwi.OperationalStatus
			select nwi).ToList().ForEach(delegate(NetworkInterface nwi)
			{
				_builder.Append(nwi.GetStateAsString());
			});
			return _builder.ToString();
		}

		protected override bool ShouldSendPackage()
		{
			return NetworkInterface.GetAllNetworkInterfaces().Count() != _numberOfInterfacesAtLastCount;
		}
	}
}
