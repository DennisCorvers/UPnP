using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace UPnP.Utils
{
    internal static class InputConstraints
    {
        internal static void NumericTextbox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        internal static bool EnsureBetween(int lower, int upper, int actual, 
            string valueName = "", bool visible = true)
        {
            if (actual < lower || actual > upper)
            {
                if (!visible)
                    return false;

                StringBuilder sb = new StringBuilder(64);
                sb.Append(actual.ToString("N0"));
                sb.Append(" is not a valid entry");

                if (!string.IsNullOrWhiteSpace(valueName))
                    sb.Append(" for ").Append(valueName);

                sb.Append(". Please specify a value between ");
                sb.Append(lower.ToString("N0"));
                sb.Append(" and ");
                sb.Append(upper.ToString("N0")).Append('.');

                MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        internal static int ParseNullOrEmpty(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;
            return int.Parse(value);
        }
    }
}
