using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace ExpressionDark
{
    class DarkMessageBox : Form
    {
        private const int CS_DROPSHADOW = 0x00020000;
        private static DarkMessageBox _darkMessageBox;
        private Panel _plHeader = new Panel();
        private Panel _plFooter = new Panel();
        private Panel _plIcon = new Panel();
        private PictureBox _picIcon = new PictureBox();
        private FlowLayoutPanel _flpButtons = new FlowLayoutPanel();
        private Label _lblTitle;
        private Label _lblMessage;
        private List<Button> _buttonCollection = new List<Button>();
        private static DialogResult _buttonResult = new DialogResult();
        private static Timer _timer;
        private static Point lastMousePos;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool MessageBeep(uint type);

        private DarkMessageBox()
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.BackColor = Color.FromArgb(68, 68,68 );
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Width = 400;

            _lblTitle = new Label();
            _lblTitle.ForeColor = Color.White;
            _lblTitle.Font = new System.Drawing.Font("Segoe UI", 18);
            _lblTitle.Dock = DockStyle.Top;
            _lblTitle.Height = 50;

            _lblMessage = new Label();
            _lblMessage.ForeColor = Color.White;
            _lblMessage.Font = new System.Drawing.Font("Segoe UI", 10);
            _lblMessage.Dock = DockStyle.Fill;

            _flpButtons.FlowDirection = FlowDirection.RightToLeft;
            _flpButtons.Dock = DockStyle.Fill;

            _plHeader.Dock = DockStyle.Fill;
            _plHeader.Padding = new Padding(20);
            _plHeader.Controls.Add(_lblMessage);
            _plHeader.Controls.Add(_lblTitle);

            _plFooter.Dock = DockStyle.Bottom;
            _plFooter.Padding = new Padding(20);
            _plFooter.BackColor = Color.FromArgb(51, 51, 51);
            _plFooter.Height = 80;
            _plFooter.Controls.Add(_flpButtons);

            _picIcon.Width = 32;
            _picIcon.Height = 32;
            _picIcon.Location = new Point(30, 50);

            _plIcon.Dock = DockStyle.Left;
            _plIcon.Padding = new Padding(20);
            _plIcon.Width = 70;
            _plIcon.Controls.Add(_picIcon);

            List<Control> controlCollection = new List<Control>();
            controlCollection.Add(this);
            controlCollection.Add(_lblTitle);
            controlCollection.Add(_lblMessage);
            controlCollection.Add(_flpButtons);
            controlCollection.Add(_plHeader);
            controlCollection.Add(_plFooter);
            controlCollection.Add(_plIcon);
            controlCollection.Add(_picIcon);

            foreach (Control control in controlCollection)
            {
                control.MouseDown += MsgBox_MouseDown;
                control.MouseMove += MsgBox_MouseMove;
            }

            this.Controls.Add(_plHeader);
            this.Controls.Add(_plIcon);
            this.Controls.Add(_plFooter);
        }

        private static void MsgBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lastMousePos = new Point(e.X, e.Y);
            }
        }


        private static void MsgBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _darkMessageBox.Left += e.X - lastMousePos.X;
                _darkMessageBox.Top += e.Y - lastMousePos.Y;
            }
        }

        public static DialogResult Show(string message)
        {
            _darkMessageBox = new DarkMessageBox();
            _darkMessageBox._lblMessage.Text = message;

            DarkMessageBox.InitButtons(Buttons.OK);

            _darkMessageBox.ShowDialog();
            return _buttonResult;
        }

        public static DialogResult Show(string message, string title)
        {
            _darkMessageBox = new DarkMessageBox();
            _darkMessageBox._lblMessage.Text = message;
            _darkMessageBox._lblTitle.Text = title;
            _darkMessageBox.Size = DarkMessageBox.MessageSize(message);

            DarkMessageBox.InitButtons(Buttons.OK);

            _darkMessageBox.ShowDialog();
            return _buttonResult;
        }

        public static DialogResult Show(string message, string title, Buttons buttons)
        {
            _darkMessageBox = new DarkMessageBox();
            _darkMessageBox._lblMessage.Text = message;
            _darkMessageBox._lblTitle.Text = title;
            _darkMessageBox._plIcon.Hide();

            DarkMessageBox.InitButtons(buttons);

            _darkMessageBox.Size = DarkMessageBox.MessageSize(message);
            _darkMessageBox.ShowDialog();
            return _buttonResult;
        }

        public static DialogResult Show(string message, string title, Buttons buttons, Icon icon)
        {
            _darkMessageBox = new DarkMessageBox();
            _darkMessageBox._lblMessage.Text = message;
            _darkMessageBox._lblTitle.Text = title;

            DarkMessageBox.InitButtons(buttons);
            DarkMessageBox.InitIcon(icon);

            _darkMessageBox.Size = DarkMessageBox.MessageSize(message);
            _darkMessageBox.ShowDialog();
            return _buttonResult;
        }

        public static DialogResult Show(string message, string title, Buttons buttons, Icon icon, AnimateStyle style)
        {
            _darkMessageBox = new DarkMessageBox();
            _darkMessageBox._lblMessage.Text = message;
            _darkMessageBox._lblTitle.Text = title;
            _darkMessageBox.Height = 0;

            DarkMessageBox.InitButtons(buttons);
            DarkMessageBox.InitIcon(icon);

            _timer = new Timer();
            Size formSize = DarkMessageBox.MessageSize(message);

            switch (style)
            {
                case DarkMessageBox.AnimateStyle.SlideDown:
                    _darkMessageBox.Size = new Size(formSize.Width, 0);
                    _timer.Interval = 1;
                    _timer.Tag = new AnimateMsgBox(formSize, style);
                    break;

                case DarkMessageBox.AnimateStyle.FadeIn:
                    _darkMessageBox.Size = formSize;
                    _darkMessageBox.Opacity = 0;
                    _timer.Interval = 20;
                    _timer.Tag = new AnimateMsgBox(formSize, style);
                    break;

                case DarkMessageBox.AnimateStyle.ZoomIn:
                    _darkMessageBox.Size = new Size(formSize.Width + 100, formSize.Height + 100);
                    _timer.Tag = new AnimateMsgBox(formSize, style);
                    _timer.Interval = 1;
                    break;
            }

            _timer.Tick += timer_Tick;
            _timer.Start();

            _darkMessageBox.ShowDialog();
            return _buttonResult;
        }

        static void timer_Tick(object sender, EventArgs e)
        {
            Timer timer = (Timer)sender;
            AnimateMsgBox animate = (AnimateMsgBox)timer.Tag;

            switch(animate.Style){
                case DarkMessageBox.AnimateStyle.SlideDown:
                    if (_darkMessageBox.Height < animate.FormSize.Height)
                    {
                        _darkMessageBox.Height += 17;
                        _darkMessageBox.Invalidate();
                    }
                    else
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }
                    break;

                case DarkMessageBox.AnimateStyle.FadeIn:
                    if (_darkMessageBox.Opacity < 1)
                    {
                        _darkMessageBox.Opacity += 0.1;
                        _darkMessageBox.Invalidate();
                    }
                    else
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }
                    break;

                case DarkMessageBox.AnimateStyle.ZoomIn:
                    if (_darkMessageBox.Width > animate.FormSize.Width )
                    {
                        _darkMessageBox.Width -= 17;
                        _darkMessageBox.Invalidate();
                    }
                    if (_darkMessageBox.Height > animate.FormSize.Height)
                    {
                        _darkMessageBox.Height -= 17;
                        _darkMessageBox.Invalidate();
                    }
                    break;
            }
        }

        private static void InitButtons(Buttons buttons)
        {
            switch (buttons)
            {
                case DarkMessageBox.Buttons.AbortRetryIgnore:
                    _darkMessageBox.InitAbortRetryIgnoreButtons();
                    break;

                case DarkMessageBox.Buttons.OK:
                    _darkMessageBox.InitOKButton();
                    break;

                case DarkMessageBox.Buttons.OKCancel:
                    _darkMessageBox.InitOKCancelButtons();
                    break;

                case DarkMessageBox.Buttons.RetryCancel:
                    _darkMessageBox.InitRetryCancelButtons();
                    break;

                case DarkMessageBox.Buttons.YesNo:
                    _darkMessageBox.InitYesNoButtons();
                    break;

                case DarkMessageBox.Buttons.YesNoCancel:
                    _darkMessageBox.InitYesNoCancelButtons();
                    break;
            }

            foreach (Button btn in _darkMessageBox._buttonCollection)
            {
                btn.ForeColor = Color.FromArgb(170, 170, 170);
                btn.Font = new System.Drawing.Font("Segoe UI", 8);
                btn.Padding = new Padding(3);
                btn.FlatStyle = FlatStyle.Flat;
                btn.Height = 30;
                btn.FlatAppearance.BorderColor = Color.FromArgb(99, 99, 98);

                _darkMessageBox._flpButtons.Controls.Add(btn);
            }
        }

        private static void InitIcon(Icon icon)
        {
            switch (icon)
            {
                case DarkMessageBox.Icon.Application:
                    _darkMessageBox._picIcon.Image = SystemIcons.Application.ToBitmap();
                    break;

                case DarkMessageBox.Icon.Exclamation:
                    _darkMessageBox._picIcon.Image = SystemIcons.Exclamation.ToBitmap();
                    break;

                case DarkMessageBox.Icon.Error:
                    _darkMessageBox._picIcon.Image = SystemIcons.Error.ToBitmap();
                    break;

                case DarkMessageBox.Icon.Info:
                    _darkMessageBox._picIcon.Image = SystemIcons.Information.ToBitmap();
                    break;

                case DarkMessageBox.Icon.Question:
                    _darkMessageBox._picIcon.Image = SystemIcons.Question.ToBitmap();
                    break;

                case DarkMessageBox.Icon.Shield:
                    _darkMessageBox._picIcon.Image = SystemIcons.Shield.ToBitmap();
                    break;

                case DarkMessageBox.Icon.Warning:
                    _darkMessageBox._picIcon.Image = SystemIcons.Warning.ToBitmap();
                    break;
            }
        }

        private void InitAbortRetryIgnoreButtons()
        {
            Button btnAbort = new Button();
            btnAbort.Text = "Abort";
            btnAbort.Click += ButtonClick;

            Button btnRetry = new Button();
            btnRetry.Text = "Retry";
            btnRetry.Click += ButtonClick;

            Button btnIgnore = new Button();
            btnIgnore.Text = "Ignore";
            btnIgnore.Click += ButtonClick;

            this._buttonCollection.Add(btnAbort);
            this._buttonCollection.Add(btnRetry);
            this._buttonCollection.Add(btnIgnore);
        }

        private void InitOKButton()
        {
            Button btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Click += ButtonClick;

            this._buttonCollection.Add(btnOK);
        }

        private void InitOKCancelButtons()
        {
            Button btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Click += ButtonClick;

            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Click += ButtonClick;


            this._buttonCollection.Add(btnOK);
            this._buttonCollection.Add(btnCancel);
        }

        private void InitRetryCancelButtons()
        {
            Button btnRetry = new Button();
            btnRetry.Text = "OK";
            btnRetry.Click += ButtonClick;

            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Click += ButtonClick;


            this._buttonCollection.Add(btnRetry);
            this._buttonCollection.Add(btnCancel);
        }

        private void InitYesNoButtons()
        {
            Button btnYes = new Button();
            btnYes.Text = "Yes";
            btnYes.Click += ButtonClick;

            Button btnNo = new Button();
            btnNo.Text = "No";
            btnNo.Click += ButtonClick;


            this._buttonCollection.Add(btnYes);
            this._buttonCollection.Add(btnNo);
        }

        private void InitYesNoCancelButtons()
        {
            Button btnYes = new Button();
            btnYes.Text = "Abort";
            btnYes.Click += ButtonClick;

            Button btnNo = new Button();
            btnNo.Text = "Retry";
            btnNo.Click += ButtonClick;

            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Click += ButtonClick;

            this._buttonCollection.Add(btnYes);
            this._buttonCollection.Add(btnNo);
            this._buttonCollection.Add(btnCancel);
        }

        private static void ButtonClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            switch (btn.Text)
            {
                case "Abort":
                    _buttonResult = DialogResult.Abort;
                    break;

                case "Retry":
                    _buttonResult = DialogResult.Retry;
                    break;

                case "Ignore":
                    _buttonResult = DialogResult.Ignore;
                    break;

                case "OK":
                    _buttonResult = DialogResult.OK;
                    break;

                case "Cancel":
                    _buttonResult = DialogResult.Cancel;
                    break;

                case "Yes":
                    _buttonResult = DialogResult.Yes;
                    break;

                case "No":
                    _buttonResult = DialogResult.No;
                    break;
            }

            _darkMessageBox.Dispose();
        }

        private static Size MessageSize(string message)
        {
            Graphics g = _darkMessageBox.CreateGraphics();
            int width=350;
            int height = 230;

            SizeF size = g.MeasureString(message, new System.Drawing.Font("Segoe UI", 10));
            //string[] newlines = (from Match m in Regex.Matches(message, "\n") select m.Value).ToArray();

            if (message.Length < 150)
            {
                if ((int)size.Width > 350)
                {
                    width = (int)size.Width;
                }
                if ((int)size.Height*4 > 230)
                {
                    height = (int)size.Height*4;
                }
            }
            else
            {
                string[] groups = (from Match m in Regex.Matches(message, ".{1,180}") select m.Value).ToArray();
                int lines = groups.Length+1;
                width = 700;
                height += (int)(size.Height+10) * lines;
            }
            return new Size(width, height);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Rectangle rect = new Rectangle(new Point(0, 0), new Size(this.Width - 1, this.Height - 1));
            Pen pen = new Pen(Color.FromArgb(89,89,89), 2.0f);

            g.DrawRectangle(pen, rect);
        }

        public enum Buttons
        {
            AbortRetryIgnore=1,
            OK=2,
            OKCancel=3,
            RetryCancel=4,
            YesNo=5,
            YesNoCancel=6
        }

        new public enum Icon 
        {
            Application = 1,
            Exclamation = 2,
            Error = 3,
            Warning = 4,
            Info = 5,
            Question = 6,
            Shield = 7,
            Search = 8
        }

        public enum AnimateStyle
        {
            SlideDown=1,
            FadeIn= 2, 
            ZoomIn =3
        }

    }

    class AnimateMsgBox
    {
        public Size FormSize;
        public DarkMessageBox.AnimateStyle Style;

        public AnimateMsgBox(Size formSize, DarkMessageBox.AnimateStyle style)
        {
            this.FormSize = formSize;
            this.Style = style;
        }
    }
}
