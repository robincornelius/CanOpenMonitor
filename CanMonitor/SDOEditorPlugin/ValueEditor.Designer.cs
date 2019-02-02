namespace SDOEditorPlugin
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
            this.button2 = new System.Windows.Forms.Button();
            this.buttondown = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 36);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Index";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 68);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "SubIndex";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 98);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Name";
            // 
            // label_index
            // 
            this.label_index.AutoSize = true;
            this.label_index.Location = new System.Drawing.Point(111, 36);
            this.label_index.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_index.Name = "label_index";
            this.label_index.Size = new System.Drawing.Size(79, 17);
            this.label_index.TabIndex = 3;
            this.label_index.Text = "label_index";
            // 
            // label_sub
            // 
            this.label_sub.AutoSize = true;
            this.label_sub.Location = new System.Drawing.Point(111, 68);
            this.label_sub.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_sub.Name = "label_sub";
            this.label_sub.Size = new System.Drawing.Size(102, 17);
            this.label_sub.TabIndex = 4;
            this.label_sub.Text = "label_subindex";
            // 
            // label_name
            // 
            this.label_name.AutoSize = true;
            this.label_name.Location = new System.Drawing.Point(111, 98);
            this.label_name.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_name.Name = "label_name";
            this.label_name.Size = new System.Drawing.Size(102, 17);
            this.label_name.TabIndex = 5;
            this.label_name.Text = "label_subindex";
            // 
            // textBox_desc
            // 
            this.textBox_desc.Location = new System.Drawing.Point(20, 134);
            this.textBox_desc.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_desc.Multiline = true;
            this.textBox_desc.Name = "textBox_desc";
            this.textBox_desc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_desc.Size = new System.Drawing.Size(457, 144);
            this.textBox_desc.TabIndex = 6;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(51, 389);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(77, 61);
            this.button1.TabIndex = 7;
            this.button1.Text = "Update and close";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 303);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 17);
            this.label4.TabIndex = 8;
            this.label4.Text = "Default";
            // 
            // label_default
            // 
            this.label_default.AutoSize = true;
            this.label_default.Location = new System.Drawing.Point(111, 303);
            this.label_default.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_default.Name = "label_default";
            this.label_default.Size = new System.Drawing.Size(53, 17);
            this.label_default.TabIndex = 9;
            this.label_default.Text = "Default";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 331);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 17);
            this.label5.TabIndex = 10;
            this.label5.Text = "Current";
            // 
            // textBox_current
            // 
            this.textBox_current.Location = new System.Drawing.Point(115, 329);
            this.textBox_current.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_current.Name = "textBox_current";
            this.textBox_current.Size = new System.Drawing.Size(296, 22);
            this.textBox_current.TabIndex = 11;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(170, 389);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(77, 61);
            this.button2.TabIndex = 12;
            this.button2.Text = "Update";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // buttondown
            // 
            this.buttondown.Location = new System.Drawing.Point(398, 392);
            this.buttondown.Name = "buttondown";
            this.buttondown.Size = new System.Drawing.Size(75, 23);
            this.buttondown.TabIndex = 13;
            this.buttondown.Text = "button3";
            this.buttondown.UseVisualStyleBackColor = true;
            this.buttondown.Visible = false;
            this.buttondown.Click += new System.EventHandler(this.buttondown_Click);
            // 
            // ValueEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 483);
            this.Controls.Add(this.buttondown);
            this.Controls.Add(this.button2);
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
            this.Margin = new System.Windows.Forms.Padding(4);
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
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button buttondown;
    }
}