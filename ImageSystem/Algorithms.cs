using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using ImageSystem;

namespace UniversalSystem
{
    class Algorithms
    {
        public void AutoRegionGrowing(ref Data data)
        {
            var set = ProgramControl.Settings.AutoRegionGrowing;
            int deviation = set.Deviation;
            int count;
            int step = 10;
            double res;
            bool exit = false, maxChanged = false, minChanged = false;
            byte value = 2;
            while (!exit)
            {
                count = 0;
                RegionGrowing(ref data, deviation);
                foreach (byte b in data.LogicMatrix)
                {
                    if (b == 2)
                        count++;
                }
                res = (double)100 * count / (data.LogicMatrix.GetLength(0) * data.LogicMatrix.GetLength(1));
                if (res > set.PercentMax)
                {
                    if (value == 1)
                        step--;
                    deviation -= step;
                    maxChanged = true;
                    value = 0;
                }

                if (res < set.PercentMin)
                {
                    if (value == 0)
                        step--;
                    deviation += step;
                    minChanged = true;
                    value = 1;
                }

                if (step == 0 || (!maxChanged && !minChanged))
                    exit = true;
                else
                {
                    minChanged = false;
                    maxChanged = false;
                }
            }
        }

        private void RegionGrowing(ref Data data,int deviation)
        {
            var points = new Queue<Point>();
            var p = new Point(data.ImageMatrix.Width/2, data.ImageMatrix.Height/2);
            points.Enqueue(p);
            data.LogicMatrix = new byte[data.ImageMatrix.Width, data.ImageMatrix.Height];
            data.LogicMatrix[p.X, p.Y] = 2;
            double[] parameters =
            {
                data.Expectation.R + data.Dispersion.R+deviation,
                data.Expectation.R - data.Dispersion.R-deviation,
                data.Expectation.G + data.Dispersion.G+deviation,
                data.Expectation.G - data.Dispersion.G-deviation,
                data.Expectation.B + data.Dispersion.B+deviation,
                data.Expectation.B - data.Dispersion.B-deviation
            };
            while (true)
            {
                Point point;
                try
                {
                    point = points.Dequeue();
                }
                catch (Exception)
                {
                    break;
                }
                if (point.X > 0 && point.Y > 0)
                    CheckToLogicMatrix(new Point(point.X - 1, point.Y - 1), parameters, points, ref data);
                if (point.Y > 0)
                    CheckToLogicMatrix(new Point(point.X, point.Y - 1), parameters, points, ref data);
                if (point.X < data.ImageMatrix.Width - 1 && point.Y > 0)
                    CheckToLogicMatrix(new Point(point.X + 1, point.Y - 1), parameters, points, ref data);
                if (point.X > 0)
                    CheckToLogicMatrix(new Point(point.X - 1, point.Y), parameters, points, ref data);
                if (point.X < data.ImageMatrix.Width - 1)
                    CheckToLogicMatrix(new Point(point.X + 1, point.Y), parameters, points, ref data);
                if (point.X > 0 && point.Y < data.ImageMatrix.Height - 1)
                    CheckToLogicMatrix(new Point(point.X - 1, point.Y + 1), parameters, points, ref data);
                if (point.Y < data.ImageMatrix.Height - 1)
                    CheckToLogicMatrix(new Point(point.X, point.Y + 1), parameters, points, ref data);
                if (point.X < data.ImageMatrix.Width - 1 && point.Y < data.ImageMatrix.Height - 1)
                    CheckToLogicMatrix(new Point(point.X + 1, point.Y + 1), parameters, points, ref data);
            }
        }

