namespace CanMonitor
{
    partial class NMTFrm
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
            this.button_startbus = new System.Windows.Forms.Button();
            this.button_stopbus = new System.Windows.Forms.Button();
            this.button_preop = new System.Windows.Forms.Button();
            this.button_resetcomms = new System.Windows.Forms.Button();
            this.button_resetnodes = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // button_startbus
            // 
            this.button_startbus.Location = new System.Drawing.Point(28, 27);
            this.button_startbus.Name = "button_startbus";
            this.button_startbus.Size = new System.Drawing.Size(75, 23);
            this.button_startbus.TabIndex = 0;
            this.button_startbus.Text = "Start bus";
            this.button_startbus.UseVisualStyleBackColor = true;
            this.button_startbus.Click += new System.EventHandler(this.button_startbus_Click);
            // 
            // button_stopbus
            // 
            this.button_stopbus.Location = new System.Drawing.Point(28, 69);
            this.button_stopbus.Name = "button_stopbus";
            this.button_stopbus.Size = new System.Drawing.Size(75, 23);
            this.button_stopbus.TabIndex = 1;
            this.button_stopbus.Text = "Stop bus";
            this.button_stopbus.UseVisualStyleBackColor = true;
            this.button_stopbus.Click += new System.EventHandler(this.button_stopbus_Click);
            // 
            // button_preop
            // 
            this.button_preop.Location = new System.Drawing.Point(28, 108);
            this.button_preop.Name = "button_preop";
            this.button_preop.Size = new System.Drawing.Size(75, 23);
            this.button_preop.TabIndex = 2;
            this.button_preop.Text = "Pre op bus";
            this.button_preop.UseVisualStyleBackColor = true;
            this.button_preop.Click += new System.EventHandler(this.button_preop_Click);
            // 
            // button_resetcomms
            // 
            this.button_resetcomms.Location = new System.Drawing.Point(28, 149);
            this.button_resetcomms.Name = "button_resetcomms";
            this.button_resetcomms.Size = new System.Drawing.Size(134, 23);
            this.button_resetcomms.TabIndex = 3;
            this.button_resetcomms.Text = "Reset Communications";
            this.button_resetcomms.UseVisualStyleBackColor = true;
            this.button_resetcomms.Click += new System.EventHandler(this.button_resetcomms_Click);
            // 
            // button_resetnodes
            // 
            this.button_resetnodes.Location = new System.Drawing.Point(28, 190);
            this.button_resetnodes.Name = "button_resetnodes";
            this.button_resetnodes.Size = new System.Drawing.Size(134, 23);
            this.button_resetnodes.TabIndex = 4;
            this.button_resetnodes.Text = "Reset nodes";
            this.button_resetnodes.UseVisualStyleBackColor = true;
            this.button_resetnodes.Click += new System.EventHandler(this.button_resetnodes_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(288, 26);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(67, 20);
            this.numericUpDown1.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(206, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Node (0 for all)";
            // 
            // NMTFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 261);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.button_startbus);
            this.Controls.Add(this.button_resetnodes);
            this.Controls.Add(this.button_resetcomms);
            this.Controls.Add(this.button_preop);
            this.Controls.Add(this.button_stopbus);
            this.Name = "NMTFrm";
            this.Text = "NMTFrm";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_startbus;
        private System.Windows.Forms.Button button_stopbus;
        private System.Windows.Forms.Button button_preop;
        private System.Windows.Forms.Button button_resetcomms;
        private System.Windows.Forms.Button button_resetnodes;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label1;
    }
}