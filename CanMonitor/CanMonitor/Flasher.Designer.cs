namespace CanMonitor
{
    partial class Flasher
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
            this.button1 = new System.Windows.Forms.Button();
            this.button_flash = new System.Windows.Forms.Button();
            this.numericUpDown_node = new System.Windows.Forms.NumericUpDown();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_node)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(27, 38);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 45);
            this.button1.TabIndex = 0;
            this.button1.Text = "Load";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button_flash
            // 
            this.button_flash.Location = new System.Drawing.Point(27, 117);
            this.button_flash.Name = "button_flash";
            this.button_flash.Size = new System.Drawing.Size(150, 45);
            this.button_flash.TabIndex = 1;
            this.button_flash.Text = "Flash";
            this.button_flash.UseVisualStyleBackColor = true;
            this.button_flash.Click += new System.EventHandler(this.button_flash_Click);
            // 
            // numericUpDown_node
            // 
            this.numericUpDown_node.Location = new System.Drawing.Point(215, 131);
            this.numericUpDown_node.Name = "numericUpDown_node";
            this.numericUpDown_node.Size = new System.Drawing.Size(63, 20);
            this.numericUpDown_node.TabIndex = 2;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 200);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(617, 167);
            this.textBox1.TabIndex = 3;
            // 
            // Flasher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(656, 379);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.numericUpDown_node);
            this.Controls.Add(this.button_flash);
            this.Controls.Add(this.button1);
            this.Name = "Flasher";
            this.Text = "Flasher";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_node)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button_flash;
        private System.Windows.Forms.NumericUpDown numericUpDown_node;
        private System.Windows.Forms.TextBox textBox1;
    }
}