using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GAF;
using GAF.Extensions;
using GAF.Operators;

namespace BikeSharingStations
{

    class StationLocating
    {
        public static string strRootResaultPath = @"c:\BikeSharingReault\";
        static List<StationCell> StationCells;

        static double SumOfStationWeights;
        static int MaximumNumberOfbikeInEachStation = 20;
        static int MaximumNumberOfBikesInSystem = 200;
        //static int MinimumNumberOfBikesInSystem = 150;
        static int NumberOfChoromosomes = 500;
        static int NumberOfCells = 100;
        static int NumberOfCalculateNextState = 10;
        Matrix MovementProbilityMatrix;
        double NumberOfBikesToBeRebalance = 0;
        double AccessibilityAverage = 0;
        double TotalDistance = 0;
        int RoundCount = 0;
        double fitnessValue = 0;
        double AccessibilityFitness=0;
        double RebalancingFitness=0;

        int intFitnessCount=0;
        int intWrongFitnessCount = 0;
        public void StartLocating()
        {
            System.Data.DataTable dtStationCells = new System.Data.DataTable();

            //Create XML Datat for first time
            dtStationCells.TableName = "dt";
            dtStationCells.Columns.Add("Index", typeof(int));
            dtStationCells.Columns.Add("Name", typeof(string));
            dtStationCells.Columns.Add("Weight", typeof(double));
            dtStationCells.Columns.Add("Latitude", typeof(double));
            dtStationCells.Columns.Add("Longitude", typeof(double));
            dtStationCells.Columns.Add("RequestCount", typeof(int));

            MovementProbilityMatrix = new Matrix(NumberOfCells, NumberOfCells);

            Random rndi = new Random();
            //CreateShiraz Map Cells
            //29.5280603    29.57823449     29.72383715     29.69640358
            //52.55447388   52.62863159     52.47344971     52.40478516
            double latStart = 29.5280603, latEnd = 29.7238371, longStart = 52.4047851, longEnd = 52.6286315, currentLat = 0, currentLong = 0, latDistance = 0, longDistance = 0;
            currentLat = latStart;
            currentLong = longStart;
            longDistance = (longEnd - longStart) / 2;
            latDistance = (latEnd - latStart) / (NumberOfCells / 2);
            for (int i = 0; i < NumberOfCells; i++)
            {
                

                int weight = rndi.Next(0, 100);
                //dtStationCells.Rows.Add(i, i.ToString(), weight, rndi.Next(295280603, 297238371) / (double)100000000, rndi.Next(524047851, 526286315) / (double)100000000, weight);
                dtStationCells.Rows.Add(i, i.ToString(), weight, currentLat , currentLong , weight);

                currentLat += latDistance;
                if (currentLat > latEnd) currentLat = latStart;
                currentLong += longDistance;
                if (currentLong > longEnd) currentLong = longStart;
            }
            dtStationCells.WriteXmlSchema(StationLocating.strRootResaultPath + "stationsSchema.xml");
            dtStationCells.WriteXml(StationLocating.strRootResaultPath + "stations.xml");

            //ReadPoints from Excel
            //Microsoft.Office.Interop.Excel.Application xlApp1 = new Microsoft.Office.Interop.Excel.Application();
            //Microsoft.Office.Interop.Excel.Workbook xlWorkBook1 = xlApp1.Workbooks.Open(Application.StartupPath + "\\Neighborhood_Labels.xls");
            //Microsoft.Office.Interop.Excel.Worksheet xlSheet1 = xlWorkBook1.Sheets[1];
            //Microsoft.Office.Interop.Excel.Range xlRange = xlSheet1.UsedRange;
            //NumberOfCells = xlRange.Rows.Count;
            //for (int i = 2; i <= xlRange.Rows.Count; i++)
            //{
            //    if (xlRange.Cells[i, 1] != null && xlRange.Cells[i, 1].Value2 != null)
            //        dtStationCells.Rows.Add(xlRange.Cells[i, 3].Value2, rndi.Next(0, 100), Convert.ToDouble(xlRange.Cells[i, 2].Value2), Convert.ToDouble(xlRange.Cells[i, 1].Value2));
            //}
            //dtStationCells.WriteXmlSchema(@"c:\stationsSchema.xml");
            //dtStationCells.WriteXml(@"c:\stations.xml");

            //Create Movement Probility Matrix and Save it
            MovementProbilityMatrix.FillMatrixWithEqalTotal();
            //MovementProbilityMatrix.PrintMatrix();
            //MovementProbilityMatrix.SaveMarixToJson("c:\\MovementProbilityMatrix.json");

            // read Movement Probility Matrix from file
            //MovementProbilityMatrix.ReadMatrixFromJson("c:\\MovementProbilityMatrix.json");


            //Read stations data from XML file
            //dtStationCells = new System.Data.DataTable();
            //dtStationCells.ReadXmlSchema(@"c:\stationsSchema.xml");
            //dtStationCells.ReadXml(@"c:\stations.xml");

            
            StationCells = new List<BikeSharingStations.StationCell>();

            for (int i = 0; i < dtStationCells.Rows.Count; i++)
            {
                StationCells.Add(new StationCell(Convert.ToInt32(dtStationCells.Rows[i]["Index"]), dtStationCells.Rows[i]["Name"].ToString(), (double)dtStationCells.Rows[i]["Weight"], (double)dtStationCells.Rows[i]["Latitude"], (double)dtStationCells.Rows[i]["Longitude"], Convert.ToInt32(dtStationCells.Rows[i]["RequestCount"])));
            }



            foreach (StationCell item in StationCells)
            {
                SumOfStationWeights += item.Weight;
            }

            const double crossoverProbability = 0.65;
            const double mutationProbability = 0.08;
            const int elitismPercentage = 5;


            Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();;
            Microsoft.Office.Interop.Excel.Workbook xlWorkBook = xlApp.Workbooks.Open(StationLocating.strRootResaultPath + "StationsData2.xls"); ;
            object misValue = System.Reflection.Missing.Value;
            Microsoft.Office.Interop.Excel.Worksheet xlSheetFinalData = xlWorkBook.Sheets[1];

            for (RoundCount = 1; RoundCount <= 20; RoundCount++)
            {

                //create the population
                var population = new Population();//NumberOfChoromosomes, NumberOfCells, false, false);
                Random rndGeneIndex = new Random();
                Random rnd = new Random();
                //create the chromosomes;
                for (var p = 0; p < NumberOfChoromosomes; p++)
                {
                    var chromosome = new Chromosome();
                    int NumberOfBikesInThisChoromosome = 0;
                    for (var g = 0; g < NumberOfCells; g++)
                    {
                        chromosome.Genes.Add(new Gene(0));
                    }

                    //int geneIndex = 0;
                    while (NumberOfBikesInThisChoromosome < MaximumNumberOfBikesInSystem)
                    {
                        int temp = rnd.Next(MaximumNumberOfbikeInEachStation /2 , MaximumNumberOfbikeInEachStation);
                        if (NumberOfBikesInThisChoromosome + temp > MaximumNumberOfBikesInSystem) temp = MaximumNumberOfBikesInSystem - NumberOfBikesInThisChoromosome;
                        NumberOfBikesInThisChoromosome += temp;
                        chromosome.Genes[rndGeneIndex.Next(0, NumberOfCells)] = new Gene(temp);
                        //chromosome.Genes.Add(new Gene(temp));
                        //geneIndex++;
                    }
                    //chromosome.Genes.ShuffleFast();

                    int sumBikes = 0;
                    for (int i = 0; i < chromosome.Count; i++)
                    {
                        sumBikes += Convert.ToInt32(chromosome.Genes[i].RealValue);
                    }
                    if (sumBikes > MaximumNumberOfBikesInSystem)
                        sumBikes = 0;
                    population.Solutions.Add(chromosome);
                }


                //create the chromosomes;
                //for (var p = 0; p < NumberOfChoromosomes; p++)
                //{
                //    var chromosome = new Chromosome();
                //    for (var g = 0; g < NumberOfCells; g++)
                //    {
                //        chromosome.Genes.Add(new Gene(rnd.Next(0, MaximumNumberOfbikeInEachStation)));
                //    }
                //    chromosome.Genes.ShuffleFast();
                //    population.Solutions.Add(chromosome);
                //}

                //create the genetic operators 
                var elite = new Elite(elitismPercentage);


                //create the crossover operator
                var crossover = new Crossover(crossoverProbability, true)
                {
                    CrossoverType = CrossoverType.SinglePoint

                };
                //crossover.ReplacementMethod = ReplacementMethod.DeleteLast;

                var mutation = new BinaryMutate(mutationProbability, true);

                //create the GA itself 
                var ga = new GeneticAlgorithm(population, EvaluateFitness);

                //subscribe to the GAs Generation Complete event 
                ga.OnGenerationComplete += ga_OnGenerationComplete;
                //add the operators to the ga process pipeline 
                ga.Operators.Add(elite);
                ga.Operators.Add(crossover);
                ga.Operators.Add(mutation);
                
                //run the GA 
                ga.Run(TerminateAlgorithm);

                Console.WriteLine("intFitnessCount : " + intFitnessCount.ToString());
                Console.WriteLine("intWrongFitnessCount : " + intWrongFitnessCount.ToString());
                //Export Resault to excel
                Microsoft.Office.Interop.Excel.Worksheet xlStationsDataSheet;
                xlWorkBook.Worksheets.Add();
                xlStationsDataSheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                int excelColumnIndex = 1;
                xlStationsDataSheet.Name = string.Format("Round{0:00}",RoundCount);
                xlStationsDataSheet.Cells[1, excelColumnIndex] = "CellIndex"; // Index
                xlStationsDataSheet.Cells[2, excelColumnIndex] = "StationName"; // Name
                xlStationsDataSheet.Cells[3, excelColumnIndex] = "Latitude"; // Latitude
                xlStationsDataSheet.Cells[4, excelColumnIndex] = "Longitude"; // Longitude
                xlStationsDataSheet.Cells[5, excelColumnIndex] = "Weight"; // Weight
                xlStationsDataSheet.Cells[6, excelColumnIndex] = "NumberOfBikes"; // NumberOfBikes
                xlStationsDataSheet.Cells[7, excelColumnIndex] = "Station capacity"; // Station capacity
                xlStationsDataSheet.Cells[8, excelColumnIndex] = "Total Distance"; // Avrage Of Accessibility
                xlStationsDataSheet.Cells[9, excelColumnIndex] = "Number of Bikes for Rebalancing"; // Number of Bikes for Rebalancing


                excelColumnIndex += 1;

                var Topchromosome = population.Solutions[0];
                ComputeAccessibility(Topchromosome);
                ComputeRebalancingCost(Topchromosome);
                SaveProbilityMartix(string.Format(StationLocating.strRootResaultPath + "ProbibilityMatrix-Round{0:00}.json", RoundCount),Topchromosome ); 
                double totalbikeinthissystem = 0;
                for (int i = 0; i < Topchromosome.Count; i++)
                {
                    if (Topchromosome.Genes[i].RealValue != 0)
                    {
                        xlStationsDataSheet.Cells[1, excelColumnIndex] = StationCells[i].Index; // Index
                        xlStationsDataSheet.Cells[2, excelColumnIndex] = StationCells[i].Name; // Name
                        xlStationsDataSheet.Cells[3, excelColumnIndex] = StationCells[i].Latitude; // Latitude
                        xlStationsDataSheet.Cells[4, excelColumnIndex] = StationCells[i].Longitude; // Longitude
                        xlStationsDataSheet.Cells[5, excelColumnIndex] = StationCells[i].Weight; // Weight
                        xlStationsDataSheet.Cells[6, excelColumnIndex] = StationCells[i].NumberOfBikes; // NumberOfBikes
                        xlStationsDataSheet.Cells[7, excelColumnIndex] = Topchromosome.Genes[i].RealValue; // Station capacity
                        xlStationsDataSheet.Cells[8, excelColumnIndex] = TotalDistance; // Avrage Of Accessibility
                        xlStationsDataSheet.Cells[9, excelColumnIndex] = NumberOfBikesToBeRebalance; // Number of Bikes for Rebalancing
                        totalbikeinthissystem += Topchromosome.Genes[i].RealValue;
                        excelColumnIndex++;
                    }
                }

                xlSheetFinalData.Cells[1, 1] = "Round Count";
                xlSheetFinalData.Cells[1, 2] = "TotalDistance";
                xlSheetFinalData.Cells[1, 3] = "NumberOfBikesToBeRebalance";
                xlSheetFinalData.Cells[1, 4] = "Number of Bikes in system";
                xlSheetFinalData.Cells[1, 5] = "Number of stations";
                xlSheetFinalData.Cells[1, 6] = "Rebalancing is Computed";
                xlSheetFinalData.Cells[1, 7] = "fitnessValue";
                xlSheetFinalData.Cells[1, 8] = "AccessibilityFitness";
                xlSheetFinalData.Cells[1, 9] = "RebalancingFitness";

                xlSheetFinalData.Cells[RoundCount + 1, 1] = RoundCount;
                xlSheetFinalData.Cells[RoundCount + 1, 2] = TotalDistance;
                xlSheetFinalData.Cells[RoundCount + 1, 3] = NumberOfBikesToBeRebalance; // Number of Bikes for Rebalancing
                xlSheetFinalData.Cells[RoundCount + 1, 4] = totalbikeinthissystem; // Number of Bikes in system
                xlSheetFinalData.Cells[RoundCount + 1, 5] = excelColumnIndex - 2; // Number of stations
                xlSheetFinalData.Cells[RoundCount + 1, 6] = (RoundCount % 2 == 0) ? "True" : "False";
                xlSheetFinalData.Cells[RoundCount + 1, 7] = fitnessValue; // Number of Bikes for Rebalancing
                xlSheetFinalData.Cells[RoundCount + 1, 8] = AccessibilityFitness; // Number of Bikes for Rebalancing
                xlSheetFinalData.Cells[RoundCount + 1, 9] = (RoundCount % 2 == 0)? RebalancingFitness : 0; // Number of Bikes for Rebalancing
               
                releaseObject(xlStationsDataSheet);

                xlWorkBook.Save(); 
 
                //xlWorkBook.SaveAs(string.Format("c:\\StationsData.xls"), Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
               
            }

            xlWorkBook.SaveAs(string.Format(StationLocating.strRootResaultPath + "StationsData.xls"), Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);

            releaseObject(xlSheetFinalData);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            MessageBox.Show("Excel file created , you can find the file " + StationLocating.strRootResaultPath );

        }

       
        public  double EvaluateFitness(Chromosome chromosome)
        {
            intFitnessCount++;
            fitnessValue = 0;
            int sumOfBikesInStations = 0;
            for (int i = 0; i < chromosome.Count; i++)
            {
                sumOfBikesInStations += Convert.ToInt32(chromosome.Genes[i].RealValue);
                if (chromosome.Genes[i].RealValue < 0)
                    return 0;
            }
            if (!(sumOfBikesInStations <= MaximumNumberOfBikesInSystem) || (sumOfBikesInStations < MaximumNumberOfBikesInSystem - 10))
            {
                intWrongFitnessCount++; 
                fitnessValue = 0;
                return fitnessValue; 
            }


            AccessibilityFitness = ComputeAccessibility(chromosome) / (double)10;

            if (RoundCount % 2 == 0)
            {
                RebalancingFitness = ComputeRebalancingCost(chromosome);
                fitnessValue = (AccessibilityFitness + RebalancingFitness) / 2;
            }
            else
            {
                fitnessValue = AccessibilityFitness;
            }

            if (fitnessValue > 1 || fitnessValue < 0)
            {
                Console.WriteLine("Wrong Fitness Value " + fitnessValue.ToString());
                return 0;
            }
            //Console.WriteLine("EvaluateFitness  Total Value : {0} Fitness : {1} f1:{2} f2:{3} Choromosome : {4}", sumOfBikesInStations, fitnessValue, AccessibilityFitness, RebalancingFitness, chromosome.ToString());
            return 1 - fitnessValue;
        }

