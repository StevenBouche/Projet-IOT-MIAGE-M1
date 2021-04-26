using ControlerExample.Models;
using SharpDX;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ControlerExample.Services
{
    public class ControllerService
    {

        public  ControllerVModel ControllerView { get; set; }
        
        private List<string> CurrentListItems { get; set; }

        Dispatcher Dispatcher;

        public ControllerService(Dispatcher dispatcher)
        {
            this.Dispatcher = dispatcher;
            this.ControllerView = new ControllerVModel(new ControllerModel(), dispatcher);
            CurrentListItems = new List<string>();
            this.Init();
        }

        private void Init()
        {

            this.ControllerView.PropertyChanged += ViewHaveChange;

            this.ControllerView.Controllers = new Dictionary<String, Controller>();
            this.ControllerView.Controllers.Add(Guid.NewGuid().ToString(), new Controller(UserIndex.One));
            this.ControllerView.Controllers.Add(Guid.NewGuid().ToString(), new Controller(UserIndex.Two));
            this.ControllerView.Controllers.Add(Guid.NewGuid().ToString(), new Controller(UserIndex.Three));
            this.ControllerView.Controllers.Add(Guid.NewGuid().ToString(), new Controller(UserIndex.Four));

            RefreshItems();

        }

        private void RefreshItems()
        {

            List<string> keyConnected = GetControllersConnected();

            List<string> newKeyConnected =  keyConnected.Where(key => !CurrentListItems.Any(item => key.Equals(item))).ToList();
            List<string> keyDisconnected = CurrentListItems.Where(item => !keyConnected.Any(key => key.Equals(item))).ToList();

            if(newKeyConnected.Count() > 0 || keyDisconnected.Count() > 0)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ObservableCollection<ComboBoxItem> collection = new ObservableCollection<ComboBoxItem>();
                    CurrentListItems.Clear();
                    foreach (string key in keyConnected)
                    {
                        if (ControllerView.Controllers.TryGetValue(key, out Controller value))
                        {

                            collection.Add(new ComboBoxItem { Tag = key, Content = value.UserIndex.ToString() });
                            CurrentListItems.Add(key);
                        }
                    }
                    ControllerView.SelectedcbItem = collection.FirstOrDefault(key => key.Equals(ControllerView.KeyCurrentController));
                    ControllerView.CbItems = collection;
                });
            }
        }

        private void ViewHaveChange(object sender, PropertyChangedEventArgs e)
        {
            
        }

        public List<String> GetControllersConnected()
        {
            return this.ControllerView.Controllers.Where(c => c.Value.IsConnected).Select(c => c.Key).ToList();
        }

        public void Havechange()
        {

            RefreshItems();

            if (this.ControllerView.CurrentController == null) 
                return;
 
            try
            {
                var gamepad = this.ControllerView.CurrentController.GetState();

                ApplyDeadZone(
                    gamepad.Gamepad.LeftThumbX,
                    gamepad.Gamepad.LeftThumbY, 
                    out double LX,
                    out double LY,
                    out double magnitudeL,
                    7849);

                ApplyDeadZone(
                    gamepad.Gamepad.RightThumbX,
                    gamepad.Gamepad.RightThumbY,
                    out double RX,
                    out double RY,
                    out double magnitudeR,
                    7849);

                ControllerView.MagnitudeL = magnitudeL;
                ControllerView.MagnitudeR = magnitudeR;

                ControllerView.LeftThumbX = LX;
                ControllerView.LeftThumbY = LY;
                ControllerView.RightThumbX = RX;
                ControllerView.RightThumbY = RY;


            } catch (SharpDXException e)
            {
                Console.WriteLine(e.Message);
            }
        }


        public void ApplyDeadZone(short X, short Y, out double Xf, out double Yf, out double normalizedMagnitude, int inputDeadZone)
        {

            int max = 32767;

            //rotate coord to -45 degree
            double rad = (-45 * Math.PI) / 180;
            double cosa = Math.Cos(rad);
            double sina = Math.Sin(rad);
            double x = X * cosa - Y * sina; 
            double y = Y * cosa + X * sina;

            double magnitude = Math.Sqrt((x * x) + y * y);
            double delta = Math.Atan2(y, x);

            double xPolar = magnitude * Math.Cos(delta);
            double yPolar = magnitude * Math.Sin(delta);

            Xf = (100 * xPolar) / max;
            Yf = (100 * yPolar) / max;

            //check if the controller is outside a circular dead zone
            if (magnitude > inputDeadZone)
            {
                //clip the magnitude at its expected maximum value
                if (magnitude > max) magnitude = max;

                //adjust magnitude relative to the end of the dead zone
                magnitude -= inputDeadZone;

                //optionally normalize the magnitude with respect to its expected range
                //giving a magnitude value of 0.0 to 1.0
                normalizedMagnitude = (float)(magnitude / (max - inputDeadZone));
            }
            else //if the controller is in the deadzone zero out the magnitude
            {
                Xf = 0;
                Yf = 0;
                normalizedMagnitude = 0;
            }
        }
    }
}
