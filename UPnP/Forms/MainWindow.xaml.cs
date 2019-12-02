﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UPnP.Exceptions;
using UPnP.Forms;
using UPnP.Objects;

namespace UPnP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            InitializeComponent();

            btnRefresh_Click(null, null);
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
            catch (NoNatDeviceException)
            { HandleNoNatDeviceException(); }
        }
        private async Task RemoveMapping(MyNatMapping mapping)
        {
            try
            {
                //await m_device.RemoveMapping(mapping.Mapping);
                await RefreshMappings();
            }
            catch (NoNatDeviceException)
            { HandleNoNatDeviceException(); }
        }
        private async Task RefreshMappings()
        {
            if (!MyNatDevice.HasDevice)
            {
                try
                { await MyNatDevice.Instance.FindDevice(); }
                catch
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
            catch (NoNatDeviceException)
            {
                MessageBox.Show(
                    "Unable to get UPnP mappings.\nNo NAT device available.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            FillMappings(mappings);
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            IsBusy = true;
            await RefreshMappings();
            IsBusy = false;
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            IsBusy = true;
            //var selection = UPnPGrid.SelectedItem;
            IsBusy = false;
        }
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
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
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        { MyNatDevice.Instance.CancelPendingRequests(); }

        private void HandleNoNatDeviceException()
        {
            MessageBox.Show(
                "No NAT device available.\nTry refreshing first.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
