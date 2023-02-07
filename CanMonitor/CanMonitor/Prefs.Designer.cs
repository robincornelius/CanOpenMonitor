namespace CanMonitor
{
    partial class Prefs
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
            this.listView_plugins = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_saveplugins = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_filelogfolder = new System.Windows.Forms.TextBox();
            this.checkBox_filelog = new System.Windows.Forms.CheckBox();
            this.textBox_linelimit = new System.Windows.Forms.TextBox();
            this.checkBox_limitlines = new System.Windows.Forms.CheckBox();
            this.checkBox_autostart = new System.Windows.Forms.CheckBox();
            this.button_close = new System.Windows.Forms.Button();
            this.checkBox_startwithwindows = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_lastpport = new System.Windows.Forms.TextBox();
            this.textBox_rate = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView_plugins
            // 
            this.listView_plugins.CheckBoxes = true;
            this.listView_plugins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView_plugins.GridLines = true;
            this.listView_plugins.HideSelection = false;
            this.listView_plugins.Location = new System.Drawing.Point(11, 28);
            this.listView_plugins.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listView_plugins.Name = "listView_plugins";
            this.listView_plugins.Size = new System.Drawing.Size(478, 217);
            this.listView_plugins.TabIndex = 0;
            this.listView_plugins.UseCompatibleStateImageBehavior = false;
            this.listView_plugins.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Autoload";
            this.columnHeader1.Width = 75;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Plugin";
            this.columnHeader2.Width = 554;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.listView_plugins);
            this.groupBox1.Location = new System.Drawing.Point(16, 11);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(505, 308);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Plugins";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // button_saveplugins
            // 
            this.button_saveplugins.Location = new System.Drawing.Point(382, 515);
            this.button_saveplugins.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_saveplugins.Name = "button_saveplugins";
            this.button_saveplugins.Size = new System.Drawing.Size(122, 30);
            this.button_saveplugins.TabIndex = 1;
            this.button_saveplugins.Text = "Save and close";
            this.button_saveplugins.UseVisualStyleBackColor = true;
            this.button_saveplugins.Click += new System.EventHandler(this.button_saveplugins_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.textBox_rate);
            this.groupBox2.Controls.Add(this.textBox_lastpport);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.checkBox_startwithwindows);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.textBox_filelogfolder);
            this.groupBox2.Controls.Add(this.checkBox_filelog);
            this.groupBox2.Controls.Add(this.textBox_linelimit);
            this.groupBox2.Controls.Add(this.checkBox_limitlines);
            this.groupBox2.Controls.Add(this.checkBox_autostart);
            this.groupBox2.Location = new System.Drawing.Point(20, 337);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Size = new System.Drawing.Size(500, 158);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(290, 49);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Desktop folder";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // textBox_filelogfolder
            // 
            this.textBox_filelogfolder.Location = new System.Drawing.Point(381, 47);
            this.textBox_filelogfolder.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_filelogfolder.Name = "textBox_filelogfolder";
            this.textBox_filelogfolder.Size = new System.Drawing.Size(103, 20);
            this.textBox_filelogfolder.TabIndex = 4;
            this.textBox_filelogfolder.Text = "canmonitorlog";
            this.textBox_filelogfolder.TextChanged += new System.EventHandler(this.textBox_filelogfolder_TextChanged);
            // 
            // checkBox_filelog
            // 
            this.checkBox_filelog.AutoSize = true;
            this.checkBox_filelog.Location = new System.Drawing.Point(293, 17);
            this.checkBox_filelog.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_filelog.Name = "checkBox_filelog";
            this.checkBox_filelog.Size = new System.Drawing.Size(63, 17);
            this.checkBox_filelog.TabIndex = 3;
            this.checkBox_filelog.Text = "File Log";
            this.checkBox_filelog.UseVisualStyleBackColor = true;
            this.checkBox_filelog.CheckedChanged += new System.EventHandler(this.checkBox_filelog_CheckedChanged);
            // 
            // textBox_linelimit
            // 
            this.textBox_linelimit.Location = new System.Drawing.Point(107, 125);
            this.textBox_linelimit.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_linelimit.Name = "textBox_linelimit";
            this.textBox_linelimit.Size = new System.Drawing.Size(103, 20);
            this.textBox_linelimit.TabIndex = 2;
            this.textBox_linelimit.Text = "1000";
            // 
            // checkBox_limitlines
            // 
            this.checkBox_limitlines.AutoSize = true;
            this.checkBox_limitlines.Location = new System.Drawing.Point(18, 127);
            this.checkBox_limitlines.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_limitlines.Name = "checkBox_limitlines";
            this.checkBox_limitlines.Size = new System.Drawing.Size(71, 17);
            this.checkBox_limitlines.TabIndex = 1;
            this.checkBox_limitlines.Text = "Limit lines";
            this.checkBox_limitlines.UseVisualStyleBackColor = true;
            // 
            // checkBox_autostart
            // 
            this.checkBox_autostart.AutoSize = true;
            this.checkBox_autostart.Location = new System.Drawing.Point(17, 28);
            this.checkBox_autostart.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_autostart.Name = "checkBox_autostart";
            this.checkBox_autostart.Size = new System.Drawing.Size(90, 17);
            this.checkBox_autostart.TabIndex = 0;
            this.checkBox_autostart.Text = "Auto connect";
            this.checkBox_autostart.UseVisualStyleBackColor = true;
            // 
            // button_close
            // 
            this.button_close.Location = new System.Drawing.Point(38, 515);
            this.button_close.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(88, 30);
            this.button_close.TabIndex = 3;
            this.button_close.Text = "Close";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // checkBox_startwithwindows
            // 
            this.checkBox_startwithwindows.AutoSize = true;
            this.checkBox_startwithwindows.Location = new System.Drawing.Point(18, 106);
            this.checkBox_startwithwindows.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_startwithwindows.Name = "checkBox_startwithwindows";
            this.checkBox_startwithwindows.Size = new System.Drawing.Size(114, 17);
            this.checkBox_startwithwindows.TabIndex = 6;
            this.checkBox_startwithwindows.Text = "Start with windows";
            this.checkBox_startwithwindows.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 47);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Port";
            // 
            // textBox_lastpport
            // 
            this.textBox_lastpport.Location = new System.Drawing.Point(56, 44);
            this.textBox_lastpport.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_lastpport.Name = "textBox_lastpport";
            this.textBox_lastpport.ReadOnly = true;
            this.textBox_lastpport.Size = new System.Drawing.Size(210, 20);
            this.textBox_lastpport.TabIndex = 8;
            // 
            // textBox_rate
            // 
            this.textBox_rate.Location = new System.Drawing.Point(56, 68);
            this.textBox_rate.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_rate.Name = "textBox_rate";
            this.textBox_rate.ReadOnly = true;
            this.textBox_rate.Size = new System.Drawing.Size(210, 20);
            this.textBox_rate.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 71);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Rate";
            // 
            // Prefs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(532, 555);
            this.Controls.Add(this.button_close);
            this.Controls.Add(this.button_saveplugins);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Prefs";
            this.Text = "Preferences";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView_plugins;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_saveplugins;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBox_autostart;
        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.TextBox textBox_linelimit;
        private System.Windows.Forms.CheckBox checkBox_limitlines;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_filelogfolder;
        private System.Windows.Forms.CheckBox checkBox_filelog;
        private System.Windows.Forms.CheckBox checkBox_startwithwindows;
        private System.Windows.Forms.TextBox textBox_lastpport;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_rate;
    }
}