using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UPnPWin.Forms.Components
{
    public partial class IPTextBox : UserControl
    {
        public delegate void OnFaultedOctetDelegate(int actual, int lower, int upper);
        public event OnFaultedOctetDelegate OnFaultedOctet;

        private const int LowerLimit = 0;
        private const int UpperLimit = 255;

        private static readonly List<Key> m_digitKeys;
        private static readonly List<Key> m_allowedKeys;

        private readonly List<TextBox> m_segments;

        public IPAddress IPAddress
        {
            get
            {
                byte[] octets = new byte[4]
                {
                    TextBoxOctet(Segment1),
                    TextBoxOctet(Segment2),
                    TextBoxOctet(Segment3),
                    TextBoxOctet(Segment4),
                };
                return new IPAddress(octets);
            }
            set
            {
                byte[] octets = value.GetAddressBytes();
                for (int i = 0; i < 4; i++)
                    m_segments[i].Text = octets[i].ToString();
            }
        }

        static IPTextBox()
        {
            m_digitKeys = new List<Key>(20)
            {
                Key.D0,
                Key.D1,
                Key.D2,
                Key.D3,
                Key.D4,
                Key.D5,
                Key.D6,
                Key.D7,
                Key.D8,
                Key.D9,
                Key.NumPad0,
                Key.NumPad1,
                Key.NumPad2,
                Key.NumPad3,
                Key.NumPad4,
                Key.NumPad5,
                Key.NumPad6,
                Key.NumPad7,
                Key.NumPad8,
                Key.NumPad9,
            };
            m_allowedKeys = new List<Key>(2)
            {
                Key.Tab,
                Key.Delete
            };
        }
        public IPTextBox()
        {
            InitializeComponent();

            m_segments = new List<TextBox>(4)
            { Segment1, Segment2, Segment3, Segment4 };
        }

        private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Handling of numerical keys
            if (m_digitKeys.Contains(e.Key))
            { e.Handled = KeyDigitPress(); return; }

            //Handling of pre-registered keys.
            switch (e.Key)
            {
                case Key.Left:
                    e.Handled = KeyLeftPress();
                    return;

                case Key.Right:
                    e.Handled = KeyRightPress();
                    return;

                case Key.Back:
                    KeyBackspacePress();
                    return;

                case Key.OemPeriod:
                    e.Handled = true;
                    KeyPeriodPress();
                    return;
                case Key.Decimal: goto case Key.OemPeriod;

                case Key.Home:
                    e.Handled = true;
                    KeyHomePress();
                    return;

                case Key.End:
                    e.Handled = true;
                    KeyEndPress();
                    return;

                case Key.Space:
                    e.Handled = true;
                    KeySpacePress();
                    return;

                default:
                    break;
            }

            //Handling of other keys
            e.Handled = !KeyOtherPress(e);
        }
        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (FocusManager.GetFocusedElement(this) is TextBox current
                && current.Text.Length == 3)
            {
                if (current.CaretIndex == 3)
                    MoveFocusRight(current);
                else
                    FinishOctet(current);
            }
        }
        private void TextBox_OnFocusLost(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox current)
            {
                FinishOctet(current);
                e.Handled = true;
            }
        }

        private bool KeyLeftPress()
        {
            if (FocusManager.GetFocusedElement(this) is TextBox currentTextBox
                && currentTextBox.CaretIndex == 0)
            {
                MoveFocusLeft(currentTextBox);
                return true;
            }

            return false;
        }
        private bool KeyRightPress()
        {
            if (FocusManager.GetFocusedElement(this) is TextBox currentTextBox
                && currentTextBox.CaretIndex == currentTextBox.Text.Length)
            {
                MoveFocusRight(currentTextBox);
                return true;
            }

            return false;
        }
        private void KeyBackspacePress()
        {
            if (FocusManager.GetFocusedElement(this) is TextBox current
                && current.CaretIndex == 0
                && current.SelectedText.Length == 0)
            {
                MoveFocusLeft(current);
                //TODO delete last character of previous
            }

        }
        private void KeyPeriodPress()
        {
            if (FocusManager.GetFocusedElement(this) is TextBox current)
            {
                if (current.Text.Length == 0)
                {
                    current.Text = "0";
                    current.CaretIndex = 1;
                }
                MoveFocusRight(current);
            }
        }
        private bool KeyDigitPress()
        {
            if (FocusManager.GetFocusedElement(this) is TextBox current
                && current.Text.Length == 3
                && current.CaretIndex == 3
                && current.SelectedText.Length == 0)
            {
                MoveFocusRight(current);
                return true;
            }

            return false;
        }
        private void KeyHomePress()
        {
            if (FocusManager.GetFocusedElement(this) is TextBox)
            {
                TextBox next = m_segments[0];
                next.Focus();
                next.CaretIndex = 0;
            }
        }
        private void KeyEndPress()
        {
            if (FocusManager.GetFocusedElement(this) is TextBox)
            {
                TextBox next = m_segments[3];
                next.Focus();
                next.CaretIndex = next.Text.Length;
            }
        }
        private void KeySpacePress()
        {
            if (FocusManager.GetFocusedElement(this) is TextBox current)
                MoveFocusRight(current);
        }

        private bool KeyOtherPress(KeyEventArgs e)
        {
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0)
            {
                switch (e.Key)
                {
                    case Key.C: return true;
                    case Key.V: return true;
                    case Key.A: return true;
                    case Key.X: return true;

                    default: break;
                }
            }

            return m_allowedKeys.Contains(e.Key);
        }

        private void MoveFocusLeft(TextBox current)
        {
            current.SelectionLength = 0;
            if (!FinishOctet(current)) { return; }

            int id = TextBoxID(current);
            if (id > 0)
            {
                TextBox previous = m_segments[id - 1];
                previous.Focus();
                previous.CaretIndex = previous.Text.Length;
            }
        }
        private void MoveFocusRight(TextBox current)
        {
            current.SelectionLength = 0;
            if (!FinishOctet(current)) { return; }

            int id = TextBoxID(current);
            if (id < 3)
            {
                TextBox next = m_segments[id + 1];
                next.Focus();
            }
        }
        private bool FinishOctet(TextBox current)
        {
            if (current.Text == string.Empty)
            { return true; }

            if (!int.TryParse(current.Text, out int octet))
                throw new Exception("Invalid number was entered.");

            int id = TextBoxID(current);
            if (octet < LowerLimit || octet > UpperLimit)
            {
                if (id == 0 && octet < LowerLimit)
                    current.Text = LowerLimit.ToString();
                else
                    current.Text = UpperLimit.ToString();
                current.CaretIndex = 0;

                OnFaultedOctet?.Invoke(octet, LowerLimit, UpperLimit);

                return false;
            }
            return true;
        }

        private int TextBoxID(TextBox textBox)
        {
            if (textBox.Name.Length != 8)
                throw new Exception("TextBox has an invalid name! " + textBox.Name);

            return textBox.Name[7] - '0' - 1;
        }

        private byte TextBoxOctet(TextBox textBox)
        { return textBox.Text == string.Empty ? (byte)0 : byte.Parse(textBox.Text); }
    }
}
