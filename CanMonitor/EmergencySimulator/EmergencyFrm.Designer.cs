namespace EmergencySimulator
{
    partial class EmergencyFrm
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
            this.button_send = new System.Windows.Forms.Button();
            this.numericUpDown_node = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_errorbits = new System.Windows.Forms.ComboBox();
            this.comboBox_errorcode = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown_errorbits = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_errorcode = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_additional = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_msg = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_node)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_errorbits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_errorcode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_additional)).BeginInit();
            this.SuspendLayout();
            // 
            // button_send
            // 
            this.button_send.Location = new System.Drawing.Point(315, 359);
            this.button_send.Name = "button_send";
            this.button_send.Size = new System.Drawing.Size(119, 36);
            this.button_send.TabIndex = 0;
            this.button_send.Text = "Send";
            this.button_send.UseVisualStyleBackColor = true;
            this.button_send.Click += new System.EventHandler(this.button_send_Click);
            // 
            // numericUpDown_node
            // 
            this.numericUpDown_node.Location = new System.Drawing.Point(143, 55);
            this.numericUpDown_node.Name = "numericUpDown_node";
            this.numericUpDown_node.Size = new System.Drawing.Size(92, 22);
            this.numericUpDown_node.TabIndex = 1;
            this.numericUpDown_node.ValueChanged += new System.EventHandler(this.numericUpDown_node_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Node";
            // 
            // comboBox_errorbits
            // 
            this.comboBox_errorbits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_errorbits.FormattingEnabled = true;
            this.comboBox_errorbits.Location = new System.Drawing.Point(143, 112);
            this.comboBox_errorbits.Name = "comboBox_errorbits";
            this.comboBox_errorbits.Size = new System.Drawing.Size(645, 24);
            this.comboBox_errorbits.TabIndex = 3;
            this.comboBox_errorbits.SelectedIndexChanged += new System.EventHandler(this.comboBox_errorbits_SelectedIndexChanged);
            // 
            // comboBox_errorcode
            // 
            this.comboBox_errorcode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_errorcode.FormattingEnabled = true;
            this.comboBox_errorcode.Location = new System.Drawing.Point(143, 174);
            this.comboBox_errorcode.Name = "comboBox_errorcode";
            this.comboBox_errorcode.Size = new System.Drawing.Size(645, 24);
            this.comboBox_errorcode.TabIndex = 4;
            this.comboBox_errorcode.SelectedIndexChanged += new System.EventHandler(this.comboBox_errorcode_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(47, 119);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Error Bits";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(47, 177);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Error Code";
            // 
            // numericUpDown_errorbits
            // 
            this.numericUpDown_errorbits.Hexadecimal = true;
            this.numericUpDown_errorbits.Location = new System.Drawing.Point(143, 142);
            this.numericUpDown_errorbits.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDown_errorbits.Name = "numericUpDown_errorbits";
            this.numericUpDown_errorbits.Size = new System.Drawing.Size(204, 22);
            this.numericUpDown_errorbits.TabIndex = 7;
            this.numericUpDown_errorbits.ValueChanged += new System.EventHandler(this.numericUpDown_errorbits_ValueChanged);
            // 
            // numericUpDown_errorcode
            // 
            this.numericUpDown_errorcode.Hexadecimal = true;
            this.numericUpDown_errorcode.Location = new System.Drawing.Point(143, 204);
            this.numericUpDown_errorcode.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDown_errorcode.Name = "numericUpDown_errorcode";
            this.numericUpDown_errorcode.Size = new System.Drawing.Size(204, 22);
            this.numericUpDown_errorcode.TabIndex = 8;
            this.numericUpDown_errorcode.ValueChanged += new System.EventHandler(this.numericUpDown_errorcode_ValueChanged);
            // 
            // numericUpDown_additional
            // 
            this.numericUpDown_additional.Location = new System.Drawing.Point(143, 259);
            this.numericUpDown_additional.Name = "numericUpDown_additional";
            this.numericUpDown_additional.Size = new System.Drawing.Size(204, 22);
            this.numericUpDown_additional.TabIndex = 9;
            this.numericUpDown_additional.ValueChanged += new System.EventHandler(this.numericUpDown_additional_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(47, 264);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 17);
            this.label4.TabIndex = 10;
            this.label4.Text = "Additional";
            // 
            // textBox_msg
            // 
            this.textBox_msg.Location = new System.Drawing.Point(81, 309);
            this.textBox_msg.Name = "textBox_msg";
            this.textBox_msg.ReadOnly = true;
            this.textBox_msg.Size = new System.Drawing.Size(638, 22);
            this.textBox_msg.TabIndex = 11;
            // 
            // EmergencyFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBox_msg);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numericUpDown_additional);
            this.Controls.Add(this.numericUpDown_errorcode);
            this.Controls.Add(this.numericUpDown_errorbits);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_errorcode);
            this.Controls.Add(this.comboBox_errorbits);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown_node);
            this.Controls.Add(this.button_send);
            this.Name = "EmergencyFrm";
            this.Text = "EmergencyFrm";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_node)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_errorbits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_errorcode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_additional)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_send;
        private System.Windows.Forms.NumericUpDown numericUpDown_node;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_errorbits;
        private System.Windows.Forms.ComboBox comboBox_errorcode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDown_errorbits;
        private System.Windows.Forms.NumericUpDown numericUpDown_errorcode;
        private System.Windows.Forms.NumericUpDown numericUpDown_additional;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_msg;
    }
}