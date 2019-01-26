namespace openCVShrap_1
{
    partial class UIForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.Barometer = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.Barometer)).BeginInit();
            this.SuspendLayout();
            // 
            // Barometer
            // 
            chartArea1.AxisX.LabelStyle.Enabled = false;
            chartArea1.AxisX.Maximum = 2D;
            chartArea1.AxisX.Minimum = 0D;
            chartArea1.Name = "ChartArea1";
            this.Barometer.ChartAreas.Add(chartArea1);
            this.Barometer.Location = new System.Drawing.Point(12, 68);
            this.Barometer.Name = "Barometer";
            series1.ChartArea = "ChartArea1";
            series1.Name = "Series1";
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series1.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            this.Barometer.Series.Add(series1);
            this.Barometer.Size = new System.Drawing.Size(300, 300);
            this.Barometer.TabIndex = 0;
            this.Barometer.Text = "chart1";
            this.Barometer.Click += new System.EventHandler(this.Barometer_Click);
            // 
            // UIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(773, 508);
            this.Controls.Add(this.Barometer);
            this.Name = "UIForm";
            this.Text = "UIForm";
            ((System.ComponentModel.ISupportInitialize)(this.Barometer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart Barometer;
    }
}