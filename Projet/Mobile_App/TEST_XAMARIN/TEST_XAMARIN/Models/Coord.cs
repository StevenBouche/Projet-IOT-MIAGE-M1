using System;

namespace TEST_XAMARIN.Models
{
    public class Coord
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Coord(double lat, double lng)
        {
            Latitude = lat;
            Longitude = lng;
        }
    }
}