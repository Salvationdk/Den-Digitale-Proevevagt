using DPV.Properties;
using DpvClassLibrary;
using DpvClassLibrary.Logging;
using DpvClassLibrary.Tools;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;

namespace DPV
{
	public class UserIdEntryForm : Form
	{
		private bool _HACK_UserIdFoundInQRCode = false;

		private MainForm _mainForm;

		private System.Timers.Timer _stateTimer = new System.Timers.Timer();

		private int _loginAttempt = 0;

		private IContainer components = null;

		private TextBox txtUserID;

		private Label lblStudentID;

		private Label label1;

		private PictureBox picSpinner;

		private Button btnOk;

		public bool ClosingFromExternalSource
		{
			get;
			set;
		} = false;


		public bool SpinnerEnabled
		{
			get
			{
				return picSpinner.Visible;
			}
			set
			{
				picSpinner.Visible = value;
			}
		}

		public string UserID
		{
			get
			{
				return txtUserID.Text.ToUpper();
			}
			set
			{
				if (IsValidStudentIdFormat(value))
				{
					_stateTimer.Enabled = false;
					_mainForm.StudentId = value;
				}
			}
		}

		public UserIdEntryForm(MainForm mainForm)
		{
			_mainForm = mainForm;
			InitializeComponent();
			_stateTimer.Interval = 1000.0;
			_stateTimer.SynchronizingObject = this;
			_stateTimer.Elapsed += _stateTimer_Tick;
			_stateTimer.Start();
			base.FormClosing += UserIdEntryForm_FormClosing;
		}

		private void UserIdEntryForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!ClosingFromExternalSource)
			{
				if (string.IsNullOrWhiteSpace(UserID))
				{
					e.Cancel = !_mainForm.AskForPermissionBeforeClosing();
				}
				else
				{
					_stateTimer.Stop();
					e.Cancel = _mainForm.AskForPermissionBeforeClosing();
				}
			}
		}

		private void _stateTimer_Tick(object sender, EventArgs e)
		{
			_loginAttempt++;
			StaticFileLogger.Current.LogEvent(GetType().Name + $"._stateTimer_Tick(), _loginAttempt={_loginAttempt}", "Look for QR code", "", EventLogEntryType.Information);
			string text = null;
			using (Bitmap bitmap = (Bitmap)ScreenCaptureTool.CaptureScreenNew())
			{
				text = QRCodeHelper.GetQRCode(bitmap);
				if (!IsValidStudentIdFormat(text))
				{
					string description = Convert.ToBase64String(ScreenCaptureTool.ImageToByteArray(bitmap, 75));
					StaticFileLogger.Current.LogEvent(GetType().Name + "._stateTimer_Tick()", "No QR code found. Screenshot encoded in BASE64:", description, EventLogEntryType.Information);
				}
				else
				{
					StaticFileLogger.Current.LogEvent(GetType().Name + "._stateTimer_Tick()", $"QR code found: '{text}'", "", EventLogEntryType.Information);
					UserID = text;
					_HACK_UserIdFoundInQRCode = true;
				}
				if (_loginAttempt >= 5)
				{
					StaticFileLogger.Current.LogEvent(GetType().Name + "._stateTimer_Tick()", "log in info", $"_loginAttempt is larger than five and _HACK_UserIdFoundInQRCode={_HACK_UserIdFoundInQRCode}", EventLogEntryType.Information);
					if (!_HACK_UserIdFoundInQRCode)
					{
						_mainForm.InvokeUI(delegate
						{
							Show();
							base.WindowState = FormWindowState.Minimized;
							base.WindowState = FormWindowState.Normal;
							BringToFront();
							Focus();
							Activate();
							base.TopMost = true;
						});
					}
					_stateTimer.Enabled = false;
				}
			}
		}

		private bool IsValidStudentIdFormat(string content)
		{
			return !string.IsNullOrEmpty(content) && content.Length == 6;
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void txtUserID_TextChanged(object sender, EventArgs e)
		{
			if (IsValidStudentIdFormat(UserID))
			{
				btnOk.Enabled = true;
			}
			else
			{
				btnOk.Enabled = false;
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			UserID = txtUserID.Text.ToUpper();
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
            this.txtUserID = new System.Windows.Forms.TextBox();
            this.lblStudentID = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.picSpinner = new System.Windows.Forms.PictureBox();
            this.btnOk = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picSpinner)).BeginInit();
            this.SuspendLayout();
            // 
            // txtUserID
            // 
            this.txtUserID.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserID.Location = new System.Drawing.Point(218, 10);
            this.txtUserID.Margin = new System.Windows.Forms.Padding(6);
            this.txtUserID.Name = "txtUserID";
            this.txtUserID.Size = new System.Drawing.Size(199, 38);
            this.txtUserID.TabIndex = 0;
            this.txtUserID.TextChanged += new System.EventHandler(this.txtUserID_TextChanged);
            // 
            // lblStudentID
            // 
            this.lblStudentID.AutoSize = true;
            this.lblStudentID.Location = new System.Drawing.Point(12, 17);
            this.lblStudentID.Name = "lblStudentID";
            this.lblStudentID.Size = new System.Drawing.Size(162, 26);
            this.lblStudentID.TabIndex = 1;
            this.lblStudentID.Text = "Eksamenskode";
            this.lblStudentID.Click += new System.EventHandler(this.lblStudentID_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(214, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(203, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Indtast eksamenskode (6 tegn)";
            // 
            // picSpinner
            // 
            this.picSpinner.Image = global::DPV.Properties.Resources.ajax_loader_gray_48;
            this.picSpinner.Location = new System.Drawing.Point(490, 7);
            this.picSpinner.Name = "picSpinner";
            this.picSpinner.Size = new System.Drawing.Size(48, 48);
            this.picSpinner.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picSpinner.TabIndex = 5;
            this.picSpinner.TabStop = false;
            this.picSpinner.Visible = false;
            // 
            // btnOk
            // 
            this.btnOk.AutoSize = true;
            this.btnOk.Enabled = false;
            this.btnOk.Location = new System.Drawing.Point(420, 10);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(64, 39);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "OK";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // UserIdEntryForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(548, 95);
            this.Controls.Add(this.picSpinner);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblStudentID);
            this.Controls.Add(this.txtUserID);
            this.Controls.Add(this.btnOk);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserIdEntryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Digital prøvevagt";
            ((System.ComponentModel.ISupportInitialize)(this.picSpinner)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        private void lblStudentID_Click(object sender, EventArgs e)
        {

        }
    }
}
