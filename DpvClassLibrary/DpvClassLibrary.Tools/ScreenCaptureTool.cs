using DpvClassLibrary.Logging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DpvClassLibrary.Tools
{
	public static class ScreenCaptureTool
	{
		private enum ProcessDPIAwareness
		{
			ProcessDPIUnaware,
			ProcessSystemDPIAware,
			ProcessPerMonitorDPIAware
		}

		public static Image CaptureScreenNew()
		{
			try
			{
				Rectangle bounds = Screen.PrimaryScreen.Bounds;
				Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
				Size blockRegionSize = new Size(bitmap.Width, bitmap.Height);
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.CopyFromScreen(0, 0, 0, 0, blockRegionSize);
					return bitmap;
				}
			}
			catch (Exception ex)
			{
				StaticFileLogger.Current.LogEvent("ScreenCaptureTool.CaptureScreenNew()", "Exception taking screenshot", $"Error is: '{ex.ToString()}'", EventLogEntryType.Information);
				return WriteExceptionToImage(ex);
			}
		}

		private static Image WriteExceptionToImage(Exception ex)
		{
			Bitmap bitmap = new Bitmap(800, 600);
			Size size = bitmap.Size;
			size.Height -= 20;
			size.Width -= 20;
			RectangleF layoutRectangle = new RectangleF(new PointF(10f, 10f), size);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				GraphicsUnit pageUnit = GraphicsUnit.Pixel;
				graphics.FillRectangle(Brushes.Wheat, bitmap.GetBounds(ref pageUnit));
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphics.DrawString(ex.ToString(), new Font("Tahoma", 10f), Brushes.Black, layoutRectangle);
			}
			return bitmap;
		}

		public static byte[] ImageToByteArray(Image imageIn, int jpgCompressionLevel = 30)
		{
			MemoryStream memoryStream = new MemoryStream();
			EncoderParameters encoderParameters = new EncoderParameters(1);
			encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, jpgCompressionLevel);
			imageIn.Save(memoryStream, GetEncoder(ImageFormat.Jpeg), encoderParameters);
			return memoryStream.ToArray();
		}

		[DllImport("shcore.dll")]
		private static extern int SetProcessDpiAwareness(ProcessDPIAwareness value);

		public static void SetDpiAwareness()
		{
			try
			{
				if (Environment.OSVersion.Version.Major >= 6)
				{
					SetProcessDpiAwareness(ProcessDPIAwareness.ProcessPerMonitorDPIAware);
				}
			}
			catch (Exception ex)
			{
				StaticFileLogger.Current.LogEvent("ScreenCaptureTool", "Error setting DPI awareness", ex.Message, EventLogEntryType.Error);
			}
		}

		private static ImageCodecInfo GetEncoder(ImageFormat format)
		{
			ImageCodecInfo[] imageDecoders = ImageCodecInfo.GetImageDecoders();
			ImageCodecInfo[] array = imageDecoders;
			foreach (ImageCodecInfo imageCodecInfo in array)
			{
				if (imageCodecInfo.FormatID == format.Guid)
				{
					return imageCodecInfo;
				}
			}
			return null;
		}
	}
}
