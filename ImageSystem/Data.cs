using System.Collections.Generic;
using System.Drawing;

namespace UniversalSystem
{
    public class Data
    {
        public int Id { get; set; }
        public ImageMatrix ImageMatrix { get; set; }
        public byte[,] LogicMatrix { get; set; }
        public Bitmap Image { get; set; }
        public Vector3D Dispersion { get; set; }
        public Vector3D Expectation { get; set; }
        public double RatioSP { get; set; }
        public double RatioDiagonals { get; set; }
        public int[,] Gistogram { get; set; }

        public Data()
        {
            ImageMatrix = new ImageMatrix();
            Dispersion = new Vector3D();
            Expectation = new Vector3D();
        }
    }

    class SearchData
    {
        public List<Data> Data { get; set; }
        public string Kind { get; set; }
        public string Sort { get; set; }
        public Vector3D ColorPercent { get; set; }
        public double RatioSPPercent { get; set; }
        public double RatioDiagonalsPercent { get; set; }
        public bool Result { get; set; }
        public SearchData()
        {
            ColorPercent = new Vector3D();
            Data = new List<Data>();
        }
    }
}
