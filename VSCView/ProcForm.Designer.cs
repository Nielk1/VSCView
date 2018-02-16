namespace VSCView
{
    partial class ProcForm
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
            this.txtTemp1 = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.txtTemp2 = new System.Windows.Forms.TextBox();
            this.txtTemp3 = new System.Windows.Forms.TextBox();
            this.txtTemp4 = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtTemp1
            // 
            this.txtTemp1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTemp1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTemp1.Location = new System.Drawing.Point(3, 3);
            this.txtTemp1.Multiline = true;
            this.txtTemp1.Name = "txtTemp1";
            this.txtTemp1.ReadOnly = true;
            this.txtTemp1.Size = new System.Drawing.Size(229, 647);
            this.txtTemp1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.txtTemp4, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtTemp3, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtTemp2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtTemp1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(940, 637);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // txtTemp2
            // 
            this.txtTemp2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTemp2.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTemp2.Location = new System.Drawing.Point(238, 3);
            this.txtTemp2.Multiline = true;
            this.txtTemp2.Name = "txtTemp2";
            this.txtTemp2.ReadOnly = true;
            this.txtTemp2.Size = new System.Drawing.Size(229, 647);
            this.txtTemp2.TabIndex = 1;
            // 
            // txtTemp3
            // 
            this.txtTemp3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTemp3.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTemp3.Location = new System.Drawing.Point(473, 3);
            this.txtTemp3.Multiline = true;
            this.txtTemp3.Name = "txtTemp3";
            this.txtTemp3.ReadOnly = true;
            this.txtTemp3.Size = new System.Drawing.Size(229, 647);
            this.txtTemp3.TabIndex = 2;
            // 
            // txtTemp4
            // 
            this.txtTemp4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTemp4.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTemp4.Location = new System.Drawing.Point(708, 3);
            this.txtTemp4.Multiline = true;
            this.txtTemp4.Name = "txtTemp4";
            this.txtTemp4.ReadOnly = true;
            this.txtTemp4.Size = new System.Drawing.Size(229, 647);
            this.txtTemp4.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(940, 637);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtTemp1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox txtTemp4;
        private System.Windows.Forms.TextBox txtTemp3;
        private System.Windows.Forms.TextBox txtTemp2;
    }
}