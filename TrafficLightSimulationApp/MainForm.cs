using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using TrafficLightSimulationApp.Menus;
using TrafficLightSimulationApp.Views;
using TrafficSimulationModels;
using TrafficSimulationModels.Junctions;

namespace TrafficLightSimulationApp
{
    public partial class MainForm : Form
    {
        // Holds the file name for saving after a save as
        private string fileName = null;

        // Grid that is currently being worked on
        private Grid grid;

        // View that represents the grid
        private GridView gridView;

        // Simulation object that is attached to the grid
        private Simulation simulation;

        // Menu to make changes to the selected junction
        private JunctionMenu junctionSettingsView;

        private Color hoveredColor = Color.FromArgb(120, 120, 120);

        // Prefix for the name of picture boxes to change the type of a junction
        string pbJunctionTypePrefix = "pbJunctionType";

        public MainForm()
        {
            InitializeComponent();

            // Create a new grid
            grid = new Grid();
            this.setGrid(grid);

            // Create junction settings view and add to toolbox
            junctionSettingsView = new JunctionMenu();
            junctionSettingsView.JunctionDeletePressed += junctionSettingsView_JunctionDeletePressed;
            junctionSettingsView.JunctionTypeChangePressed += junctionSettingsView_JunctionTypeChangePressed;
            panelLeft.Controls.Add(junctionSettingsView.GetControl());
        }

        private void setGrid(Grid grid)
        {
            this.grid = grid;
            pbGrid.Controls.Clear();
            gridView = new GridView(grid, AddGridControl, pbGrid.Width, pbGrid.Height);
            gridView.SelectedJunctionChanged += gridView_SelectedJunctionSlotChanged;
            gridView.AmountOfJunctionsChanged += gridView_AmountOfJunctionsChanged;
            pJunctionCreation.Visible = false;
            simulation = new Simulation(grid);
            simulation.Updated += OnUpdate;
        }

        private void AddGridControl(Control control)
        {
            pbGrid.Controls.Add(control);
        }

        // Form event handlers
        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            if (simulation.IsStarted())
            {
                simulation.Continue();
            }
        }

        private void MainForm_ResizeBegin(object sender, EventArgs e)
        {
            if (simulation.IsStarted())
            {
                simulation.Pause();
            }
        }

        void pbGrid_SizeChanged(object sender, EventArgs e)
        {
            gridView.SetSize(pbGrid.Width, pbGrid.Height);
        }

        // Grid view event handlers
        void gridView_AmountOfJunctionsChanged(int amountOfJunctions)
        {
            if (amountOfJunctions > 0)
            {
                tsbStartSimulation.Enabled = true;
            }
            else
            {
                tsbStartSimulation.Enabled = false;
            }
        }

        void gridView_SelectedJunctionSlotChanged(bool junctionSlotSelected, Junction junction)
        {
            junctionSettingsView.SetJunction(junction);

            if (junction == null && junctionSlotSelected)
            {
                pJunctionCreation.Visible = true;
            }
            else
            {
                pJunctionCreation.Visible = false;
            }
        }

        public void OnUpdate(int deltaTime)
        {
            gridView.Update();
        }

        // Toolstrip event handlers
        private void tsbStartSimulation_Click(object sender, EventArgs e)
        {
            panelLeft.Enabled = false;
            gridView.SetHovering(false);
            tsbStartSimulation.Enabled = false;
            tsbStopSimulation.Enabled = true;
            simulation.Start();
        }

        private void tsbStopSimulation_Click(object sender, EventArgs e)
        {
            panelLeft.Enabled = true;
            gridView.SetHovering(true);
            tsbStartSimulation.Enabled = true;
            tsbStopSimulation.Enabled = false;
            simulation.Stop();
            gridView.Update();
        }

        // Toolbox event handlers
        private void pbJunctionType_MouseEnter(object sender, EventArgs e)
        {
            PictureBox senderPictureBox = (PictureBox)sender;
            senderPictureBox.BackColor = hoveredColor;
        }

        private void pbJunctionType_MouseLeave(object sender, EventArgs e)
        {
            PictureBox senderPictureBox = (PictureBox)sender;
            senderPictureBox.BackColor = Color.Empty;
        }

        private void junctionType_Click(object sender, EventArgs e)
        {
            pJunctionCreation.Visible = false;
            PictureBox pictureBoxSender = (PictureBox)sender;
            string idString = pictureBoxSender.Name.Substring(pbJunctionTypePrefix.Length, pictureBoxSender.Name.Length - pbJunctionTypePrefix.Length);
            int id = -1;
            Int32.TryParse(idString, out id);
            int maxID = Enum.GetNames(typeof(JunctionType)).Length - 1;

            if (id < 0 || id > maxID)
            {
                throw new Exception("Invalid junction type id: " + idString);
            }

            gridView.AddJunction((JunctionType)id);
        }

        void junctionSettingsView_JunctionTypeChangePressed()
        {
            gridView.ChangeJunctionType();
        }

        void junctionSettingsView_JunctionDeletePressed()
        {
            gridView.DeleteJunction();
            junctionSettingsView.SetJunction(null);
            pJunctionCreation.Visible = true;
        }

        // Menu strip event handlers
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileName != null)
            {
                Grid g = new Grid();
                g = grid;

                Stream saveStream;
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.FileName = fileName;

                if ((saveStream = saveFileDialog1.OpenFile()) != null)
                {
                    IFormatter formater = new BinaryFormatter();
                    formater.Serialize(saveStream, g);
                    saveStream.Close();
                }
            }
            else
            {
                MessageBox.Show("No file to save to.");
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Grid g = grid;

            Stream saveStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((saveStream = saveFileDialog1.OpenFile()) != null)
                {
                    fileName = saveFileDialog1.FileName;
                    IFormatter formater = new BinaryFormatter();
                    formater.Serialize(saveStream, g);
                    saveStream.Close();
                }
            }
            else
            {

                MessageBox.Show("save failed or canceled");
            }
            saveToolStripMenuItem.Enabled = true;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Grid gr = new Grid();
            Stream openSteam;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "txt files (*.txt)|*.txt";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((openSteam = openFileDialog1.OpenFile()) != null)
                {
                    fileName = openFileDialog1.FileName;
                    IFormatter formater = new BinaryFormatter();
                    gr = (Grid)formater.Deserialize(openSteam);
                    openSteam.Close();
                }

                this.setGrid(gr);
                if (gr.GetAmountOfJunctions() > 0)
                {
                    tsbStartSimulation.Enabled = true;
                }
                saveToolStripMenuItem.Enabled = true;
            }
            else
            {
                MessageBox.Show("Nothing to load");
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to save this?", "Are you sure", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                Grid g = new Grid();
                this.setGrid(g);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to save this?", "Are you sure", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                MainForm.ActiveForm.Close();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.ShowDialog();
        }
    }
}
