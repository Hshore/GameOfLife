using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GoL
{
    class Extensions
    {
        public static Bitmap ArrayToBitmap(double[] rgbArray, int width, int height)
        {
            //string outputFile = "C:\\Users\\Hayden\\Desktop\\" + filename;
            Bitmap newFile = new Bitmap(width, height);
            Color c;
            int rgbValue;
            int counter = 0;
            for (int i = 0; i < width; i++)
            {

                for (int j = 0; j < height; j++)
                {
                    if (rgbArray[counter] == 0)
                    {
                        newFile.SetPixel(i, j, Color.Black);
                    }
                    else
                    {
                        rgbValue = (int)Math.Round(rgbArray[counter] * 255f);
                        newFile.SetPixel(i, j, Color.LightBlue);
                    }
                    
                                 
                    counter++;
                }
            }
            Bitmap imgcopy = (Bitmap)newFile.Clone();
            Size ogSize = imgcopy.Size;
            Bitmap zoomed;
            //if (zoomed != null) zoomed.Dispose();
            int zoom = 1;
            zoomed = new Bitmap((int)(ogSize.Width * zoom), (int)(ogSize.Height * zoom));
            using (Graphics g = Graphics.FromImage(zoomed))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(imgcopy, new Rectangle(Point.Empty, zoomed.Size));
            }
            return zoomed;
            //newFile.Save(outputFile);
            //return bmp;
        }


    }
}
