using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BikeSharingStations
{
    class Simulation
    {
       
        public Simulation()
        {

        }

        #region Parameters

        double BikeSpeed = 20; //Riding speed[km / h]
        double MaximuWalkingDistance = 15;//maximum walking distance[km]
        double WalkSpeed = 5; // Walking speed[km / h]

        #endregion

        #region Global Variables

        List<Station> StationList = new List<Station>();
        Matrix MovementProbilityMatrix;

        double TotalDistanceOfUserWalk;
        double TotalDistanceOfBikeRides;
        int TotoalNumberOfbikesForPickup = 0;
        int AvailableBikesInTruck = 0;
        int EventID = 1;
        int GlobalTime = 0;

        #endregion

        public void StartSimulation()
        {
            try
            {

                for (int RoundCount = 1; RoundCount <= 20; RoundCount++)
                {
                    TotalDistanceOfUserWalk=0;
                    TotalDistanceOfBikeRides = 0; ;
                    AvailableBikesInTruck = 0;
                    TotoalNumberOfbikesForPickup = 0;
                    EventID = 1;
                    GlobalTime = 0;

                    ReadData(RoundCount);

                    List<FutureEvent> FutureEventList = new List<FutureEvent>();
                    int MissRequestCount = 0;
                    int HitRequestCount = 0;
                    int RebalancingCount = 0;
                    // For each cell in the list creat events and add new events to the FutureEventList
                    //Read stations data from XML file
                    System.Data.DataTable dtStationCells = new System.Data.DataTable();
                    dtStationCells.ReadXmlSchema(StationLocating.strRootResaultPath + "stationsSchema.xml");
                    dtStationCells.ReadXml(StationLocating.strRootResaultPath + "stations.xml");

                    foreach (System.Data.DataRow dr in dtStationCells.Rows)
                    {
                        Troschuetz.Random.Distributions.Discrete.PoissonDistribution rnd = new Troschuetz.Random.Distributions.Discrete.PoissonDistribution();
                        rnd.Lambda = 100 - Convert.ToDouble(dr["Weight"]);

                        int t = 0;
                        while (t < 1440) // OneDay = 1440 minute
                        {
                            t += rnd.Next();
                            FutureEventList.Add(new FutureEvent(EventID++, t, EventType.CustomerRequest, Convert.ToDouble(dr["Latitude"]), Convert.ToDouble(dr["Longitude"])));
                        }
                    }

                    //foreach (FutureEvent fe in FutureEventList)
                    //{
                    //    StationList[fe.StationIndex].RequestCount++; 
                    //}

                    

                    //for (int GlobalTime = 0; GlobalTime < 1440; GlobalTime++)
                    //{
                    //    foreach (var item in FutureEventList.Where(e => e.StartTime == GlobalTime).OrderBy(e => e.EventId).ToList())
                    //    {
                    //        Console.WriteLine(string.Format("ID: {0} Start: {1} Lat:{2} Long:{3}", item.EventId, item.StartTime, item.Latitud, item.Longitud));
                    //    }
                    //}


                    for (int GlobalTime = 0; GlobalTime < 1440; GlobalTime++)
                    {
                        List<FutureEvent> events = FutureEventList.Where(e => e.StartTime == GlobalTime).OrderBy(e => e.EventId).ToList();
                        //List<FutureEvent> events =  FutureEventList.OrderBy(e => new { e.StartTime, e.EventId }).ToList();

                        foreach (FutureEvent fe in events)
                        {
                            Random rndNextStation = new Random();

                            if (fe.EventType == EventType.CustomerRequest)
                            {
                                double NearestStationDistance = 0;// (km)
                                int NearestStationIndex = FindNearestStation(fe.Latitud, fe.Longitud, out NearestStationDistance);

                                if (NearestStationIndex == -1)
                                    MissRequestCount++;
                                else
                                {
                                    FutureEvent ne = new FutureEvent();
                                    ne.StationIndex = NearestStationIndex;
                                    ne.StartTime = fe.StartTime + Convert.ToInt32(Math.Round((NearestStationDistance / WalkSpeed) * 60, 0)); // t = s / v -> time = distance  / speed , Average speed of walking for age of 20-50 is 3.6 km/h
                                    ne.EventType = EventType.BikeRentStart;
                                    FutureEventList.Add(ne);
                                }
                            }
                            else if (fe.EventType == EventType.BikeRentStart)
                            {

                                // If there is no Available Bike then Add Miss Count And go to next event
                                if (StationList[fe.StationIndex].AvailebleBikes <= 0)
                                {
                                    MissRequestCount++;
                                    continue;
                                }

                                HitRequestCount++;

                                StationList[fe.StationIndex].AvailebleBikes -= 1;
                                StationList[fe.StationIndex].RequestCount++;
                                Console.WriteLine("1 Bike Pickedup from station " + fe.StationIndex); 


                                //Find Next Station based on Movement Probility Matrix
                                double NextStationProbibility = rndNextStation.Next(0, 100) /100.0;
                                int NextStationIndex = -1;
                                double tempTotalProbility = 0;
                                for (int j = 0; j < MovementProbilityMatrix.Width; j++)
                                {
                                    //x >= 1 && x <= 100
                                    if (NextStationProbibility >= tempTotalProbility && NextStationProbibility < tempTotalProbility + MovementProbilityMatrix[fe.StationIndex, j])
                                    {
                                        NextStationIndex = j;
                                        break;
                                    }
                                    else
                                    {
                                        tempTotalProbility += MovementProbilityMatrix[fe.StationIndex, j];
                                    }
                                }

                                FutureEvent ne = new FutureEvent();
                                ne.EventType = EventType.BikeRentFinish;

                                ne.StationIndex = NextStationIndex;
                                double DistanceToNextStation = StationList[fe.StationIndex].GetDistanceFromPosition(StationList[NextStationIndex].Latitude, StationList[NextStationIndex].Longitude);
                                TotalDistanceOfBikeRides += DistanceToNextStation;
                                ne.StartTime = fe.StartTime + Convert.ToInt32(Math.Round((DistanceToNextStation / BikeSpeed) * 60, 0)); // t = s / v -> time = distance / speed , Average speed of bikes is 15 km/h

                                FutureEventList.Add(ne);

                                if (StationList[fe.StationIndex].AvailebleBikes <= 0)
                                {
                                    FutureEvent re = new FutureEvent();
                                    re.EventType = EventType.Rebalancing;
                                    re.StationIndex = fe.StationIndex;
                                    re.StartTime = fe.StartTime + 1;
                                    FutureEventList.Add(re);
                                }

                            }
                            else if (fe.EventType == EventType.BikeRentFinish)
                            {

                                StationList[fe.StationIndex].AvailebleBikes += 1;
                                Console.WriteLine("1 Bike withdraw in station " + fe.StationIndex);

                                if (StationList[fe.StationIndex].AvailebleBikes >= StationList[fe.StationIndex].Capasity)
                                {
                                    FutureEvent re = new FutureEvent();
                                    re.EventType = EventType.Rebalancing;
                                    re.StationIndex = fe.StationIndex;
                                    re.StartTime = fe.StartTime + 1;
                                    FutureEventList.Add(re);
                                }

                            }
                            else if (fe.EventType == EventType.Rebalancing)
                            {
                                //?? Reblancing To which stations should be done? one station or multiple station ?
                                //e.StartTime = DateTime.Now;  // #### distance and duration to next station Should be determined in here 
                                //e.Distance = 0;  // #### distance and duration to next station Should be determined in here 

                                RebalancingCount++;

                                foreach (var item in StationList) //Pickup Extra Bikes from each station
                                {
                                    if (item.AvailebleBikes > item.NumberOfBikes)
                                    {
                                        int NumberOfbikesForPickup = item.AvailebleBikes - item.NumberOfBikes;
                                        Console.WriteLine(NumberOfbikesForPickup.ToString() + " Bike Pickedup from station " + item.Index.ToString() + " for rebalancing" + " AvailebleBikes:" + item.AvailebleBikes + " NumberOfBikes: " + item.NumberOfBikes);
                                        TotoalNumberOfbikesForPickup += NumberOfbikesForPickup;
                                        AvailableBikesInTruck += NumberOfbikesForPickup;
                                        item.AvailebleBikes = item.NumberOfBikes;

                                    }
                                }

                                foreach (var item in StationList.OrderByDescending(s=> s.NumberOfBikes - s.AvailebleBikes)) // withdraw bilke in each station which is needed
                                {
                                    if (AvailableBikesInTruck > 0)
                                    {
                                        if (item.AvailebleBikes < item.NumberOfBikes)
                                        {
                                            if (AvailableBikesInTruck >= item.NumberOfBikes - item.AvailebleBikes)
                                            {
                                                AvailableBikesInTruck -= item.NumberOfBikes - item.AvailebleBikes;
                                                Console.WriteLine((item.NumberOfBikes - item.AvailebleBikes).ToString() + " Bike withdraw in station " + item.Index.ToString() + " for rebalancing" + " AvailebleBikes:" + item.AvailebleBikes + " NumberOfBikes: " + item.NumberOfBikes);
                                                item.AvailebleBikes = item.NumberOfBikes;
                                            }
                                            else
                                            {
                                                Console.WriteLine(AvailableBikesInTruck.ToString() + " Bike withdraw in station " + item.Index.ToString() + " for rebalancing" + " AvailebleBikes:" + item.AvailebleBikes + " NumberOfBikes: " + item.NumberOfBikes);

                                                item.AvailebleBikes = AvailableBikesInTruck;
                                                AvailableBikesInTruck -= AvailableBikesInTruck;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        
                    }
                    foreach (var item in StationList)
                    {
                        Console.WriteLine(string.Format("Station Index : {0}  Weight : {1}  Request : {2}", item.Index, item.Weight, item.RequestCount));
                    }

                    xlResaultDataSheet.Cells[1, 10] = "TotalDistanceOfUserWalk";
                    xlResaultDataSheet.Cells[1, 11] = "TotalDistanceOfBikeRides";
                    xlResaultDataSheet.Cells[1, 12] = "MissRequestCount";
                    xlResaultDataSheet.Cells[1, 13] = "HitRequestCount";
                    xlResaultDataSheet.Cells[1, 14] = "NumberOfBikesMoved";
                    xlResaultDataSheet.Cells[1, 15] = "RebalancingCount";
                    
                    xlResaultDataSheet.Cells[RoundCount+1, 10] = TotalDistanceOfUserWalk;
                    xlResaultDataSheet.Cells[RoundCount + 1, 11] = TotalDistanceOfBikeRides;
                    xlResaultDataSheet.Cells[RoundCount + 1, 12] = MissRequestCount;
                    xlResaultDataSheet.Cells[RoundCount + 1, 13] = HitRequestCount;
                    xlResaultDataSheet.Cells[RoundCount + 1, 14] = TotoalNumberOfbikesForPickup;
                    xlResaultDataSheet.Cells[RoundCount + 1, 15] = RebalancingCount;

                    xlStationsDataSheet.Cells[10, 1] = "SimulationRequestCount";

                    foreach (var item in StationList)
                    {
                        xlStationsDataSheet.Cells[10,item.Index +2] = item.RequestCount;  
                    }
                }

                xlWorkBook.SaveAs(StationLocating.strRootResaultPath + "StationsData3.xls");


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int FindNearestStation(double Latitude, double Longitude, out double NearestStationDistance)
        {
            double minimumDistance = -1;
            int NearestStationIndex = -1;
            NearestStationDistance = 0;
            //find nearest station 
            foreach (Station s in StationList)
            {
                //if (s.AvailebleBikes >  0) // if station has Bike
                //{
                double Distance = s.GetDistanceFromPosition(Latitude, Longitude);
                if ((Distance < minimumDistance || minimumDistance == -1))
                {
                    minimumDistance = Distance;
                    NearestStationIndex = s.Index;
                }
                //}
            }

            if (minimumDistance != -1)
                StationList[NearestStationIndex].RequestCount++;

            if (minimumDistance < MaximuWalkingDistance && minimumDistance != -1)
            {
                TotalDistanceOfUserWalk += minimumDistance;
                NearestStationDistance = minimumDistance;
            }
            else
            {
                TotalDistanceOfMissRequest += minimumDistance;
                TotalDistanceOfMissRequestCount++;
                NearestStationIndex = -1;
            }
            return NearestStationIndex;
        }
        double TotalDistanceOfMissRequest = 0;
        int TotalDistanceOfMissRequestCount = 0;

        #region Import Data From Excel

        Microsoft.Office.Interop.Excel.Application xlApp;
        Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
        Microsoft.Office.Interop.Excel.Worksheet xlStationsDataSheet;
        Microsoft.Office.Interop.Excel.Worksheet xlResaultDataSheet;
        public void ReadData(int RoundCount)
        {
            object misValue = System.Reflection.Missing.Value;

            if (xlApp == null)
            {
                xlApp = new Microsoft.Office.Interop.Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(StationLocating.strRootResaultPath + "StationsData.xls");
                for (int i = 1; i <= xlWorkBook.Sheets.Count; i++)
                {
                    xlResaultDataSheet = xlWorkBook.Sheets[i];
                    if (xlResaultDataSheet.Name == string.Format("FinalData", RoundCount))
                        break;
                }
            }
            xlStationsDataSheet = xlWorkBook.Sheets[1];

            for (int i = 1; i <= xlWorkBook.Sheets.Count ; i++)
            {
                xlStationsDataSheet = xlWorkBook.Sheets[i];
                if (xlStationsDataSheet.Name == string.Format(@"Round{0:00}",RoundCount))
                    break;
            }

            Microsoft.Office.Interop.Excel.Range xlRange = xlStationsDataSheet.UsedRange;
            StationList = new List<Station>();
            for (int i = 1; i < xlRange.Columns.Count; i++)
            {
                Station s = new Station();
                s.Index = i-1;
                s.Name = Convert.ToString(xlRange.Cells[2, i + 1].Value2);
                s.Latitude = Convert.ToDouble(xlRange.Cells[3, i+1].Value2);
                s.Longitude = Convert.ToDouble(xlRange.Cells[4, i+1].Value2);
                s.AvailebleBikes = Convert.ToInt32 ( xlRange.Cells[7, i+1].Value2);
                s.NumberOfBikes = Convert.ToInt32 ( xlRange.Cells[7, i+1].Value2);
                s.Capasity = Convert.ToInt32(Math.Round(s.AvailebleBikes / 4.0,0)) + s.AvailebleBikes;
                s.Weight = Convert.ToDouble(xlRange.Cells[5, i + 1].Value2);
                StationList.Add(s);

                //if (Topchromosome.Genes[i].RealValue != 0)
                //{
                //    xlStationsDataSheet.Cells[1, excelColumnIndex] = StationCells[i].Index; // Index
                //    xlStationsDataSheet.Cells[2, excelColumnIndex] = StationCells[i].Name; // Name
                //    xlStationsDataSheet.Cells[3, excelColumnIndex] = StationCells[i].Latitude; // Latitude
                //    xlStationsDataSheet.Cells[4, excelColumnIndex] = StationCells[i].Longitude; // Longitude
                //    xlStationsDataSheet.Cells[5, excelColumnIndex] = StationCells[i].Weight; // Weight
                //    xlStationsDataSheet.Cells[6, excelColumnIndex] = StationCells[i].NumberOfBikes; // NumberOfBikes
                //    xlStationsDataSheet.Cells[7, excelColumnIndex] = Topchromosome.Genes[i].RealValue; // Station capacity
                //    xlStationsDataSheet.Cells[8, excelColumnIndex] = TotalDistance; // Avrage Of Accessibility
                //    xlStationsDataSheet.Cells[9, excelColumnIndex] = NumberOfBikesToBeRebalance; // Number of Bikes for Rebalancing
                //    totalbikeinthissystem += Topchromosome.Genes[i].RealValue;
                //    excelColumnIndex++;
                //}

              
            }
            MovementProbilityMatrix = new Matrix(1,1); 
            MovementProbilityMatrix.ReadMatrixFromJson(StationLocating.strRootResaultPath + string.Format("ProbibilityMatrix-Round{0:00}.json", RoundCount)); 


        }

        #endregion

       

    }
}

