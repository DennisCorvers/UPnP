using Open.Nat;
using System;
using System.Collections.Generic;
using System.Linq;
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
using UPnP.Objects;

namespace UPnP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            m_device = new MyNatDevice(10000);
            InitializeComponent();

            Initialize();
        }

        /// <summary>
        /// Finds the first NATDevice and fills the data grid with available UPnP bindings.
        /// </summary>
        private async void Initialize()
        {
            IsBusy = true;

            await TryGetNatDevice();
            FillMappings(await m_device.GetAllMappings());

            IsBusy = false;
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
            btnAdd.IsEnabled = true;
            btnDelete.IsEnabled = true;
            btnRefresh.IsEnabled = true;
        }

        private void FillMappings(List<MyNATMapping> mappings)
        {
            UPnPGrid.Items.Clear();
            if (mappings == null) { return; }

            foreach (MyNATMapping map in mappings)
            { UPnPGrid.Items.Add(map); }
        }

        private async Task TryGetNatDevice()
        {
            try
            { await m_device.FindDevice(); }
            catch (NatDeviceNotFoundException)
            {
                MessageBox.Show(
                    "Unable to discover NAT Device." +
                    "\n" +
                    "Make sure UPnP is enabled on your router!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            return;
        }
    }
}
