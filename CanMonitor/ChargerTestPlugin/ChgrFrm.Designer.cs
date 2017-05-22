namespace ChargerTestPlugin
{
    partial class ChgrFrm
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
            this.button_charge = new System.Windows.Forms.Button();
            this.button_stop = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // button_charge
            // 
            this.button_charge.Location = new System.Drawing.Point(126, 42);
            this.button_charge.Name = "button_charge";
            this.button_charge.Size = new System.Drawing.Size(75, 23);
            this.button_charge.TabIndex = 0;
            this.button_charge.Text = "Charge";
            this.button_charge.UseVisualStyleBackColor = true;
            this.button_charge.Click += new System.EventHandler(this.button_charge_Click);
            // 
            // button_stop
            // 
            this.button_stop.Location = new System.Drawing.Point(126, 114);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(75, 23);
            this.button_stop.TabIndex = 1;
            this.button_stop.Text = "Stop";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(25, 40);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(87, 20);
            this.numericUpDown1.TabIndex = 2;
            // 
            // ChgrFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_charge);
            this.Name = "ChgrFrm";
            this.Text = "ChgrFrm";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_charge;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
    }
}