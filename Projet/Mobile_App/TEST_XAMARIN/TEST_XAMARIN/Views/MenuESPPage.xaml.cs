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

namespace TEST_XAMARIN.Views
{
    public partial class MenuESPPage : ContentPage
    {
        IBluetoothLE ble;
        IAdapter adapter;
        ObservableCollection<IDevice> deviceList;
        IDevice device;
        Location location;

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
                location = await Geolocation.GetLocationAsync();

                if(location != null)
                {
                    await DisplayAlert("Location", $"Latitude :{ location.Latitude}, Longitude: {location.Longitude}", "OK");
                }
                else
                {
                    await DisplayAlert("Problème", "Pas de location", "OK");
                }
            }
            catch(Exception ex)
            {
               await DisplayAlert("Error", ex.Message.ToString(), "OK");
            }
        }

        private void btnStatus_Clicked(object sender, EventArgs e)
        {
            var state = ble.State;
            this.DisplayAlert("Bluetooth STATE", state.ToString(), "OK");
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
                await DisplayAlert("Attention", "Veuillez sélectionner un appareil dans la liste", "OK");
            }
            else
            {
                try
                {
                    await adapter.ConnectToDeviceAsync(device);
                    await DisplayAlert("BT", "Connected to device", "OK");
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
            if (location != null)
            {
                Coord c = new Coord(location.Latitude, location.Longitude);
                string msg = JsonConvert.SerializeObject(c);
                byte[] data = Encoding.UTF8.GetBytes(msg);
                var service = await device.GetServiceAsync(new Guid("4fafc201-1fb5-459e-8fcc-c5c9c331914b"));
                var charatecistics = await service.GetCharacteristicAsync(new Guid("beb5483e-36e1-4688-b7f5-ea07361b26a8"));
                await charatecistics.WriteAsync(data);
                await DisplayAlert("Success", "Location send to device", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Setup your location wtih the Location button", "OK");
            }
        }
    }
}