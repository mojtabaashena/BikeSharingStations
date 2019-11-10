using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BikeSharingStations
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            rnd2.Lambda = 40;

           

        }

        Troschuetz.Random.Distributions.Discrete.PoissonDistribution rnd2 = new Troschuetz.Random.Distributions.Discrete.PoissonDistribution();
        private void button1_Click(object sender, EventArgs e)
        {
             
            MessageBox.Show(rnd2.Next().ToString());
        }

        private void BtnStartSimulation_Click(object sender, EventArgs e)
        {

            //StationLocating l = new StationLocating();
            //l.StartLocating();

            Simulation s = new Simulation();
            s.StartSimulation();

        }
    }

}
