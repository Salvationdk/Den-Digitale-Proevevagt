using DpvClassLibrary;
using DpvClassLibrary.Logging;
using DpvClassLibrary.Receivers;
using DpvClassLibrary.Tools;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DPV
{
	public class MainForm : Form
	{
		private const string DEMOMODE_EXAMCODE = "000000";

		private const string INTERNAL_CGI_TEST_EXAMCODE = "_TEST_";

		private int _borderWidth = 4;

		private static readonly Color _borderColor = Color.FromArgb(255, 177, 0);

		private static readonly Color _transparentColor = Color.Lime;

		private static readonly SolidBrush _borderBrush = new SolidBrush(_borderColor);

		private static readonly SolidBrush _transparentBrush = new SolidBrush(_transparentColor);

		private CommunicationsManager _communicationsManager;

		private static MainForm Instance;

		private ActiveWindowChangedDetectorTool _activeWindowChangedDetectorTool = new ActiveWindowChangedDetectorTool();

		private DPVStatusBar _statusbar;

		private const int WM_DEVICECHANGE = 537;

		private const int DBT_DEVICEARRIVAL = 32768;

		private const int DBT_DEVICEREMOVECOMPLETE = 32772;

		protected bool _hasSentSigninPacketToServer = false;

		private UserIdEntryForm _frmUser;

		private string _studentId;

		private int changesToActiveWindow = 0;

		private int screenShotsSent = 0;

		private IContainer components = null;

		private NotifyIcon ntfy_DPV;

		private BackgroundWorker signInBackgroundWorker;

		public string StudentId
		{
			get
			{
				return _studentId;
			}
			set
			{
				if (!string.IsNullOrEmpty(value) && _studentId != value)
				{
					InvokeUI(delegate
					{
						_frmUser.SpinnerEnabled = true;
						_studentId = value;
						if (_studentId == "000000")
						{
							SetDemoModeOnReceiverSoItDoesntSendHarvestedData();
						}
						else if (_studentId == "_TEST_")
						{
							SetInternalCGITestModeOnReceiverSoItSimulatesSendingHarvestedData();
							SetDemoModeOnReceiverSoItDoesntSendHarvestedData();
						}
						_communicationsManager.StudentId = value;
						SendSignInPacketToServer();
					});
				}
			}
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= 128;
				return createParams;
			}
		}

		private static string AssemblyVersion
		{
			get
			{
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				string result = string.Empty;
				if (ApplicationDeployment.IsNetworkDeployed)
				{
					result = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
				}
				else if (executingAssembly != null)
				{
					result = executingAssembly.GetName().Version.ToString();
				}
				return result;
			}
		}

		private void SetDemoModeOnReceiverSoItDoesntSendHarvestedData()
		{
			_communicationsManager.Receiver.IsStudentDemoMode = true;
		}

		private void SetInternalCGITestModeOnReceiverSoItSimulatesSendingHarvestedData()
		{
			_communicationsManager.Receiver.IsInternalCGITestMode = true;
		}

		public MainForm(string studentId = null)
		{
			StaticFileLogger.Current.LogEvent(GetType().Name + ".MainForm()", "DPV starts running", "", EventLogEntryType.Information);
			Instance = this;
			InitializeComponent();
			CreateContextMenu();
			SubscribeToMessagesFromCommunicationsManager();
			SetupStatusBar();
			StudentId = studentId;
			SetWindowsFormsStyleToEnsureFitToScreenAndInteractionWithTaskbar();
			SubscribeToChangesInScreenResolution();
		}




		private static void SubscribeToChangesInScreenResolution()
		{
			SystemEvents.DisplaySettingsChanged += ChangeInDisplaySettings;
			SystemEvents.DisplaySettingsChanging += ChangeInDisplaySettings;
		}

		private static void ChangeInDisplaySettings(object sender, EventArgs e)
		{
			Instance.SetWindowsFormsStyleToEnsureFitToScreenAndInteractionWithTaskbar();
		}

		private void SetWindowsFormsStyleToEnsureFitToScreenAndInteractionWithTaskbar()
		{
			base.FormBorderStyle = FormBorderStyle.Sizable;
			base.WindowState = FormWindowState.Maximized;
			Size size = Screen.PrimaryScreen.WorkingArea.Size;
			Size size2 = Screen.PrimaryScreen.Bounds.Size;
			StaticFileLogger.Current.LogEvent(GetType().Name + ".SetWindowsFormsStyleToEnsureFitToScreenAndInteractionWithTaskbar()", $"Window size: '{size2}', Working size: '{size}'", "", EventLogEntryType.Information);
			size2.Height--;
			MaximumSize = size2;
			base.TopMost = true;
			base.FormBorderStyle = FormBorderStyle.None;
			base.ShowInTaskbar = false;
		}

		private void _communicationsManager_TimeToStop(object sender, EventArgs e)
		{
			StaticFileLogger.Current.LogEvent(GetType().Name + "._communicationsManager_TimeToStop()", "Time to stop received from _communicationsManager", "", EventLogEntryType.Information);
			Invoke((Action)delegate
			{
				MessageBox.Show(this, "Den digitale prøvevagt afsluttes fordi du\nhar afleveret din besvarelse eller fordi sluttiden for\ndin eksamen er passeret.", "Den digitale prøvevagt afsluttes", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			});
			CleanupAndCloseApplication();
		}

		private void _communicationsManager_EventFromServer(object sender, ResponseFromServerEventArgs e)
		{
			InvokeUI(delegate
			{
				ResponseFromServerEventArgs.ServerResponseStatus status = e.Status;
				switch (status)
				{
				case ResponseFromServerEventArgs.ServerResponseStatus.Empty:
					break;
				default:
					if ((uint)(status - 10) <= 1u)
					{
						_frmUser.SpinnerEnabled = false;
						if (_communicationsManager.CurrentStatus != WorkflowStatus.SignOutSent)
						{
							MessageBox.Show(this, e.Receipt.Text, "Fejl i elev ID", MessageBoxButtons.OK, MessageBoxIcon.Hand);
							CreateContextMenu();
							_communicationsManager = new CommunicationsManager();
							_communicationsManager.EventFromServer += _communicationsManager_EventFromServer;
							_communicationsManager.TimeToStop += _communicationsManager_TimeToStop;
							StudentId = null;
						}
					}
					else
					{
						MessageBox.Show(this, e.Receipt.Text, "Message from server", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					}
					break;
				case ResponseFromServerEventArgs.ServerResponseStatus.SuccessfulLogin:
					_frmUser.SpinnerEnabled = false;
					_frmUser.ClosingFromExternalSource = true;
					_frmUser.Close();
					_statusbar.StudentId = "Eksamenskode: " + StudentId;
					SubscribeToActiveWindowChangedTool();
					if (_communicationsManager.Receiver.IsInternalCGITestMode)
					{
						MessageBox.Show(this, e.Receipt.Text, "Message from server", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					}
					break;
				}
			});
		}

		private void SetupStatusBar()
		{
			_statusbar = new DPVStatusBar();
			base.Controls.Add(_statusbar);
			_statusbar.Top = 0;
			_statusbar.SetBackColor(_borderColor);
			_statusbar.Visible = true;
			_statusbar.Expanded = false;
		}

		private void SubscribeToActiveWindowChangedTool()
		{
			_activeWindowChangedDetectorTool.ActiveWindowChanged += _activeWindowChangedTool_ActiveWindowChanged;
			base.Disposed += MainForm_Disposed;
		}

		private void SendSignInPacketToServer()
		{
			signInBackgroundWorker.RunWorkerAsync();
		}

		private void MainForm_Disposed(object sender, EventArgs e)
		{
			CurrentBrowserUrlsTool.Stop();
			_communicationsManager.StopWorkerTimers();
			_communicationsManager.DisposeResources();
		}

		private async void _activeWindowChangedTool_ActiveWindowChanged(object sender, string e)
		{
			StaticFileLogger.Current.LogEvent(GetType().Name, "Active window changed", "", EventLogEntryType.Information);
			changesToActiveWindow++;
			await ForceTakeScreenShot();
		}

		private async Task ForceTakeScreenShot()
		{
			await(_communicationsManager?.TakeAndSendScreenshot());
			screenShotsSent++;
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			if (m.Msg == 537)
			{
				StaticFileLogger.Current.LogEvent(GetType().Name + ".WndProc()", "Devicechange", "", EventLogEntryType.Information);
				ForceTakeScreenShot().ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		private void CreateContextMenu()
		{
			ContextMenu contextMenu = new ContextMenu();
			MenuItem item = new MenuItem("Open &AWS log website", delegate
			{
				Process.Start("https://eu-west-1.console.aws.amazon.com/cloudwatch/home?region=eu-west-1#logs:");
			});
			contextMenu.MenuItems.Add(item);
			contextMenu.MenuItems.Add(new MenuItem("-"));
			MenuItem item2 = new MenuItem("&Exit", delegate
			{
				AskForPermissionBeforeClosing();
			});
			contextMenu.MenuItems.Add(item2);
			ntfy_DPV.ContextMenu = contextMenu;
		}

		public void CleanupAndCloseApplication()
		{
            
             if (_frmUser != null && _frmUser.Visible)
			{
                //	_frmUser.ClosingFromExternalSource = true;
                //	_frmUser.Close();

                _frmUserCloseFromExternalSource(); // Replaces the actions above.

            }





            _communicationsManager.SignOut();
			_activeWindowChangedDetectorTool.IsEnabled = false;
			Thread.Sleep(1500);
			Invoke((Action)delegate
			{
				Close();
			});
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			CenterStatusbar();
		}

		private void CenterStatusbar()
		{
			if (_statusbar != null)
			{
				_statusbar.Left = (base.Width - _statusbar.Width) / 2;
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.FillRectangle(_borderBrush, new Rectangle(0, 0, base.Width, base.Height));
			e.Graphics.FillRectangle(_transparentBrush, new Rectangle(_borderWidth, _borderWidth, base.Width - _borderWidth * 2, base.Height - _borderWidth * 2));
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			ntfy_DPV.Visible = false;
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			if (StudentId == null)
			{
				_frmUser = new UserIdEntryForm(this);
				_frmUser.Hide();
			}
		}

		private void signInBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			_communicationsManager.TrySignIn();
		}

		internal void InvokeUI(Action a)
		{
			BeginInvoke(new MethodInvoker(a.Invoke));
		}

		public bool AskForPermissionBeforeClosing()
		{
			if (MessageBox.Show(this, "Du prøver at lukke den digitale prøvevagt!!\n\nEr eksamen IKKE slut, må du ikke afslutte den digitale prøvevagt, det kan medføre bortvisning fra eksamen.", "Afslut den digitale prøvevagt", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
			{
				CleanupAndCloseApplication();
				return true;
			}
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(DPV.MainForm));
			ntfy_DPV = new System.Windows.Forms.NotifyIcon(components);
			signInBackgroundWorker = new System.ComponentModel.BackgroundWorker();
			SuspendLayout();
			ntfy_DPV.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("ntfy_DPV.Icon");
			ntfy_DPV.Text = "Digital prøvevagt kører...";
			ntfy_DPV.Visible = true;
			signInBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(signInBackgroundWorker_DoWork);
			base.AutoScaleDimensions = new System.Drawing.SizeF(12f, 25f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.SystemColors.Control;
			base.ClientSize = new System.Drawing.Size(1914, 1030);
			DoubleBuffered = true;
			Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			base.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			base.Name = "MainForm";
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Digital Prøvevagt";
			base.TransparencyKey = System.Drawing.Color.Lime;
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(MainForm_FormClosing);
			base.Load += new System.EventHandler(MainForm_Load);
			base.Resize += new System.EventHandler(MainForm_Resize);
			ResumeLayout(performLayout: false);
		}






        #region Sections to comment out




        //Active cheat


        public static bool CheatActive = true;






        private void _communicationsManager_TimeToStart(object sender, EventArgs e) //Disable to deactive the application from registering window changes.
        {

          
         //  if(!CheatActive)
                _activeWindowChangedDetectorTool.IsEnabled = true;

            
        }

        private void _frmUserCloseFromExternalSource() //Used in CleanupAndCloseApplication() - This *may* note that the application was closed by an external source.
        {
            //  if(!CheatActive)
         //   {
                _frmUser.ClosingFromExternalSource = true;
                _frmUser.Close();
            //}
        }


        private void SubscribeToMessagesFromCommunicationsManager() //Starts and subscribes to logging events.
        {
            _communicationsManager = new CommunicationsManager();
            _communicationsManager.CheatActive = CheatActive;
            _communicationsManager.EventFromServer += _communicationsManager_EventFromServer;
            _communicationsManager.TimeToStop += _communicationsManager_TimeToStop;
            _communicationsManager.TimeToStart += _communicationsManager_TimeToStart;
        }









        #endregion

















    }
}
