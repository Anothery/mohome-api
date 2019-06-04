using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace mohome_api.Infrastructure
{
    public class PhotoHelper
    {
        public Bitmap ResizeImage(Image image)
        {
            int width = 250, height = (int)(image.Height / ((double)image.Width / width));
            using (image)
            {
                Bitmap cpy = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(cpy))
                {
                    gr.Clear(Color.Transparent);
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.DrawImage(image,
                        new Rectangle(0, 0, width, height),
                        new Rectangle(0, 0, image.Width, image.Height),
                        GraphicsUnit.Pixel);
                }
                return cpy;
            }

        }


        public void SaveImage(Image image, string path)
        {
            image.Save(path);
        }

        public string GetBase64Image(string path)
        {
            return Convert.ToBase64String(System.IO.File.ReadAllBytes(path));
        }
    }
}
