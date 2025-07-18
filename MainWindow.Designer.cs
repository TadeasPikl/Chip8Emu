﻿namespace Chip8Emu
{
    partial class MainWindow
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
            pictureBox1 = new PictureBox();
            menuStrip1 = new MenuStrip();
            loadROMToolStripMenuItem = new ToolStripMenuItem();
            displayToolStripMenuItem = new ToolStripMenuItem();
            colorsToolStripMenuItem = new ToolStripMenuItem();
            foregroundToolStripMenuItem = new ToolStripMenuItem();
            backgroundToolStripMenuItem = new ToolStripMenuItem();
            displayHzToolStripMenuItem = new ToolStripMenuItem();
            frequencyLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(0, 28);
            pictureBox1.MinimumSize = new Size(64, 32);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(605, 346);
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { loadROMToolStripMenuItem, displayToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(605, 28);
            menuStrip1.TabIndex = 3;
            menuStrip1.Text = "menuStrip1";
            // 
            // loadROMToolStripMenuItem
            // 
            loadROMToolStripMenuItem.Name = "loadROMToolStripMenuItem";
            loadROMToolStripMenuItem.Size = new Size(93, 24);
            loadROMToolStripMenuItem.Text = "Load ROM";
            loadROMToolStripMenuItem.Click += loadROMToolStripMenuItem_Click;
            // 
            // displayToolStripMenuItem
            // 
            displayToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { colorsToolStripMenuItem, displayHzToolStripMenuItem });
            displayToolStripMenuItem.Name = "displayToolStripMenuItem";
            displayToolStripMenuItem.Size = new Size(72, 24);
            displayToolStripMenuItem.Text = "Display";
            // 
            // colorsToolStripMenuItem
            // 
            colorsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { foregroundToolStripMenuItem, backgroundToolStripMenuItem });
            colorsToolStripMenuItem.Name = "colorsToolStripMenuItem";
            colorsToolStripMenuItem.Size = new Size(163, 26);
            colorsToolStripMenuItem.Text = "Colors";
            // 
            // foregroundToolStripMenuItem
            // 
            foregroundToolStripMenuItem.Name = "foregroundToolStripMenuItem";
            foregroundToolStripMenuItem.Size = new Size(171, 26);
            foregroundToolStripMenuItem.Text = "Foreground";
            foregroundToolStripMenuItem.Click += foregroundToolStripMenuItem_Click;
            // 
            // backgroundToolStripMenuItem
            // 
            backgroundToolStripMenuItem.Name = "backgroundToolStripMenuItem";
            backgroundToolStripMenuItem.Size = new Size(171, 26);
            backgroundToolStripMenuItem.Text = "Background";
            backgroundToolStripMenuItem.Click += backgroundToolStripMenuItem_Click;
            // 
            // displayHzToolStripMenuItem
            // 
            displayHzToolStripMenuItem.Checked = true;
            displayHzToolStripMenuItem.CheckState = CheckState.Checked;
            displayHzToolStripMenuItem.Name = "displayHzToolStripMenuItem";
            displayHzToolStripMenuItem.Size = new Size(163, 26);
            displayHzToolStripMenuItem.Text = "Display Hz";
            displayHzToolStripMenuItem.Click += displayHzToolStripMenuItem_Click;
            // 
            // frequencyLabel
            // 
            frequencyLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            frequencyLabel.Location = new Point(531, 354);
            frequencyLabel.Name = "frequencyLabel";
            frequencyLabel.Size = new Size(74, 20);
            frequencyLabel.TabIndex = 4;
            frequencyLabel.Text = "000,00 Hz";
            frequencyLabel.TextAlign = ContentAlignment.BottomRight;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(605, 374);
            Controls.Add(frequencyLabel);
            Controls.Add(pictureBox1);
            Controls.Add(menuStrip1);
            Name = "MainWindow";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private PictureBox pictureBox1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem loadROMToolStripMenuItem;
        private Label frequencyLabel;
        private ToolStripMenuItem displayToolStripMenuItem;
        private ToolStripMenuItem colorsToolStripMenuItem;
        private ToolStripMenuItem foregroundToolStripMenuItem;
        private ToolStripMenuItem backgroundToolStripMenuItem;
        private ToolStripMenuItem displayHzToolStripMenuItem;
    }
}