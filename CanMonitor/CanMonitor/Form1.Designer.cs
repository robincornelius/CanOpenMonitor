namespace CanMonitor
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.checkBox_heartbeats = new System.Windows.Forms.CheckBox();
            this.button_open = new System.Windows.Forms.Button();
            this.comboBox_rate = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_port = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkbox_autoscroll = new System.Windows.Forms.CheckBox();
            this.button_clear = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sDOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nMTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eEPROMResetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.errorInjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.sDOUPLOADToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ManualSDOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sTARTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pDOTESTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.charge100vToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopChargeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ctrlOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctrlOffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.AccessibleRole = System.Windows.Forms.AccessibleRole.MenuBar;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.checkBox_heartbeats);
            this.splitContainer1.Panel1.Controls.Add(this.button_open);
            this.splitContainer1.Panel1.Controls.Add(this.comboBox_rate);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.comboBox_port);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.checkbox_autoscroll);
            this.splitContainer1.Panel1.Controls.Add(this.button_clear);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView1);
            this.splitContainer1.Size = new System.Drawing.Size(1098, 674);
            this.splitContainer1.SplitterDistance = 43;
            this.splitContainer1.TabIndex = 1;
            // 
            // checkBox_heartbeats
            // 
            this.checkBox_heartbeats.AutoSize = true;
            this.checkBox_heartbeats.Checked = true;
            this.checkBox_heartbeats.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_heartbeats.Location = new System.Drawing.Point(730, 15);
            this.checkBox_heartbeats.Name = "checkBox_heartbeats";
            this.checkBox_heartbeats.Size = new System.Drawing.Size(78, 17);
            this.checkBox_heartbeats.TabIndex = 4;
            this.checkBox_heartbeats.Text = "Heartbeats";
            this.checkBox_heartbeats.UseVisualStyleBackColor = true;
            // 
            // button_open
            // 
            this.button_open.Location = new System.Drawing.Point(352, 13);
            this.button_open.Name = "button_open";
            this.button_open.Size = new System.Drawing.Size(106, 23);
            this.button_open.TabIndex = 3;
            this.button_open.Text = "Open";
            this.button_open.UseVisualStyleBackColor = true;
            this.button_open.Click += new System.EventHandler(this.button_open_Click);
            // 
            // comboBox_rate
            // 
            this.comboBox_rate.FormattingEnabled = true;
            this.comboBox_rate.Items.AddRange(new object[] {
            "10Kbit",
            "20Kbit",
            "50Kbit",
            "100Kbit",
            "125Kbit",
            "250Kbit",
            "500Kbit",
            "800Kbit",
            "1Mbit"});
            this.comboBox_rate.Location = new System.Drawing.Point(224, 15);
            this.comboBox_rate.Name = "comboBox_rate";
            this.comboBox_rate.Size = new System.Drawing.Size(109, 21);
            this.comboBox_rate.TabIndex = 2;
            this.comboBox_rate.SelectedIndexChanged += new System.EventHandler(this.comboBox_rate_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(181, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Bitrate";
            // 
            // comboBox_port
            // 
            this.comboBox_port.FormattingEnabled = true;
            this.comboBox_port.Location = new System.Drawing.Point(53, 15);
            this.comboBox_port.Name = "comboBox_port";
            this.comboBox_port.Size = new System.Drawing.Size(109, 21);
            this.comboBox_port.TabIndex = 0;
            this.comboBox_port.Text = "COM4";
            this.comboBox_port.SelectedIndexChanged += new System.EventHandler(this.comboBox_port_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Port";
            // 
            // checkbox_autoscroll
            // 
            this.checkbox_autoscroll.AutoSize = true;
            this.checkbox_autoscroll.Checked = true;
            this.checkbox_autoscroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkbox_autoscroll.Location = new System.Drawing.Point(528, 17);
            this.checkbox_autoscroll.Name = "checkbox_autoscroll";
            this.checkbox_autoscroll.Size = new System.Drawing.Size(72, 17);
            this.checkbox_autoscroll.TabIndex = 0;
            this.checkbox_autoscroll.Text = "Autoscroll";
            this.checkbox_autoscroll.UseVisualStyleBackColor = true;
            // 
            // button_clear
            // 
            this.button_clear.Location = new System.Drawing.Point(606, 13);
            this.button_clear.Name = "button_clear";
            this.button_clear.Size = new System.Drawing.Size(75, 23);
            this.button_clear.TabIndex = 1;
            this.button_clear.Text = "Clear all";
            this.button_clear.UseVisualStyleBackColor = true;
            this.button_clear.Click += new System.EventHandler(this.button_clear_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1098, 627);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Type";
            this.columnHeader1.Width = 74;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "COB";
            this.columnHeader2.Width = 71;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Node";
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Payload";
            this.columnHeader4.Width = 192;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Info";
            this.columnHeader5.Width = 310;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.actionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1098, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(97, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sDOToolStripMenuItem,
            this.nMTToolStripMenuItem,
            this.eEPROMResetToolStripMenuItem,
            this.errorInjectToolStripMenuItem,
            this.flashToolStripMenuItem,
            this.toolStripSeparator1,
            this.sDOUPLOADToolStripMenuItem,
            this.ManualSDOToolStripMenuItem,
            this.sTARTToolStripMenuItem,
            this.pDOTESTToolStripMenuItem,
            this.charge100vToolStripMenuItem,
            this.stopChargeToolStripMenuItem,
            this.ctrlOnToolStripMenuItem,
            this.ctrlOffToolStripMenuItem});
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            this.actionsToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.actionsToolStripMenuItem.Text = "Actions";
            // 
            // sDOToolStripMenuItem
            // 
            this.sDOToolStripMenuItem.Name = "sDOToolStripMenuItem";
            this.sDOToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.sDOToolStripMenuItem.Text = "SDO Editor";
            this.sDOToolStripMenuItem.Click += new System.EventHandler(this.sDOToolStripMenuItem_Click);
            // 
            // nMTToolStripMenuItem
            // 
            this.nMTToolStripMenuItem.Name = "nMTToolStripMenuItem";
            this.nMTToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.nMTToolStripMenuItem.Text = "NMT";
            this.nMTToolStripMenuItem.Click += new System.EventHandler(this.nMTToolStripMenuItem_Click);
            // 
            // eEPROMResetToolStripMenuItem
            // 
            this.eEPROMResetToolStripMenuItem.Name = "eEPROMResetToolStripMenuItem";
            this.eEPROMResetToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.eEPROMResetToolStripMenuItem.Text = "EEPROM reset";
            this.eEPROMResetToolStripMenuItem.Click += new System.EventHandler(this.eEPROMResetToolStripMenuItem_Click);
            // 
            // errorInjectToolStripMenuItem
            // 
            this.errorInjectToolStripMenuItem.Name = "errorInjectToolStripMenuItem";
            this.errorInjectToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.errorInjectToolStripMenuItem.Text = "error inject";
            this.errorInjectToolStripMenuItem.Click += new System.EventHandler(this.errorInjectToolStripMenuItem_Click);
            // 
            // flashToolStripMenuItem
            // 
            this.flashToolStripMenuItem.Name = "flashToolStripMenuItem";
            this.flashToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.flashToolStripMenuItem.Text = "Flash";
            this.flashToolStripMenuItem.Click += new System.EventHandler(this.flashToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(165, 6);
            // 
            // sDOUPLOADToolStripMenuItem
            // 
            this.sDOUPLOADToolStripMenuItem.Name = "sDOUPLOADToolStripMenuItem";
            this.sDOUPLOADToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.sDOUPLOADToolStripMenuItem.Text = "SDO UPLOAD";
            this.sDOUPLOADToolStripMenuItem.Click += new System.EventHandler(this.sDOUPLOADToolStripMenuItem_Click);
            // 
            // ManualSDOToolStripMenuItem
            // 
            this.ManualSDOToolStripMenuItem.Name = "ManualSDOToolStripMenuItem";
            this.ManualSDOToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.ManualSDOToolStripMenuItem.Text = "SDO DOWNLOAD";
            this.ManualSDOToolStripMenuItem.Click += new System.EventHandler(this.ManualSDOToolStripMenuItem_Click);
            // 
            // sTARTToolStripMenuItem
            // 
            this.sTARTToolStripMenuItem.Name = "sTARTToolStripMenuItem";
            this.sTARTToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.sTARTToolStripMenuItem.Text = "START";
            this.sTARTToolStripMenuItem.Click += new System.EventHandler(this.sTARTToolStripMenuItem_Click);
            // 
            // pDOTESTToolStripMenuItem
            // 
            this.pDOTESTToolStripMenuItem.Name = "pDOTESTToolStripMenuItem";
            this.pDOTESTToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.pDOTESTToolStripMenuItem.Text = "PDOTEST";
            this.pDOTESTToolStripMenuItem.Click += new System.EventHandler(this.pDOTESTToolStripMenuItem_Click);
            // 
            // charge100vToolStripMenuItem
            // 
            this.charge100vToolStripMenuItem.Name = "charge100vToolStripMenuItem";
            this.charge100vToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.charge100vToolStripMenuItem.Text = "Charge Test";
            this.charge100vToolStripMenuItem.Click += new System.EventHandler(this.charge100vToolStripMenuItem_Click);
            // 
            // stopChargeToolStripMenuItem
            // 
            this.stopChargeToolStripMenuItem.Name = "stopChargeToolStripMenuItem";
            this.stopChargeToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.stopChargeToolStripMenuItem.Text = "Stop Charge";
            this.stopChargeToolStripMenuItem.Click += new System.EventHandler(this.stopChargeToolStripMenuItem_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ctrlOnToolStripMenuItem
            // 
            this.ctrlOnToolStripMenuItem.Name = "ctrlOnToolStripMenuItem";
            this.ctrlOnToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.ctrlOnToolStripMenuItem.Text = "Ctrl On";
            this.ctrlOnToolStripMenuItem.Click += new System.EventHandler(this.ctrlOnToolStripMenuItem_Click);
            // 
            // ctrlOffToolStripMenuItem
            // 
            this.ctrlOffToolStripMenuItem.Name = "ctrlOffToolStripMenuItem";
            this.ctrlOffToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.ctrlOffToolStripMenuItem.Text = "Ctrl Off";
            this.ctrlOffToolStripMenuItem.Click += new System.EventHandler(this.ctrlOffToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1098, 698);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "CanOpen monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button button_open;
        private System.Windows.Forms.ComboBox comboBox_rate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_port;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.CheckBox checkbox_autoscroll;
        private System.Windows.Forms.Button button_clear;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem actionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sDOToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ManualSDOToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nMTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eEPROMResetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem errorInjectToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBox_heartbeats;
        private System.Windows.Forms.ToolStripMenuItem flashToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem sDOUPLOADToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sTARTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pDOTESTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem charge100vToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopChargeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ctrlOnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ctrlOffToolStripMenuItem;
    }
}

