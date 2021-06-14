
using ControlerWPF.Models;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ControlerWPF.ViewModels
{
    public class ControlerVModel : VModelBase
    {

        private readonly ControllerModel Model;
        public readonly short MIN = -100;
        public readonly short MAX = 100;
        public readonly int TOT = 200;
        public readonly int Pourcent = 100;
        public readonly int arroundDecimal = 10000;

        private ComboBoxItem _selectedcbItem;
        private ObservableCollection<ComboBoxItem> _cbItems;
        public Controller CurrentController { get; private set; }

        public ControlerVModel(ControllerModel model, Dispatcher dispatcher) : base(dispatcher)
        {
            Model = model;
            CbItems = new ObservableCollection<ComboBoxItem>();
        }

        public ObservableCollection<ComboBoxItem> CbItems {
            get => _cbItems;
            set => SetProperty(ref _cbItems, value);
        }

        public ComboBoxItem SelectedcbItem
        {
            get => _selectedcbItem;
            set {

                SetProperty(ref _selectedcbItem, value);
                this.KeyCurrentController = value is null ? null : (string)value.Tag;
            }
        }

        public Dictionary<String, Controller> Controllers { get; set; }

        private string _keyCurrentController;

        public string KeyCurrentController {
            get => _keyCurrentController;
            private set
            {
                SetProperty(ref _keyCurrentController, value);

                if(value is null)
                    this.CurrentController = null;
                else if (Controllers.TryGetValue(value, out Controller c))
                {
                    this.CurrentController = c;
                }
                else this.CurrentController = null;
            }
        }

        public MotorValues MotorValues
        {
            get => Model.MotorValues;
            set {
                if(Model.MotorValues.MotorL != value.MotorL || Model.MotorValues.MotorR != value.MotorR)
                    SetPropertyMotor(ref Model.MotorValues, value);
            }
        }

        public string LeftThumbXLib
        {
            get => Model.LeftThumbXLib;
            private set => SetProperty(ref Model.LeftThumbXLib, value);
        }

        public string LeftThumbYLib
        {
            get => Model.LeftThumbYLib;
            private set => SetProperty(ref Model.LeftThumbYLib, value);
        }

        public string RightThumbXLib
        {
            get => Model.RightThumbXLib;
            private set => SetProperty(ref Model.RightThumbXLib, value);

        }

        public string RightThumbYLib
        {
            get => Model.RightThumbYLib;
            private set => SetProperty(ref Model.RightThumbYLib, value);

        }

        public double LeftThumbX { 
            get => Model.LeftThumbX;
            set {
                if (Model.LeftThumbX != value) 
                {
                    SetProperty(ref Model.LeftThumbX, value);
                    LeftThumbXPourcent = ThumbInPourcent(value);
                    LeftThumbXLib = $"X : {ArroundValue(value)}";
                }
            }
        }

        public double LeftThumbY
        {
            get => Model.LeftThumbY;
            set
            {
                if(Model.LeftThumbY != value)
                {
                    SetProperty(ref Model.LeftThumbY, value);
                    LeftThumbYPourcent = ThumbInPourcent(value);
                    LeftThumbYLib = $"Y : {ArroundValue(value)}";
                }
            }
        }

        public double RightThumbX
        {
            get => Model.RightThumbX;
            set
            {
                if (Model.RightThumbX != value)
                {
                    SetProperty(ref Model.RightThumbX, value);
                    RightThumbXPourcent = ThumbInPourcent(value);
                    RightThumbXLib = $"X : {ArroundValue(value)}";
                }
            }
        }

        public double RightThumbY
        {
            get => Model.RightThumbY;
            set 
            {
                if (Model.RightThumbY != value)
                {
                    SetProperty(ref Model.RightThumbY, value);
                    RightThumbYPourcent = ThumbInPourcent(value);
                    RightThumbYLib = $"Y : {ArroundValue(value)}";
                }
            }
        }

        public double LeftThumbXPourcent
        {
            get => Model.LeftThumbXPourcent;
            set => SetProperty(ref Model.LeftThumbXPourcent, value);
        }

        public double LeftThumbYPourcent
        {
            get => Model.LeftThumbYPourcent;
            set => SetProperty(ref Model.LeftThumbYPourcent, value);
        }

        public double RightThumbXPourcent
        {
            get => Model.RightThumbXPourcent;
            set => SetProperty(ref Model.RightThumbXPourcent, value);
        }

        public double RightThumbYPourcent
        {
            get => Model.RightThumbYPourcent;
            set => SetProperty(ref Model.RightThumbYPourcent, value);
        }

        public double MagnitudeL {
            get => Model.MagnitudeLeft;
            set => SetProperty(ref Model.MagnitudeLeft, value);
        }

        public double MagnitudeR { 
            get => Model.MagnitudeRight;
            set => SetProperty(ref Model.MagnitudeRight, value);
        }

        private float ArroundValue(double value)
        {
            return (float)(Math.Round(value * arroundDecimal) / arroundDecimal);
        }

        private double ThumbInPourcent(double value)
        {
            return Math.Round(((MAX + value) * Pourcent) / TOT);
        }

        public event PropertyChangedEventHandler PropertyChangedMotor;

        protected bool SetPropertyMotor<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {

                field = newValue;

                if (Dispatcher.CheckAccess())
                {
                    PropertyChangedMotor?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    Dispatcher.BeginInvoke((Action)(() => PropertyChangedMotor?.Invoke(this, new PropertyChangedEventArgs(propertyName))));
                }
                return true;
            }
            return false;
        }
    }
}
