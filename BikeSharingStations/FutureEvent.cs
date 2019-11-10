using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BikeSharingStations
{
    public enum EventType
    {
        CustomerRequest,
        BikeRentStart,
        BikeRentFinish,
        Rebalancing,
    }

    class FutureEvent
    {
        public FutureEvent()
        {
        }

        public FutureEvent(int eventId, int stationIndex, int time, EventType e)
        {
            StationIndex = stationIndex;
            StartTime = time;
            EventType = e;
            EventId = eventId;
        }

        public FutureEvent(int eventId, int time, EventType e, double latitud, double longitud)
        {
            StartTime = time;
            EventType =  e;
            Latitud = latitud;
            Longitud = longitud;
            EventId = eventId;
        }

        public int StationIndex;
        public int StartTime;
        public EventType EventType;

        //public int Duration;
      //public float Distance;
        public int NumberOfBikesForRebalancing;
        public double Latitud;
        public double Longitud;
        public int EventId;
    }
}
