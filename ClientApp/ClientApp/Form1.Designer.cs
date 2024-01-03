namespace ClientApp
{
	partial class Form1
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
			this.btnOn = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.btnOff = new System.Windows.Forms.Button();
			this.labelStatus = new System.Windows.Forms.Label();
			this.tbAppName = new System.Windows.Forms.TextBox();
			this.btnCreateApp = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.labelAppStatus = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOn
			// 
			this.btnOn.Location = new System.Drawing.Point(44, 42);
			this.btnOn.Name = "btnOn";
			this.btnOn.Size = new System.Drawing.Size(75, 27);
			this.btnOn.TabIndex = 0;
			this.btnOn.Text = "ON";
			this.btnOn.UseVisualStyleBackColor = true;
			this.btnOn.Click += new System.EventHandler(this.btnOn_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(41, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(47, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Status:";
			// 
			// btnOff
			// 
			this.btnOff.Location = new System.Drawing.Point(44, 75);
			this.btnOff.Name = "btnOff";
			this.btnOff.Size = new System.Drawing.Size(75, 27);
			this.btnOff.TabIndex = 2;
			this.btnOff.Text = "OFF";
			this.btnOff.UseVisualStyleBackColor = true;
			this.btnOff.Click += new System.EventHandler(this.btnOff_Click);
			// 
			// labelStatus
			// 
			this.labelStatus.AutoSize = true;
			this.labelStatus.Location = new System.Drawing.Point(94, 23);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(0, 16);
			this.labelStatus.TabIndex = 3;
			// 
			// tbAppName
			// 
			this.tbAppName.Location = new System.Drawing.Point(91, 28);
			this.tbAppName.Name = "tbAppName";
			this.tbAppName.Size = new System.Drawing.Size(220, 22);
			this.tbAppName.TabIndex = 4;
			this.tbAppName.Text = "Switch";
			// 
			// btnCreateApp
			// 
			this.btnCreateApp.Location = new System.Drawing.Point(16, 56);
			this.btnCreateApp.Name = "btnCreateApp";
			this.btnCreateApp.Size = new System.Drawing.Size(91, 30);
			this.btnCreateApp.TabIndex = 5;
			this.btnCreateApp.Text = "Create App";
			this.btnCreateApp.UseVisualStyleBackColor = true;
			this.btnCreateApp.Click += new System.EventHandler(this.btnCreateApp_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 31);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 16);
			this.label2.TabIndex = 6;
			this.label2.Text = "App Name";
			// 
			// labelAppStatus
			// 
			this.labelAppStatus.AutoSize = true;
			this.labelAppStatus.Location = new System.Drawing.Point(113, 63);
			this.labelAppStatus.Name = "labelAppStatus";
			this.labelAppStatus.Size = new System.Drawing.Size(0, 16);
			this.labelAppStatus.TabIndex = 7;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.labelAppStatus);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.btnCreateApp);
			this.groupBox1.Controls.Add(this.tbAppName);
			this.groupBox1.Location = new System.Drawing.Point(26, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(328, 101);
			this.groupBox1.TabIndex = 8;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Application";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.labelStatus);
			this.groupBox2.Controls.Add(this.btnOff);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.btnOn);
			this.groupBox2.Location = new System.Drawing.Point(107, 128);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(162, 113);
			this.groupBox2.TabIndex = 9;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Motor";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(380, 253);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "Form1";
			this.Text = "Client App";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnOn;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnOff;
		private System.Windows.Forms.Label labelStatus;
		private System.Windows.Forms.TextBox tbAppName;
		private System.Windows.Forms.Button btnCreateApp;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label labelAppStatus;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
	}
}

