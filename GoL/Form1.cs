using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using OpenCL;

namespace GoL
{
    public partial class Form1 : Form
    {
        static int w = 378; //1920;//
        static int h = 189; //1080;//
        double[] a = new double[w * h];
        double[] a2 = new double[w * h];
        Board b = new Board(w, h);
        public int framecounter = 0;
        public double fps = 0;


        public OpenCLTemplate.CLCalc.Program.Kernel k1;
        public OpenCLTemplate.CLCalc.Program.Kernel k2;
        public OpenCLTemplate.CLCalc.Program.Kernel k3;
        public Form1()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
            b.InitializeOpenCL();
           
            Console.WriteLine("Form1 Started");
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
           
        }
        
       

        private void InitializeBackgroundWorker()
        {
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);

            

        }




        //Random Noise button
        private void btnClickThis_Click(object sender, EventArgs e)
        {
            
            lblHelloWorld.Text = "Hello World!";
            b.RandomNoise();
            label1.Text = b.err1;
            label2.Text = b.err2;
            b.BuildImage();
            pictureBox1.Image = b.image; ;
            

        }

        //Play Button
        private void button1_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                lblHelloWorld.Text = "Starting";
                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync();
                button1.Enabled = false;
                StopButton.Enabled = true;
            }

        }

        //Speed Scroller
        private void speedBar_Scroll(object sender, EventArgs e)
        {
            b.speed = speedBar.Value;
        }

        //Stop Button
        private void StopButton_Click(object sender, EventArgs e)
        {
            // Cancel the asynchronous operation.
            this.backgroundWorker1.CancelAsync();

            // Disable the Cancel button.
            StopButton.Enabled = false;
            button1.Enabled = true;
        }






        // This event handler does the work.
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            
            BackgroundWorker worker = sender as BackgroundWorker;
            //b.InitGPU();
            for (int p = 1; p <= 10000; p++)
            {
                Stopwatch w = new Stopwatch();
                w.Start();
                if (worker.CancellationPending == true)
                {
                    Console.WriteLine("Cancel fired");
                    e.Cancel = true;
                    break;
                }
                else
                {
                    
                    
                        //b.BuildNextGen();
                        b.BuildNextGenGPU();
                        b.BuildImage();
                        e.Result = b;
                        worker.ReportProgress(p);
                   
                }
                Thread.Sleep(b.speed * 50);
                fps = w.Elapsed.TotalSeconds;
            }
            //b.GpuCleanUp();

        }

        // This event handler updates the progress.
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            Console.WriteLine("Progress fired");
            lblHelloWorld.Text = ("Gen" + e.ProgressPercentage.ToString());
            pictureBox1.Image = b.image;
            var n = (int)(1 / fps);
            fps_lable.Text = n.ToString() ;
            label1.Text = "Data In Formatting: " + b.dataInFormatting.ToString();
            label2.Text = "GpuCalc: " + b.gpucalc.ToString();
            label3.Text = "Data Out Formatting: " + b.dataOutFormatting.ToString();
            label4.Text = "Image Build: " + b.imageBuild.ToString();
        }

        // This event handler deals with the results of the background operation.
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
           
            if (e.Cancelled == true)
            {
                lblHelloWorld.Text = "Canceled!";
            }
            else if (e.Error != null)
            {
                lblHelloWorld.Text = "Error: " + e.Error.Message;
            }
            else
            {
                lblHelloWorld.Text = "Done!";
                //pictureBox1.Image = Extensions.ArrayToBitmap((double[])e.Result, w, h);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
