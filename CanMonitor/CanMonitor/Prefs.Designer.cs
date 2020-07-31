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
            this.textBox_linelimit = new System.Windows.Forms.TextBox();
            this.checkBox_limitlines = new System.Windows.Forms.CheckBox();
            this.checkBox_autostart = new System.Windows.Forms.CheckBox();
            this.button_close = new System.Windows.Forms.Button();
            this.checkBox_filelog = new System.Windows.Forms.CheckBox();
            this.textBox_filelogfolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
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
            this.listView_plugins.Location = new System.Drawing.Point(15, 34);
            this.listView_plugins.Name = "listView_plugins";
            this.listView_plugins.Size = new System.Drawing.Size(636, 266);
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
            this.groupBox1.Location = new System.Drawing.Point(22, 14);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(673, 379);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Plugins";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // button_saveplugins
            // 
            this.button_saveplugins.Location = new System.Drawing.Point(555, 634);
            this.button_saveplugins.Name = "button_saveplugins";
            this.button_saveplugins.Size = new System.Drawing.Size(118, 37);
            this.button_saveplugins.TabIndex = 1;
            this.button_saveplugins.Text = "Save and close";
            this.button_saveplugins.UseVisualStyleBackColor = true;
            this.button_saveplugins.Click += new System.EventHandler(this.button_saveplugins_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.textBox_filelogfolder);
            this.groupBox2.Controls.Add(this.checkBox_filelog);
            this.groupBox2.Controls.Add(this.textBox_linelimit);
            this.groupBox2.Controls.Add(this.checkBox_limitlines);
            this.groupBox2.Controls.Add(this.checkBox_autostart);
            this.groupBox2.Location = new System.Drawing.Point(27, 415);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(667, 194);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General";
            // 
            // textBox_linelimit
            // 
            this.textBox_linelimit.Location = new System.Drawing.Point(141, 69);
            this.textBox_linelimit.Name = "textBox_linelimit";
            this.textBox_linelimit.Size = new System.Drawing.Size(136, 22);
            this.textBox_linelimit.TabIndex = 2;
            this.textBox_linelimit.Text = "1000";
            // 
            // checkBox_limitlines
            // 
            this.checkBox_limitlines.AutoSize = true;
            this.checkBox_limitlines.Location = new System.Drawing.Point(23, 71);
            this.checkBox_limitlines.Name = "checkBox_limitlines";
            this.checkBox_limitlines.Size = new System.Drawing.Size(92, 21);
            this.checkBox_limitlines.TabIndex = 1;
            this.checkBox_limitlines.Text = "Limit lines";
            this.checkBox_limitlines.UseVisualStyleBackColor = true;
            // 
            // checkBox_autostart
            // 
            this.checkBox_autostart.AutoSize = true;
            this.checkBox_autostart.Location = new System.Drawing.Point(23, 34);
            this.checkBox_autostart.Name = "checkBox_autostart";
            this.checkBox_autostart.Size = new System.Drawing.Size(91, 21);
            this.checkBox_autostart.TabIndex = 0;
            this.checkBox_autostart.Text = "Auto start";
            this.checkBox_autostart.UseVisualStyleBackColor = true;
            // 
            // button_close
            // 
            this.button_close.Location = new System.Drawing.Point(50, 634);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(118, 37);
            this.button_close.TabIndex = 3;
            this.button_close.Text = "Close";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // checkBox_filelog
            // 
            this.checkBox_filelog.AutoSize = true;
            this.checkBox_filelog.Location = new System.Drawing.Point(23, 108);
            this.checkBox_filelog.Name = "checkBox_filelog";
            this.checkBox_filelog.Size = new System.Drawing.Size(80, 21);
            this.checkBox_filelog.TabIndex = 3;
            this.checkBox_filelog.Text = "File Log";
            this.checkBox_filelog.UseVisualStyleBackColor = true;
            // 
            // textBox_filelogfolder
            // 
            this.textBox_filelogfolder.Location = new System.Drawing.Point(141, 152);
            this.textBox_filelogfolder.Name = "textBox_filelogfolder";
            this.textBox_filelogfolder.Size = new System.Drawing.Size(136, 22);
            this.textBox_filelogfolder.TabIndex = 4;
            this.textBox_filelogfolder.Text = "canmonitorlog";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 155);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "Desktop folder";
            // 
            // Prefs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(709, 683);
            this.Controls.Add(this.button_close);
            this.Controls.Add(this.button_saveplugins);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
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
    }
}