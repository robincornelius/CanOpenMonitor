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
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button_open = new System.Windows.Forms.Button();
            this.comboBox_rate = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_port = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.checkbox_autoscroll = new System.Windows.Forms.CheckBox();
            this.button_clear = new System.Windows.Forms.Button();
            this.textBox_sdoindex = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_sdosub = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_value = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBox_type = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button_sendsdo = new System.Windows.Forms.Button();
            this.textBox_node = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.button_eepromreset = new System.Windows.Forms.Button();
            this.button_readAD = new System.Windows.Forms.Button();
            this.textBox_ad = new System.Windows.Forms.TextBox();
            this.button_sdo = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
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
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.button_sdo);
            this.splitContainer1.Panel1.Controls.Add(this.button_open);
            this.splitContainer1.Panel1.Controls.Add(this.comboBox_rate);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.comboBox_port);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1098, 698);
            this.splitContainer1.SplitterDistance = 45;
            this.splitContainer1.TabIndex = 1;
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
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.listView1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.textBox_ad);
            this.splitContainer2.Panel2.Controls.Add(this.button_readAD);
            this.splitContainer2.Panel2.Controls.Add(this.button_eepromreset);
            this.splitContainer2.Panel2.Controls.Add(this.label8);
            this.splitContainer2.Panel2.Controls.Add(this.textBox_node);
            this.splitContainer2.Panel2.Controls.Add(this.button_sendsdo);
            this.splitContainer2.Panel2.Controls.Add(this.label7);
            this.splitContainer2.Panel2.Controls.Add(this.comboBox_type);
            this.splitContainer2.Panel2.Controls.Add(this.label6);
            this.splitContainer2.Panel2.Controls.Add(this.textBox_value);
            this.splitContainer2.Panel2.Controls.Add(this.label5);
            this.splitContainer2.Panel2.Controls.Add(this.textBox_sdosub);
            this.splitContainer2.Panel2.Controls.Add(this.label4);
            this.splitContainer2.Panel2.Controls.Add(this.textBox_sdoindex);
            this.splitContainer2.Panel2.Controls.Add(this.button_clear);
            this.splitContainer2.Panel2.Controls.Add(this.checkbox_autoscroll);
            this.splitContainer2.Size = new System.Drawing.Size(1098, 649);
            this.splitContainer2.SplitterDistance = 694;
            this.splitContainer2.TabIndex = 0;
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
            this.listView1.Size = new System.Drawing.Size(694, 649);
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
            // checkbox_autoscroll
            // 
            this.checkbox_autoscroll.AutoSize = true;
            this.checkbox_autoscroll.Checked = true;
            this.checkbox_autoscroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkbox_autoscroll.Location = new System.Drawing.Point(19, 20);
            this.checkbox_autoscroll.Name = "checkbox_autoscroll";
            this.checkbox_autoscroll.Size = new System.Drawing.Size(72, 17);
            this.checkbox_autoscroll.TabIndex = 0;
            this.checkbox_autoscroll.Text = "Autoscroll";
            this.checkbox_autoscroll.UseVisualStyleBackColor = true;
            // 
            // button_clear
            // 
            this.button_clear.Location = new System.Drawing.Point(97, 14);
            this.button_clear.Name = "button_clear";
            this.button_clear.Size = new System.Drawing.Size(75, 23);
            this.button_clear.TabIndex = 1;
            this.button_clear.Text = "Clear all";
            this.button_clear.UseVisualStyleBackColor = true;
            this.button_clear.Click += new System.EventHandler(this.button_clear_Click);
            // 
            // textBox_sdoindex
            // 
            this.textBox_sdoindex.Location = new System.Drawing.Point(64, 85);
            this.textBox_sdoindex.Name = "textBox_sdoindex";
            this.textBox_sdoindex.Size = new System.Drawing.Size(85, 20);
            this.textBox_sdoindex.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Index";
            // 
            // textBox_sdosub
            // 
            this.textBox_sdosub.Location = new System.Drawing.Point(210, 85);
            this.textBox_sdosub.Name = "textBox_sdosub";
            this.textBox_sdosub.Size = new System.Drawing.Size(33, 20);
            this.textBox_sdosub.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(165, 88);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(26, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Sub";
            // 
            // textBox_value
            // 
            this.textBox_value.Location = new System.Drawing.Point(64, 120);
            this.textBox_value.Name = "textBox_value";
            this.textBox_value.Size = new System.Drawing.Size(85, 20);
            this.textBox_value.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 123);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Value";
            // 
            // comboBox_type
            // 
            this.comboBox_type.FormattingEnabled = true;
            this.comboBox_type.Items.AddRange(new object[] {
            "U8",
            "U16",
            "U32",
            "Float"});
            this.comboBox_type.Location = new System.Drawing.Point(210, 120);
            this.comboBox_type.Name = "comboBox_type";
            this.comboBox_type.Size = new System.Drawing.Size(78, 21);
            this.comboBox_type.TabIndex = 9;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(165, 123);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(31, 13);
            this.label7.TabIndex = 10;
            this.label7.Text = "Type";
            // 
            // button_sendsdo
            // 
            this.button_sendsdo.Location = new System.Drawing.Point(64, 160);
            this.button_sendsdo.Name = "button_sendsdo";
            this.button_sendsdo.Size = new System.Drawing.Size(84, 22);
            this.button_sendsdo.TabIndex = 11;
            this.button_sendsdo.Text = "Send SDO";
            this.button_sendsdo.UseVisualStyleBackColor = true;
            this.button_sendsdo.Click += new System.EventHandler(this.button_sendsdo_Click);
            // 
            // textBox_node
            // 
            this.textBox_node.Location = new System.Drawing.Point(63, 59);
            this.textBox_node.Name = "textBox_node";
            this.textBox_node.Size = new System.Drawing.Size(85, 20);
            this.textBox_node.TabIndex = 12;
            this.textBox_node.Text = "1";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(17, 62);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(33, 13);
            this.label8.TabIndex = 13;
            this.label8.Text = "Node";
            // 
            // button_eepromreset
            // 
            this.button_eepromreset.Location = new System.Drawing.Point(188, 160);
            this.button_eepromreset.Name = "button_eepromreset";
            this.button_eepromreset.Size = new System.Drawing.Size(117, 22);
            this.button_eepromreset.TabIndex = 14;
            this.button_eepromreset.Text = "EEPROM reset";
            this.button_eepromreset.UseVisualStyleBackColor = true;
            this.button_eepromreset.Click += new System.EventHandler(this.button_eepromreset_Click);
            // 
            // button_readAD
            // 
            this.button_readAD.Location = new System.Drawing.Point(63, 212);
            this.button_readAD.Name = "button_readAD";
            this.button_readAD.Size = new System.Drawing.Size(84, 22);
            this.button_readAD.TabIndex = 15;
            this.button_readAD.Text = "read A/D";
            this.button_readAD.UseVisualStyleBackColor = true;
            this.button_readAD.Click += new System.EventHandler(this.button_readAD_Click);
            // 
            // textBox_ad
            // 
            this.textBox_ad.Location = new System.Drawing.Point(188, 212);
            this.textBox_ad.Name = "textBox_ad";
            this.textBox_ad.Size = new System.Drawing.Size(100, 20);
            this.textBox_ad.TabIndex = 16;
            // 
            // button_sdo
            // 
            this.button_sdo.Location = new System.Drawing.Point(642, 12);
            this.button_sdo.Name = "button_sdo";
            this.button_sdo.Size = new System.Drawing.Size(106, 23);
            this.button_sdo.TabIndex = 4;
            this.button_sdo.Text = "SDO";
            this.button_sdo.UseVisualStyleBackColor = true;
            this.button_sdo.Click += new System.EventHandler(this.button_sdo_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1098, 698);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
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
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.CheckBox checkbox_autoscroll;
        private System.Windows.Forms.Button button_clear;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox_node;
        private System.Windows.Forms.Button button_sendsdo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBox_type;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_value;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_sdosub;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_sdoindex;
        private System.Windows.Forms.Button button_eepromreset;
        private System.Windows.Forms.TextBox textBox_ad;
        private System.Windows.Forms.Button button_readAD;
        private System.Windows.Forms.Button button_sdo;
    }
}

