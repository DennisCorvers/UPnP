using Open.Nat;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UPnPWin.Objects;
using UPnPWin.Utils;

namespace UPnPWin.Forms
{
    /// <summary>
    /// Interaction logic for NewMappingForm.xaml
    /// </summary>
    public partial class NewMappingForm : Window
    {
#pragma warning disable IDE0052
        private readonly CheckBoxLock m_cbLock;
#pragma warning restore IDE0052
        private bool m_didUpdate;

        public bool UpdateAvailable
        {
            get
            {
                if (m_didUpdate)
                { m_didUpdate = false; return true; }
                return false;
            }
        }

        public NewMappingForm()
        {
            InitializeComponent();
            m_cbLock = new CheckBoxLock(CbUDP, CbTCP);
        }

        public void SetWindow(IPAddress address)
        {
            m_didUpdate = false;
            TbPrivatePort.Text = "";
            TbPublicPort.Text = "";
            TbLifetime.Text = "";
            TbDescription.Text = "";
            TbPrivateIP.IPAddress = address;
        }

        private void SetBusy(bool isBusy)
        {
            if (isBusy)
                BtnAddMapping.IsEnabled = false;
            else
                BtnAddMapping.IsEnabled = true;
        }

        private async Task<bool> AddMappings(List<Mapping> mappings)
        {
            foreach (var mapping in mappings)
            {
                try
                { await MyNatDevice.Instance.AddMapping(mapping); }
                catch (NatDeviceNotFoundException)
                {
                    MessageBox.Show("Unable to add UPnP mapping.\nNo Nat device available!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                catch (MappingException e)
                {
                    throw e;
                }
            }

            MessageBox.Show("Successfully added UPnP port mappings.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }
        private async Task<bool> AddMapping(Mapping mapping)
        {
            try
            { await MyNatDevice.Instance.AddMapping(mapping); }
            catch (NatDeviceNotFoundException)
            {
                MessageBox.Show("Unable to add UPnP mapping.\nNo Nat device available!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
           => InputConstraints.NumericTextbox(sender, e);

        private async void BtnAddMapping_Click(object sender, RoutedEventArgs e)
        {
            int pubPort = InputConstraints.ParseNullOrEmpty(TbPublicPort.Text);
            if (!InputConstraints.EnsureBetween(1, ushort.MaxValue, pubPort, "Public Port"))
                return;

            int privPort = InputConstraints.ParseNullOrEmpty(TbPrivatePort.Text);
            if (!InputConstraints.EnsureBetween(1, ushort.MaxValue, privPort, "Private Port"))
                return;

            string description = string.IsNullOrWhiteSpace(TbDescription.Text)
                ? string.Empty : TbDescription.Text;

            int lifetimeHours = InputConstraints.ParseNullOrEmpty(TbLifetime.Text);
            if (!InputConstraints.EnsureBetween(0, 596523, lifetimeHours, "Lifetime"))
                return;

            List<Mapping> mappings = new List<Mapping>(2);
            if ((bool)CbTCP.IsChecked)
            {
                mappings.Add(new Mapping(Protocol.Tcp, TbPrivateIP.IPAddress,
                    privPort, pubPort, lifetimeHours * 3600, description));
            }
            if ((bool)CbUDP.IsChecked)
            {
                mappings.Add(new Mapping(Protocol.Udp, TbPrivateIP.IPAddress,
                    privPort, pubPort, lifetimeHours * 3600, description));
            }

            if (mappings.Count < 1)
            {
                MessageBox.Show("No valid UPnP mapping configuration was entered.\nSelect at least one protocol."
                    , "Error", MessageBoxButton.OK, MessageBoxImage.Error); return;
            }

            SetBusy(true);
            if (await AddMappings(mappings))
            {
                m_didUpdate = true;
                Close();
            }
            SetBusy(false);
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        { Close(); }

#pragma warning disable IDE0051
        private void TbPrivateIP_OnFaultedOctet(int actual, int lower, int upper)
        {
            MessageBox.Show(string.Format(
                "{0} is not a valid entry. Please specify a value between {1} and {2}."
                , actual, lower, upper),
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
                );
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
#pragma warning restore IDE0051
    }
}
