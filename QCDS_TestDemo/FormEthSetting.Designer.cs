namespace QCDS_TestDemo
{
    partial class FormEthSetting
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
            this.btEthSetting = new System.Windows.Forms.Button();
            this._txtIpFirstSegment = new System.Windows.Forms.TextBox();
            this._txtIpFourthSegment = new System.Windows.Forms.TextBox();
            this._txtIpSecondSegment = new System.Windows.Forms.TextBox();
            this._lblHighSpeedPort = new System.Windows.Forms.Label();
            this._txtIpThirdSegment = new System.Windows.Forms.TextBox();
            this._lblIpSeparator3 = new System.Windows.Forms.Label();
            this._lblIpSeparator2 = new System.Windows.Forms.Label();
            this._txtHighSpeedPort = new System.Windows.Forms.TextBox();
            this._lblIpSeparator1 = new System.Windows.Forms.Label();
            this._txtCommandPort = new System.Windows.Forms.TextBox();
            this._lblIpAddress = new System.Windows.Forms.Label();
            this._lblCommandPort = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btEthSetting
            // 
            this.btEthSetting.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btEthSetting.Location = new System.Drawing.Point(44, 120);
            this.btEthSetting.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btEthSetting.Name = "btEthSetting";
            this.btEthSetting.Size = new System.Drawing.Size(85, 26);
            this.btEthSetting.TabIndex = 21;
            this.btEthSetting.Text = "确定";
            this.btEthSetting.UseVisualStyleBackColor = true;
            this.btEthSetting.Click += new System.EventHandler(this.btEthSetting_Click);
            // 
            // _txtIpFirstSegment
            // 
            this._txtIpFirstSegment.Location = new System.Drawing.Point(144, 16);
            this._txtIpFirstSegment.Margin = new System.Windows.Forms.Padding(3, 7, 3, 7);
            this._txtIpFirstSegment.MaxLength = 3;
            this._txtIpFirstSegment.Name = "_txtIpFirstSegment";
            this._txtIpFirstSegment.Size = new System.Drawing.Size(28, 21);
            this._txtIpFirstSegment.TabIndex = 12;
            this._txtIpFirstSegment.Text = "192";
            // 
            // _txtIpFourthSegment
            // 
            this._txtIpFourthSegment.Location = new System.Drawing.Point(260, 16);
            this._txtIpFourthSegment.Margin = new System.Windows.Forms.Padding(3, 7, 3, 7);
            this._txtIpFourthSegment.MaxLength = 3;
            this._txtIpFourthSegment.Name = "_txtIpFourthSegment";
            this._txtIpFourthSegment.Size = new System.Drawing.Size(28, 21);
            this._txtIpFourthSegment.TabIndex = 15;
            this._txtIpFourthSegment.Text = "1";
            // 
            // _txtIpSecondSegment
            // 
            this._txtIpSecondSegment.Location = new System.Drawing.Point(183, 16);
            this._txtIpSecondSegment.Margin = new System.Windows.Forms.Padding(3, 7, 3, 7);
            this._txtIpSecondSegment.MaxLength = 3;
            this._txtIpSecondSegment.Name = "_txtIpSecondSegment";
            this._txtIpSecondSegment.Size = new System.Drawing.Size(28, 21);
            this._txtIpSecondSegment.TabIndex = 13;
            this._txtIpSecondSegment.Text = "168";
            // 
            // _lblHighSpeedPort
            // 
            this._lblHighSpeedPort.AutoSize = true;
            this._lblHighSpeedPort.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._lblHighSpeedPort.Location = new System.Drawing.Point(28, 79);
            this._lblHighSpeedPort.Name = "_lblHighSpeedPort";
            this._lblHighSpeedPort.Size = new System.Drawing.Size(101, 12);
            this._lblHighSpeedPort.TabIndex = 22;
            this._lblHighSpeedPort.Text = "TCP 高速模式端口";
            // 
            // _txtIpThirdSegment
            // 
            this._txtIpThirdSegment.Location = new System.Drawing.Point(221, 16);
            this._txtIpThirdSegment.Margin = new System.Windows.Forms.Padding(3, 7, 3, 7);
            this._txtIpThirdSegment.MaxLength = 3;
            this._txtIpThirdSegment.Name = "_txtIpThirdSegment";
            this._txtIpThirdSegment.Size = new System.Drawing.Size(28, 21);
            this._txtIpThirdSegment.TabIndex = 14;
            this._txtIpThirdSegment.Text = "0";
            // 
            // _lblIpSeparator3
            // 
            this._lblIpSeparator3.AutoSize = true;
            this._lblIpSeparator3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._lblIpSeparator3.Location = new System.Drawing.Point(250, 25);
            this._lblIpSeparator3.Name = "_lblIpSeparator3";
            this._lblIpSeparator3.Size = new System.Drawing.Size(11, 12);
            this._lblIpSeparator3.TabIndex = 18;
            this._lblIpSeparator3.Text = ".";
            // 
            // _lblIpSeparator2
            // 
            this._lblIpSeparator2.AutoSize = true;
            this._lblIpSeparator2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._lblIpSeparator2.Location = new System.Drawing.Point(213, 25);
            this._lblIpSeparator2.Name = "_lblIpSeparator2";
            this._lblIpSeparator2.Size = new System.Drawing.Size(11, 12);
            this._lblIpSeparator2.TabIndex = 17;
            this._lblIpSeparator2.Text = ".";
            // 
            // _txtHighSpeedPort
            // 
            this._txtHighSpeedPort.Location = new System.Drawing.Point(219, 76);
            this._txtHighSpeedPort.Margin = new System.Windows.Forms.Padding(3, 7, 3, 7);
            this._txtHighSpeedPort.MaxLength = 5;
            this._txtHighSpeedPort.Name = "_txtHighSpeedPort";
            this._txtHighSpeedPort.Size = new System.Drawing.Size(68, 21);
            this._txtHighSpeedPort.TabIndex = 23;
            this._txtHighSpeedPort.Text = "24692";
            // 
            // _lblIpSeparator1
            // 
            this._lblIpSeparator1.AutoSize = true;
            this._lblIpSeparator1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._lblIpSeparator1.Location = new System.Drawing.Point(173, 25);
            this._lblIpSeparator1.Name = "_lblIpSeparator1";
            this._lblIpSeparator1.Size = new System.Drawing.Size(11, 12);
            this._lblIpSeparator1.TabIndex = 16;
            this._lblIpSeparator1.Text = ".";
            // 
            // _txtCommandPort
            // 
            this._txtCommandPort.Location = new System.Drawing.Point(220, 49);
            this._txtCommandPort.Margin = new System.Windows.Forms.Padding(3, 7, 3, 7);
            this._txtCommandPort.MaxLength = 5;
            this._txtCommandPort.Name = "_txtCommandPort";
            this._txtCommandPort.Size = new System.Drawing.Size(68, 21);
            this._txtCommandPort.TabIndex = 20;
            this._txtCommandPort.Text = "24691";
            // 
            // _lblIpAddress
            // 
            this._lblIpAddress.AutoSize = true;
            this._lblIpAddress.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._lblIpAddress.Location = new System.Drawing.Point(28, 22);
            this._lblIpAddress.Name = "_lblIpAddress";
            this._lblIpAddress.Size = new System.Drawing.Size(77, 12);
            this._lblIpAddress.TabIndex = 11;
            this._lblIpAddress.Text = "控制器IP地址";
            // 
            // _lblCommandPort
            // 
            this._lblCommandPort.AutoSize = true;
            this._lblCommandPort.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._lblCommandPort.Location = new System.Drawing.Point(28, 51);
            this._lblCommandPort.Name = "_lblCommandPort";
            this._lblCommandPort.Size = new System.Drawing.Size(53, 12);
            this._lblCommandPort.TabIndex = 19;
            this._lblCommandPort.Text = "TCP 端口";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(183, 120);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(85, 26);
            this.button1.TabIndex = 24;
            this.button1.Text = "取消";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // FormEthSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(314, 165);
            this.ControlBox = false;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btEthSetting);
            this.Controls.Add(this._txtIpFirstSegment);
            this.Controls.Add(this._txtIpFourthSegment);
            this.Controls.Add(this._txtIpSecondSegment);
            this.Controls.Add(this._lblHighSpeedPort);
            this.Controls.Add(this._txtIpThirdSegment);
            this.Controls.Add(this._lblIpSeparator3);
            this.Controls.Add(this._lblIpSeparator2);
            this.Controls.Add(this._txtHighSpeedPort);
            this.Controls.Add(this._lblIpSeparator1);
            this.Controls.Add(this._txtCommandPort);
            this.Controls.Add(this._lblIpAddress);
            this.Controls.Add(this._lblCommandPort);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormEthSetting";
            this.Text = "控制器以太网设定";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btEthSetting;
        private System.Windows.Forms.TextBox _txtIpFirstSegment;
        private System.Windows.Forms.TextBox _txtIpFourthSegment;
        private System.Windows.Forms.TextBox _txtIpSecondSegment;
        private System.Windows.Forms.Label _lblHighSpeedPort;
        private System.Windows.Forms.TextBox _txtIpThirdSegment;
        private System.Windows.Forms.Label _lblIpSeparator3;
        private System.Windows.Forms.Label _lblIpSeparator2;
        private System.Windows.Forms.TextBox _txtHighSpeedPort;
        private System.Windows.Forms.Label _lblIpSeparator1;
        private System.Windows.Forms.TextBox _txtCommandPort;
        private System.Windows.Forms.Label _lblIpAddress;
        private System.Windows.Forms.Label _lblCommandPort;
        private System.Windows.Forms.Button button1;
    }
}