        private void CheckToLogicMatrix(Point p, double[] parameters, Queue<Point> points, ref Data data)
        {
            if (data.LogicMatrix[p.X, p.Y] != 0) return;
            if (data.ImageMatrix.Matrix[p.X, p.Y, 0] >= parameters[1] &&
                data.ImageMatrix.Matrix[p.X, p.Y, 0] <= parameters[0] &&
                data.ImageMatrix.Matrix[p.X, p.Y, 1] >= parameters[3] &&
                data.ImageMatrix.Matrix[p.X, p.Y, 1] <= parameters[2] &&
                data.ImageMatrix.Matrix[p.X, p.Y, 2] >= parameters[5] &&
                data.ImageMatrix.Matrix[p.X, p.Y, 2] <= parameters[4])
            {
                data.LogicMatrix[p.X, p.Y] = 2;
                points.Enqueue(new Point(p.X, p.Y));
            }
            else
            {
                data.LogicMatrix[p.X, p.Y] = 1;
            }
        }



        public void CalculateDispAndExpectation(ref Data data)
        {
            var set = ProgramControl.Settings.DispersionAndExpectation;
            double minR = Int32.MaxValue, minG = Int32.MaxValue, minB = Int32.MaxValue;
            double maxR = Int32.MinValue, maxG = Int32.MinValue, maxB = Int32.MinValue;
            int percentW = data.ImageMatrix.Width*set.Percent/100;
            int percentH = data.ImageMatrix.Height*set.Percent/100;
            int centerW = data.ImageMatrix.Width/2;
            int centerH = data.ImageMatrix.Height/2;
            var points = new Point[9];
            points[0] = new Point(centerW, centerH);
            points[1] = new Point(centerW - percentW, centerH - percentH);
            points[2] = new Point(centerW + percentW, centerH + percentH);
            points[3] = new Point(centerW - percentW, centerH + percentH);
            points[4] = new Point(centerW + percentW, centerH - percentH);
            points[5] = new Point(centerW + percentW, centerH);
            points[6] = new Point(centerW - percentW, centerH);
            points[7] = new Point(centerW, centerH + percentH);
            points[8] = new Point(centerW, centerH - percentH);
            foreach (Point p in points)
            {
                if (data.ImageMatrix.Matrix[p.X, p.Y, 0] > maxR)
                    maxR = data.ImageMatrix.Matrix[p.X, p.Y, 0];
                if (data.ImageMatrix.Matrix[p.X, p.Y, 0] < minR)
                    minR = data.ImageMatrix.Matrix[p.X, p.Y, 0];
                if (data.ImageMatrix.Matrix[p.X, p.Y, 1] > maxG)
                    maxG = data.ImageMatrix.Matrix[p.X, p.Y, 1];
                if (data.ImageMatrix.Matrix[p.X, p.Y, 1] < minG)
                    minG = data.ImageMatrix.Matrix[p.X, p.Y, 1];
                if (data.ImageMatrix.Matrix[p.X, p.Y, 2] > maxB)
                    maxB = data.ImageMatrix.Matrix[p.X, p.Y, 2];
                if (data.ImageMatrix.Matrix[p.X, p.Y, 2] < minB)
                    minB = data.ImageMatrix.Matrix[p.X, p.Y, 2];
            }
            double eR = (minR + maxR)/2;
            double eG = (minG + maxG) / 2;
            double eB = (minB + maxB) / 2;
            data.Expectation = new Vector3D(eR, eG, eB);
            double dR = Math.Sqrt(Math.Pow(maxR - minR, 2)/12);
            double dG = Math.Sqrt(Math.Pow(maxG - minG, 2)/12);
            double dB = Math.Sqrt(Math.Pow(maxB - minB, 2)/12);
            data.Dispersion = new Vector3D(dR, dG, dB);
        }

        public void CalculateRatioSp(ref Data data)
        {
            double perimeter = 0, square = 0;
            var matrix = data.LogicMatrix;
            for (int i = 1; i < matrix.GetLength(0) - 1; i++)
                for (int j = 1; j < matrix.GetLength(1) - 1; j++)
                {
                    if (matrix[i, j] == 2 &&
                        (matrix[i - 1, j - 1] == 1 || matrix[i - 1, j] == 1 || matrix[i - 1, j + 1] == 1 ||
                         matrix[i, j - 1] == 1 || matrix[i, j + 1] == 1 || matrix[i + 1, j - 1] == 1 ||
                         matrix[i + 1, j] == 1 || matrix[i + 1, j + 1] == 1))
                        perimeter++;
                }
            foreach (var index in matrix)
            {
                if (index == 2)
                    square++;
            }
            data.RatioSP = square/perimeter;
        }

