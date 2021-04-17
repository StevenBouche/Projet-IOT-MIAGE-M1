using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectCoordinateMapping.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {

        public ObservableCollection<Body> _bodyCollection { get; set; }
        private MainWindow _mainWindow;
        // Property backing fields

        string bodyTrackingId;

        public MainWindowViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _bodyCollection = new ObservableCollection<Body>();
        }

        public string SelectedBodyTracking
        {
            get { return this.bodyTrackingId; }
            set
            {
                this.bodyTrackingId = value;
                this.NotifyPropertyChanged("BodyTrackingId");
                ulong id = Convert.ToUInt64(bodyTrackingId);
                _mainWindow.CurrentBodyChanged(id);
            }
        }

        public void RemoveBody(Body body)
        {
            _bodyCollection.Remove(body);
        }

        public void AddBody(Body body)
        {
            _bodyCollection.Add(body);
            if (_bodyCollection.Count == 1)
            {
               
                
                SelectedBodyTracking = ""+ body.TrackingId;
                this.NotifyPropertyChanged("SelectedBodyTracking");
            }
           
        }
    }
}
