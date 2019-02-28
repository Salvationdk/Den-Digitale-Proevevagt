using DPV.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DPV
{
	public class DPVStatusBar : UserControl
	{
		private bool _expanded;

		private Point _lastMousePosition;

		private IContainer components = null;

		private PictureBox picHatLogo;

		private PictureBox picFoldingArrow;

		private LinkLabel lnkExit;

		private ImageList imglstIcons;

		private TextBox txtMessage;

		private Label lblStudentId;

		public override string Text
		{
			get
			{
				return txtMessage.Text;
			}
			set
			{
				txtMessage.Text = value;
			}
		}

		public string StudentId
		{
			get
			{
				return lblStudentId.Text;
			}
			set
			{
				lblStudentId.Text = value;
				lblStudentId.Visible = !string.IsNullOrEmpty(lblStudentId.Text);
			}
		}

		private bool MouseIsDown
		{
			get;
			set;
		}

		public bool Expanded
		{
			get
			{
				return _expanded;
			}
			set
			{
				_expanded = value;
				if (Expanded)
				{
					base.Height = 250;
					lnkExit.Top = base.Height - (lnkExit.Height + 20);
					lblStudentId.Top = lnkExit.Top - (lblStudentId.Height + 40);
					picFoldingArrow.Image = imglstIcons.Images[1];
				}
				else
				{
					base.Height = 35;
					picFoldingArrow.Image = imglstIcons.Images[0];
				}
				base.Left = GetStatusbarLeftToEnsureItIsOnscreen(base.Left);
				lblStudentId.Visible = Expanded;
				txtMessage.Visible = Expanded;
				lnkExit.Visible = Expanded;
			}
		}

		public DPVStatusBar()
		{
			InitializeComponent();
			base.MouseDown += StatusbarMouseDown;
			base.MouseUp += StatusbarMouseUp;
			base.MouseMove += StatusbarMouseMove;
			picFoldingArrow.Click += PicFoldingArrow_Click;
			Text = "Den Digitale Prøvevagt overvåger dig KUN under prøven og afsluttes automatisk, når du afleverer.";
			Expanded = false;
		}

		private void PicFoldingArrow_Click(object sender, EventArgs e)
		{
			Expanded = !Expanded;
		}

		private void StatusbarMouseDown(object sender, MouseEventArgs e)
		{
			MouseIsDown = true;
			_lastMousePosition = e.Location;
		}

		private void StatusbarMouseUp(object sender, MouseEventArgs e)
		{
			MouseIsDown = false;
		}

		private void StatusbarMouseMove(object sender, MouseEventArgs e)
		{
			if (MouseIsDown)
			{
				int num = e.Location.X - _lastMousePosition.X;
				int desiredLeft = base.Left + num;
				_lastMousePosition.X = e.Location.X - num;
				desiredLeft = (base.Left = GetStatusbarLeftToEnsureItIsOnscreen(desiredLeft));
				Refresh();
			}
		}

		private int GetStatusbarLeftToEnsureItIsOnscreen(int desiredLeft)
		{
			int num = 30;
			if (desiredLeft < num)
			{
				return num;
			}
			if (desiredLeft + base.Width > Screen.PrimaryScreen.Bounds.Width - num)
			{
				return Screen.PrimaryScreen.Bounds.Width - num - base.Width;
			}
			return desiredLeft;
		}

		private void lnkExit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			((MainForm)base.Parent).AskForPermissionBeforeClosing();
		}

		internal void SetBackColor(Color borderColor)
		{
			BackColor = borderColor;
			txtMessage.BackColor = borderColor;
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
			System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(DPV.DPVStatusBar));
			picFoldingArrow = new System.Windows.Forms.PictureBox();
			picHatLogo = new System.Windows.Forms.PictureBox();
			lnkExit = new System.Windows.Forms.LinkLabel();
			imglstIcons = new System.Windows.Forms.ImageList(components);
			txtMessage = new System.Windows.Forms.TextBox();
			lblStudentId = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)picFoldingArrow).BeginInit();
			((System.ComponentModel.ISupportInitialize)picHatLogo).BeginInit();
			SuspendLayout();
			picFoldingArrow.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			picFoldingArrow.Cursor = System.Windows.Forms.Cursors.Hand;
			picFoldingArrow.Image = DPV.Properties.Resources.arrow_down;
			picFoldingArrow.Location = new System.Drawing.Point(339, 9);
			picFoldingArrow.Margin = new System.Windows.Forms.Padding(4);
			picFoldingArrow.Name = "picFoldingArrow";
			picFoldingArrow.Size = new System.Drawing.Size(24, 10);
			picFoldingArrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			picFoldingArrow.TabIndex = 1;
			picFoldingArrow.TabStop = false;
			picHatLogo.Image = DPV.Properties.Resources.icons8_graduation_cap_50;
			picHatLogo.Location = new System.Drawing.Point(16, 0);
			picHatLogo.Margin = new System.Windows.Forms.Padding(4);
			picHatLogo.Name = "picHatLogo";
			picHatLogo.Size = new System.Drawing.Size(32, 30);
			picHatLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			picHatLogo.TabIndex = 0;
			picHatLogo.TabStop = false;
			lnkExit.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			lnkExit.AutoSize = true;
			lnkExit.BackColor = System.Drawing.Color.Transparent;
			lnkExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			lnkExit.LinkColor = System.Drawing.Color.Black;
			lnkExit.Location = new System.Drawing.Point(24, 189);
			lnkExit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			lnkExit.MinimumSize = new System.Drawing.Size(50, 25);
			lnkExit.Name = "lnkExit";
			lnkExit.Size = new System.Drawing.Size(50, 25);
			lnkExit.TabIndex = 2;
			lnkExit.TabStop = true;
			lnkExit.Text = "Stop";
			lnkExit.VisitedLinkColor = System.Drawing.Color.Black;
			lnkExit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkExit_LinkClicked);
			imglstIcons.ImageStream = (System.Windows.Forms.ImageListStreamer)componentResourceManager.GetObject("imglstIcons.ImageStream");
			imglstIcons.TransparentColor = System.Drawing.Color.Transparent;
			imglstIcons.Images.SetKeyName(0, "arrow_down.png");
			imglstIcons.Images.SetKeyName(1, "arrow_up.png");
			txtMessage.BackColor = System.Drawing.SystemColors.Control;
			txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
			txtMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			txtMessage.Location = new System.Drawing.Point(28, 48);
			txtMessage.Margin = new System.Windows.Forms.Padding(4);
			txtMessage.Multiline = true;
			txtMessage.Name = "txtMessage";
			txtMessage.Size = new System.Drawing.Size(310, 137);
			txtMessage.TabIndex = 4;
			txtMessage.Text = "Den Digitale Prøvevagt overvåger dig KUN under prøven og afsluttes automatisk, når du afleverer.";
			txtMessage.MouseDown += new System.Windows.Forms.MouseEventHandler(StatusbarMouseDown);
			txtMessage.MouseMove += new System.Windows.Forms.MouseEventHandler(StatusbarMouseMove);
			txtMessage.MouseUp += new System.Windows.Forms.MouseEventHandler(StatusbarMouseUp);
			lblStudentId.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			lblStudentId.AutoSize = true;
			lblStudentId.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			lblStudentId.Location = new System.Drawing.Point(24, 144);
			lblStudentId.Name = "lblStudentId";
			lblStudentId.Size = new System.Drawing.Size(219, 20);
			lblStudentId.TabIndex = 5;
			lblStudentId.Text = "Ingen eksamenskode endnu.";
			base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(lblStudentId);
			base.Controls.Add(lnkExit);
			base.Controls.Add(picFoldingArrow);
			base.Controls.Add(picHatLogo);
			base.Controls.Add(txtMessage);
			base.Margin = new System.Windows.Forms.Padding(4);
			base.Name = "DPVStatusBar";
			base.Size = new System.Drawing.Size(375, 225);
			((System.ComponentModel.ISupportInitialize)picFoldingArrow).EndInit();
			((System.ComponentModel.ISupportInitialize)picHatLogo).EndInit();
			ResumeLayout(performLayout: false);
			PerformLayout();
		}
	}
}
