using Open.Nat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UPnPWin.Objects;

namespace UPnPWin.Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NewMappingForm m_newMapForm;
        private NewMappingForm NewMapForm
        {
            get
            {
                if (m_newMapForm == null)
                    m_newMapForm = new NewMappingForm();
                return m_newMapForm;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            btnAdd.IsEnabled = false;
            btnDelete.IsEnabled = false;
#if !FASTDEBUG
            BtnRefresh_Click(null, null);
#endif
        }

        private void FillMappings(List<MyNatMapping> mappings)
        {
            UPnPGrid.Items.Clear();
            if (mappings == null) { return; }

            foreach (MyNatMapping map in mappings)
            { UPnPGrid.Items.Add(map); }
        }

        private async Task<MyNatDevice> GetDevice()
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
                    return null;
                }
                catch (TaskCanceledException)
                {
                    MessageBox.Show(
                        "Nat device discovery timed-out.\nMake sure UPnP is enabled on your router!",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            return MyNatDevice.Instance;
        }
        private async Task<List<MyNatMapping>> GetMappings()
        {
            try
            { return await MyNatDevice.Instance.GetAllMappings(); }
            catch (NatDeviceNotFoundException)
            {
                MessageBox.Show(
                    "Unable to get UPnP mappings.\nNo NAT device available.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<MyNatMapping>();
            }
        }
        private async Task<bool> RemoveMapping(List<Mapping> mappings)
        {
            try { await MyNatDevice.Instance.RemoveMappings(mappings); }
            catch (NatDeviceNotFoundException)
            {
                HandleNoNatDeviceException();
                return false;
            }
            return true;
        }

        private async Task RefreshUI()
        {
            MyNatDevice device = await GetDevice();
            if (device == null) return;

            SetIPLabel(labPublicIP, device.PublicIP);
            SetIPLabel(labPrivateIP, device.LocalIP);
            SetIPLabel(labDeviceIP, device.DeviceEndpoint.Address);

            FillMappings(await GetMappings());
        }
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            SetBusy(true);
            await RefreshUI();
            SetBusy(false);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            IPAddress myAddress = MyNatDevice.Instance.LocalIP;
            if (myAddress == null)
                myAddress = IPAddress.Any;

            var form = NewMapForm;
            form.SetWindow(myAddress);

            form.ShowDialog();

            if (form.UpdateAvailable)
                OnMappingAdded();
        }
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selection = UPnPGrid.SelectedItems;
            if (selection.Count == 0)
            {
                MessageBox.Show("No mapping selected.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                SetBusy(true);

                if (await RemoveMapping(GetSelectedMappings(selection)))
                    FillMappings(await GetMappings());

                SetBusy(false);
                UPnPGrid.UnselectAllCells();
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        { MyNatDevice.Instance.CancelPendingRequests(); }

        private List<Mapping> GetSelectedMappings(IList selection)
        {
            List<Mapping> returnValue = new List<Mapping>(selection.Count);
            foreach (var item in selection)
                returnValue.Add(((MyNatMapping)item).Mapping);

            return returnValue;
        }

        private void HandleNoNatDeviceException()
        {
            MessageBox.Show(
                "No NAT device available.\nTry refreshing first.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private void SetBusy(bool isBusy)
        {
            //Load spinner?
            if (isBusy)
            {
                btnAdd.IsEnabled = false;
                btnDelete.IsEnabled = false;
                btnRefresh.IsEnabled = false;
            }
            else
            {
                if (MyNatDevice.HasDevice)
                {
                    btnAdd.IsEnabled = true;
                    btnDelete.IsEnabled = true;
                }
                btnRefresh.IsEnabled = true;
            }
        }
        private void SetIPLabel(Label label, IPAddress ip)
        {
            if (ip == null || ip == IPAddress.Any)
                label.Content = "N/A";
            else
                label.Content = ip;
        }

        private async void OnMappingAdded()
        {
            FillMappings(await Task.Run(GetMappings));
        }

        protected override void OnClosed(EventArgs e)
        {
            NewMapForm.Close();
            base.OnClosed(e);

            Application.Current.Shutdown();
        }
    }
}
