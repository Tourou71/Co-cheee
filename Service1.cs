using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Web;
using System.Net.Sockets;

namespace lab1_bai3
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
            CheckConnection();
            ReverseShell();
        }

        private void CheckConnection()
        {
            HttpWebResponse response = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.google.com");
                request.Method = "GET";

                response = (HttpWebResponse)request.GetResponse();

                StreamReader sr = new StreamReader(response.GetResponseStream());
                if ((int)response.StatusCode == 200)
                {
                    WriteToFile("Connected !");
                }
                //WriteToFile("Code: " + (int)response.StatusCode);
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    response = (HttpWebResponse)e.Response;
                    WriteToFile("Out of connection !!!");

                }
                else
                {
                    WriteToFile("Error: " + e.Status);
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }
        protected override void OnStop()
        {

        }

        private void ReverseShell()
        {
            //  Tạo TCP Listener với loopback Port 8080.
            try
            {
                IPAddress ipAddres = IPAddress.Parse("127.0.0.1");
                TcpListener server = new TcpListener(ipAddres, 8080);
                server.Start();
                //  Do Windows Service là 1 chương trình chạy như 1 tiến trình nên cần phải dùng Thread.
                var t = new Thread(() =>
                {
                    while (true)
                    {
                        TcpClient client = server.AcceptTcpClient();

                        var childThread = new Thread(() =>
                        {


                            NetworkStream stream = client.GetStream();
                            //  Nhận dữ liệu nhập vào từ telnet.
                            StreamReader streamreader = new StreamReader(client.GetStream(), Encoding.ASCII);
                            string line = null;
                            while ((line = streamreader.ReadLine()) != "<EOF>")
                            {  
                                WriteToFile(line);
                            }

                            stream.Close();
                            client.Close();
                        });
                        childThread.Start();
                    }
                });
                t.Start();
            }
            catch
            {
                WriteToFile("Error has been found!");
            }
        }

        private void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory +
           "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') +
           ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
