namespace CanMonitor
{
    partial class ErrorInject
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
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown_errorbity = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_info = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.button_inject = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDown_code = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_node)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_errorbity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_info)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_code)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDown_node
            // 
            this.numericUpDown_node.Hexadecimal = true;
            this.numericUpDown_node.Location = new System.Drawing.Point(81, 36);
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
            this.numericUpDown_node.Size = new System.Drawing.Size(90, 20);
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
            this.label1.Location = new System.Drawing.Point(25, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Node";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 111);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(19, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Bit";
            // 
            // numericUpDown_errorbity
            // 
            this.numericUpDown_errorbity.Hexadecimal = true;
            this.numericUpDown_errorbity.Location = new System.Drawing.Point(81, 109);
            this.numericUpDown_errorbity.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDown_errorbity.Name = "numericUpDown_errorbity";
            this.numericUpDown_errorbity.Size = new System.Drawing.Size(90, 20);
            this.numericUpDown_errorbity.TabIndex = 3;
            this.numericUpDown_errorbity.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numericUpDown_info
            // 
            this.numericUpDown_info.Hexadecimal = true;
            this.numericUpDown_info.Location = new System.Drawing.Point(81, 135);
            this.numericUpDown_info.Maximum = new decimal(new int[] {
            -1,
            0,
            0,
            0});
            this.numericUpDown_info.Name = "numericUpDown_info";
            this.numericUpDown_info.Size = new System.Drawing.Size(90, 20);
            this.numericUpDown_info.TabIndex = 4;
            this.numericUpDown_info.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 135);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Info";
            // 
            // button_inject
            // 
            this.button_inject.Location = new System.Drawing.Point(92, 161);
            this.button_inject.Name = "button_inject";
            this.button_inject.Size = new System.Drawing.Size(79, 26);
            this.button_inject.TabIndex = 6;
            this.button_inject.Text = "Inject";
            this.button_inject.UseVisualStyleBackColor = true;
            this.button_inject.Click += new System.EventHandler(this.button_inject_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(25, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Code";
            // 
            // numericUpDown_code
            // 
            this.numericUpDown_code.Hexadecimal = true;
            this.numericUpDown_code.Location = new System.Drawing.Point(81, 69);
            this.numericUpDown_code.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDown_code.Name = "numericUpDown_code";
            this.numericUpDown_code.Size = new System.Drawing.Size(90, 20);
            this.numericUpDown_code.TabIndex = 8;
            this.numericUpDown_code.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // ErrorInject
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 311);
            this.Controls.Add(this.numericUpDown_code);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button_inject);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericUpDown_info);
            this.Controls.Add(this.numericUpDown_errorbity);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown_node);
            this.Name = "ErrorInject";
            this.Text = "ErrorInject";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_node)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_errorbity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_info)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_code)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numericUpDown_node;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown_errorbity;
        private System.Windows.Forms.NumericUpDown numericUpDown_info;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_inject;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDown_code;
    }
}