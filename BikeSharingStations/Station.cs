using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BikeSharingStations
{
    class StationCell
    {
        public StationCell()
        {

        }
        public StationCell(int index, string name, double weight, double latitude, double longitude, int requestCount)
        {
            Index = index;
            Name = name;
            Weight = weight;
            Latitude = latitude;
            Longitude = longitude;
            RequestCount = requestCount;
        }

        public int Index { get; set; }

        public string Name { get; set; }

        public int NumberOfBikes { get; set; }

        public double Weight { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int RequestCount { get; set; }

        public override string ToString()
        {
            return "Name : " + Name;// + "        Wieght : " + Weight + "       Value : " + Value + "     ResultWV : " + ResultWV;
        }

        public double GetDistanceFromPosition(double latitude, double longitude)
        {
            var R = 6371; // radius of the earth in km
            var dLat = DegreesToRadians(latitude - Latitude);
            var dLon = DegreesToRadians(longitude - Longitude);
            var a =
                System.Math.Sin(dLat / 2) * System.Math.Sin(dLat / 2) +
                System.Math.Cos(DegreesToRadians(Latitude)) * System.Math.Cos(DegreesToRadians(latitude)) *
                System.Math.Sin(dLon / 2) * System.Math.Sin(dLon / 2)
                ;
            var c = 2 * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1 - a));
            var d = R * c; // distance in km
            return d;
        }

        private static double DegreesToRadians(double deg)
        {
            return deg * (System.Math.PI / 180);
        }
    }


    class Station : StationCell 
    {
        public int AvailebleBikes { get; set; }
        public int Capasity { get; set; }
    }
}
