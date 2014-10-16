using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrafficSimulationModels.Junctions;
using TrafficSimulationModels.TrafficLightSystem;

namespace TrafficLightSimulationApp.Menus
{
    class JunctionMenu
    {
        private Junction junction;
        private Control control;
        private TabControl trafficLightSystemTabControl;
        private TabControl spawningDistributionTabControl;
        private List<TabPage> trafficLightSystemTabPages;
        private List<TrafficLightPhaseMenu> phaseMenus;

        public delegate void JunctionTypeChangePressedHandler();
        public event JunctionTypeChangePressedHandler JunctionDeletePressed;

        public delegate void JunctionDeletePressedHandler();
        public event JunctionDeletePressedHandler JunctionTypeChangePressed;

        public JunctionMenu()
        {
            // Label1
            Label label1 = new Label();
            label1.Text = "Traffic light system settings";
            label1.Location = new Point(30, 30);
            label1.Dock = DockStyle.Top;

            Panel panelButtons = new Panel();
            panelButtons.Height = 45;
            panelButtons.Dock = DockStyle.Top;

            // Junction delete
            Button btnJunctionDelete = new Button();
            btnJunctionDelete.Text = "Delete";
            btnJunctionDelete.Width = 115;
            btnJunctionDelete.Height = 40;
            btnJunctionDelete.Location = new Point(0,0);
            btnJunctionDelete.Click += btnJunctionDelete_Click;

            Button btnJunctionChangeType = new Button();
            btnJunctionChangeType.Text = "Change junction type";
            btnJunctionChangeType.Width = 130;
            btnJunctionChangeType.Height = 40;
            btnJunctionChangeType.Location = new Point(124, 0);
            btnJunctionChangeType.Click += btnJunctionChangeType_Click;

            panelButtons.Controls.AddRange(new Control[] {
                btnJunctionDelete,
                btnJunctionChangeType
            });
            // Junction type change

            // Traffic light system tab control
            trafficLightSystemTabControl = new TabControl();
            trafficLightSystemTabControl.Dock = DockStyle.Top;
            trafficLightSystemTabControl.Location = new Point(10, 10);
            trafficLightSystemTabControl.Height = 200;

            // Label 2
            Label label2 = new Label();
            label2.Text = "Distribution and spawning";
            label2.Location = new Point(30, 30);
            label2.Dock = DockStyle.Top;
            label2.Padding = new Padding(0, 10, 0, 0);
            label2.Height = 30;

            // Distribution and spawning tab control
            spawningDistributionTabControl = new TabControl();
            spawningDistributionTabControl.Dock = DockStyle.Top;
            spawningDistributionTabControl.Location = new Point(10, 10);
            spawningDistributionTabControl.Height = 200;

            // Panel
            Panel panel = new Panel();
            panel.Dock = DockStyle.Top;
            panel.AutoSize = true;
            panel.Controls.AddRange(new Control[] {
                spawningDistributionTabControl,
                label2,
                trafficLightSystemTabControl,
                panelButtons,
                label1
            });

            this.control = panel;
            SetJunction(null);
        }

        void btnJunctionChangeType_Click(object sender, EventArgs e)
        {
            JunctionTypeChangePressed();
            SetJunction(junction);
        }

        void btnJunctionDelete_Click(object sender, EventArgs e)
        {
            JunctionDeletePressed();
        }

        public void SetJunction(Junction junction)
        {
            if (junction == null)
            {
                control.Visible = false;
                return;
            }
            control.Visible = true;

            this.junction = junction;

            refreshTrafficLightSystemTabControl();
            refreshSpawningDistributionTabControl();

        }

        private void refreshTrafficLightSystemTabControl()
        {
            trafficLightSystemTabControl.Controls.Clear();
            trafficLightSystemTabPages = new List<TabPage>();

            TabPage test = new TabPage();

            List<TrafficLightPhase> phases = junction.GetTrafficLightPhases();
            phaseMenus = new List<TrafficLightPhaseMenu>();

            foreach (TrafficLightPhase phase in phases)
            {
                TrafficLightPhaseMenu phaseMenu = new TrafficLightPhaseMenu(phase);

                TabPage tabPage = new TabPage(phase.GetNameAbbreviation());
                tabPage.Padding = new Padding(5);
                tabPage.AutoSize = true;

                tabPage.Controls.Add(phaseMenu.GetControl());

                trafficLightSystemTabControl.Controls.Add(tabPage);
                phaseMenus.Add(phaseMenu);
            }
        }

        private void refreshSpawningDistributionTabControl()
        {
            spawningDistributionTabControl.Controls.Clear();

            foreach (Directions direction in (Directions[])Enum.GetValues(typeof(Directions)))
            {
                TabPage tabPage;
                if (junction == null)
                {
                    tabPage = new TabPage();
                }
                else
                {
                    JunctionDirection junctionDirection = junction.GetJunctionDirection(direction);
                    JunctionDirectionMenu junctionDirectionMenu = new JunctionDirectionMenu(junctionDirection);

                    tabPage = junctionDirectionMenu.GetTabPage();
                }

                switch (direction)
                {
                    case Directions.North:
                        tabPage.Text = "North";
                        break;

                    case Directions.East:
                        tabPage.Text = "East";
                        break;

                    case Directions.South:
                        tabPage.Text = "South";
                        break;

                    case Directions.West:
                        tabPage.Text = "West";
                        break;
                }

                spawningDistributionTabControl.Controls.Add(tabPage);
            }
        }

        public Control GetControl()
        {
            return control;
        }
    }
}
