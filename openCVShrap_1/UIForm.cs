using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Windows.Forms.DataVisualization.Charting;
using System.Runtime.Serialization;
using System.IO;

namespace openCVShrap_1
{
    public partial class UIForm : Form
    {
        Series barometer = new Series();

        public UIForm()
        {
            InitializeComponent();
        }

        public void UpdateBarometer(EmotionDTO emotionDTO)
        {
            Task task = Task.Run(() =>
            {
                if(this.Barometer.Series.Count != 0)
                {
                    this.Barometer.Series.Remove(barometer);
                }

                Series series = new Series();
                series.ChartType = SeriesChartType.Column;

                series.Points.AddXY(0, emotionDTO.Anger + emotionDTO.Sadness);

                barometer = series;
                

                Invoke(new setResultDelegateBarometer(setResultBarometer));
                
            });

        }

        delegate void setResultDelegateBarometer();
        void setResultBarometer()
        {
            this.Barometer.Series.Add(barometer);
        }



        private void Barometer_Click(object sender, EventArgs e)
        {

        }
    }
}
