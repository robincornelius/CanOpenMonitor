namespace CanMonitor
{
    partial class ValueEditor
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label_index = new System.Windows.Forms.Label();
            this.label_sub = new System.Windows.Forms.Label();
            this.label_name = new System.Windows.Forms.Label();
            this.textBox_desc = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label_default = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_current = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Index";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "SubIndex";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Name";
            // 
            // label_index
            // 
            this.label_index.AutoSize = true;
            this.label_index.Location = new System.Drawing.Point(83, 29);
            this.label_index.Name = "label_index";
            this.label_index.Size = new System.Drawing.Size(60, 13);
            this.label_index.TabIndex = 3;
            this.label_index.Text = "label_index";
            // 
            // label_sub
            // 
            this.label_sub.AutoSize = true;
            this.label_sub.Location = new System.Drawing.Point(83, 55);
            this.label_sub.Name = "label_sub";
            this.label_sub.Size = new System.Drawing.Size(77, 13);
            this.label_sub.TabIndex = 4;
            this.label_sub.Text = "label_subindex";
            // 
            // label_name
            // 
            this.label_name.AutoSize = true;
            this.label_name.Location = new System.Drawing.Point(83, 80);
            this.label_name.Name = "label_name";
            this.label_name.Size = new System.Drawing.Size(77, 13);
            this.label_name.TabIndex = 5;
            this.label_name.Text = "label_subindex";
            // 
            // textBox_desc
            // 
            this.textBox_desc.Location = new System.Drawing.Point(15, 109);
            this.textBox_desc.Multiline = true;
            this.textBox_desc.Name = "textBox_desc";
            this.textBox_desc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_desc.Size = new System.Drawing.Size(344, 118);
            this.textBox_desc.TabIndex = 6;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(133, 295);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(58, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Update";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 246);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Default";
            // 
            // label_default
            // 
            this.label_default.AutoSize = true;
            this.label_default.Location = new System.Drawing.Point(83, 246);
            this.label_default.Name = "label_default";
            this.label_default.Size = new System.Drawing.Size(41, 13);
            this.label_default.TabIndex = 9;
            this.label_default.Text = "Default";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 269);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Current";
            // 
            // textBox_current
            // 
            this.textBox_current.Location = new System.Drawing.Point(86, 267);
            this.textBox_current.Name = "textBox_current";
            this.textBox_current.Size = new System.Drawing.Size(223, 20);
            this.textBox_current.TabIndex = 11;
            // 
            // ValueEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 330);
            this.Controls.Add(this.textBox_current);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label_default);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox_desc);
            this.Controls.Add(this.label_name);
            this.Controls.Add(this.label_sub);
            this.Controls.Add(this.label_index);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ValueEditor";
            this.Text = "ValueEditor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label_index;
        private System.Windows.Forms.Label label_sub;
        private System.Windows.Forms.Label label_name;
        private System.Windows.Forms.TextBox textBox_desc;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label_default;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_current;
    }
}