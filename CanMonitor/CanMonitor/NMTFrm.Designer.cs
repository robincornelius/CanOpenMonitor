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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.richTextBox3 = new System.Windows.Forms.RichTextBox();
            this.richTextBox4 = new System.Windows.Forms.RichTextBox();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_startbus
            // 
            this.button_startbus.Location = new System.Drawing.Point(177, 12);
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
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.richTextBox1);
            this.flowLayoutPanel1.Controls.Add(this.richTextBox2);
            this.flowLayoutPanel1.Controls.Add(this.richTextBox3);
            this.flowLayoutPanel1.Controls.Add(this.richTextBox4);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(223, 13);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(409, 214);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(3, 3);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(170, 23);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // richTextBox2
            // 
            this.richTextBox2.Location = new System.Drawing.Point(179, 3);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.Size = new System.Drawing.Size(170, 23);
            this.richTextBox2.TabIndex = 1;
            this.richTextBox2.Text = "";
            // 
            // richTextBox3
            // 
            this.richTextBox3.Location = new System.Drawing.Point(3, 32);
            this.richTextBox3.Name = "richTextBox3";
            this.richTextBox3.Size = new System.Drawing.Size(170, 23);
            this.richTextBox3.TabIndex = 2;
            this.richTextBox3.Text = "";
            // 
            // richTextBox4
            // 
            this.richTextBox4.Location = new System.Drawing.Point(179, 32);
            this.richTextBox4.Name = "richTextBox4";
            this.richTextBox4.Size = new System.Drawing.Size(170, 23);
            this.richTextBox4.TabIndex = 3;
            this.richTextBox4.Text = "";
            // 
            // NMTFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 261);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.button_startbus);
            this.Controls.Add(this.button_resetnodes);
            this.Controls.Add(this.button_resetcomms);
            this.Controls.Add(this.button_preop);
            this.Controls.Add(this.button_stopbus);
            this.Name = "NMTFrm";
            this.Text = "NMTFrm";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_startbus;
        private System.Windows.Forms.Button button_stopbus;
        private System.Windows.Forms.Button button_preop;
        private System.Windows.Forms.Button button_resetcomms;
        private System.Windows.Forms.Button button_resetnodes;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.RichTextBox richTextBox3;
        private System.Windows.Forms.RichTextBox richTextBox4;
    }
}