using DpvClassLibrary.Receivers;
using DpvClassLibrary.Tools;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace DpvClassLibrary.Workers
{
	public class ScreenshotWorker : DataPackageEnvelopeWorkerBase
	{
		protected override ActiveColsEnum ActiveColType => 3;

		public ScreenshotWorker(IDataPackageEnvelopeReceiver receiver)
			: base(receiver)
		{
		}

		protected override List<DataPackage> GetDataPackages()
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			Console.WriteLine("ScreenshotWorker.GetDataPackages");
			using (Image imageIn = ScreenCaptureTool.CaptureScreenNew())
			{
				byte[] array = ScreenCaptureTool.ImageToByteArray(imageIn);
				return new List<DataPackage>
				{
					new DataPackage(3, (bool?)false, array, (DateTime?)DataPackageEnvelopeAwsReceiver.ServerTime, (long?)GetAndIncrementWorkSequence())
				};
			}
		}

		public void TakeAndSendScreenShot()
		{
			DoWork();
		}
	}
}
