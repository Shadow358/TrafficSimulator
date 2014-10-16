using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using TrafficSimulationModels.Junctions;
using TrafficSimulationModels;

namespace TrafficLightSimulationApp.Views
{
    public class GridView
    {
        // Grid that the view represents
        private Grid grid;

        // Prefix for the name of picture boxes that represent a junction in the grid
        string pbJunctionPrefix = "pbJunction";

        // The array containing the picturesboxes that represent the junctions, using x and y as indices
        private PictureBox[,] pictureBoxes;

        // Junction that is currently selected
        public JunctionView selectedJunction = null;

        // The size of each picture box expressed in pixels
        private int pictureBoxSize = 200;

        private float scale = (float)0.05;

        private bool hovering = true;

        public Point currentSlotDraw;

        private void setPictureBoxSize(int size)
        {
            this.pictureBoxSize = size;
            updatePictureBoxes();
        }

        // The space between each picture box on the grid
        private int pictureBoxMargin = 2;

        public void SetSize(int width, int height)
        {
            int slotsX = grid.GetJunctionSlotsX();
            int slotsY = grid.GetJunctionSlotsY();

            int spacePerSlotX = Convert.ToInt32(Math.Floor((double)(width - pictureBoxMargin * (slotsX + 1)) / slotsX));
            int spacePerSlotY = Convert.ToInt32(Math.Floor((double)(height - pictureBoxMargin * (slotsY + 1)) / slotsY));

            if (spacePerSlotY < spacePerSlotX)
            {
                pictureBoxSize = spacePerSlotY;
                scale = (float)spacePerSlotY / 2000;
            }
            else
            {
                pictureBoxSize = spacePerSlotX;
                scale = (float)spacePerSlotX / 2000;
            }

            updatePictureBoxes();
        }

        // Views that represent the junctions
        JunctionView[,] junctionViews;
        
        // Delegate used to add controls to whatever represents the grid in the main form (we don't want to expose
        // pbGrid itself to this class, but we still want to add picture boxes to it).
        public delegate void AddControl(Control control);
        private AddControl addControl;

        public delegate void SelectedJunctionChangedHandler(bool junctionSlotSelected, Junction junction);
        public event SelectedJunctionChangedHandler SelectedJunctionChanged;

        public delegate void AmmountOfJunctionsChangedHandler(int AmmountOfJunctions);
        public event AmmountOfJunctionsChangedHandler AmountOfJunctionsChanged;

        public void ChangeSelectedPictureBox(PictureBox selectedPictureBox)
        {
            if (selectedJunction != null)
            {
                selectedJunction.Deselect();
            }

            if (selectedPictureBox == null)
            {
                selectedJunction = null;
            }
            else
            {
                selectedJunction = getJunctionView(selectedPictureBox);
                selectedJunction.Select();
            }

            if (selectedJunction == null)
            {
                SelectedJunctionChanged(false, null);
            }
            else
            {
                SelectedJunctionChanged(true, selectedJunction.GetJunction());
            }
        }

        public GridView(Grid grid, AddControl addControl, int width, int height)
        {
            this.addControl = addControl;
            this.grid = grid;
            createPictureBoxes();
            SetSize(width, height);
        }

        private void setBackground(JunctionView junctionView, object background)
        {
            PictureBox pictureBox = getPictureBox(junctionView);

            if (pictureBox == null)
            {
                throw new Exception("Unknown sender");
            }

            if (background == null)
            {
                pictureBox.Image = null;
                pictureBox.BackColor = Color.Transparent;
                return;
            }

            if (background is Color)
            {
                pictureBox.BackColor = (Color)background;
                return;
            }
            
            if (background is Image)
            {
                pictureBox.Image = (Image)background;
                return;
            }

            throw new NotImplementedException();
        }