        public  bool TerminateAlgorithm(Population population, int currentGeneration, long currentEvaluation)
        {
            return currentGeneration > 100;
        }

        private  void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            //get the best solution 
            var chromosome = e.Population.GetTop(1)[0];


            int sumBikes = 0;
            for (int i = 0; i < chromosome.Count; i++)
            {
                sumBikes += Convert.ToInt32(chromosome.Genes[i].RealValue);

            }
            Console.WriteLine(" ga_OnGenerationComplete   Total Value : {0} Fitness : {1}  Choromosome : {2}", sumBikes, e.Population.MaximumFitness, chromosome.ToString());

        }

        private double ComputeAccessibility(Chromosome Stationchromosome)
        {
            TotalDistance = 0;
            AccessibilityAverage = 0;

            for (int i = 0; i < Stationchromosome.Genes.Count; i++)
            {

                    double minimumValue = -1;
                    double minimumDistance = 0;
                    //find nearest station 
                    for (int j = 0; j < Stationchromosome.Count; j++)
                    {
                        //if (Stationchromosome.Genes[j].RealValue != 0) // if this cell is station
                        if (Stationchromosome.Genes[j].BinaryValue != 0) // if this cell is station
                        {
                            double Distance = StationCells[i].GetDistanceFromPosition(StationCells[j].Latitude, StationCells[j].Longitude);
                            double temp = ((double)1 / Convert.ToDouble(Stationchromosome.Genes[j].RealValue)) * Distance;
                            if ((temp < minimumValue || minimumValue == -1) && temp != 0)
                            {
                                minimumValue = temp;
                                minimumDistance = Distance;
                            }
                        }
                    }
                    TotalDistance += minimumDistance; 
                    AccessibilityAverage += minimumValue * StationCells[i].Weight;
            }
          
            return AccessibilityAverage / SumOfStationWeights;   
        }

