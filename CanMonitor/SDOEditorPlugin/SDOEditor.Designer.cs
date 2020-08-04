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
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_edsfilename
            // 
            this.textBox_edsfilename.Location = new System.Drawing.Point(75, 9);
            this.textBox_edsfilename.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_edsfilename.Name = "textBox_edsfilename";
            this.textBox_edsfilename.Size = new System.Drawing.Size(333, 22);
            this.textBox_edsfilename.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Device";
            // 
            // numericUpDown_node
            // 
            this.numericUpDown_node.Hexadecimal = true;
            this.numericUpDown_node.Location = new System.Drawing.Point(75, 44);
            this.numericUpDown_node.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDown_node.Name = "numericUpDown_node";
            this.numericUpDown_node.Size = new System.Drawing.Size(124, 22);
            this.numericUpDown_node.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 47);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 17);
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
            this.listView1.Location = new System.Drawing.Point(4, 104);
            this.listView1.Margin = new System.Windows.Forms.Padding(4);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1432, 410);
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
            this.button_read.Location = new System.Drawing.Point(228, 41);
            this.button_read.Margin = new System.Windows.Forms.Padding(4);
            this.button_read.Name = "button_read";
            this.button_read.Size = new System.Drawing.Size(119, 28);
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 28);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.36937F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80.63063F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1440, 518);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // panel1
            // 
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
            this.panel1.Location = new System.Drawing.Point(4, 4);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1432, 92);
            this.panel1.TabIndex = 6;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // button_writeDCF
            // 
            this.button_writeDCF.Enabled = false;
            this.button_writeDCF.Location = new System.Drawing.Point(621, 44);
            this.button_writeDCF.Margin = new System.Windows.Forms.Padding(4);
            this.button_writeDCF.Name = "button_writeDCF";
            this.button_writeDCF.Size = new System.Drawing.Size(129, 28);
            this.button_writeDCF.TabIndex = 13;
            this.button_writeDCF.Text = "WriteDCF";
            this.button_writeDCF.UseVisualStyleBackColor = true;
            this.button_writeDCF.Click += new System.EventHandler(this.button_writeDCF_Click);
            // 
            // button_flush_queue
            // 
            this.button_flush_queue.Location = new System.Drawing.Point(621, 4);
            this.button_flush_queue.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_flush_queue.Name = "button_flush_queue";
            this.button_flush_queue.Size = new System.Drawing.Size(129, 32);
            this.button_flush_queue.TabIndex = 12;
            this.button_flush_queue.Text = "Flush SDO queue";
            this.button_flush_queue.UseVisualStyleBackColor = true;
            this.button_flush_queue.Click += new System.EventHandler(this.button_flush_queue_Click);
            // 
            // label_sdo_queue_size
            // 
            this.label_sdo_queue_size.AutoSize = true;
            this.label_sdo_queue_size.Location = new System.Drawing.Point(435, 12);
            this.label_sdo_queue_size.Name = "label_sdo_queue_size";
            this.label_sdo_queue_size.Size = new System.Drawing.Size(132, 17);
            this.label_sdo_queue_size.TabIndex = 11;
            this.label_sdo_queue_size.Text = "SDO Queue Size: 0";
            // 
            // checkBox_useronly
            // 
            this.checkBox_useronly.AutoSize = true;
            this.checkBox_useronly.Checked = true;
            this.checkBox_useronly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_useronly.Location = new System.Drawing.Point(964, 44);
            this.checkBox_useronly.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_useronly.Name = "checkBox_useronly";
            this.checkBox_useronly.Size = new System.Drawing.Size(190, 21);
            this.checkBox_useronly.TabIndex = 10;
            this.checkBox_useronly.Text = "Manufacture specific only";
            this.checkBox_useronly.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(964, 6);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(235, 28);
            this.button1.TabIndex = 9;
            this.button1.Text = "Send save req via SDO 0x1010";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(368, 47);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 17);
            this.label3.TabIndex = 8;
            this.label3.Text = "Type";
            // 
            // comboBoxtype
            // 
            this.comboBoxtype.FormattingEnabled = true;
            this.comboBoxtype.Items.AddRange(new object[] {
            "EEPROM",
            "RAM",
            "ROM",
            "ALL"});
            this.comboBoxtype.Location = new System.Drawing.Point(417, 43);
            this.comboBoxtype.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxtype.Name = "comboBoxtype";
            this.comboBoxtype.Size = new System.Drawing.Size(189, 24);
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
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1440, 28);
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
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadEDSXMLToolStripMenuItem
            // 
            this.loadEDSXMLToolStripMenuItem.Name = "loadEDSXMLToolStripMenuItem";
            this.loadEDSXMLToolStripMenuItem.Size = new System.Drawing.Size(365, 26);
            this.loadEDSXMLToolStripMenuItem.Text = "Load Datasheet/Device file XDD/EDS/DCF";
            this.loadEDSXMLToolStripMenuItem.Click += new System.EventHandler(this.loadEDSXMLToolStripMenuItem_Click);
            // 
            // saveDifferenceToolStripMenuItem
            // 
            this.saveDifferenceToolStripMenuItem.Name = "saveDifferenceToolStripMenuItem";
            this.saveDifferenceToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.saveDifferenceToolStripMenuItem.Text = "Save difference";
            this.saveDifferenceToolStripMenuItem.Click += new System.EventHandler(this.saveDifferenceToolStripMenuItem_Click);
            // 
            // mnuRecentlyUsed
            // 
            this.mnuRecentlyUsed.Name = "mnuRecentlyUsed";
            this.mnuRecentlyUsed.Size = new System.Drawing.Size(216, 26);
            this.mnuRecentlyUsed.Text = "Recent";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(213, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.closeToolStripMenuItem.Text = "Close";
            // 
            // SDOEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1440, 546);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SDOEditor";
            this.Text = "Object dictionary browser";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SDOEditor_FormClosed);
            this.Load += new System.EventHandler(this.SDOEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_node)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
    }
}