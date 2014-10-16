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
    class TrafficLightPhaseMenu
    {
        private Panel control;
        private TrafficLightPhase phase;
        private NumericUpDown phaseTimeNud;
        private List<TrafficLightSubPhaseMenu> subphaseMenus;

        public TrafficLightPhaseMenu(TrafficLightPhase phase)
        {
            this.phase = phase;
            createControl();
        }

        private void createControl()
        {
            control = new Panel();
            control.Dock = DockStyle.Top;
            control.AutoSize = true;

            // Add the name of the phase as title
            Label phaseName = new Label();
            phaseName.Text = phase.GetName();
            phaseName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            phaseName.Dock = DockStyle.Top;

            Panel field = new Panel();
            field.Dock = DockStyle.Top;
            field.AutoSize = true;
            Label titleLabel = new Label();
            titleLabel.Text = "Total time";
            titleLabel.Width = 80;
            titleLabel.Location = new Point(0, 3);

            // Add a numeric up/down for setting total phase time
            Panel phaseTimePanel = new Panel();
            phaseTimePanel.Dock = DockStyle.Top;
            phaseTimePanel.AutoSize = true;
            phaseTimeNud = new NumericUpDown();
            phaseTimeNud.Minimum = 3;
            phaseTimeNud.Maximum = 300;
            phaseTimeNud.Width = 50;
            phaseTimeNud.Location = new Point(100, 0);
            phaseTimeNud.Value = phase.GetTotalTime();
            phaseTimeNud.ValueChanged += phaseTimeNud_ValueChanged;

            Label unitLabel = new Label();
            unitLabel.Text = "s";
            unitLabel.Location = new Point(154, 3);

            phaseTimePanel.Dock = DockStyle.Top;
            phaseTimePanel.Controls.AddRange(new Control[] { titleLabel, phaseTimeNud, unitLabel });

            // Add subphase menus
            List<TrafficLightSubPhase> subphases = phase.GetTrafficLightSubPhases();
            subphaseMenus = new List<TrafficLightSubPhaseMenu>();
            List<Control> subphaseControls = new List<Control>();
            foreach (TrafficLightSubPhase subphase in subphases)
            {
                TrafficLightSubPhaseMenu subphaseMenu = new TrafficLightSubPhaseMenu(subphase);
                subphaseControls.Add(subphaseMenu.GetControl());
                subphaseMenus.Add(subphaseMenu);
            }

            subphaseControls.Reverse();

            control.Controls.AddRange(subphaseControls.ToArray());
            control.Controls.AddRange(new Control[]{
                phaseTimePanel,
                phaseName
            });
        }

        void phaseTimeNud_ValueChanged(object sender, EventArgs e)
        {
            phase.SetTotalTime(Convert.ToInt32(phaseTimeNud.Value));
            foreach(TrafficLightSubPhaseMenu subphaseMenu in subphaseMenus)
            {
                subphaseMenu.UpdateTime();
            }
        }

        public Control GetControl()
        {
            return control;
        }
    }
}