        public void CalculateRatioDiagonals(ref Data data)
        {
            var diag1 = new PointF[2];
            var diag2 = new PointF[2];
            for (int i = 0; i < data.LogicMatrix.GetLength(0); i++)
                for (int j = 0; j < data.LogicMatrix.GetLength(1); j++)
                {
                    if (data.LogicMatrix[i, j] == 1)
                    {
                        diag1[0] = new Point(i, j);
                        break;
                    }
                }
            for (int i = data.LogicMatrix.GetLength(0) - 1; i >= 0; i--)
                for (int j = data.LogicMatrix.GetLength(1) - 1; j >= 0; j--)
                {
                    if (data.LogicMatrix[i, j] == 1)
                    {
                        diag1[1] = new Point(i, j);
                        break;
                    }
                }
            var centerY = (diag1[1].Y - diag1[0].Y) / 2 + diag1[0].Y;
            for (int i = 0; i < data.LogicMatrix.GetLength(0); i++)
            {
                if (data.LogicMatrix[i, (int)Math.Round(centerY, 0)] == 1)
                {
                    diag2[0] = new Point(i, (int)Math.Round(centerY, 0));
                    break;
                }
            }
            for (int i = data.LogicMatrix.GetLength(0) - 1; i >= 0; i--)
            {
                if (data.LogicMatrix[i, (int)Math.Round(centerY, 0)] == 1)
                {
                    diag2[1] = new Point(i, (int)Math.Round(centerY, 0));
                    break;
                }
            }
            data.RatioDiagonals = (diag2[1].X - diag2[0].X) / centerY;
        }

        public void LogicToImageMatrix(ref Data data,Color color)
        {
            for(int i=0;i<data.LogicMatrix.GetLength(0);i++)
                for (int j = 0; j < data.LogicMatrix.GetLength(1); j++)
                {
                    if (data.LogicMatrix[i,j]==2)
                    {
                        data.ImageMatrix.Matrix[i, j, 0] = color.R;
                        data.ImageMatrix.Matrix[i, j, 1] = color.G;
                        data.ImageMatrix.Matrix[i, j, 2] = color.B;
                    }
                }
        }

        public void CreateGistogram(ref Data data)
        {
            data.Gistogram = new int[256,3];
            for (int i = 0; i < data.ImageMatrix.Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < data.ImageMatrix.Matrix.GetLength(1); j++)
                {
                    data.Gistogram[data.ImageMatrix.Matrix[i, j, 0], 0]++;
                    data.Gistogram[data.ImageMatrix.Matrix[i, j, 1], 1]++;
                    data.Gistogram[data.ImageMatrix.Matrix[i, j, 2], 2]++;
                }
            }
        }

