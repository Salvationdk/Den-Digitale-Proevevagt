using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DpvClassLibrary.Tools
{
	public static class KeyloggerHelper
	{
		public delegate void KeyEventHandler(object sender, KeyEventArgs e);

		private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		public class KeyPressedArgs : EventArgs
		{
			public Keys Key
			{
				get;
				set;
			}
		}

		private const int WH_KEYBOARD_LL = 13;

		private const int WM_KEYDOWN = 256;

		private static LowLevelKeyboardProc _proc = HookCallback;

		private static IntPtr _hookID = IntPtr.Zero;

		public static event KeyEventHandler KeyPressed;

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		private static void OnKeyPressed(Keys keyPressed)
		{
			KeyloggerHelper.KeyPressed?.Invoke("no object", new KeyEventArgs(keyPressed));
		}

		public static void HookUp()
		{
			_hookID = SetHook(_proc);
		}

		public static void Unhook()
		{
			UnhookWindowsHookEx(_hookID);
		}

		private static IntPtr SetHook(LowLevelKeyboardProc proc)
		{
			using (Process process = Process.GetCurrentProcess())
			{
				using (ProcessModule processModule = process.MainModule)
				{
					return SetWindowsHookEx(13, proc, GetModuleHandle(processModule.ModuleName), 0u);
				}
			}
		}

		private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 && wParam == (IntPtr)256)
			{
				int keyPressed = Marshal.ReadInt32(lParam);
				OnKeyPressed((Keys)keyPressed);
			}
			return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}
	}
}
