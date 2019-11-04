namespace VSCView
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
            this.components = new System.ComponentModel.Container();
            this.cmsMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiTheme = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiController = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiReloadThemes = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiReloadControllers = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiSetBackgroundColor = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.minimizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiExit = new System.Windows.Forms.ToolStripMenuItem();
            this.lblHint1 = new System.Windows.Forms.Label();
            this.lblHint2 = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.hIDGuardianWhitelistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmsMain
            // 
            this.cmsMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiTheme,
            this.tsmiController,
            this.toolStripSeparator1,
            this.tsmiReloadThemes,
            this.tsmiReloadControllers,
            this.toolStripSeparator2,
            this.hIDGuardianWhitelistToolStripMenuItem,
            this.tsmiSetBackgroundColor,
            this.tsmiAbout,
            this.toolStripSeparator3,
            this.minimizeToolStripMenuItem,
            this.tsmiExit});
            this.cmsMain.Name = "cmsMain";
            this.cmsMain.Size = new System.Drawing.Size(195, 242);
            // 
            // tsmiTheme
            // 
            this.tsmiTheme.Name = "tsmiTheme";
            this.tsmiTheme.Size = new System.Drawing.Size(194, 22);
            this.tsmiTheme.Text = "&Theme";
            // 
            // tsmiController
            // 
            this.tsmiController.Name = "tsmiController";
            this.tsmiController.Size = new System.Drawing.Size(194, 22);
            this.tsmiController.Text = "&Controller";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(191, 6);
            // 
            // tsmiReloadThemes
            // 
            this.tsmiReloadThemes.Name = "tsmiReloadThemes";
            this.tsmiReloadThemes.Size = new System.Drawing.Size(194, 22);
            this.tsmiReloadThemes.Text = "Reload Themes";
            this.tsmiReloadThemes.Click += new System.EventHandler(this.tsmiReloadThemes_Click);
            // 
            // tsmiReloadControllers
            // 
            this.tsmiReloadControllers.Name = "tsmiReloadControllers";
            this.tsmiReloadControllers.Size = new System.Drawing.Size(194, 22);
            this.tsmiReloadControllers.Text = "Reload Controllers";
            this.tsmiReloadControllers.Click += new System.EventHandler(this.tsmiReloadControllers_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(191, 6);
            // 
            // tsmiSetBackgroundColor
            // 
            this.tsmiSetBackgroundColor.Name = "tsmiSetBackgroundColor";
            this.tsmiSetBackgroundColor.Size = new System.Drawing.Size(194, 22);
            this.tsmiSetBackgroundColor.Text = "Set Background Color";
            this.tsmiSetBackgroundColor.Click += new System.EventHandler(this.tsmiSetBackgroundColor_Click);
            // 
            // tsmiAbout
            // 
            this.tsmiAbout.Name = "tsmiAbout";
            this.tsmiAbout.Size = new System.Drawing.Size(194, 22);
            this.tsmiAbout.Text = "&About";
            this.tsmiAbout.Click += new System.EventHandler(this.tsmiAbout_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(191, 6);
            // 
            // minimizeToolStripMenuItem
            // 
            this.minimizeToolStripMenuItem.Name = "minimizeToolStripMenuItem";
            this.minimizeToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.minimizeToolStripMenuItem.Text = "&Minimize";
            this.minimizeToolStripMenuItem.Click += new System.EventHandler(this.MinimizeToolStripMenuItem_Click);
            // 
            // tsmiExit
            // 
            this.tsmiExit.Name = "tsmiExit";
            this.tsmiExit.Size = new System.Drawing.Size(194, 22);
            this.tsmiExit.Text = "E&xit";
            this.tsmiExit.Click += new System.EventHandler(this.tsmiExit_Click);
            // 
            // lblHint1
            // 
            this.lblHint1.AutoSize = true;
            this.lblHint1.Location = new System.Drawing.Point(87, 77);
            this.lblHint1.Name = "lblHint1";
            this.lblHint1.Size = new System.Drawing.Size(142, 13);
            this.lblHint1.TabIndex = 1;
            this.lblHint1.Text = "Right Click for Context Menu";
            // 
            // lblHint2
            // 
            this.lblHint2.AutoSize = true;
            this.lblHint2.Location = new System.Drawing.Point(87, 109);
            this.lblHint2.Name = "lblHint2";
            this.lblHint2.Size = new System.Drawing.Size(119, 13);
            this.lblHint2.TabIndex = 2;
            this.lblHint2.Text = "Click and Drag to Move";
            // 
            // colorDialog1
            // 
            this.colorDialog1.Color = System.Drawing.Color.Lime;
            // 
            // hIDGuardianWhitelistToolStripMenuItem
            // 
            this.hIDGuardianWhitelistToolStripMenuItem.Name = "hIDGuardianWhitelistToolStripMenuItem";
            this.hIDGuardianWhitelistToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.hIDGuardianWhitelistToolStripMenuItem.Text = "HID Guardian &Whitelist";
            this.hIDGuardianWhitelistToolStripMenuItem.Visible = false;
            this.hIDGuardianWhitelistToolStripMenuItem.Click += new System.EventHandler(this.HIDGuardianWhitelistToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lime;
            this.ClientSize = new System.Drawing.Size(314, 264);
            this.ContextMenuStrip = this.cmsMain;
            this.Controls.Add(this.lblHint2);
            this.Controls.Add(this.lblHint1);
            this.DoubleBuffered = true;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "VSCView";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            this.cmsMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip cmsMain;
        private System.Windows.Forms.ToolStripMenuItem tsmiTheme;
        private System.Windows.Forms.ToolStripMenuItem tsmiController;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tsmiExit;
        private System.Windows.Forms.ToolStripMenuItem tsmiReloadThemes;
        private System.Windows.Forms.ToolStripMenuItem tsmiReloadControllers;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem tsmiAbout;
        private System.Windows.Forms.Label lblHint1;
        private System.Windows.Forms.Label lblHint2;
        private System.Windows.Forms.ToolStripMenuItem tsmiSetBackgroundColor;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.ToolStripMenuItem minimizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hIDGuardianWhitelistToolStripMenuItem;
    }
}