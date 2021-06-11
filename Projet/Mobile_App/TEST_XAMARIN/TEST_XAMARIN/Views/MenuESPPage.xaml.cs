using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using TEST_XAMARIN.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

namespace TEST_XAMARIN.Views
{
    public partial class MenuESPPage : ContentPage
    {
        IBluetoothLE ble;
        IAdapter adapter;
        ObservableCollection<IDevice> deviceList;
        IDevice device;
        Position pos;
        byte[] data;
        bool isSharing = false;

        public MenuESPPage()
        {
            InitializeComponent();
            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            deviceList = new ObservableCollection<IDevice>();
            list.ItemsSource = deviceList;
        }

        private async void btnLocation_Clicked(object sender, EventArgs e)
        {
            try
            {
                pos = await CrossGeolocator.Current.GetPositionAsync();

                if (pos != null)
                {
                    Coord c = new Coord(pos.Latitude, pos.Longitude);
                    string msg = JsonConvert.SerializeObject(c);
                    data = Encoding.UTF8.GetBytes(msg);
                    await DisplayAlert("Succes", "Location set and ready to be send", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message.ToString(), "OK");
            }
        }

        private void btnStatus_Clicked(object sender, EventArgs e)
        {
            var state = ble.State;
            DisplayAlert("Bluetooth STATE", state.ToString(), "OK");
        }

        private async void btnScan_Clicked(object sender, EventArgs e)
        {
            deviceList.Clear();
            adapter.DeviceDiscovered += (s, a) =>
            {
                deviceList.Add(a.Device);

            };
            if (!ble.Adapter.IsScanning)
            {
                await adapter.StartScanningForDevicesAsync();
                await DisplayAlert("SCAN", "Scan over", "OK");
            }
        }

        private void list_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (list.SelectedItem == null)
            {
                return;
            }
            device = (IDevice)list.SelectedItem;
        }

        private async void btnConnect_Clicked(object sender, EventArgs e)
        {
            if (device == null)
            {
                await DisplayAlert("Please", "Select a device in the list", "OK");
            }
            else
            {
                try
                {
                    await adapter.ConnectToDeviceAsync(device);
                    await DisplayAlert("Success", "Connected to device", "OK");
                    sndLoc.IsEnabled = true;
                }
                catch (DeviceConnectionException ex)
                {
                    await DisplayAlert("Notice", ex.Message.ToString(), "OK");
                }
            }
        }


        private async void btnSendLocation_Clicked(object sender, EventArgs e)
        {

            await StartListeningPos();
        }


        async Task StartListeningPos()
        {
            if (isSharing == false)
            {
                isSharing = true;
            }
            else
            {
                isSharing = false;
            }
            while (isSharing)
            {
                await Task.Delay(5000);
                Coord c = new Coord(pos.Latitude, pos.Longitude);
                string msg = JsonConvert.SerializeObject(c);
                data = Encoding.UTF8.GetBytes(msg);
                var service = await device.GetServiceAsync(new Guid("4fafc201-1fb5-459e-8fcc-c5c9c331914b"));
                var charatecistics = await service.GetCharacteristicAsync(new Guid("beb5483e-36e1-4688-b7f5-ea07361b26a8"));
                await charatecistics.WriteAsync(data);
            }


        }
    }

}
