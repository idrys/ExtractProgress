using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExtractProgress
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {

            Task.Run(() =>

                SevenZipExtractProgress(@"c:\Users\Slawek\Repo\plytki_cersanit_06_2017.exe", @"C:\CADProjekt\CAD Decor Paradyz v. 2.3.0\", onProgres)
            );
        }

        private void onProgres(int uploaded)
        {
            Debug.WriteLine("Postęp: " + uploaded);
            progressBar1.Invoke((MethodInvoker)delegate { progressBar1.Value = (int)uploaded; });
        }

        public bool SevenZipExtractProgress(string pathFile, string folderExtract, Action<int> onProgress)
        {
            Regex REX_SevenZipStatus = new Regex(@"(?<p>[0-9]+)%");

            int EverythingOK = -1;
            string testInfo = string.Empty;
            string path7zip = "x86\\7z.exe";

            if (Environment.Is64BitOperatingSystem)
                path7zip = "x64\\7z.exe";

            
            Process p = new Process();
            p.StartInfo.FileName = path7zip;
            p.StartInfo.Arguments = "e " + "\"" + pathFile + "\"" + " -o\"" + folderExtract + "\"" + " -y -bsp1 -bse1 -bso1";
            p.StartInfo.UseShellExecute = false;    // Nie zbędne do odczytu wartości z wyjścia 7z
            p.StartInfo.RedirectStandardOutput = true;  // Nie zbędne do odczytu wartości z wyjścia 7z

            p.OutputDataReceived += (sender, e) => {    // Odczyt procentów z wyjścia 7z
                Debug.WriteLine("onProgress: " + onProgress.ToString());
                if (onProgress != null)
                {
                    Match m = REX_SevenZipStatus.Match(e.Data ?? "");
                    if (m != null && m.Success)
                    {
                        int procent = int.Parse(m.Groups["p"].Value);
                        //progressBar1.Invoke((MethodInvoker)delegate { progressBar1.Maximum = 100; });
                        Debug.WriteLine("Procenty: " + procent);
                        onProgress(procent); // delegat link do metody                        
                    }
                }
            };

            p.StartInfo.CreateNoWindow = true; // Ukrycie okna

            p.Start();
            p.BeginOutputReadLine(); // Nie zbędne do odczytu wartości z wyjścia 7z
            p.WaitForExit();
            p.Close();

            EverythingOK = testInfo.IndexOf("Everything is Ok");
            return EverythingOK == -1 ? false : true;
        }

    }
}
