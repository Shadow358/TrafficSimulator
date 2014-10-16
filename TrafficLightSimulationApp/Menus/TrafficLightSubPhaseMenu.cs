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
    class TrafficLightSubPhaseMenu
    {
        GroupBox control;
        TrafficLightSubPhase subphase;
        List<TrafficLightGroupMenu> groupMenus;

        public TrafficLightSubPhaseMenu(TrafficLightSubPhase subphase)
        {
            this.subphase = subphase;

            control = new GroupBox();
            control.Text = subphase.GetName();
            control.Dock = DockStyle.Top;
            control.AutoSize = true;

            List<TrafficLightGroup> groups = subphase.GetTrafficLightGroups();
            List<Control> controls = new List<Control>();

            groupMenus = new List<TrafficLightGroupMenu>();
            int i = 0;

            foreach (TrafficLightGroup group in groups)
            {
                TrafficLightGroupMenu groupMenu = new TrafficLightGroupMenu(group);
                groupMenu.PercentageChanged += groupMenu_PercentageChanged;

                Control groupMenuControl = groupMenu.GetControl();

                if (i + 1 == groups.Count())
                {
                    groupMenuControl.Enabled = false;
                }

                groupMenus.Add(groupMenu);
                controls.Add(groupMenu.GetControl());
                i++;
            }

            updatePercentages();
            controls.Reverse();
            control.Controls.AddRange(controls.ToArray());
        }

        public void UpdateTime()
        {
            foreach (TrafficLightGroupMenu groupMenu in groupMenus)
            {
                groupMenu.UpdateTime();
            }
        }

        private void groupMenu_PercentageChanged(TrafficLightGroupMenu sender, int percentage)
        {
            int i = -1;
            foreach (TrafficLightGroupMenu menu in groupMenus)
            {
                i++;

                if(menu == sender) {
                    break;
                }
            }

            if (i < 0 || i >= groupMenus.Count)
            {
                throw new Exception("Unknown traffic light group menu sender.");
            }

            subphase.SetTrafficLightGroupPercentage(i, percentage);
            updatePercentages();
        }

        private void updatePercentages()
        {
            List<int> percentages = subphase.GetTrafficLightGroupPercentages();

            int i = 0;
            foreach (TrafficLightGroupMenu groupMenu in groupMenus)
            {
                groupMenu.SetPercentage(percentages[i]);
                i++;
            }
        }

        public Control GetControl()
        {
            return control;
        }
    }
}
