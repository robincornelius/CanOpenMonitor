
namespace GUIComponent
{
    partial class TrafficLight
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox_red = new System.Windows.Forms.PictureBox();
            this.pictureBox_orange = new System.Windows.Forms.PictureBox();
            this.pictureBox_green = new System.Windows.Forms.PictureBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_red)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_orange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_green)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox_red
            // 
            this.pictureBox_red.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox_red.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_red.Image = global::GUIComponents.Properties.Resources.trafficlightredoff;
            this.pictureBox_red.Location = new System.Drawing.Point(0, 0);
            this.pictureBox_red.Name = "pictureBox_red";
            this.pictureBox_red.Size = new System.Drawing.Size(331, 322);
            this.pictureBox_red.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_red.TabIndex = 0;
            this.pictureBox_red.TabStop = false;
            // 
            // pictureBox_orange
            // 
            this.pictureBox_orange.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox_orange.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_orange.Image = global::GUIComponents.Properties.Resources.trafficlightorangeoff;
            this.pictureBox_orange.Location = new System.Drawing.Point(0, 0);
            this.pictureBox_orange.Name = "pictureBox_orange";
            this.pictureBox_orange.Size = new System.Drawing.Size(331, 271);
            this.pictureBox_orange.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_orange.TabIndex = 1;
            this.pictureBox_orange.TabStop = false;
            // 
            // pictureBox_green
            // 
            this.pictureBox_green.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox_green.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_green.Image = global::GUIComponents.Properties.Resources.trafficlightgreenoff;
            this.pictureBox_green.Location = new System.Drawing.Point(0, 0);
            this.pictureBox_green.Name = "pictureBox_green";
            this.pictureBox_green.Size = new System.Drawing.Size(331, 275);
            this.pictureBox_green.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_green.TabIndex = 2;
            this.pictureBox_green.TabStop = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox_red);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(331, 876);
            this.splitContainer1.SplitterDistance = 322;
            this.splitContainer1.TabIndex = 3;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.pictureBox_orange);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.pictureBox_green);
            this.splitContainer2.Size = new System.Drawing.Size(331, 550);
            this.splitContainer2.SplitterDistance = 271;
            this.splitContainer2.TabIndex = 0;
            // 
            // TrafficLight
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "TrafficLight";
            this.Size = new System.Drawing.Size(331, 876);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_red)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_orange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_green)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox_red;
        private System.Windows.Forms.PictureBox pictureBox_orange;
        private System.Windows.Forms.PictureBox pictureBox_green;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
    }
}
