namespace CanMonitor
{
    partial class LogControls
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
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button_clear = new System.Windows.Forms.Button();
            this.checkbox_autoscroll = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_showNMT = new System.Windows.Forms.CheckBox();
            this.checkBox_showEMCY = new System.Windows.Forms.CheckBox();
            this.checkBox_heartbeats = new System.Windows.Forms.CheckBox();
            this.checkBox_showNMTEC = new System.Windows.Forms.CheckBox();
            this.checkBox_showPDO = new System.Windows.Forms.CheckBox();
            this.checkBox_showSDO = new System.Windows.Forms.CheckBox();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button_clear);
            this.groupBox3.Controls.Add(this.checkbox_autoscroll);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox3.Size = new System.Drawing.Size(181, 96);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "View Options";
            // 
            // button_clear
            // 
            this.button_clear.Location = new System.Drawing.Point(24, 23);
            this.button_clear.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_clear.Name = "button_clear";
            this.button_clear.Size = new System.Drawing.Size(100, 28);
            this.button_clear.TabIndex = 1;
            this.button_clear.Text = "Clear all";
            this.button_clear.UseVisualStyleBackColor = true;
            this.button_clear.Click += new System.EventHandler(this.button_clear_Click);
            // 
            // checkbox_autoscroll
            // 
            this.checkbox_autoscroll.AutoSize = true;
            this.checkbox_autoscroll.Checked = true;
            this.checkbox_autoscroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkbox_autoscroll.Location = new System.Drawing.Point(28, 63);
            this.checkbox_autoscroll.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkbox_autoscroll.Name = "checkbox_autoscroll";
            this.checkbox_autoscroll.Size = new System.Drawing.Size(85, 20);
            this.checkbox_autoscroll.TabIndex = 0;
            this.checkbox_autoscroll.Text = "Autoscroll";
            this.checkbox_autoscroll.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_showNMT);
            this.groupBox1.Controls.Add(this.checkBox_showEMCY);
            this.groupBox1.Controls.Add(this.checkBox_heartbeats);
            this.groupBox1.Controls.Add(this.checkBox_showNMTEC);
            this.groupBox1.Controls.Add(this.checkBox_showPDO);
            this.groupBox1.Controls.Add(this.checkBox_showSDO);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Location = new System.Drawing.Point(181, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(256, 96);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Packet Filters";
            // 
            // checkBox_showNMT
            // 
            this.checkBox_showNMT.AutoSize = true;
            this.checkBox_showNMT.Location = new System.Drawing.Point(155, 20);
            this.checkBox_showNMT.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox_showNMT.Name = "checkBox_showNMT";
            this.checkBox_showNMT.Size = new System.Drawing.Size(56, 20);
            this.checkBox_showNMT.TabIndex = 8;
            this.checkBox_showNMT.Text = "NMT";
            this.checkBox_showNMT.UseVisualStyleBackColor = true;
            // 
            // checkBox_showEMCY
            // 
            this.checkBox_showEMCY.AutoSize = true;
            this.checkBox_showEMCY.Location = new System.Drawing.Point(155, 63);
            this.checkBox_showEMCY.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox_showEMCY.Name = "checkBox_showEMCY";
            this.checkBox_showEMCY.Size = new System.Drawing.Size(64, 20);
            this.checkBox_showEMCY.TabIndex = 10;
            this.checkBox_showEMCY.Text = "EMCY";
            this.checkBox_showEMCY.UseVisualStyleBackColor = true;
            // 
            // checkBox_heartbeats
            // 
            this.checkBox_heartbeats.AutoSize = true;
            this.checkBox_heartbeats.Location = new System.Drawing.Point(31, 21);
            this.checkBox_heartbeats.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox_heartbeats.Name = "checkBox_heartbeats";
            this.checkBox_heartbeats.Size = new System.Drawing.Size(93, 20);
            this.checkBox_heartbeats.TabIndex = 4;
            this.checkBox_heartbeats.Text = "Heartbeats";
            this.checkBox_heartbeats.UseVisualStyleBackColor = true;
            // 
            // checkBox_showNMTEC
            // 
            this.checkBox_showNMTEC.AutoSize = true;
            this.checkBox_showNMTEC.Location = new System.Drawing.Point(155, 42);
            this.checkBox_showNMTEC.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox_showNMTEC.Name = "checkBox_showNMTEC";
            this.checkBox_showNMTEC.Size = new System.Drawing.Size(74, 20);
            this.checkBox_showNMTEC.TabIndex = 9;
            this.checkBox_showNMTEC.Text = "NMTEC";
            this.checkBox_showNMTEC.UseVisualStyleBackColor = true;
            // 
            // checkBox_showPDO
            // 
            this.checkBox_showPDO.AutoSize = true;
            this.checkBox_showPDO.Location = new System.Drawing.Point(31, 43);
            this.checkBox_showPDO.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox_showPDO.Name = "checkBox_showPDO";
            this.checkBox_showPDO.Size = new System.Drawing.Size(55, 20);
            this.checkBox_showPDO.TabIndex = 6;
            this.checkBox_showPDO.Text = "PDO";
            this.checkBox_showPDO.UseVisualStyleBackColor = true;
            // 
            // checkBox_showSDO
            // 
            this.checkBox_showSDO.AutoSize = true;
            this.checkBox_showSDO.Location = new System.Drawing.Point(31, 65);
            this.checkBox_showSDO.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox_showSDO.Name = "checkBox_showSDO";
            this.checkBox_showSDO.Size = new System.Drawing.Size(55, 20);
            this.checkBox_showSDO.TabIndex = 7;
            this.checkBox_showSDO.Text = "SDO";
            this.checkBox_showSDO.UseVisualStyleBackColor = true;
            // 
            // LogControls
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(447, 96);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "LogControls";
            this.Text = "LogControls";
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button_clear;
        private System.Windows.Forms.CheckBox checkbox_autoscroll;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_showNMT;
        private System.Windows.Forms.CheckBox checkBox_showEMCY;
        private System.Windows.Forms.CheckBox checkBox_heartbeats;
        private System.Windows.Forms.CheckBox checkBox_showNMTEC;
        private System.Windows.Forms.CheckBox checkBox_showPDO;
        private System.Windows.Forms.CheckBox checkBox_showSDO;
    }
}