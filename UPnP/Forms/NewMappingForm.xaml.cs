using Open.Nat;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using UPnP.Objects;

namespace UPnP.Forms
{
    /// <summary>
    /// Interaction logic for NewMappingForm.xaml
    /// </summary>
    public partial class NewMappingForm : Window
    {
        public NewMappingForm()
        {
            InitializeComponent();
        }

        private void AddMapping(Mapping mapping)
        {
            try
            {
                //Try add mapping
            }
            catch
            {
                //Watch the world burn
            }
        }


        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnAddMapping_Click(object sender, RoutedEventArgs e)
        {
            if (!MyNatDevice.HasDevice) { }
            //Return to overview..

            //TODO Validate, Add Mapping...
            AddMapping(null);
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
