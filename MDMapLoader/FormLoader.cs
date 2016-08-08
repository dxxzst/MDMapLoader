using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace MDMapLoader
{
    //为了使网页能够与winform交互 将com的可访问性设置为真
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisible(true)]
    public partial class MDMapLoader : Form
    {
        public MDMapLoader()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webBrowser1.ObjectForScripting = this;
            string path = Application.StartupPath + @"\MapHtml\demo.html";
            webBrowser1.Url = new System.Uri(path, System.UriKind.RelativeOrAbsolute);
        }


        #region 瓦片文件下载
        // 点击下载
        private void button1_Click(object sender, EventArgs e)
        {
            if (numericUpDown1.Value > numericUpDown2.Value)
            {
                MessageBox.Show("等级设置错误", "提示");
            }
            webBrowser1.Document.InvokeScript("findtiles", new object[] {numericUpDown1.Value, numericUpDown2.Value});
        }

        //调用js 获取Tiles
        public void ShowMessage(string message)
        {
            var products = JsonConvert.DeserializeObject<List<MapLink>>(message);
            DownloadMapTiles(products);
        }

        //开始下载文件
        private int DownloadCount = 0;
        private void DownloadMapTiles(List<MapLink> tempTiles)
        {
            var SelectedPath = textBox1.Text + "\\";
            for (var i = 0; i < tempTiles.Count; ++i)
            {
                var DownloadPath = SelectedPath + tempTiles[i].path;
                var FileName = tempTiles[i].filename;
                var reqestUrl = tempTiles[i].url;

                try
                {
                    if (!Directory.Exists(DownloadPath))
                    {
                        Directory.CreateDirectory(DownloadPath);
                    }

                    using (FileStream fileStream = new FileStream(DownloadPath + FileName, FileMode.Create, FileAccess.Write))
                    {
                        //创建请求
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(reqestUrl);
                        //接收响应
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        //输出流
                        Stream responseStream = response.GetResponseStream();
                        byte[] bufferBytes = new byte[10000];//缓冲字节数组
                        int bytesRead = -1;
                        while ((bytesRead = responseStream.Read(bufferBytes, 0, bufferBytes.Length)) > 0)
                        {
                            fileStream.Write(bufferBytes, 0, bytesRead);
                        }
                        //关闭写入
                        fileStream.Flush();
                        fileStream.Close();
                    }

                }
                catch (Exception exp)
                {
                    //返回错误消息
                    Console.Write(exp.Message);
                }

                toolStripProgressBar1.Value = i;
            }

            toolStripProgressBar1.Value = 0;

            DownloadCount++;
            if (DownloadCount == (numericUpDown2.Value - numericUpDown1.Value))
            {
                DownloadCount = 0;
                MessageBox.Show("下载完成", "提示");
            }
        }
        #endregion

        #region js交互
        //获取Point经纬度
        public void ShowMousePoint(string message)
        {
            toolStripStatusLabel1.Text = message;
        }

        //是否显示网格线
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                webBrowser1.Document.InvokeScript("addOrRemoveLayerLine", new object[] { "YES" });
            }
           else
            {
                webBrowser1.Document.InvokeScript("addOrRemoveLayerLine", new object[] { "NO" });
            }
        }
        #endregion

        #region 选择存储位置
        //选择存储位置
        private void textBox1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            this.textBox1.Text = path.SelectedPath;
        }
        #endregion
    }
}
