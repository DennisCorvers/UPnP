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

        public MainWindow()
        {
            m_device = new MyNatDevice(10000);
            InitializeComponent();

            Setup();
        }

        private async void Setup()
        {
            await GetNatDevice();
            var doSomethingWithThis = await m_device.GetAllMappings();
        }

        private async Task GetNatDevice()
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
        }

    }
}
