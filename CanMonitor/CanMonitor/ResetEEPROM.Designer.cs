namespace CanMonitor
{
    partial class ResetEEPROM
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
            this.numericUpDown_node = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button_savetoeeprom = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_node)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDown_node
            // 
            this.numericUpDown_node.Location = new System.Drawing.Point(116, 32);
            this.numericUpDown_node.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDown_node.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_node.Name = "numericUpDown_node";
            this.numericUpDown_node.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown_node.TabIndex = 0;
            this.numericUpDown_node.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Node";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(65, 80);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(126, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Reset EEPROM";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button_savetoeeprom
            // 
            this.button_savetoeeprom.Location = new System.Drawing.Point(65, 148);
            this.button_savetoeeprom.Name = "button_savetoeeprom";
            this.button_savetoeeprom.Size = new System.Drawing.Size(126, 23);
            this.button_savetoeeprom.TabIndex = 3;
            this.button_savetoeeprom.Text = "Save to EEPROM";
            this.button_savetoeeprom.UseVisualStyleBackColor = true;
            this.button_savetoeeprom.Click += new System.EventHandler(this.button_savetoeeprom_Click);
            // 
            // ResetEEPROM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 206);
            this.Controls.Add(this.button_savetoeeprom);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown_node);
            this.Name = "ResetEEPROM";
            this.Text = "ResetEEPROM";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_node)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numericUpDown_node;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button_savetoeeprom;
    }
}