        private Point? getPoint(JunctionView junctionView)
        {
            for (int x = 0; x < grid.GetJunctionSlotsX(); x++)
            {
                for (int y = 0; y < grid.GetJunctionSlotsY(); y++)
                {
                    if (junctionViews[x, y] == junctionView)
                    {
                        return new Point(x, y);
                    }
                }
            }
            return null;
        }

        private PictureBox getPictureBox(JunctionView junctionView)
        {
            for (int x = 0; x < grid.GetJunctionSlotsX(); x++)
            {
                for (int y = 0; y < grid.GetJunctionSlotsY(); y++)
                {
                    if (junctionViews[x, y] == junctionView)
                    {
                        return pictureBoxes[x, y];
                    }
                }
            }

            return null;
        }

        public JunctionView getJunctionView(PictureBox pictureBox)
        {
            for (int x = 0; x < grid.GetJunctionSlotsX(); x++)
            {
                for (int y = 0; y < grid.GetJunctionSlotsY(); y++)
                {
                    if (pictureBoxes[x, y] == pictureBox)
                    {
                        return junctionViews[x, y];
                    }
                }
            }

            return null;
        }

        private void createPictureBoxes()
        {
            pictureBoxes = new PictureBox[grid.GetJunctionSlotsX(), grid.GetJunctionSlotsY()];
            junctionViews = new JunctionView[grid.GetJunctionSlotsX(), grid.GetJunctionSlotsY()];

            for (int x = 0; x < grid.GetJunctionSlotsX(); x++)
            {
                for (int y = 0; y < grid.GetJunctionSlotsY(); y++)
                {
                    // Picture boxes are identified by their x and y coordinate
                    string pictureBoxName = pbJunctionPrefix + "x" + x + "y" + "y";
                    int xCoordinate = pictureBoxMargin + x * (pictureBoxMargin + pictureBoxSize);
                    int yCoordinate = pictureBoxMargin + y * (pictureBoxMargin + pictureBoxSize);
                    pictureBoxes[x, y] = createJunctionPictureBox(pictureBoxName, new Point(xCoordinate, yCoordinate));

                    Junction junction = grid.GetJunction(new Point(x, y));
                    if (junction == null)
                    {
                        junctionViews[x, y] = new JunctionView(setBackground);
                    }
                    else
                    {
                        junctionViews[x, y] = new JunctionView(setBackground);
                        junctionViews[x, y].SetJunction(junction);
                    }
                    pictureBoxes[x, y].MouseEnter += GridViewJunction_MouseEnter;
                    pictureBoxes[x, y].MouseLeave += GridViewJunction_MouseLeave;
                    pictureBoxes[x, y].Paint += GridView_Paint;
                }
            }

            foreach(JunctionView junctionView in junctionViews) {
                junctionView.Draw();
            }

            foreach (PictureBox pictureBox in pictureBoxes)
            {
                addControl(pictureBox);
            }
        }

        void GridView_Paint(object sender, PaintEventArgs e)
        { 
            PictureBox pictureBox = (PictureBox)sender;
            Point? point = getSlot(pictureBox);

            if (point == null)
            {
                throw new Exception("Picture box not found.");
            }

            JunctionView junctionView = junctionViews[((Point)point).X, ((Point)point).Y];
            Graphics g = e.Graphics;

            g.ScaleTransform(scale, scale);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            junctionView.Draw(g);
        }

        private void draw()
        {
            for (int x = 0; x < grid.GetJunctionSlotsX(); x++)
            {
                for (int y = 0; y < grid.GetJunctionSlotsY(); y++)
                {
                    currentSlotDraw = new Point(x, y);
                    pictureBoxes[x, y].Invalidate();
                }
            }
        }

        private Point? getSlot(PictureBox pictureBox)
        {
            for (int x = 0; x < grid.GetJunctionSlotsX(); x++)
            {
                for (int y = 0; y < grid.GetJunctionSlotsY(); y++)
                {
                    if (pictureBoxes[x, y] == pictureBox)
                    {
                        return new Point(x, y);
                    }
                }
            }

            return null;
        }

