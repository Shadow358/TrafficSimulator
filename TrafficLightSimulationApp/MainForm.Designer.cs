namespace TrafficLightSimulationApp
{
    partial class MainForm
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
            this.panelLeft = new System.Windows.Forms.Panel();
            this.pJunctionCreation = new System.Windows.Forms.Panel();
            this.pbJunctionType0 = new System.Windows.Forms.PictureBox();
            this.pbJunctionType1 = new System.Windows.Forms.PictureBox();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pbGrid = new System.Windows.Forms.PictureBox();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.tsbStartSimulation = new System.Windows.Forms.ToolStripButton();
            this.tsbStopSimulation = new System.Windows.Forms.ToolStripButton();
            this.panelLeft.SuspendLayout();
            this.pJunctionCreation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbJunctionType0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbJunctionType1)).BeginInit();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbGrid)).BeginInit();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.pJunctionCreation);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 49);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Padding = new System.Windows.Forms.Padding(15);
            this.panelLeft.Size = new System.Drawing.Size(283, 692);
            this.panelLeft.TabIndex = 2;
            // 
            // pJunctionCreation
            // 
            this.pJunctionCreation.Controls.Add(this.pbJunctionType0);
            this.pJunctionCreation.Controls.Add(this.pbJunctionType1);
            this.pJunctionCreation.Location = new System.Drawing.Point(18, 18);
            this.pJunctionCreation.Name = "pJunctionCreation";
            this.pJunctionCreation.Size = new System.Drawing.Size(259, 280);
            this.pJunctionCreation.TabIndex = 0;
            // 
            // pbJunctionType0
            // 
            this.pbJunctionType0.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pbJunctionType0.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbJunctionType0.Image = global::TrafficLightSimulationApp.Properties.Resources.junctionBasic;
            this.pbJunctionType0.Location = new System.Drawing.Point(3, 3);
            this.pbJunctionType0.Name = "pbJunctionType0";
            this.pbJunctionType0.Size = new System.Drawing.Size(150, 130);
            this.pbJunctionType0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbJunctionType0.TabIndex = 3;
            this.pbJunctionType0.TabStop = false;
            this.pbJunctionType0.Click += new System.EventHandler(this.junctionType_Click);
            // 
            // pbJunctionType1
            // 
            this.pbJunctionType1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pbJunctionType1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbJunctionType1.Image = global::TrafficLightSimulationApp.Properties.Resources.junctionCrossing;
            this.pbJunctionType1.InitialImage = global::TrafficLightSimulationApp.Properties.Resources.junctionCrossing;
            this.pbJunctionType1.Location = new System.Drawing.Point(3, 148);
            this.pbJunctionType1.Name = "pbJunctionType1";
            this.pbJunctionType1.Size = new System.Drawing.Size(151, 127);
            this.pbJunctionType1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbJunctionType1.TabIndex = 2;
            this.pbJunctionType1.TabStop = false;
            this.pbJunctionType1.Click += new System.EventHandler(this.junctionType_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(960, 24);
            this.menuStrip.TabIndex = 15;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.loadToolStripMenuItem,
            this.newToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveAsToolStripMenuItem.Text = "Save as";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // pbGrid
            // 
            this.pbGrid.BackColor = System.Drawing.Color.Black;
            this.pbGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbGrid.Location = new System.Drawing.Point(283, 49);
            this.pbGrid.Name = "pbGrid";
            this.pbGrid.Size = new System.Drawing.Size(677, 692);
            this.pbGrid.TabIndex = 13;
            this.pbGrid.TabStop = false;
            this.pbGrid.SizeChanged += new System.EventHandler(this.pbGrid_SizeChanged);
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbStartSimulation,
            this.tsbStopSimulation});
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(960, 25);
            this.toolStrip.TabIndex = 16;
            this.toolStrip.Text = "toolStrip1";
            // 
            // tsbStartSimulation
            // 
            this.tsbStartSimulation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbStartSimulation.Enabled = false;
            this.tsbStartSimulation.Image = global::TrafficLightSimulationApp.Properties.Resources.play;
            this.tsbStartSimulation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbStartSimulation.Name = "tsbStartSimulation";
            this.tsbStartSimulation.Size = new System.Drawing.Size(23, 22);
            this.tsbStartSimulation.Text = "Start simulation";
            this.tsbStartSimulation.Click += new System.EventHandler(this.tsbStartSimulation_Click);
            // 
            // tsbStopSimulation
            // 
            this.tsbStopSimulation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbStopSimulation.Enabled = false;
            this.tsbStopSimulation.Image = global::TrafficLightSimulationApp.Properties.Resources.stop;
            this.tsbStopSimulation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbStopSimulation.Name = "tsbStopSimulation";
            this.tsbStopSimulation.Size = new System.Drawing.Size(23, 22);
            this.tsbStopSimulation.Text = "Stop simulation";
            this.tsbStopSimulation.Click += new System.EventHandler(this.tsbStopSimulation_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 741);
            this.Controls.Add(this.pbGrid);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "Traffic Simulator";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResizeBegin += new System.EventHandler(this.MainForm_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
            this.panelLeft.ResumeLayout(false);
            this.pJunctionCreation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbJunctionType0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbJunctionType1)).EndInit();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbGrid)).EndInit();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.PictureBox pbGrid;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Panel pJunctionCreation;
        private System.Windows.Forms.PictureBox pbJunctionType0;
        private System.Windows.Forms.PictureBox pbJunctionType1;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton tsbStartSimulation;
        private System.Windows.Forms.ToolStripButton tsbStopSimulation;
    }
}

