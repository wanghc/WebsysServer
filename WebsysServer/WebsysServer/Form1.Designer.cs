namespace WebsysServer
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.msg = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbLogLevel = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbPort = new System.Windows.Forms.TextBox();
            this.lbUrl = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MgrToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hTTPSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hTTP界面ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StartupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ShutDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(140, 333);
            this.btnStart.Margin = new System.Windows.Forms.Padding(4);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(130, 44);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "启用";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(300, 333);
            this.btnStop.Margin = new System.Windows.Forms.Padding(4);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(122, 44);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(129, 446);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(260, 18);
            this.label2.TabIndex = 4;
            this.label2.Text = "东华医为客户端管理软件 1.0.0";
            // 
            // msg
            // 
            this.msg.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.msg.AutoSize = true;
            this.msg.Location = new System.Drawing.Point(488, 446);
            this.msg.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.msg.Name = "msg";
            this.msg.Size = new System.Drawing.Size(0, 18);
            this.msg.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 96);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 18);
            this.label1.TabIndex = 9;
            this.label1.Text = "日志级别";
            // 
            // cbLogLevel
            // 
            this.cbLogLevel.FormattingEnabled = true;
            this.cbLogLevel.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.cbLogLevel.Location = new System.Drawing.Point(132, 93);
            this.cbLogLevel.Name = "cbLogLevel";
            this.cbLogLevel.Size = new System.Drawing.Size(112, 26);
            this.cbLogLevel.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 161);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 18);
            this.label4.TabIndex = 11;
            this.label4.Text = "监听端口";
            // 
            // tbPort
            // 
            this.tbPort.Enabled = false;
            this.tbPort.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.tbPort.Location = new System.Drawing.Point(132, 158);
            this.tbPort.Name = "tbPort";
            this.tbPort.Size = new System.Drawing.Size(112, 28);
            this.tbPort.TabIndex = 12;
            // 
            // lbUrl
            // 
            this.lbUrl.AutoSize = true;
            this.lbUrl.Location = new System.Drawing.Point(26, 53);
            this.lbUrl.Name = "lbUrl";
            this.lbUrl.Size = new System.Drawing.Size(80, 18);
            this.lbUrl.TabIndex = 13;
            this.lbUrl.Text = "监听路径";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipText = "医为科技客户端应用";
            this.notifyIcon1.BalloonTipTitle = "医为客户端管理";
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "医为客户端管理";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.contextMenuStrip1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MgrToolStripMenuItem,
            this.StartupToolStripMenuItem,
            this.ShutDownToolStripMenuItem,
            this.ExitToolStripMenuItem,
            this.AboutToolStripMenuItem1});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(241, 197);
            // 
            // MgrToolStripMenuItem
            // 
            this.MgrToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hTTPSToolStripMenuItem,
            this.hTTP界面ToolStripMenuItem});
            this.MgrToolStripMenuItem.Name = "MgrToolStripMenuItem";
            this.MgrToolStripMenuItem.Size = new System.Drawing.Size(240, 32);
            this.MgrToolStripMenuItem.Text = "管理";
            this.MgrToolStripMenuItem.Click += new System.EventHandler(this.MgrToolStripMenuItem_Click);
            // 
            // hTTPSToolStripMenuItem
            // 
            this.hTTPSToolStripMenuItem.Name = "hTTPSToolStripMenuItem";
            this.hTTPSToolStripMenuItem.Size = new System.Drawing.Size(252, 32);
            this.hTTPSToolStripMenuItem.Text = "HTTPS界面";
            this.hTTPSToolStripMenuItem.Click += new System.EventHandler(this.hTTPSToolStripMenuItem_Click);
            // 
            // hTTP界面ToolStripMenuItem
            // 
            this.hTTP界面ToolStripMenuItem.Name = "hTTP界面ToolStripMenuItem";
            this.hTTP界面ToolStripMenuItem.Size = new System.Drawing.Size(252, 32);
            this.hTTP界面ToolStripMenuItem.Text = "HTTP界面";
            this.hTTP界面ToolStripMenuItem.Click += new System.EventHandler(this.hTTP界面ToolStripMenuItem_Click);
            // 
            // StartupToolStripMenuItem
            // 
            this.StartupToolStripMenuItem.Name = "StartupToolStripMenuItem";
            this.StartupToolStripMenuItem.Size = new System.Drawing.Size(240, 32);
            this.StartupToolStripMenuItem.Text = "启用";
            this.StartupToolStripMenuItem.Click += new System.EventHandler(this.StartupToolStripMenuItem_Click);
            // 
            // ShutDownToolStripMenuItem
            // 
            this.ShutDownToolStripMenuItem.Name = "ShutDownToolStripMenuItem";
            this.ShutDownToolStripMenuItem.Size = new System.Drawing.Size(240, 32);
            this.ShutDownToolStripMenuItem.Text = "停止";
            this.ShutDownToolStripMenuItem.Click += new System.EventHandler(this.ShutDownToolStripMenuItem_Click);
            // 
            // ExitToolStripMenuItem
            // 
            this.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem";
            this.ExitToolStripMenuItem.Size = new System.Drawing.Size(240, 32);
            this.ExitToolStripMenuItem.Text = "退出";
            this.ExitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // AboutToolStripMenuItem1
            // 
            this.AboutToolStripMenuItem1.Name = "AboutToolStripMenuItem1";
            this.AboutToolStripMenuItem1.Size = new System.Drawing.Size(240, 32);
            this.AboutToolStripMenuItem1.Text = "关于";
            this.AboutToolStripMenuItem1.Click += new System.EventHandler(this.AboutToolStripMenuItem1_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(429, 333);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(89, 44);
            this.button1.TabIndex = 1;
            this.button1.Text = "Test";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 477);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lbUrl);
            this.Controls.Add(this.tbPort);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbLogLevel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.msg);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "医为客户端管理";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label msg;
        //private System.Windows.Forms.ToolStripMenuItem 服务信息ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 本地控件信息ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 帮助ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 帮助ToolStripMenuItem1;
        //private System.Windows.Forms.ToolStripMenuItem 关于ToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbLogLevel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbPort;
        private System.Windows.Forms.Label lbUrl;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ExitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem StartupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ShutDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MgrToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolStripMenuItem hTTPSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hTTP界面ToolStripMenuItem;
    }
}

