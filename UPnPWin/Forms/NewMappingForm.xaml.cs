using Open.Nat;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UPnPWin.Exceptions;
using UPnPWin.Objects;
using UPnPWin.Utils;
using UPnPWin.Utils.UI;

namespace UPnPWin.Forms
{
    /// <summary>
    /// Interaction logic for NewMappingForm.xaml
    /// </summary>
    public partial class NewMappingForm : Window
    {
#pragma warning disable IDE0052
        private readonly CheckBoxLock m_cbLock;
        private readonly DuplicateTextbox m_tbPrivatePort;
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
            m_tbPrivatePort = new DuplicateTextbox(TbPrivatePort, TbPublicPort);
        }

        public void SetWindow(IPAddress address)
        {
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
            try
            { await MyNatDevice.Instance.AddMappings(mappings); }
            catch (NatDeviceNotFoundException)
            {
                MessageBox.Show("Unable to add UPnP mapping.\nNo Nat device available!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (DuplicateMappingException e)
            {
                StringBuilder sb = new StringBuilder(64);
                sb.Append("UPnP mapping is already present.\n");
                sb.Append(e.DuplicateMapping.Protocol).Append(" ");
                sb.Append(e.DuplicateMapping.PublicPort).Append(" to IP > ");
                sb.Append(e.DuplicateMapping.PrivateIP);
                MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch(MappingException e)
            {
                switch (e.ErrorCode)
                {
                    case 720:
                        MessageBox.Show("The UPnP device's mapping table is full.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    default:
                        MessageBox.Show("An unknown UPnP mapping exception occured.\nCould not map UPnP.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                }
            }

            MessageBox.Show("Successfully added UPnP port mappings.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
           => InputConstraints.NumericTextbox(sender, e);

        private async void BtnAddMapping_Click(object sender, RoutedEventArgs e)
        {
            int pubPort = TbPublicPort.ToInt();
            if (!InputConstraints.EnsureBetween(1, ushort.MaxValue, pubPort, "Public Port"))
                return;

            int privPort = TbPrivatePort.ToInt();
            if (!InputConstraints.EnsureBetween(1, ushort.MaxValue, privPort, "Private Port"))
                return;

            string description = string.IsNullOrWhiteSpace(TbDescription.Text)
                ? string.Empty : TbDescription.Text;

            int lifetimeHours = TbLifetime.ToInt();
            if (!InputConstraints.EnsureBetween(0, 596523, lifetimeHours, "Lifetime"))
                return;

            lifetimeHours *= 3600;

            List<Mapping> mappings = new List<Mapping>(2);
            if ((bool)CbTCP.IsChecked)
            {
                mappings.Add(new Mapping(Protocol.Tcp, TbPrivateIP.IPAddress,
                    privPort, pubPort, lifetimeHours, description));
            }
            if ((bool)CbUDP.IsChecked)
            {
                mappings.Add(new Mapping(Protocol.Udp, TbPrivateIP.IPAddress,
                    privPort, pubPort, lifetimeHours, description));
            }

            if (mappings.Count < 1) //This should never happen!
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

#pragma warning disable IDE0051, IDE0060
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

        private void Window_Loaded(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!((Window)sender).IsVisible)
                return;

            m_tbPrivatePort.Reset();
            m_didUpdate = false;

            TbPrivatePort.Text = "";
            TbPublicPort.Text = "";
            TbLifetime.Text = "";
            TbDescription.Text = "";
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            FocusManager.SetFocusedElement(this, TbPublicPort);
            Visibility = Visibility.Hidden;
        }
#pragma warning restore IDE0051, IDE0060
    }
}
