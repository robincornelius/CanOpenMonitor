namespace FlashLoader
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
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_node)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(27, 38);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 45);
            this.button1.TabIndex = 0;
            this.button1.Text = "Load Hex";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button_flash
            // 
            this.button_flash.Location = new System.Drawing.Point(27, 117);
            this.button_flash.Name = "button_flash";
            this.button_flash.Size = new System.Drawing.Size(150, 45);
            this.button_flash.TabIndex = 1;
            this.button_flash.Text = "Flash node";
            this.button_flash.UseVisualStyleBackColor = true;
            this.button_flash.Click += new System.EventHandler(this.button_flash_Click);
            // 
            // numericUpDown_node
            // 
            this.numericUpDown_node.Hexadecimal = true;
            this.numericUpDown_node.Location = new System.Drawing.Point(215, 131);
            this.numericUpDown_node.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDown_node.Name = "numericUpDown_node";
            this.numericUpDown_node.Size = new System.Drawing.Size(63, 20);
            this.numericUpDown_node.TabIndex = 2;
            this.numericUpDown_node.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 308);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(617, 167);
            this.textBox1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(215, 115);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Node";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 237);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(617, 23);
            this.progressBar1.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(253, 263);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Flash progress";
            // 
            // Flasher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(656, 487);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label1);
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label2;
    }
}