        public void DrawGistogramNew(ref Data data, string path)
        {
            int max = 0;
            foreach (int i in data.Gistogram)
            {
                if (i > max)
                    max = i;
            }
            if (max > 10000)
                max = 10000;
            Bitmap bmp = new Bitmap(512, 512, PixelFormat.Format24bppRgb);
            for (int i = 0; i < 256; i++)
            {
                if (data.Gistogram[i, 0] == 0)
                {
                    bmp.SetPixel(i*2,0,Color.Red);
                    bmp.SetPixel(i*2+1, 0, Color.Red);
                }
                else
                {
                    if (data.Gistogram[i, 0] >= 10000)
                    {
                        bmp.SetPixel(i*2, 511, Color.Red);
                        bmp.SetPixel(i*2 + 1, 511, Color.Red);
                    }
                    else
                    {
                        bmp.SetPixel(i*2, 511 - (data.Gistogram[i, 0] * 511 / max), Color.Red);
                        bmp.SetPixel(i*2+1, 511 - (data.Gistogram[i, 0] * 511 / max), Color.Red);
                    }
                        
                }
                if (data.Gistogram[i, 1] == 0)
                {
                    bmp.SetPixel(i*2, 0, Color.Green);
                    bmp.SetPixel(i*2 + 1, 0, Color.Green);
                }
                else
                {
                    if (data.Gistogram[i, 1] >= 10000)
                    {
                        bmp.SetPixel(i*2, 511, Color.Green);
                        bmp.SetPixel(i*2 + 1, 511, Color.Green);
                    }
                    else
                    {
                        bmp.SetPixel(i*2, 511 - (data.Gistogram[i, 1] * 511 / max), Color.Green);
                        bmp.SetPixel(i*2 + 1, 511 - (data.Gistogram[i, 1] * 511 / max), Color.Green);
                    }

                }
                if (data.Gistogram[i, 2] == 0)
                {
                    bmp.SetPixel(i*2, 0, Color.Blue);
                    bmp.SetPixel(i*2 + 1, 0, Color.Blue);
                }
                else
                {
                    if (data.Gistogram[i, 2] >= 10000)
                    {
                        bmp.SetPixel(i*2, 511, Color.Blue);
                        bmp.SetPixel(i*2 + 1, 511, Color.Blue);
                    }
                    else
                    {
                        bmp.SetPixel(i*2, 511 - (data.Gistogram[i, 2] * 511 / max), Color.Blue);
                        bmp.SetPixel(i*2 + 1, 511 - (data.Gistogram[i, 2] * 511 / max), Color.Blue);
                    }

                }
            }
            bmp.Save(path, ImageFormat.Jpeg);
        }

        public void DrawGistogram(ref Data data, string path)
        {
            int max = 0;
            foreach (int i in data.Gistogram)
            {
                if (i > max)
                    max = i;
            }
            max =2000;
            Bitmap bmp = new Bitmap(2048,max,PixelFormat.Format24bppRgb);
            int height = bmp.Height-1;
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < data.Gistogram[i, 0]/10; j++)
                {
                    if (j >= 2000)
                        break;
                    for (int k = 0; k < 8; k++)
                        bmp.SetPixel(i*8 + k, height - j, Color.Red);
                }
                for (int j = 0; j < data.Gistogram[i, 1]/10; j++)
                {
                    if (j >= 2000)
                        break;
                    if(data.Gistogram[i,0]>=j && data.Gistogram[i,2]>=j)    
                        for (int k = 0; k < 8; k++)
                            bmp.SetPixel(i*8 + k, height - j, Color.White);
                    else
                    {
                        if(data.Gistogram[i,0]>=j)
                            for (int k = 0; k < 8; k++)
                                bmp.SetPixel(i * 8 + k, height - j, Color.Yellow);
                        else
                        {
                            if(data.Gistogram[i,2]>=j)
                                for (int k = 0; k < 8; k++)
                                    bmp.SetPixel(i * 8 + k, height - j, Color.Cyan);
                            else
                                for (int k = 0; k < 8; k++)
                                    bmp.SetPixel(i * 8 + k, height - j, Color.Green);
                        }
                    }
                }
                for (int j = 0; j < data.Gistogram[i, 2]/10; j++)
                {
                    if (j >= 2000)
                        break;
                    if (data.Gistogram[i, 0] >= j && data.Gistogram[i, 1] >= j)
                        for (int k = 0; k < 8; k++)
                            bmp.SetPixel(i*8 + k, height - j, Color.White);
                    else
                    {
                        if (data.Gistogram[i, 0] >= j)
                            for (int k = 0; k < 8; k++)
                                bmp.SetPixel(i*8 + k, height - j, Color.Purple);
                        else
                        {
                            if (data.Gistogram[i, 1] >= j)
                                for (int k = 0; k < 8; k++)
                                    bmp.SetPixel(i*8 + k, height - j, Color.Cyan);
                            else
                                for (int k = 0; k < 8; k++)
                                    bmp.SetPixel(i*8 + k, height - j, Color.Blue);
                        }
                    }
                }

            }
            bmp.Save(path,ImageFormat.Bmp);
        }
    }
}
