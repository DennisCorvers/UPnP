using Open.Nat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UPnP.Exceptions;
using UPnP.Objects;

namespace UPnP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int Timeout = 5000;

        private MyNatDevice m_device;
        private bool m_isBusy;
        private bool IsBusy
        {
            get { return m_isBusy; }
            set
            {
                if (value)
                { StartBusy(); }
                else
                { EndBusy(); }
                m_isBusy = value;
            }
        }

        public MainWindow()
        {
            m_isBusy = false;
            m_device = new MyNatDevice(Timeout);
            InitializeComponent();

            RefreshMappings();
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
            if (m_device.HasDevice)
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

        private async Task<bool> TryGetNatDevice()
        {
            try
            {
                await m_device.FindDevice();
                return true;
            }
            catch
            {
                MessageBox.Show(
                    "Unable to discover NAT Device.\nMake sure UPnP is enabled on your router!",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }
        private async Task<List<MyNatMapping>> TryGetMappings()
        {
            try
            { return await m_device.GetAllMappings(); }
            catch (NoNatDeviceException)
            {
                MessageBox.Show(
                    "Unable to get UPnP mappings.\nNo NAT device available.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return new List<MyNatMapping>();
        }

        private async void RefreshMappings()
        {
            IsBusy = true;
            if (!m_device.HasDevice)
            {
                if (!await TryGetNatDevice())
                { IsBusy = false; return; }
            }
            FillMappings(await TryGetMappings());
            IsBusy = false;
        }
        private async void AddMapping(MyNatMapping mapping)
        {
            IsBusy = true;

            if (!m_device.HasDevice)
                MessageBox.Show(
                "No NAT device available.\nTry refreshing first.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                try
                {
                    //await m_device.AddMapping(mapping.Mapping);
                    FillMappings(await TryGetMappings()); ;
                }
                catch
                {
                    //TODO
                    throw new Exception();
                }

            IsBusy = false;
        }
        private async void RemoveMapping(MyNatMapping mapping)
        {

            IsBusy = true;
            if (!m_device.HasDevice)
                MessageBox.Show(
                "No NAT device available.\nTry refreshing first.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                try
                {
                    //await m_device.RemoveMapping(mapping.Mapping);
                    FillMappings(await TryGetMappings()); ;
                }
                catch
                {
                    //TODO
                    throw new Exception();
                }

            IsBusy = false;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        { RefreshMappings(); }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //var selection = UPnPGrid.SelectedItem;
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selection = UPnPGrid.SelectedItem;
            if (selection == null)
            {
                MessageBox.Show("No mapping selected.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            { RemoveMapping((MyNatMapping)selection); }
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        { m_device.CancelPendingRequests(); }
    }
}