        void GridViewJunction_MouseLeave(object sender, EventArgs e)
        {
            if (!hovering)
            {
                return;
            }
            PictureBox pictureBox = (PictureBox)sender;
            JunctionView junctionView = getJunctionView(pictureBox);
            junctionView.SetHovered(false);
        }

        void GridViewJunction_MouseEnter(object sender, EventArgs e)
        {
            if (!hovering)
            {
                return;
            }
            PictureBox pictureBox = (PictureBox)sender;
            JunctionView junctionView = getJunctionView(pictureBox);
            junctionView.SetHovered(true);
        }

        private PictureBox createJunctionPictureBox(string name, Point point)
        {

            PictureBox pictureBox = new PictureBox();
            pictureBox.Name = name;
            pictureBox.Location = point;

            // This information is taken from the generated code in Form1.Designer.cs
            pictureBox.Name = name;
            pictureBox.BackColor = System.Drawing.Color.White;
            pictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            pictureBox.Size = new System.Drawing.Size(pictureBoxSize, pictureBoxSize);
            pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox.TabIndex = 4;
            pictureBox.TabStop = false;
            pictureBox.Click += new System.EventHandler(pbJunction_Click);

            return pictureBox;
        }

        private void updatePictureBoxes()
        {
            for (int x = 0; x < grid.GetJunctionSlotsX(); x++)
            {
                for (int y = 0; y < grid.GetJunctionSlotsY(); y++)
                {
                    PictureBox pictureBox = pictureBoxes[x, y];
                    int xCoordinate = pictureBoxMargin + x * (pictureBoxMargin + pictureBoxSize);
                    int yCoordinate = pictureBoxMargin + y * (pictureBoxMargin + pictureBoxSize);
                    pictureBox.Location = new Point(xCoordinate, yCoordinate);
                    pictureBox.Size = new System.Drawing.Size(pictureBoxSize, pictureBoxSize);
                }
            }
        }

        /// <summary>
        /// Event handler for a click on any junction picture box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pbJunction_Click(object sender, EventArgs e)
        {
            if (!hovering)
            {
                return;
            }
            PictureBox pictureBoxSender = (PictureBox)sender;
            ChangeSelectedPictureBox(pictureBoxSender);
        }

        public void AddJunction(JunctionType type)
        {
            Point point = getSelectedPoint();
            grid.AddJunction(point, type);
            selectedJunction.SetJunction(grid.GetJunction(point));
            SelectedJunctionChanged(true, selectedJunction.GetJunction());
            AmountOfJunctionsChanged(grid.GetAmountOfJunctions());
            draw();
        }

        public void DeleteJunction()
        {
            Point point = getSelectedPoint();
            grid.DeleteJunction(point);
            selectedJunction.DeleteJunction();
            AmountOfJunctionsChanged(grid.GetAmountOfJunctions());
            draw();
        }

        public int GetAmountOfJunctions()
        {
            return grid.GetAmountOfJunctions();
        }

        public void ChangeJunctionType()
        {
            Point point = getSelectedPoint();
            JunctionType junctionType = selectedJunction.GetJunction().GetJunctionType();
            JunctionType newJunctionType = JunctionType.Basic;
            if (junctionType == JunctionType.Basic)
            {
                newJunctionType = JunctionType.Crossing;
            }
            DeleteJunction();
            AddJunction(newJunctionType);
            selectedJunction.Draw();
        }

        public void Update()
        {
            draw();
        }

        private Point getSelectedPoint()
        {
            Point point = (Point)getPoint(selectedJunction);
            return point;
        }

        private void deselectJunction() {
            if (selectedJunction == null)
            {
                return;
            }

            selectedJunction.Deselect();
            selectedJunction = null;
            ChangeSelectedPictureBox(null);
        }

        public void SetHovering(bool hovering)
        {
            if (!hovering)
            {
                deselectJunction();
            }

            this.hovering = hovering;
        }
    }
}
