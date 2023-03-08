namespace SDOEditorPlugin
{
    partial class SDOEditor
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
            this.textBox_edsfilename = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown_node = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_read = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button_scan = new System.Windows.Forms.Button();
            this.button_addcustom = new System.Windows.Forms.Button();
            this.numericUpDown_refreshtime = new System.Windows.Forms.NumericUpDown();
            this.checkBox_autorefresh = new System.Windows.Forms.CheckBox();
            this.button_writeDCF = new System.Windows.Forms.Button();
            this.button_flush_queue = new System.Windows.Forms.Button();
            this.label_sdo_queue_size = new System.Windows.Forms.Label();
            this.checkBox_useronly = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxtype = new System.Windows.Forms.ComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadEDSXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveDifferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRecentlyUsed = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_node)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_refreshtime)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_edsfilename
            // 
            this.textBox_edsfilename.Location = new System.Drawing.Point(56, 7);
            this.textBox_edsfilename.Name = "textBox_edsfilename";
            this.textBox_edsfilename.Size = new System.Drawing.Size(251, 20);
            this.textBox_edsfilename.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Device";
            // 
            // numericUpDown_node
            // 
            this.numericUpDown_node.Hexadecimal = true;
            this.numericUpDown_node.Location = new System.Drawing.Point(56, 36);
            this.numericUpDown_node.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDown_node.Name = "numericUpDown_node";
            this.numericUpDown_node.Size = new System.Drawing.Size(93, 20);
            this.numericUpDown_node.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Node";
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader6,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader7});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(3, 84);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1074, 333);
            this.listView1.TabIndex = 5;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Index";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Sub";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 251;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Datatype";
            this.columnHeader3.Width = 148;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Default";
            this.columnHeader4.Width = 164;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Current";
            this.columnHeader5.Width = 155;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "DCF";
            // 
            // button_read
            // 
            this.button_read.Location = new System.Drawing.Point(171, 33);
            this.button_read.Name = "button_read";
            this.button_read.Size = new System.Drawing.Size(89, 23);
            this.button_read.TabIndex = 6;
            this.button_read.Text = "Read";
            this.button_read.UseVisualStyleBackColor = true;
            this.button_read.Click += new System.EventHandler(this.button_read_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.listView1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.36937F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80.63063F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1080, 420);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button_scan);
            this.panel1.Controls.Add(this.button_addcustom);
            this.panel1.Controls.Add(this.numericUpDown_refreshtime);
            this.panel1.Controls.Add(this.checkBox_autorefresh);
            this.panel1.Controls.Add(this.button_writeDCF);
            this.panel1.Controls.Add(this.button_flush_queue);
            this.panel1.Controls.Add(this.label_sdo_queue_size);
            this.panel1.Controls.Add(this.checkBox_useronly);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.comboBoxtype);
            this.panel1.Controls.Add(this.numericUpDown_node);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.textBox_edsfilename);
            this.panel1.Controls.Add(this.button_read);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1074, 75);
            this.panel1.TabIndex = 6;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // button_scan
            // 
            this.button_scan.Location = new System.Drawing.Point(466, 53);
            this.button_scan.Name = "button_scan";
            this.button_scan.Size = new System.Drawing.Size(97, 23);
            this.button_scan.TabIndex = 17;
            this.button_scan.Text = "Scan";
            this.button_scan.UseVisualStyleBackColor = true;
            this.button_scan.Click += new System.EventHandler(this.button_scan_Click);
            // 
            // button_addcustom
            // 
            this.button_addcustom.Location = new System.Drawing.Point(591, 12);
            this.button_addcustom.Name = "button_addcustom";
            this.button_addcustom.Size = new System.Drawing.Size(97, 49);
            this.button_addcustom.TabIndex = 16;
            this.button_addcustom.Text = "Add selection to custom";
            this.button_addcustom.UseVisualStyleBackColor = true;
            this.button_addcustom.Click += new System.EventHandler(this.button_addcustom_Click);
            // 
            // numericUpDown_refreshtime
            // 
            this.numericUpDown_refreshtime.Location = new System.Drawing.Point(916, 41);
            this.numericUpDown_refreshtime.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDown_refreshtime.Name = "numericUpDown_refreshtime";
            this.numericUpDown_refreshtime.Size = new System.Drawing.Size(80, 20);
            this.numericUpDown_refreshtime.TabIndex = 15;
            this.numericUpDown_refreshtime.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // checkBox_autorefresh
            // 
            this.checkBox_autorefresh.AutoSize = true;
            this.checkBox_autorefresh.Checked = true;
            this.checkBox_autorefresh.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_autorefresh.Location = new System.Drawing.Point(915, 12);
            this.checkBox_autorefresh.Name = "checkBox_autorefresh";
            this.checkBox_autorefresh.Size = new System.Drawing.Size(83, 17);
            this.checkBox_autorefresh.TabIndex = 14;
            this.checkBox_autorefresh.Text = "Auto refresh";
            this.checkBox_autorefresh.UseVisualStyleBackColor = true;
            this.checkBox_autorefresh.CheckedChanged += new System.EventHandler(this.checkBox_autorefresh_CheckedChanged);
            // 
            // button_writeDCF
            // 
            this.button_writeDCF.Enabled = false;
            this.button_writeDCF.Location = new System.Drawing.Point(466, 30);
            this.button_writeDCF.Name = "button_writeDCF";
            this.button_writeDCF.Size = new System.Drawing.Size(97, 23);
            this.button_writeDCF.TabIndex = 13;
            this.button_writeDCF.Text = "WriteDCF";
            this.button_writeDCF.UseVisualStyleBackColor = true;
            this.button_writeDCF.Click += new System.EventHandler(this.button_writeDCF_Click);
            // 
            // button_flush_queue
            // 
            this.button_flush_queue.Location = new System.Drawing.Point(466, 3);
            this.button_flush_queue.Margin = new System.Windows.Forms.Padding(2);
            this.button_flush_queue.Name = "button_flush_queue";
            this.button_flush_queue.Size = new System.Drawing.Size(97, 26);
            this.button_flush_queue.TabIndex = 12;
            this.button_flush_queue.Text = "Flush SDO queue";
            this.button_flush_queue.UseVisualStyleBackColor = true;
            this.button_flush_queue.Click += new System.EventHandler(this.button_flush_queue_Click);
            // 
            // label_sdo_queue_size
            // 
            this.label_sdo_queue_size.AutoSize = true;
            this.label_sdo_queue_size.Location = new System.Drawing.Point(326, 10);
            this.label_sdo_queue_size.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_sdo_queue_size.Name = "label_sdo_queue_size";
            this.label_sdo_queue_size.Size = new System.Drawing.Size(100, 13);
            this.label_sdo_queue_size.TabIndex = 11;
            this.label_sdo_queue_size.Text = "SDO Queue Size: 0";
            // 
            // checkBox_useronly
            // 
            this.checkBox_useronly.AutoSize = true;
            this.checkBox_useronly.Checked = true;
            this.checkBox_useronly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_useronly.Location = new System.Drawing.Point(723, 36);
            this.checkBox_useronly.Name = "checkBox_useronly";
            this.checkBox_useronly.Size = new System.Drawing.Size(147, 17);
            this.checkBox_useronly.TabIndex = 10;
            this.checkBox_useronly.Text = "Manufacture specific only";
            this.checkBox_useronly.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(723, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(176, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "Send save req via SDO 0x1010";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(276, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Type";
            // 
            // comboBoxtype
            // 
            this.comboBoxtype.FormattingEnabled = true;
            this.comboBoxtype.Items.AddRange(new object[] {
            "EEPROM",
            "PERSIST_COMM",
            "RAM",
            "ROM",
            "ALL"});
            this.comboBoxtype.Location = new System.Drawing.Point(313, 35);
            this.comboBoxtype.Name = "comboBoxtype";
            this.comboBoxtype.Size = new System.Drawing.Size(143, 21);
            this.comboBoxtype.TabIndex = 7;
            this.comboBoxtype.Text = "EEPROM";
            this.comboBoxtype.SelectedIndexChanged += new System.EventHandler(this.comboBoxtype_SelectedIndexChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1080, 24);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadEDSXMLToolStripMenuItem,
            this.saveDifferenceToolStripMenuItem,
            this.mnuRecentlyUsed,
            this.toolStripSeparator1,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadEDSXMLToolStripMenuItem
            // 
            this.loadEDSXMLToolStripMenuItem.Name = "loadEDSXMLToolStripMenuItem";
            this.loadEDSXMLToolStripMenuItem.Size = new System.Drawing.Size(292, 22);
            this.loadEDSXMLToolStripMenuItem.Text = "Load Datasheet/Device file XDD/EDS/DCF";
            this.loadEDSXMLToolStripMenuItem.Click += new System.EventHandler(this.loadEDSXMLToolStripMenuItem_Click);
            // 
            // saveDifferenceToolStripMenuItem
            // 
            this.saveDifferenceToolStripMenuItem.Name = "saveDifferenceToolStripMenuItem";
            this.saveDifferenceToolStripMenuItem.Size = new System.Drawing.Size(292, 22);
            this.saveDifferenceToolStripMenuItem.Text = "Save difference";
            this.saveDifferenceToolStripMenuItem.Click += new System.EventHandler(this.saveDifferenceToolStripMenuItem_Click);
            // 
            // mnuRecentlyUsed
            // 
            this.mnuRecentlyUsed.Name = "mnuRecentlyUsed";
            this.mnuRecentlyUsed.Size = new System.Drawing.Size(292, 22);
            this.mnuRecentlyUsed.Text = "Recent";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(289, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(292, 22);
            this.closeToolStripMenuItem.Text = "Close";
            // 
            // SDOEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1080, 444);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "SDOEditor";
            this.Text = "Device OD Editor";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SDOEditor_FormClosed);
            this.Load += new System.EventHandler(this.SDOEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_node)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_refreshtime)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_edsfilename;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown_node;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.Button button_read;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadEDSXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuRecentlyUsed;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxtype;
        private System.Windows.Forms.ToolStripMenuItem saveDifferenceToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBox_useronly;
        private System.Windows.Forms.Button button_flush_queue;
        private System.Windows.Forms.Label label_sdo_queue_size;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.Button button_writeDCF;
        private System.Windows.Forms.NumericUpDown numericUpDown_refreshtime;
        private System.Windows.Forms.CheckBox checkBox_autorefresh;
        private System.Windows.Forms.Button button_addcustom;
        private System.Windows.Forms.Button button_scan;
    }
}