        private double ComputeRebalancingCost(Chromosome Stationchromosome)
        {

            //Get List of stations in this Chromosome and create an array of this system
            List<int> currentsystem = new List<int>();
            for (int j = 0; j < Stationchromosome.Genes.Count; j++)
            {
                if (Stationchromosome.Genes[j].BinaryValue != 0)
                {
                    currentsystem.Add(j);
                }
            }

            //Create Matix of current system
            Matrix matrP = new Matrix(currentsystem.Count, currentsystem.Count);
            for (int j = 0; j < currentsystem.Count; j++)
            {
                for (int k = 0; k < currentsystem.Count; k++)
                {
                    // Get Weight of Travel to Another Station
                    matrP[j, k] = MovementProbilityMatrix[StationCells[currentsystem[j]].Index, StationCells[currentsystem[k]].Index];
                }
            }


            //rebuild matrix to get 100 percent in each row
            for (int j = 0; j < currentsystem.Count; j++)
            {
                double sumofeachrow = 0;
                for (int k = 0; k < currentsystem.Count; k++)
                {
                    sumofeachrow += matrP[j, k];
                }
                for (int k = 0; k < currentsystem.Count; k++)
                {
                    matrP[j, k] = matrP[j, k] / sumofeachrow;
                }
            }

            //matrP.PrintMatrix();
 
            //Create CurrentState Matrix based on request Matrix
            Matrix matrV = new Matrix(1, currentsystem.Count);
            double totalRequestCounts = 0;
            for (int j = 0; j < currentsystem.Count; j++)
            {
                matrV[0, j] = StationCells[currentsystem[j]].RequestCount;
                totalRequestCounts += matrV[0, j];
            }

            //create matrix based on percent of request
            for (int j = 0; j < currentsystem.Count; j++)
            {
                matrV[0, j] = matrV[0, j] / totalRequestCounts;
            }

            //Calculate Next States up to n=10
            List<Matrix> V = new List<Matrix>();
            V.Add(new Matrix(1, currentsystem.Count));
            V[0] = matrV;

            for (int j = 1; j < NumberOfCalculateNextState; j++)
            {
                V.Add(new Matrix(1, currentsystem.Count));
                V[j] = matrP.MatrixMultiplication(V[j - 1], matrP);
            }


            // Create Matrix of first State based on bike Numbers in each station
            Matrix vFirstStateBikeNumbers = new Matrix(1, currentsystem.Count);
            double totalBikeNumbes = 0;
            for (int j = 0; j < currentsystem.Count; j++)
            {
                vFirstStateBikeNumbers[0, j] = Stationchromosome.Genes[currentsystem[j]].RealValue;
                totalBikeNumbes += vFirstStateBikeNumbers[0, j];
            }

            //vFirstStateBikeNumbers.PrintMatrix(); 
           

            // Create Matrix of final State based on final probility of request in each station
            Matrix vFinalStateBikeNumbers = new Matrix(1, currentsystem.Count);
            for (int j = 0; j < currentsystem.Count; j++)
            {
                vFinalStateBikeNumbers[0, j] = System.Math.Round(V[NumberOfCalculateNextState-1][0,j] * totalBikeNumbes,0)  ;
            }

            //vFinalStateBikeNumbers.PrintMatrix(); 


            //Score Final State 
            double totalScore = 0;
            NumberOfBikesToBeRebalance = 0;
            for (int j = 0; j < currentsystem.Count; j++)
            {
                if (vFinalStateBikeNumbers[0, j] > 0 && vFinalStateBikeNumbers[0, j] <= vFirstStateBikeNumbers[0, j]) // Count Stations Wich is in balance state
                    totalScore += 1;

                if (vFinalStateBikeNumbers[0, j] > vFirstStateBikeNumbers[0, j])
                    NumberOfBikesToBeRebalance += vFinalStateBikeNumbers[0, j] - vFirstStateBikeNumbers[0, j];
            }
            return NumberOfBikesToBeRebalance / totalBikeNumbes;
            //return totalScore / (double)currentsystem.Count;

            ////Score Each state based on first Step
            //double totalScore = 0;
            //for (int j = 0; j < NumberOfCalculateNextState; j++)
            //{
            //    totalScore += ScoreEachState(V[0], V[j]);
            //}
            //return totalScore / NumberOfCalculateNextState;
        }

