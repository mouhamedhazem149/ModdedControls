using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ModdedControls
{
    public class ModdedTextBox : TextBox
    {
        private ListBox _listBox;
        private Button _showPasswordButton = null;
        private IButtonControl _tempButton { get;  set; }
        private bool _isAdded;
        private bool _visible_password_button;
        private string[] _values;
        private string _formerValue = string.Empty;

        public ModdedTextBox()
        {
            InitializeComponent();
            originalColor = ForeColor; originalFont = Font;
            ResetListBox();
        }

        private void InitializeComponent()
        {
            _listBox = new ListBox();
            _listBox.Click += listBox1_Click;
            _listBox.KeyDown += this_KeyDown;
            KeyDown += this_KeyDown;
            KeyUp += this_KeyUp;
            Enter += this_Enter;
            Leave += this_Leave;
            LostFocus += this_LostFocus;
            ParentChanged += this_ParentChanged; ;
        }

        private void this_ParentChanged(object sender, EventArgs e)
        { this_Enter(this, EventArgs.Empty); this_Leave(this, EventArgs.Empty); }

        private void _showPasswordButton_Click(object sender, EventArgs e)
        {
            this.UseSystemPasswordChar = !this.UseSystemPasswordChar;
            if (!UseSystemPasswordChar) _showPasswordButton.BackgroundImage = Properties.Resources.eye;
            else _showPasswordButton.BackgroundImage = Properties.Resources.show_password;
        }

        void PrepareButton()
        {
            _showPasswordButton = new Button();
            Parent.Controls.Add(_showPasswordButton);
            if (UseSystemPasswordChar)_showPasswordButton.BackgroundImage = Properties.Resources.eye;
            else _showPasswordButton.BackgroundImage = Properties.Resources.show_password;
            _showPasswordButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            _showPasswordButton.Top = Top;
            _showPasswordButton.Left = Left;
            _showPasswordButton.Size = new Size((int)(Height /** 1.30*/), (int)(Height /** 1.30*/));
            _showPasswordButton.Visible = true;
            _showPasswordButton.BringToFront();
            _showPasswordButton.Click += _showPasswordButton_Click;
            Parent.Update();
        }
        private Color originalColor = Color.Black;
        private Font originalFont;
        private bool _ispasswordtextbox = false;
        private string _placeholder = string.Empty;
        private Font _placeholderFont;
        private Color _placeholderColor = Color.Silver;
        private bool _isPlaceholder = true;
        public bool IsPlaceholder
        {
            get { return _isPlaceholder; }
            set
            {
                if (value) { _isPlaceholder = value; ForeColor = placeholderColor; Text = Placeholder; UseSystemPasswordChar = false; }
                else { ForeColor = originalColor; UseSystemPasswordChar = IsPasswordTextbox; }
                _isPlaceholder = value;
            }
        }
        public new string Text
        {
            get { return _isPlaceholder ? string.Empty : base.Text; }
            set
            {
                if (value != string.Empty && value != Placeholder) IsPlaceholder = false;
                base.Text = value;
            }
        }

        void listBox1_Click(object sender, EventArgs e)
        {
            if (_listBox.Items.Count > 0)
            {
                this.Text = _listBox.Items[_listBox.SelectedIndex % _listBox.Items.Count].ToString();
                _listBox.Hide();
                _isAdded = false;
            }
        }
        private void ShowListBox()
        {
            if (!_isAdded)
            {
                Parent.Controls.Add(_listBox);
                _listBox.Left = Left;
                _listBox.Top = Top + Height;
                _isAdded = true;
            }
            _listBox.Visible = true;
            _listBox.BringToFront();
            if (FindForm() != null && FindForm().AcceptButton != null)
            { _tempButton = FindForm().AcceptButton; FindForm().AcceptButton = null; }
        }

        private void ResetListBox()
        {
            _listBox.Visible = false;
            if (FindForm() != null && FindForm().AcceptButton == null)
                FindForm().AcceptButton = _tempButton;
        }

        private void this_Enter(object sender, EventArgs e)
        {
            if (Visible_Password_Button && _showPasswordButton == null) PrepareButton();
            if (Visible_Password_Button == false) _showPasswordButton = null;
            if (_isPlaceholder)
            {
                ForeColor = originalColor; Text = "";/* Font = originalFont;*/ UseSystemPasswordChar = IsPasswordTextbox;
                _isPlaceholder = false;
            }
            Update();
        }
        private void this_Leave(object sender, EventArgs e)
        {
            if (_showPasswordButton != null && _showPasswordButton.Focused) return;
            IsPlaceholder =/* UseSystemPasswordChar =*/ Text.Length < 1; Update();
        }
        private void this_LostFocus(object sender, EventArgs e)
        {
            if (_listBox.Visible && !_listBox.Focused) _listBox.Visible = false;
        }

        private void this_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                UpdateListBox();
        }

        private void this_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                    {
                        UpdateListBox();
                        _listBox.Visible = !_listBox.Visible;
                        break;
                    }
                case Keys.Enter:
                    {
                        if (_listBox.Items.Count > 0)
                        {
                            this.Text = _listBox.Items[_listBox.SelectedIndex % _listBox.Items.Count].ToString();
                            _listBox.Hide();
                            _isAdded = false;
                        }
                        break;
                    }
                case Keys.Down:
                    {
                        if (_listBox.Items.Count > 0)
                            if ((_listBox.Visible) && (_listBox.SelectedIndex < _listBox.Items.Count - 1))
                                _listBox.SelectedIndex = (_listBox.SelectedIndex + 1) % _listBox.Items.Count;
                        break;
                    }
                case Keys.Up:
                    {
                        if (_listBox.Items.Count > 0)
                            if ((_listBox.Visible) && (_listBox.SelectedIndex > 0))
                                _listBox.SelectedIndex = (_listBox.SelectedIndex - 1) % _listBox.Items.Count;
                        break;
                    }
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Tab:
                    return true;
                default:
                    return base.IsInputKey(keyData);
            }
        }

        private void UpdateListBox()
        {
            if (Text == _formerValue) return;
            _formerValue = Text;
            if (_values != null && Text.Length > 0)
            {
                string[] matches = Array.FindAll(_values, x => (x.Contains(Text)));
                if (matches.Length > 0)
                {
                    ShowListBox();
                    _listBox.Items.Clear();
                    Array.ForEach(matches, x => _listBox.Items.Add(x));
                    _listBox.SelectedIndex = 0;
                    _listBox.Height = 0;
                    _listBox.Width = 0;
                    Focus();
                    using (Graphics graphics = _listBox.CreateGraphics())
                    {
                        for (int i = 0; i < _listBox.Items.Count; i++)
                        {
                            _listBox.Height += _listBox.GetItemHeight(i);
                            //int itemWidth = (int)graphics.MeasureString(((String)_listBox.Items[i]) + "_", _listBox.Font).Width;_listBox.Width = (_listBox.Width < itemWidth) ? itemWidth : _listBox.Width;
                            _listBox.Width = this.Width;
                        }
                        _listBox.Height = Math.Min(_listBox.Height, Parent.Height - (Location.Y + Height));
                    }
                }
                else ResetListBox();
            }
            else ResetListBox();
        }

        public string[] Values
        {
            get
            { return _values; }
            set
            { _values = value; }
        }

        public bool IsPasswordTextbox
        {
            get { return _ispasswordtextbox; }
            set { _ispasswordtextbox = value; }
        }
        public bool Visible_Password_Button
        {
            get { return _visible_password_button; }
            set { _visible_password_button = value; }
        }
        public Font placeholderFont
        {
            get { return _placeholderFont; }
            set { _placeholderFont = value; this_Leave(this, EventArgs.Empty); }
        }
        public Color placeholderColor
        {
            get { return _placeholderColor; }
            set { _placeholderColor = value; this_Leave(this, EventArgs.Empty); }
        }

        public string Placeholder
        {
            get { return _placeholder; }
            set { { _placeholder = value; this_Leave(this, EventArgs.Empty); } }
        }
    }
}