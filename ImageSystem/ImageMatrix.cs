using System.Drawing;
using System.Drawing.Imaging;

namespace UniversalSystem
{
    public class ImageMatrix
    {
        public byte[,,] Matrix { get; set; }
        private int width;
        private int height;

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public unsafe void CreateMatrix(Bitmap bmp)
        {
            width = bmp.Width;
            height = bmp.Height;
            Matrix = new byte[width, height, 3];
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            try
            {
                byte* curpos;
                for (int h = 0; h < height; h++)
                {
                    curpos = ((byte*)bd.Scan0) + h * bd.Stride;
                    for (int w = 0; w < width; w++)
                    {
                        Matrix[w, h, 2] = *(curpos++);
                        Matrix[w, h, 1] = *(curpos++);
                        Matrix[w, h, 0] = *(curpos++);
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
        }

        public unsafe Bitmap TranslateToBitmap()
        {
            Bitmap bmp = new Bitmap(width,height);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            try
            {
                byte* curpos;
                for (int h = 0; h < height; h++)
                {
                    curpos = ((byte*)bd.Scan0) + h * bd.Stride;
                    for (int w = 0; w < width; w++)
                    {
                        *(curpos++) = Matrix[w, h, 2];
                        *(curpos++) = Matrix[w, h, 1];
                        *(curpos++) = Matrix[w, h, 0];
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return bmp;
        }
    }
}
