using QRCodeEncoderDecoderLibrary;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace DpvClassLibrary
{
	public static class QRCodeHelper
	{
		public static string GetQRCode(Bitmap imageToSearch)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Expected O, but got Unknown
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Expected O, but got Unknown
			if (false)
			{
				Dictionary<DecodeHintType, object> dictionary = new Dictionary<DecodeHintType, object>();
				QRCodeReader val = new QRCodeReader();
				BitmapLuminanceSource val2 = new BitmapLuminanceSource(imageToSearch);
				BinaryBitmap val3 = new BinaryBitmap(new HybridBinarizer(val2));
				Result val4 = val.decode(val3);
				return (val4 != null) ? val4.Text : null;
			}
			QRDecoder val5 = new QRDecoder();
			byte[][] array = val5.ImageDecoder(imageToSearch);
			if (array == null)
			{
				return null;
			}
			if (array.GetLength(0) > 0)
			{
				return Encoding.UTF8.GetString(array[0]);
			}
			return null;
		}
	}
}
