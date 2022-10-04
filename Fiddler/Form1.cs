using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Fiddler
{
    
    public partial class Form1 : Form
    {
        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;
        static bool settingsReturn, refreshReturn;


        public WebClient client;
        bool downloadComplete;
        int iFileNameNumber;
        string episodetype;
        

        delegate void UpdateUI();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CertMaker.trustRootCert();
            TextBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+"\\nfaudio\\";
            Fiddler.FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
            Fiddler.FiddlerApplication.Startup(0, Fiddler.FiddlerCoreStartupFlags.Default);
          
        }
        void FiddlerApplication_AfterSessionComplete(Fiddler.Session oSession)
        {
            AlistBox1.Invoke(new UpdateUI(() =>
            {
                AlistBox1.Items.Add(oSession.url);
                
                {
                    

                    {
                        if (oSession.url.Contains("nflxvideo") & oSession.url.Contains("range"))
                        {
                            string SearchWithinThis = oSession.GetResponseBodyAsString();
                            string SearchForThis = "ftypmp42";
                            int FirstCharacter = SearchWithinThis.IndexOf(SearchForThis);

                            {
                                if (FirstCharacter == -1)
                                {
                                }
                                else
                                    FilteredlistBox2.Items.Add("https://" + oSession.url);
                            }
                        }
                        
                    }
                    
                    
                }


            }));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            Fiddler.FiddlerApplication.Shutdown();
            Microsoft.Win32.RegistryKey registry = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            registry.SetValue("ProxyEnable", 0);
            settingsReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            refreshReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

            Fiddler.FiddlerApplication.Shutdown();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label8.Text = FilteredlistBox2.Items.Count + " link yakalandı";
            //label9.Text = iFileNameNumber.ToString();
            int bfr = iFileNameNumber - 1;

            if (iFileNameNumber>0)
            { progressBar2.Value = bfr*100 + progressBar1.Value;
            int bc =progressBar2.Value / Output.Items.Count;
                label9.Text = "%" + bc.ToString();
            }
            
            
        }

        private void yakalanmisgonder_Click(object sender, EventArgs e)
        {
            RichTextBox1.Clear();
            foreach (string item in FilteredlistBox2.Items)
            {
                RichTextBox1.AppendText((item + Environment.NewLine));
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //clearing
            NFLinksListBox.Items.Clear();
            Output.Items.Clear();
            RichTextBox2.Clear();
            NFLinksListBox.Items.AddRange(RichTextBox1.Lines);
            //link puring
            foreach (string link in NFLinksListBox.Items) {

    string SearchWithinThis = link;
    string SearchForThis = "range";
    int FirstCharacter = SearchWithinThis.IndexOf(SearchForThis);
    string SearchWithinThis1 = link;
    string SearchForThis1 = "?o=";
    int FirstCharacter1 = SearchWithinThis1.IndexOf(SearchForThis1);
    try {
        string pure = SearchWithinThis.Remove(FirstCharacter, (FirstCharacter1 - FirstCharacter));
        Output.Items.Add(pure);
    }
    catch (Exception es) {

    }
    
}
            foreach (string item in Output.Items)
            {
                RichTextBox2.AppendText((item + Environment.NewLine));
            }
            //



        }
        
        private void Button2_Click(object sender, EventArgs e)
        {
            //Output.Items.AddRange(RichTextBox2.Lines);
            Button2.Enabled = false;
            yakalanmisgonder.PerformClick();
            Button1.PerformClick();
            downloadComplete = false;
            radioButton1.Checked = false;
            Fiddler.FiddlerApplication.oProxy.Detach();
            progressBar2.Maximum = Output.Items.Count * 100;
            progressBar2.Minimum = 0;
            
            backgroundWorker1.RunWorkerAsync();
           
            
                // Keep UI messages moving, so the form remains 
                // responsive during the asynchronous operation.
                
                
         }



        public void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            downloadComplete = true;
            sw.Reset();

        }








        Stopwatch sw = new Stopwatch();    // The stopwatch which we will be using to calculate the download speed

        
        public void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            
            this.BeginInvoke((MethodInvoker)delegate {
                labelSpeed.Text = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
                double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            sizedownshow.Text = e.BytesReceived / (1024 * 1024) + "MB" + " of " + e.TotalBytesToReceive / (1024 * 1024) + "MB" + " İndirildi";
            progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            perc1.Text = "%" + e.ProgressPercentage.ToString();
            
            
                });
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog1.ShowDialog();
            TextBox1.Text = (FolderBrowserDialog1.SelectedPath + "\\" );
        }



        
        public void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            
            if (this.backgroundWorker1.CancellationPending)
      
            {
                
                e.Cancel = true;
          
               return;
      
            }
            downloadComplete = false;
            long toplamboyut = 0;
            foreach(string linkler in Output.Items)
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(linkler);
                req.Method = "HEAD";
                req.Timeout = 500000;
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                
                long len = resp.ContentLength;
                toplamboyut += len;
                req.Abort();
                
            }

            total.Invoke((MethodInvoker)delegate
            {
                total.Text = Convert.ToString(toplamboyut/(1024*1024)) + " MB";
            });

            {

                
                

                string myFolder = TextBox1.Text; // // folder to save files to.
                //string sFileExtension = null; // // string to get the file extension from web file.
                iFileNameNumber = 0; // // used to save file names as #'s.
                string episodetype;
                string diziname = dizininadi.Text;
                string sezon = "." + sezonunadi.Text;
                string sonek = "." + sonekadi.Text;

                System.IO.Directory.CreateDirectory(myFolder);

                

                foreach (string abcd in Output.Items)
                {

                    //sFileExtension = ".mp4"; // // get file .Extension from web file path of ListBox item.
                    iFileNameNumber += 1; // // increment for FileName.


                    {

                        if (iFileNameNumber < 10)

                            episodetype = "E0";

                        else

                            episodetype = "E";


                    }

                    

                    
                    

                    string kayitdosyasi = myFolder + diziname + sezon + episodetype + iFileNameNumber + sonek + ".mp4";
                    Dowlflnm.Invoke((MethodInvoker)delegate
                    {

                        Dowlflnm.Text = diziname + sezon + episodetype + iFileNameNumber + sonek + ".mp4";
                    });
                    if (radioButton1.Checked == false)
                    { 
                    try
                    {
                        
                        client = new WebClient();
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                        sw.Start();
                        client.DownloadFileAsync(new Uri(abcd), @kayitdosyasi);
                        
                    }
                    catch (Exception eh) { }
                    }
                    while (!downloadComplete)
                    { Application.DoEvents();
                    
                    
                    }
                    
                    downloadComplete = false;
                    
                }


            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            Fiddler.FiddlerApplication.oProxy.Attach();
            try
            {
                client.CancelAsync();
                
            }
            catch (Exception ekk)
            {

            }
            downloadComplete = true;
            Button2.Enabled = true;
            sw.Reset();
            backgroundWorker1.CancelAsync();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            FilteredlistBox2.Items.Clear();
            
        }

        
   
        

        }

        
    }

