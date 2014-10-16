using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrafficSimulationModels.Junctions;

namespace TrafficLightSimulationApp.Menus
{
    class JunctionDirectionMenu
    {
        private JunctionDirection junctionDirection;
        private TabPage tabPage;
        private NumericUpDown nudCarDistributionLeft;
        private NumericUpDown nudCarDistributionStraight;
        private NumericUpDown nudCarDistributionRight;
        private NumericUpDown nudPedestrianSpawnRate;
        private bool nudChangeByUser = true; 

        public JunctionDirectionMenu(JunctionDirection junctionDirection)
        {
            this.junctionDirection = junctionDirection;
            this.tabPage = createTabPage();
        }

        private TabPage createTabPage()
        {
            TabPage tabPage = new TabPage();

            // Car spawn rate
            Label lblCarSpawnRate = new Label();
            lblCarSpawnRate.Text = "Car spawn rate";
            lblCarSpawnRate.Location = new Point(5, 8);
            lblCarSpawnRate.Width = 85;

            NumericUpDown nudCarSpawnRate = new NumericUpDown();
            nudCarSpawnRate.Minimum = 0;
            nudCarSpawnRate.Maximum = 200;
            nudCarSpawnRate.Location = new Point(100, 5);
            nudCarSpawnRate.Width = 50;
            nudCarSpawnRate.Value = junctionDirection.GetCarSpawner().GetCarsPerMinute();
            nudCarSpawnRate.ValueChanged += nudCarSpawnRate_ValueChanged;

            Label lblCarsPerMin = new Label();
            lblCarsPerMin.Text = "cars/min";
            lblCarsPerMin.Location = new Point(160, 8);
            lblCarsPerMin.Width = 60;

            // Car distribution
            Label lblCarDistribution = new Label();
            lblCarDistribution.Text = "Car distribution";
            lblCarDistribution.Location = new Point(5, 50);
            lblCarDistribution.Width = 140;

            Label lblCarDistributionLeft = new Label();
            lblCarDistributionLeft.Text = "Left";
            lblCarDistributionLeft.Location = new Point(5, 75);
            lblCarDistributionLeft.Width = 50;
            lblCarDistributionLeft.Height = 15;

            Label lblCarDistributionStraight = new Label();
            lblCarDistributionStraight.Text = "Straight";
            lblCarDistributionStraight.Location = new Point(100, 75);
            lblCarDistributionStraight.Width = 50;
            lblCarDistributionStraight.Height = 15;

            Label lblCarDistributionRight = new Label();
            lblCarDistributionRight.Text = "Right";
            lblCarDistributionRight.Location = new Point(190, 75);
            lblCarDistributionRight.Width = 50;
            lblCarDistributionRight.Height = 15;

            nudCarDistributionLeft = new NumericUpDown();
            nudCarDistributionLeft.Minimum = 0;
            nudCarDistributionLeft.Maximum = 100;
            nudCarDistributionLeft.ValueChanged += nudCarDistributionLeft_ValueChanged;
            nudCarDistributionLeft.Location = new Point(5, 95);
            nudCarDistributionLeft.Width = 50;

            nudCarDistributionStraight = new NumericUpDown();
            nudCarDistributionStraight.Minimum = 0;
            nudCarDistributionStraight.Maximum = 100;
            nudCarDistributionStraight.Location = new Point(100, 95);
            nudCarDistributionStraight.Width = 50;
            nudCarDistributionStraight.Enabled = false;

            nudCarDistributionRight = new NumericUpDown();
            nudCarDistributionRight.Minimum = 0;
            nudCarDistributionRight.Maximum = 100;
            nudCarDistributionRight.ValueChanged += nudCarDistributionRight_ValueChanged;
            nudCarDistributionRight.Location = new Point(190, 95);
            nudCarDistributionRight.Width = 50;

            updateDistributionValues();

            // Pedestrian spawn rate
            Label lblPedestrianSpawnRate = new Label();
            lblPedestrianSpawnRate.Text = "Pedestrian spawn rate";
            lblPedestrianSpawnRate.Location = new Point(5, 138);
            lblPedestrianSpawnRate.Width = 85;

            nudPedestrianSpawnRate = new NumericUpDown();
            nudPedestrianSpawnRate.Minimum = 0;
            nudPedestrianSpawnRate.Maximum = 200;
            nudPedestrianSpawnRate.Location = new Point(100, 135);
            nudPedestrianSpawnRate.Width = 50;
            nudPedestrianSpawnRate.ValueChanged += nudPedestrianSpawnRate_ValueChanged;

            Label lblPedestriansPerMin = new Label();
            lblPedestriansPerMin.Text = "peds/min";
            lblPedestriansPerMin.Location = new Point(160, 138);
            lblPedestriansPerMin.Width = 60;

            tabPage.Controls.AddRange(new Control[] {
                lblCarSpawnRate,
                nudCarSpawnRate,
                lblCarsPerMin,
                lblCarDistribution,
                lblCarDistributionLeft,
                lblCarDistributionStraight,
                lblCarDistributionRight,
                nudCarDistributionLeft,
                nudCarDistributionStraight,
                nudCarDistributionRight,
            });

            if (!junctionDirection.IsEdgeDirection())
            {
                lblCarSpawnRate.Visible = false;
                nudCarSpawnRate.Visible = false;
                lblCarsPerMin.Visible = false;
            }

            if (junctionDirection.GetCrossing() != null)
            {
                nudPedestrianSpawnRate.Value = junctionDirection.GetCrossing().GetPedestriansPerMinute();

                // Add also pedestrian controls
                tabPage.Controls.AddRange(new Control[] {
                    lblPedestrianSpawnRate,
                    nudPedestrianSpawnRate,
                    lblPedestriansPerMin
                });

                
            }

            return tabPage;
        }

        void nudPedestrianSpawnRate_ValueChanged(object sender, EventArgs e)
        {
            if (junctionDirection.GetCrossing() != null)
            {
                junctionDirection.GetCrossing().SetPedestriansPerMinute(Convert.ToInt32(nudPedestrianSpawnRate.Value));
            }
        }

        void nudCarDistributionRight_ValueChanged(object sender, EventArgs e)
        {
            if (!nudChangeByUser)
            {
                return;
            }

            junctionDirection.SetDistribution(Lanes.Right, Convert.ToInt32(nudCarDistributionRight.Value));
            updateDistributionValues();
        }

        void nudCarDistributionLeft_ValueChanged(object sender, EventArgs e)
        {
            if (!nudChangeByUser)
            {
                return;
            }

            junctionDirection.SetDistribution(Lanes.Left, Convert.ToInt32(nudCarDistributionLeft.Value));
            updateDistributionValues();
        }

        private void updateDistributionValues()
        {
            nudChangeByUser = false;
            nudCarDistributionLeft.Value = junctionDirection.GetDistribution(Lanes.Left);
            nudCarDistributionStraight.Value = junctionDirection.GetDistribution(Lanes.Middle);
            nudCarDistributionRight.Value = junctionDirection.GetDistribution(Lanes.Right);
            nudChangeByUser = true;
        }

        void nudCarSpawnRate_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown nud = (NumericUpDown)tabPage.GetChildAtPoint(new Point(100, 5));
            int currentSpawnRate = (int)nud.Value;
            junctionDirection.GetCarSpawner().SetCarsPerMinute(currentSpawnRate);
        }

        public TabPage GetTabPage()
        {
            return tabPage;
        }
    }
}
