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
            this.checkBox_autostart = new System.Windows.Forms.CheckBox();
            this.button_close = new System.Windows.Forms.Button();
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
            this.groupBox2.Controls.Add(this.checkBox_autostart);
            this.groupBox2.Location = new System.Drawing.Point(27, 415);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(667, 166);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General";
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
    }
}