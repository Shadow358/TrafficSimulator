using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrafficSimulationModels.TrafficLightSystem;

namespace TrafficLightSimulationApp.Menus
{
    class TrafficLightGroupMenu
    {
        Panel control;
        TrafficLightGroup group;
        NumericUpDown groupPercentageNud;
        private bool setByUser = true;
        private Label secondsLabel;

        public delegate void PercentageChangedHandler(TrafficLightGroupMenu sender, int percentage);
        public event PercentageChangedHandler PercentageChanged;

        public TrafficLightGroupMenu(TrafficLightGroup group)
        {
            this.group = group;
            createControl();
        }

        void groupPercentageNud_ValueChanged(object sender, EventArgs e)
        {
            if (setByUser)
            {
                PercentageChanged(this, Convert.ToInt32(groupPercentageNud.Value));
            }


        }

        private void createControl()
        {
            control = new Panel();
            control.Dock = DockStyle.Top;
            control.AutoSize = true;
            Label titleLabel = new Label();
            titleLabel.Text = group.GetName();
            titleLabel.Width = 90;
            titleLabel.Location = new Point(0, 3);

            groupPercentageNud = new NumericUpDown();
            groupPercentageNud.Minimum = 0;
            groupPercentageNud.Maximum = 100;
            groupPercentageNud.Width = 50;
            groupPercentageNud.Location = new Point(97, 0);
            groupPercentageNud.Value = 0;
            groupPercentageNud.ValueChanged += groupPercentageNud_ValueChanged;

            Label unitLabel = new Label();
            unitLabel.Text = "% =";
            unitLabel.Width = 25;
            unitLabel.Location = new Point(150, 3);

            secondsLabel = new Label();
            secondsLabel.Text = "0";
            secondsLabel.Width = 45;
            secondsLabel.TextAlign = ContentAlignment.TopRight;
            secondsLabel.Location = new Point(170, 3);

            Label unitLabel2 = new Label();
            unitLabel2.Text = "s";
            unitLabel2.Width = 50;
            unitLabel2.Location = new Point(213, 3);

            control.Controls.AddRange(new Control[] { titleLabel, groupPercentageNud, unitLabel, unitLabel2, secondsLabel });
        }

        public void SetPercentage(int percentage)
        {
            setByUser = false;
            groupPercentageNud.Value = percentage;
            setByUser = true;
            UpdateTime();
        }

        public void UpdateTime()
        {
            secondsLabel.Text = Math.Round(((double)group.GetTotalTime() / 1000), 2).ToString();
        }

        public Control GetControl()
        {
            return control;
        }
    }
}
