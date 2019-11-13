using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BikeSharingStations
{
    class Matrix
    {
        private double[,] _matrix;
        public Matrix(int dim1, int dim2)
        {
            _matrix = new double[dim1, dim2];
        }

        public int Height { get { return _matrix.GetLength(0); } }
        public int Width { get { return _matrix.GetLength(1); } }

        public double this[int x, int y]
        {

            get { return _matrix[x, y]; }
            set
            {
                try
                {
                    _matrix[x, y] = value;

                }
                catch (Exception)
                {

                    throw new Exception(string.Format("Index Out side of arry {0},{1} ", x, y));
                }
            }
        }

        public Matrix MatrixMultiplication(Matrix m1, Matrix m2)
        {
            Matrix resultMatrix = new Matrix(m1.Height, m2.Width);
            for (int i = 0; i < resultMatrix.Height; i++)
            {
                for (int j = 0; j < resultMatrix.Width; j++)
                {
                    resultMatrix[i, j] = 0;
                    for (int k = 0; k < m1.Width; k++)
                    {
                        resultMatrix[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }
            return resultMatrix;
        }

        public void FillMatrixWithEqalTotal()
        {
            FillMatrixWithRandomNumbers();
            MakeEqualTotalInEachRow();
        }

        public void MakeEqualTotalInEachRow()
        {

            for (int j = 0; j < Height; j++)
            {
                double sumofeachrow = 0;
                for (int k = 0; k < Width; k++)
                {
                    sumofeachrow += this[j, k];
                }
                for (int k = 0; k < Width; k++)
                {
                    this[j, k] = this[j, k] / sumofeachrow;
                }
            }
        }

        public void FillMatrixWithRandomNumbers()
        {
            Random rnd2 = new Random();
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    this[i, j] = rnd2.Next(0,100);
                }

            }
        }

        public void PrintMatrix()
        {
            for (int i = 0; i < this.Height; i++)
            {
                double sumRow = 0;
                for (int j = 0; j < this.Width; j++)
                {
                    Console.Write(this[i, j].ToString() + "  -  ");
                    sumRow += this[i, j];
                }
                Console.WriteLine(" = " + sumRow.ToString());
            }

        }

        public void SaveMarixToExcel(string strFilePath)
        {
            try
            {

                // JSON.Net
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(_matrix);
                System.IO.File.WriteAllText(strFilePath, json);
                // (note: can also use File.Create etc if don't need the string in memory)

                ////read the data
                //using (var stream = new System.IO.StreamReader("c:\\MovementProbilityMatrix.json"))
                //{
                //    MovementProbilityMatrix = Newtonsoft.Json.JsonConvert.DeserializeObject<Matrix>(stream.ReadToEnd());
                //}
             
                ////read the data
                //using (var stream = new System.IO.StreamReader("c:\\MovementProbilityMatrix.json"))
                //{
                //    MovementProbilityMatrix = Newtonsoft.Json.JsonConvert.DeserializeObject<Matrix>(stream.ReadToEnd());
                //}
               
                System.Data.DataTable dtMatrix = new System.Data.DataTable();
                //Export Resault to excel
                Microsoft.Office.Interop.Excel.Application xlApp = xlApp = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
                Microsoft.Office.Interop.Excel.Worksheet xlStationsDataSheet;
                object misValue = System.Reflection.Missing.Value;

                xlWorkBook = xlApp.Workbooks.Add(misValue);
                xlStationsDataSheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
                //xlWorkBook.Worksheets.Add();

                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        xlStationsDataSheet.Cells[i+1, j+1] = this[i, j];
                    }
                }

                xlWorkBook.SaveAs(strFilePath, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                xlWorkBook.Close(true, misValue, misValue);


            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        public void ReadMatrixFromExcel(string strFilePath)
        {
            try
            {
               

                Microsoft.Office.Interop.Excel.Application xlApp1 = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook xlWorkBook1 = xlApp1.Workbooks.Open(strFilePath);
                Microsoft.Office.Interop.Excel.Worksheet xlSheet1 = xlWorkBook1.Sheets[1];
                Microsoft.Office.Interop.Excel.Range xlRange = xlSheet1.UsedRange;
                object misValue = System.Reflection.Missing.Value;
                
                _matrix = new double[xlRange.Rows.Count, xlRange.Columns.Count];

                for (int i = 0; i < xlRange.Rows.Count; i++)
                {
                    for (int j = 0; j < xlRange.Columns.Count; j++)
                    {
                        this[i, j] = Convert.ToDouble(xlRange.Cells[i+1, j+1].Value2);
                    }
                }

                xlWorkBook1.Close(true, misValue, misValue);

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        public void SaveMarixToJson(string strFilePath)
        {
            try
            {
                // JSON.Net
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(_matrix);
                System.IO.File.WriteAllText(strFilePath, json);

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        public void ReadMatrixFromJson(string strFilePath)
        {
            try
            {
                _matrix = new double[1,1];

                //read the data
                using (var stream = new System.IO.StreamReader(strFilePath))
                {
                    _matrix = Newtonsoft.Json.JsonConvert.DeserializeObject<double[,]>(stream.ReadToEnd());
                }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
    }
}
