using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace UPnPWin.Utils.UI
{
    /// <summary>
    /// Keeps at least one checkbox locked on "Checked"
    /// </summary>
    internal class CheckBoxLock
    {
        private readonly List<CheckBox> m_checkBoxes;
        private int m_checkedCount;

        internal CheckBoxLock(params CheckBox[] checkBoxes)
        {
            m_checkBoxes = new List<CheckBox>(checkBoxes.Length);

            int checkedCount = 0;
            for (int i = 0; i < checkBoxes.Length; i++)
            {
                var checkBox = checkBoxes[i];
                if ((bool)checkBox.IsChecked)
                    checkedCount++;

                checkBox.Checked += OnCheckBoxChecked;
                checkBox.Unchecked += OnCheckBoxUnchecked;
                m_checkBoxes.Add(checkBox);
            }

            if (checkedCount < 1)
            {
                m_checkBoxes[0].IsChecked = true;
                m_checkedCount = 1;
            }
            else
                m_checkedCount = checkedCount;
        }

        ~CheckBoxLock()
        {
            foreach (CheckBox cb in m_checkBoxes)
            {
                cb.Checked -= OnCheckBoxChecked;
                cb.Unchecked -= OnCheckBoxUnchecked;
            }
        }

        private void OnCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            if (++m_checkedCount == 2)
                foreach (var checkBox in m_checkBoxes)
                    checkBox.IsEnabled = true;
        }
        private void OnCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            if (--m_checkedCount == 1)
            {
                CheckBox lastChecked = null;

                for (int i = m_checkBoxes.Count - 1; i >= 0; i--)
                {
                    var checkBox = m_checkBoxes[i];
                    if ((bool)checkBox.IsChecked)
                        lastChecked = checkBox;
                }
                lastChecked.IsEnabled = false;
            }
        }
    }
}
