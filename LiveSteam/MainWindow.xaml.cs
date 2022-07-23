using AForge.Video.DirectShow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LiveSteam
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitCamera();
        }

        private readonly Dictionary<string, string> _cameraList = new Dictionary<string, string>();

        private async void btnBegin_Click(object sender, RoutedEventArgs e)
        {
            btnBegin.IsEnabled = false;
            try
            {


                string camera = cmbCamera.SelectedValue.ToString();
                string apiUrl = ConfigurationManager.AppSettings["apiUrl"];
                string centerId = ConfigurationManager.AppSettings["centerId"];
                string deviceName = ConfigurationManager.AppSettings["deviceName"];

                string title = $"{new Random().Next(1, 99999)}";
                // 生成推流地址
                string uri = $"rtmp://push.ty01.cn/live/{title}";
                string playUri = $"http://play.ty01.cn/live/{title}.flv";
                string key = "WkyaNXvKgUe1PJx7";
                string playKey = "6oSlY1sECb5A8JcG";

                DateTime dateStart = new DateTime(1970, 1, 1, 8, 0, 0);
                string exp = Convert.ToInt64((DateTime.Now - dateStart).TotalSeconds + 3600).ToString();
                string authUri = aAuth(uri, key, exp);

                string playauthUri = aAuth(playUri, playKey, exp, true);

                string args = $" -thread_queue_size 1000 -r 30 -f dshow -i video=\"{camera}\" -vcodec libx264 -preset:v ultrafast -tune:v zerolatency -max_delay 10 -g 50 -sc_threshold 0 -f flv {authUri}";

                txtMsg.Text = playauthUri;

                StreamModel obj = new StreamModel();
                obj.centerId = centerId;
                obj.monitorUrl = playauthUri;
                obj.deviceName = deviceName;
                var strJson = JsonConvert.SerializeObject(obj);

                var result=await ApiHelper.PostAsyncJson(apiUrl, strJson);

                ResponseResult objResult=JsonConvert.DeserializeObject<ResponseResult>(result);

                txtResponse.Text = objResult.tag == "1" ? $"已添加数据库{objResult.data}" : $"添加失败:{objResult.message}";

                string _ffmpegPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "ffmpeg.exe");
                if (string.IsNullOrEmpty(_ffmpegPath))
                {
                    txtMsg.Text = "ffmpegPath路径为空";
                    btnBegin.IsEnabled = true;
                    return;
                }
                Run(_ffmpegPath, args);
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.Message;
                btnBegin.IsEnabled = true;
                KillProcess("ffmpeg"); 
            }
            
        }

        private void InitCamera()
        {
            GetVideoInDevicesList();
            cmbCamera.ItemsSource = _cameraList;
            cmbCamera.DisplayMemberPath = "Key";
            cmbCamera.SelectedValuePath = "Value";
            cmbCamera.SelectedIndex = 0;
        }

        private Dictionary<string,string> GetVideoInDevicesList()
        {
            int cameraIndex = 0;
            try
            {
                var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count <= 0) { return _cameraList; }

                foreach (FilterInfo item in videoDevices)
                {
                    cameraIndex++;
                    _cameraList.Add(item.Name+'_'+cameraIndex.ToString(),item.Name);
                }
                if (_cameraList.Count == 0)
                {
                    MessageBox.Show("未获取到摄像头");
                }
                return _cameraList;

            }
            catch (Exception)
            {
                return _cameraList;
            }
            finally
            {
            }
        }

        private async void btnOver_Click(object sender, RoutedEventArgs e)
        {
            string apiUrl = ConfigurationManager.AppSettings["apiUrl"];
            string centerId = ConfigurationManager.AppSettings["centerId"];
            string deviceName = ConfigurationManager.AppSettings["deviceName"];
            StreamModel obj = new StreamModel();
            obj.centerId = centerId;
            obj.monitorUrl = "stop";
            obj.deviceName = deviceName;
            var strJson = JsonConvert.SerializeObject(obj);

            var result = await ApiHelper.PostAsyncJson(apiUrl, strJson);

            ResponseResult objResult = JsonConvert.DeserializeObject<ResponseResult>(result);

            txtResponse.Text = objResult.tag == "1" ? $"已从数据库清除{objResult.data}" : $"修改失败:{objResult.message}";
            btnBegin.IsEnabled = true;
            KillProcess("ffmpeg");
            txtMsg.Text = "已停止推流";
        }

        public void Run(string _ffmpegPath, string parameters)
        {
            // 设置启动参数
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = _ffmpegPath;
            startInfo.Arguments = parameters;
            startInfo.UseShellExecute = false;
            //startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardError = true;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;

            using (var proc = Process.Start(startInfo))
            {
                proc.ErrorDataReceived += new DataReceivedEventHandler(Output);
                proc.BeginErrorReadLine();
                proc?.WaitForExit(1000);
            }
        }

        private void Output(object sender, DataReceivedEventArgs output)
        {
            if (!string.IsNullOrEmpty(output.Data) && output.Data.Contains("error"))
            {
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "error.txt");

                if (!File.Exists(filePath))
                {
                    File.Create(filePath);
                }

                WriteLog(filePath, output.Data);
            }
        }

        public static string aAuth(string uri, string key, string exp, bool isFLV = false)
        {
            Regex regex = new Regex("^(rtmp://)?([^/?]+)(/[^?]*)?(\\\\?.*)?$");
            if (isFLV)
            {
                regex = new Regex("^(http://)?([^/?]+)(/[^?]*)?(\\\\?.*)?$");
            }
            Match m = regex.Match(uri);
            string scheme = "rtmp://", host = "", path = "/", args = "";
            if (isFLV)
            {
                scheme = "http://";
            }
            if (m.Success)
            {
                scheme = m.Groups[1].Value;
                host = m.Groups[2].Value;
                path = m.Groups[3].Value;
                args = m.Groups[4].Value;
            }
            else
            {
                Console.WriteLine("NO MATCH");
            }
            string rand = "0";  // "0" by default, other value is ok
            string uid = "0";   // "0" by default, other value is ok
            string u = String.Format("{0}-{1}-{2}-{3}-{4}", path, exp, rand, uid, key);
            string hashValue = Md5(u);
            string authKey = String.Format("{0}-{1}-{2}-{3}", exp, rand, uid, hashValue);
            if (args == "")
            {
                return String.Format("{0}{1}{2}{3}?auth_key={4}", scheme, host, path, args, authKey);
            }
            else
            {
                return String.Format("{0}{1}{2}{3}&auth_key={4}", scheme, host, path, args, authKey);
            }
        }

        public static string Md5(string value)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            byte[] encoded = md5.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < encoded.Length; ++i)
            {
                sb.Append(encoded[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private static void WriteLog(string path, string logData)
        {
            //if (!File.Exists(path))
            //{
            //    FileStream fs1 = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            //    File.SetAttributes(path, FileAttributes.ReadOnly);
            //    StreamWriter sw = new StreamWriter(fs1, Encoding.UTF8);
            //    sw.WriteLine(logData);
            //    sw.Close();
            //    fs1.Close();
            //}
            //else
            //{
            //    new FileInfo(path).Attributes = FileAttributes.Normal;
            //    FileStream fs1 = new FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
            //    StreamWriter sw = new StreamWriter(fs1, Encoding.UTF8);
            //    sw.WriteLine(logData);
            //    sw.Close();
            //    fs1.Close();
            //}
        }

        private void KillProcess(string processName)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(2000);
                }
                catch (Win32Exception ex)
                {
                    //LogInfo.WriteErrorLog(DateTime.Now + "=>" + "Win32Exception。详情：", ex);
                }
                catch (InvalidOperationException)
                {
                    // 已经退出了
                }
                catch (Exception ex)
                {
                    //LogInfo.WriteErrorLog(DateTime.Now + "=>" + "ffmpeg无法结束。详情：", ex);
                }
            }
        }

    }

    public class StreamModel
    {
        public string centerId { get; set; }

        public string monitorUrl { get; set; }
        public string deviceName { get; set; }
    }

    public class ResponseResult
    {
        public string tag { get; set; }

        public string  message { get; set; }

        public string data { get; set; }
    }
}
