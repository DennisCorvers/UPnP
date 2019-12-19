using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UPnPWin.Utils.UI
{
    internal class KeydownHandler<T> where T : IInputElement
    {
        private bool m_isPressed;

        internal delegate void KeyPressDelegate(T sender, KeyEventArgs e);
        internal event KeyPressDelegate OnKeyPress;

        internal KeydownHandler(T inputElement)
        {
            m_isPressed = false;
            inputElement.PreviewKeyDown += KeyDown;
            inputElement.PreviewKeyUp += KeyUp;
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            if (!m_isPressed)
            {
                m_isPressed = true;

                if (sender is T)
                    OnKeyPress?.Invoke((T)sender, e);
            }
            e.Handled = true;
        }
        private void KeyUp(object sender, KeyEventArgs e)
        { m_isPressed = false; }
    }
}
