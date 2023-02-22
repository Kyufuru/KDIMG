
using System;

namespace 金蝶中间层镜像
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!IsHandleCreated)
            {
                this.Close();
            }
        }
        
        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            logThr.Abort();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.Tb_Log = new System.Windows.Forms.TextBox();
            this.Btn_Manual = new System.Windows.Forms.Button();
            this.CB_BillStyle = new System.Windows.Forms.ComboBox();
            this.LBL_BillType = new System.Windows.Forms.Label();
            this.TB_BillNo = new System.Windows.Forms.TextBox();
            this.LBL_BillNo = new System.Windows.Forms.Label();
            this.LBL_ProjNo = new System.Windows.Forms.Label();
            this.TB_ProjNo = new System.Windows.Forms.TextBox();
            this.PNL_Main = new System.Windows.Forms.Panel();
            this.BTN_flush = new System.Windows.Forms.Button();
            this.PNL_Main.SuspendLayout();
            this.SuspendLayout();
            // 
            // Tb_Log
            // 
            this.Tb_Log.BackColor = System.Drawing.Color.Black;
            this.Tb_Log.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Tb_Log.CausesValidation = false;
            resources.ApplyResources(this.Tb_Log, "Tb_Log");
            this.Tb_Log.ForeColor = System.Drawing.Color.White;
            this.Tb_Log.Name = "Tb_Log";
            // 
            // Btn_Manual
            // 
            resources.ApplyResources(this.Btn_Manual, "Btn_Manual");
            this.Btn_Manual.BackColor = System.Drawing.SystemColors.Control;
            this.Btn_Manual.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Btn_Manual.Name = "Btn_Manual";
            this.Btn_Manual.UseVisualStyleBackColor = false;
            this.Btn_Manual.Click += new System.EventHandler(this.Btn_Manual_Click);
            // 
            // CB_BillStyle
            // 
            this.CB_BillStyle.AllowDrop = true;
            this.CB_BillStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.CB_BillStyle, "CB_BillStyle");
            this.CB_BillStyle.FormattingEnabled = true;
            this.CB_BillStyle.Name = "CB_BillStyle";
            // 
            // LBL_BillType
            // 
            resources.ApplyResources(this.LBL_BillType, "LBL_BillType");
            this.LBL_BillType.Name = "LBL_BillType";
            // 
            // TB_BillNo
            // 
            resources.ApplyResources(this.TB_BillNo, "TB_BillNo");
            this.TB_BillNo.Name = "TB_BillNo";
            // 
            // LBL_BillNo
            // 
            resources.ApplyResources(this.LBL_BillNo, "LBL_BillNo");
            this.LBL_BillNo.Name = "LBL_BillNo";
            // 
            // LBL_ProjNo
            // 
            resources.ApplyResources(this.LBL_ProjNo, "LBL_ProjNo");
            this.LBL_ProjNo.Name = "LBL_ProjNo";
            // 
            // TB_ProjNo
            // 
            resources.ApplyResources(this.TB_ProjNo, "TB_ProjNo");
            this.TB_ProjNo.Name = "TB_ProjNo";
            // 
            // PNL_Main
            // 
            this.PNL_Main.CausesValidation = false;
            this.PNL_Main.Controls.Add(this.BTN_flush);
            this.PNL_Main.Controls.Add(this.Tb_Log);
            this.PNL_Main.Controls.Add(this.LBL_BillType);
            this.PNL_Main.Controls.Add(this.LBL_ProjNo);
            this.PNL_Main.Controls.Add(this.TB_ProjNo);
            this.PNL_Main.Controls.Add(this.LBL_BillNo);
            this.PNL_Main.Controls.Add(this.Btn_Manual);
            this.PNL_Main.Controls.Add(this.TB_BillNo);
            this.PNL_Main.Controls.Add(this.CB_BillStyle);
            resources.ApplyResources(this.PNL_Main, "PNL_Main");
            this.PNL_Main.Name = "PNL_Main";
            // 
            // BTN_flush
            // 
            resources.ApplyResources(this.BTN_flush, "BTN_flush");
            this.BTN_flush.BackColor = System.Drawing.SystemColors.Control;
            this.BTN_flush.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.BTN_flush.Name = "BTN_flush";
            this.BTN_flush.UseVisualStyleBackColor = false;
            this.BTN_flush.Click += new System.EventHandler(this.BTN_flush_Click);
            // 
            // FormMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PNL_Main);
            this.Name = "FormMain";
            this.PNL_Main.ResumeLayout(false);
            this.PNL_Main.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button Btn_Manual;
        private System.Windows.Forms.ComboBox CB_BillStyle;
        private System.Windows.Forms.TextBox Tb_Log;
        private System.Windows.Forms.Label LBL_BillType;
        private System.Windows.Forms.TextBox TB_BillNo;
        private System.Windows.Forms.Label LBL_BillNo;
        private System.Windows.Forms.Label LBL_ProjNo;
        private System.Windows.Forms.TextBox TB_ProjNo;
        public System.Windows.Forms.Panel PNL_Main;
        private System.Windows.Forms.Button BTN_flush;
    }
}

