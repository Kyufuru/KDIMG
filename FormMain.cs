using System;
using System.Threading;
using JSysLibrary;

namespace 金蝶中间层镜像
{
    public partial class FormMain : JWindowObj_V2
    {
        /// <summary>
        /// 线程
        /// </summary>
        private readonly Thread logThr;
        /// <summary>
        /// 参数
        /// </summary>
        public static string[] NameList;
        public static string BillType = "全部";
        public static string BillNo = null;
        public static string ProjNo = null;
        public FormMain()
        {
            InitializeComponent();
            logThr = new Thread(Log)
            {
                IsBackground = true,
                Name = "log"
            };
            logThr.Start();
            CB_BillStyle.Items.Add("全部");
            CB_BillStyle.SelectedItem = "全部";
            CB_BillStyle.Items.AddRange(NameList);
        }
        public void Log(object log)
        {
            while (true)
            {
                if (Tb_Log.Text != $"{ClassMain.buffer}")
                {
                    Tb_Log.Text = $"{ClassMain.buffer}";
                    Tb_Log.SelectionStart = Tb_Log.Text.Length;
                    Tb_Log.ScrollToCaret();
                }
            }
        }
        public void Btn_Manual_Click(object sender, EventArgs e)
{
            BillType = $"{CB_BillStyle.SelectedItem}";
            BillNo = TB_BillNo.Text;
            ProjNo = TB_ProjNo.Text;
            ClassMain.isManual = true;
        }
        private void BTN_flush_Click(object sender, EventArgs e)
        {
            ClassMain.buffer.ClrClass();
        }
    }
}
