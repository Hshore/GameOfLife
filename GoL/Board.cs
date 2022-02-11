using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoL
{
    class Board
    {
        public List<Cell> cells = new List<Cell>();
        public int width;
        public int height;
        public int speed = 1;
        public OpenCLTemplate.CLCalc.Program.Kernel k1;
        public double dataInFormatting = 0;
        public double dataOutFormatting = 0;
        public double gpucalc = 0;
        public double imageBuild = 0;
        public string err1 = "";
        public string err2 = "";

        public int gen;
        public Bitmap image;


        public Board(int _w, int _h)
        {
            this.width = _w;
            this.height = _h;
            
            int c = 0;
            for (int i = 0; i < _h; i++)
            {
                for (int j = 0; j < _w; j++)
                {
                    cells.Add(new Cell(c, j, i));
                    c++;
                }
            }
        }

        

        public void RandomNoise()
        {
            foreach (var c in this.cells)
            {
                int rnd = ThreadSafeRandom.Next(0, 2);
                if (rnd == 1)
                {
                    c.alive = true;
                    c.age = 1;
                    c.value = 1;

                }
                else
                {
                    c.alive = false;
                    c.age = 0;
                    c.value = 0;
                }                              
            } 
        }

        public void BuildNextGenGPU()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int[] board = new int[width * height];
            int[] newboard = new int[width * height];
            int[] w = new int[] { width };
            int[] h = new int[] { height };
            Parallel.ForEach(this.cells, (c) =>
            //for (int i = 0; i < board.Length; i++)
            {
                board[c.id] = c.value;
            });
            dataInFormatting = watch.Elapsed.TotalSeconds;
            watch.Restart();
            
            newboard = GpuCalcGen(board, newboard, w, h);
            gpucalc = watch.Elapsed.TotalSeconds;
            watch.Restart();

            this.gen++;

            Parallel.ForEach(this.cells, (c) =>
            //foreach (var c in this.cells)
            {
                c.value = newboard[c.id];
                c.alive = (c.value == 1) ? true : false;
                c.age = (c.alive == true) ? c.age + 1 : 0;
            });

            dataOutFormatting = watch.Elapsed.TotalSeconds;
            watch.Stop();
        }
        public void BuildNextGen()
        {
            Board newGameBoard = new Board(this.width, this.height);
            Parallel.ForEach(this.cells, (cell) =>           
            {
                bool[] neighbors = new bool[8];
                int[] neighborsAddress = new int[8];
                neighborsAddress[0] = (-this.width - 1);
                neighborsAddress[1] = (-this.width);
                neighborsAddress[2] = (-this.width + 1);
                neighborsAddress[3] = (-1);
                neighborsAddress[4] = (+1);
                neighborsAddress[5] = (+this.width - 1);
                neighborsAddress[6] = (+this.width);
                neighborsAddress[7] = (+this.width + 1);

                for (int j = 0; j < neighbors.Length; j++)
                {
                    try
                    {
                        neighbors[j] = this.cells[cell.id + neighborsAddress[j]].alive;
                    }
                    catch (Exception)
                    {
                        neighbors[j] = false;
                        //throw;
                    }
                }


                double total_neighbors = 0;
                for (int k = 0; k < neighbors.Length; k++)
                {
                    if (neighbors[k] == true)
                    {
                        total_neighbors++;
                    }
                    
                }


                if (cell.alive == true) // if cell is alive
                {
                    if (total_neighbors == 2 || total_neighbors == 3)
                    {
                        newGameBoard.cells[cell.id].alive = true; // stays alive
                        newGameBoard.cells[cell.id].age = cell.age + 1;
                    }
                    else
                    {
                        newGameBoard.cells[cell.id].alive = false; // it dies
                        newGameBoard.cells[cell.id].age = 0;
                    }
                }
                if (cell.alive == false) //if cell is dead
                {
                    if (total_neighbors == 3)
                    {
                        newGameBoard.cells[cell.id].alive = true;//cell comes alive
                        newGameBoard.cells[cell.id].age = 1;
                    }
                    else
                    {
                        newGameBoard.cells[cell.id].alive = false;//stays dead
                        newGameBoard.cells[cell.id].age = 0;
                    }
                }

            });

            this.gen++;
            this.cells.Clear(); 
            this.cells = newGameBoard.cells;

        }

        public void InitializeOpenCL()
        {
            string calc = @"
                     __kernel void
                    calcGen(       __global       int * v1,
                                   __global       int * v2,
                                   __global       int * w,
                                   __global       int * h
                                   )
                    {
                        // Vector element index
                        int i = get_global_id(0);
                        int c = 0;
                     if (i >= w[0] +1){ 
                        if (v1[i - w[0] - 1] == 1) { c++; }
                     }
                     if (i >= w[0]) {
                        if (v1[i - w[0]] == 1) { c++; }
                        if (v1[i - w[0] + 1] == 1) { c++; }
                     }
                     
                        

                     if (i < w[0]*h[0] - w[0]) { 
                        if (v1[i + w[0] - 1] == 1) { c++; }
                        if (v1[i + w[0]] == 1) { c++; }
                        if (v1[i + w[0] + 1] == 1) { c++; }
                     }

                     if (i > 0) {
                        if (v1[i - 1] == 1) { c++; }
                     }
                     if (i < w[0]*h[0] - 1) {
                        if (v1[i + 1] == 1) { c++; }
                     }
                        /* if alive */
                
                        if (v1[i] == 1) { 
                            if (c == 2 || c == 3) {
                                v2[i] = 1;
                            } else {
                                v2[i] = 0;
                            }
                        }
                        
                        /* if dead */
                        if (v1[i] == 0) {
                            if (c == 3) {
                                v2[i] = 1;
                            } else {
                                v2[i] = 0;
                            }                      
                        }
                        

                        
                    }";


            //Initializes OpenCL Platforms and Devices and sets everything up
            OpenCLTemplate.CLCalc.InitCL();

            //Compiles the source codes. The source is a string array because the user may want
            //to split the source into many strings.
            OpenCLTemplate.CLCalc.Program.Compile(new string[] { calc });
            //Gets host access to the OpenCL floatVectorSum kernel
            k1 = new OpenCLTemplate.CLCalc.Program.Kernel("calcGen");
            //k2 = new OpenCLTemplate.CLCalc.Program.Kernel("sigmoid");
        }

        int[] GpuCalcGen(int[] v1, int[] v2, int[] w, int[] h)
        {
            // OpenCLTemplate.CLCalc.Program.Kernel VectorSum;
            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            //We want to sum 2000 numbers
            int n = v1.Length;

            //Creates vectors v1 and v2 in the device memory
            OpenCLTemplate.CLCalc.Program.Variable varV1 = new OpenCLTemplate.CLCalc.Program.Variable(v1);
            OpenCLTemplate.CLCalc.Program.Variable varV2 = new OpenCLTemplate.CLCalc.Program.Variable(v2);
            OpenCLTemplate.CLCalc.Program.Variable varV3 = new OpenCLTemplate.CLCalc.Program.Variable(w);
            OpenCLTemplate.CLCalc.Program.Variable varV4 = new OpenCLTemplate.CLCalc.Program.Variable(h);
            //OpenCLTemplate.CLCalc.Program.Image2D varV5 = new OpenCLTemplate.CLCalc.Program.Image2D(image) ;
            //Arguments of VectorSum kernelS
            OpenCLTemplate.CLCalc.Program.MemoryObject[] args = new OpenCLTemplate.CLCalc.Program.MemoryObject[] { varV1, varV2, varV3, varV4};
            //How many workers will there be? We need "n", one for each element
            int[] workers = new int[1] { n };
            //Execute the kernel
            //VectorSum.Execute(args, 1);
            
            k1.Execute(args, workers);
            //Read device memory varV1 to host memory v1
            varV2.ReadFromDeviceTo(v2);
            
            //watch.Stop();
            //time = watch.Elapsed.TotalSeconds.ToString();
            varV1.Dispose();
            varV2.Dispose();
            varV3.Dispose();
            varV4.Dispose();
            //varV4.Dispose();
            return v2;
        }

        public void BuildImage()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Bitmap newFile = new Bitmap(this.width, this.height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            Rectangle rect = new Rectangle(0, 0, newFile.Width, newFile.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                newFile.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                newFile.PixelFormat);
            IntPtr ptr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * newFile.Height ;
            byte[] rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            
            err1 = "RGBarray length: " + rgbValues.Length.ToString();
            err2 = "Stride length: " + Math.Abs(bmpData.Stride);

            Parallel.ForEach(this.cells, (c) =>
            //foreach (var c in this.cells)
            {
                
                int stride = bmpData.Stride;

                int cellh = c.height;
                int cellid = c.id;
                var num = ((stride - (width * 3)) * cellh) + (cellid * 3);
               
                
                if (c.alive)
                {
                    rgbValues[num] = 0;
                    rgbValues[num + 1] = 0;
                    rgbValues[num + 2] = 255;

                    
                }
                else
                {
                    rgbValues[num] = 0;
                    rgbValues[num + 1] = 0;
                    rgbValues[num + 2] = 0;
                }

            });

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            newFile.UnlockBits(bmpData);

            // Draw the modified image.
            Bitmap imgcopy = (Bitmap)newFile.Clone();

            double zoom = 6;
            if (zoom > 1)
            {
                Size ogSize = imgcopy.Size;
                Bitmap zoomed;
                //if (zoomed != null) zoomed.Dispose();

                zoomed = new Bitmap((int)(ogSize.Width * zoom), (int)(ogSize.Height * zoom), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(zoomed))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(imgcopy, new Rectangle(Point.Empty, zoomed.Size));
                }
                this.image = zoomed;
            }
            else
            {
                this.image = imgcopy;
            }
            


            imageBuild = watch.Elapsed.TotalSeconds;
            watch.Stop();
        }


    }
}
