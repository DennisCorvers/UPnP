using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace UPnPWin.Utils.UI
{
    internal class DuplicateTextbox
    {
        private bool m_isManual;
        private readonly TextBox m_original;
        private readonly TextBox m_mirror;
        private readonly Color m_originalColor;

        public string Text
            => m_original.Text;

        public DuplicateTextbox(TextBox original, TextBox mirror)
        {
            m_isManual = false;
            m_mirror = mirror;
            m_original = original;
            m_originalColor = ((SolidColorBrush)m_original.Background).Color;

            m_mirror.TextChanged += OnTextChanged;
            m_original.GotFocus += OnGotFocus;
            m_original.LostFocus += OnLostFocus;
        }



        ~DuplicateTextbox()
        {
            m_mirror.TextChanged -= OnTextChanged;
            m_original.GotFocus -= OnGotFocus;
        }

        internal void Reset()
        {
            m_original.Background = new SolidColorBrush(m_originalColor);
            m_isManual = false;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_isManual) return;

            m_original.Text = m_mirror.Text;
        }
        private void OnGotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (m_isManual) return;

            m_isManual = true;
            m_original.Background = m_mirror.Background;
            m_original.Text = "";
        }

        private void OnLostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(m_original.Text))
            { 
                Reset();
                m_original.Text = m_mirror.Text;
            }
        }
    }
}
