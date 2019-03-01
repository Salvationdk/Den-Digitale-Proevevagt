using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DpvClassLibrary.Tools
{
	public class ActiveWindowChangedDetectorTool
	{
		private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

		private static WinEventDelegate dele = null;

		private const uint WINEVENT_OUTOFCONTEXT = 0u;

		private const uint EVENT_SYSTEM_FOREGROUND = 3u;

		public bool IsEnabled
		{
			get;
			set;
		}

		public event EventHandler<string> ActiveWindowChanged;

		public ActiveWindowChangedDetectorTool()
		{
			SubscribeToActiveWindowChangedEvent();
		}

		[DllImport("user32.dll")]
		private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

		private static string GetActiveWindowTitle()
		{
			IntPtr zero = IntPtr.Zero;
			StringBuilder stringBuilder = new StringBuilder(256);
			zero = GetForegroundWindow();
			if (GetWindowText(zero, stringBuilder, 256) > 0)
			{
				return stringBuilder.ToString();
			}
			return null;
		}

		public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
		{
			OnActiveWindowChanged(GetActiveWindowTitle());
		}

		private void OnActiveWindowChanged(string newWindowTitle)
		{
			if (IsEnabled)
			{
				this.ActiveWindowChanged?.Invoke(null, newWindowTitle);
			}
		}

		private void SubscribeToActiveWindowChangedEvent()
		{
			dele = WinEventProc;
			IntPtr intPtr = SetWinEventHook(3u, 3u, IntPtr.Zero, dele, 0u, 0u, 0u);
		}
	}
}
