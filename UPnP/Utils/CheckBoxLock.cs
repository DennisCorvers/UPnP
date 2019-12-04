using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace UPnP.Utils
{
    /// <summary>
    /// Keeps at least one checkbox locked on "Checked"
    /// </summary>
    internal class CheckBoxLock
    {
        private List<CheckBox> m_checkBoxes;

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
                m_checkBoxes[0].IsChecked = true;
        }

        ~CheckBoxLock()
        {
            foreach (CheckBox cb in m_checkBoxes)
            {
                cb.Checked += OnCheckBoxChecked;
                cb.Unchecked += OnCheckBoxUnchecked;
            }
        }

        private void OnCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in m_checkBoxes)
                checkBox.IsEnabled = true;
        }
        private void OnCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            int checkedCount = 0;
            CheckBox lastChecked = null;

            for (int i = m_checkBoxes.Count - 1; i >= 0; i--)
            {
                var checkBox = m_checkBoxes[i];

                if ((bool)checkBox.IsChecked)
                {
                    checkedCount++;
                    lastChecked = checkBox;
                }
            }

            if (checkedCount == 1)
            {
                lastChecked.IsEnabled = false;
                return;
            }
        }
    }
}
