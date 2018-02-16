namespace VSCView
{
    partial class RawForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RawForm));
            this.txtUpdate = new System.Windows.Forms.TextBox();
            this.txtDevicePath = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtBattery = new System.Windows.Forms.TextBox();
            this.txtConnection = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtUpdate
            // 
            this.txtUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUpdate.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUpdate.Location = new System.Drawing.Point(12, 60);
            this.txtUpdate.Multiline = true;
            this.txtUpdate.Name = "txtUpdate";
            this.txtUpdate.ReadOnly = true;
            this.txtUpdate.Size = new System.Drawing.Size(738, 840);
            this.txtUpdate.TabIndex = 0;
            this.txtUpdate.Text = resources.GetString("txtUpdate.Text");
            // 
            // txtDevicePath
            // 
            this.txtDevicePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDevicePath.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDevicePath.Location = new System.Drawing.Point(12, 8);
            this.txtDevicePath.Name = "txtDevicePath";
            this.txtDevicePath.ReadOnly = true;
            this.txtDevicePath.Size = new System.Drawing.Size(738, 20);
            this.txtDevicePath.TabIndex = 1;
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescription.Location = new System.Drawing.Point(12, 34);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.Size = new System.Drawing.Size(738, 20);
            this.txtDescription.TabIndex = 2;
            // 
            // txtBattery
            // 
            this.txtBattery.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBattery.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBattery.Location = new System.Drawing.Point(12, 1024);
            this.txtBattery.Multiline = true;
            this.txtBattery.Name = "txtBattery";
            this.txtBattery.ReadOnly = true;
            this.txtBattery.Size = new System.Drawing.Size(738, 166);
            this.txtBattery.TabIndex = 3;
            this.txtBattery.Text = "00\r\n00\r\n00\r\n00\r\n00\r\n00\r\n00\r\n00\r\n00\r\n00\r\n00\r\n00";
            // 
            // txtConnection
            // 
            this.txtConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConnection.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConnection.Location = new System.Drawing.Point(12, 906);
            this.txtConnection.Multiline = true;
            this.txtConnection.Name = "txtConnection";
            this.txtConnection.ReadOnly = true;
            this.txtConnection.Size = new System.Drawing.Size(738, 112);
            this.txtConnection.TabIndex = 4;
            this.txtConnection.Text = "00\r\n00\r\n00\r\n00\r\n00\r\n00\r\n00\r\n00";
            // 
            // RawForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(811, 298);
            this.Controls.Add(this.txtConnection);
            this.Controls.Add(this.txtBattery);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.txtDevicePath);
            this.Controls.Add(this.txtUpdate);
            this.Name = "RawForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtUpdate;
        private System.Windows.Forms.TextBox txtDevicePath;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.TextBox txtBattery;
        private System.Windows.Forms.TextBox txtConnection;
    }
}