        private  double ScoreEachState(Matrix V0, Matrix Vi)
        {
            int score = 0;
            double NumberOfBikesToBeRebalance = 0;
            for (int i = 0; i < V0.Width; i++)
            {
                if (V0[0, i] == Vi[0, i]) score++;

                //Calculate NumberOfBikesToBeRebalance for top choromosome, this number will use in last state of top choromosome
                if (Vi[0, i] > V0[0, i]) NumberOfBikesToBeRebalance = Vi[0, i] - V0[0, i];
            }
            return score / V0.Width;
        }

        private void CalculateChoromosomeNextSteps(Chromosome Stationchromosome)
        {
            //Get List of stations in this Chromosome and create an array of this system
            List<int> currentsystem = new List<int>();
            for (int j = 0; j < Stationchromosome.Genes.Count; j++)
            {
                if (Stationchromosome.Genes[j].BinaryValue != 0)
                {
                    currentsystem.Add(j);
                }
            }

            //Create Matix of current system
            Matrix matrP = new Matrix(currentsystem.Count, currentsystem.Count);
            for (int j = 0; j < currentsystem.Count; j++)
            {
                for (int k = 0; k < currentsystem.Count; k++)
                {
                    // Get Weight of Travel to Another Station
                    matrP[j, k] = MovementProbilityMatrix[StationCells[currentsystem[j]].Index, StationCells[currentsystem[k]].Index];

                    //matrP[j, k] = StationCells[currentsystem[j]].GetWeightofTraveltoAnotherStation(currentsystem[k]);
                }
            }

            //rebuild matrix to get 100 percent in each row
            for (int j = 0; j < currentsystem.Count; j++)
            {
                double sumofeachrow = 0;
                for (int k = 0; k < currentsystem.Count; k++)
                {
                    sumofeachrow += matrP[j, k];
                }
                for (int k = 0; k < currentsystem.Count; k++)
                {
                    //matrP[j, k] = matrP[j, k] * (1- sumofeachrow) / sumofeachrow; //we dont have percent of all rows
                    matrP[j, k] = matrP[j, k] / sumofeachrow;
                }
            }

            //matrP.PrintMatrix();

            //Create CurrentState Matrix based on request Matrix
            Matrix matrV = new Matrix(1, currentsystem.Count);
            for (int j = 0; j < currentsystem.Count; j++)
            {
                matrV[0, j] = Stationchromosome.Genes[currentsystem[j]].RealValue; 
            }

            //Calculate Next States up to n=10
            List<Matrix> V = new List<Matrix>();
            V.Add(new Matrix(1, currentsystem.Count));
            V[0] = matrV;

            for (int j = 1; j < NumberOfCalculateNextState; j++)
            {
                V.Add(new Matrix(1, currentsystem.Count));
                V[j] = matrP.MatrixMultiplication(V[j - 1], matrP);
            }

            //Score Each state based on first Step
            double totalScore = 0;
            for (int j = 0; j < NumberOfCalculateNextState; j++)
            {
                totalScore += ScoreEachState(V[0], V[j]);
            }
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Exception Occured while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        private void SaveProbilityMartix(string strFilePath,Chromosome Stationchromosome)
        {
            //Get List of stations in this Chromosome and create an array of this system
            List<int> currentsystem = new List<int>();
            for (int j = 0; j < Stationchromosome.Genes.Count; j++)
            {
                if (Stationchromosome.Genes[j].BinaryValue != 0)
                {
                    currentsystem.Add(j);
                }
            }

            //Create Matix of current system
            Matrix matrP = new Matrix(currentsystem.Count, currentsystem.Count);
            for (int j = 0; j < currentsystem.Count; j++)
            {
                for (int k = 0; k < currentsystem.Count; k++)
                {
                    // Get Weight of Travel to Another Station
                    matrP[j, k] = MovementProbilityMatrix[StationCells[currentsystem[j]].Index, StationCells[currentsystem[k]].Index];
                }
            }


            //rebuild matrix to get 100 percent in each row
            for (int j = 0; j < currentsystem.Count; j++)
            {
                double sumofeachrow = 0;
                for (int k = 0; k < currentsystem.Count; k++)
                {
                    sumofeachrow += matrP[j, k];
                }
                for (int k = 0; k < currentsystem.Count; k++)
                {
                    matrP[j, k] = matrP[j, k] / sumofeachrow;
                }
            }

            matrP.SaveMarixToJson(strFilePath);
        }

    }
}
