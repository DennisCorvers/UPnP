using Open.Nat;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using UPnP.Objects;

namespace UPnP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool IsBusy
        {
            set
            {
                if (value)
                { StartBusy(); }
                else
                { EndBusy(); }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            BtnRefresh_Click(null, null);
        }

        /// <summary>
        /// Blocks the user from making new requests to the NAT device.
        /// </summary>
        private void StartBusy()
        {
            btnAdd.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnRefresh.IsEnabled = false;
        }
        /// <summary>
        /// Releases the block
        /// </summary>
        private void EndBusy()
        {
            if (MyNatDevice.HasDevice)
            {
                btnAdd.IsEnabled = true;
                btnDelete.IsEnabled = true;
            }
            btnRefresh.IsEnabled = true;
        }

        private void FillMappings(List<MyNatMapping> mappings)
        {
            UPnPGrid.Items.Clear();
            if (mappings == null) { return; }

            foreach (MyNatMapping map in mappings)
            { UPnPGrid.Items.Add(map); }
        }

        private async Task AddMapping(MyNatMapping mapping)
        {
            try
            {
                //await m_device.AddMapping(mapping.Mapping);
                await RefreshMappings();
            }
            catch (NatDeviceNotFoundException)
            { HandleNoNatDeviceException(); }
        }
        private async Task RemoveMapping(MyNatMapping mapping)
        {
            try
            {
                //await m_device.RemoveMapping(mapping.Mapping);
                await RefreshMappings();
            }
            catch (NatDeviceNotFoundException)
            { HandleNoNatDeviceException(); }
        }
        private async Task RefreshMappings()
        {
            if (!MyNatDevice.HasDevice)
            {
                try
                { await MyNatDevice.Instance.FindDevice(); }
                catch (NatDeviceNotFoundException)
                {
                    MessageBox.Show(
                        "Unable to discover NAT Device.\nMake sure UPnP is enabled on your router!",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            List<MyNatMapping> mappings;
            try
            { mappings = await MyNatDevice.Instance.GetAllMappings(); }
            catch (NatDeviceNotFoundException)
            {
                MessageBox.Show(
                    "Unable to get UPnP mappings.\nNo NAT device available.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            FillMappings(mappings);
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            IsBusy = true;
            await RefreshMappings();
            IsBusy = false;
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            IsBusy = true;
            //var selection = UPnPGrid.SelectedItem;
            IsBusy = false;
        }
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selection = UPnPGrid.SelectedItem;
            if (selection == null)
            {
                MessageBox.Show("No mapping selected.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                IsBusy = true;
                await RemoveMapping((MyNatMapping)selection);
                IsBusy = false;
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        { MyNatDevice.Instance.CancelPendingRequests(); }

        private void HandleNoNatDeviceException()
        {
            MessageBox.Show(
                "No NAT device available.\nTry refreshing first.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
