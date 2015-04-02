using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using ImageSystem;

namespace UniversalSystem
{
    public class Settings
    {
        public struct Cascade
        {
            [XmlAttribute]
            public string Name;
            public int Level;
            public List<string> Methods;
        }
        [XmlArray]
        public List<Cascade> Cascades = new List<Cascade>();

        public struct ARg
        {
            public double PercentMin;
            public double PercentMax;
            public int Deviation;
        }

        public ARg AutoRegionGrowing;

        public struct Rg
        {
            public int Deviation;
        }

        public Rg RegionGrowing;

        public struct DispAndExp
        {
            public int Percent;
        }

        public DispAndExp DispersionAndExpectation;

        public struct SearchSet
        {
            public int DispersionDeviation;
            public int ExpectationDeviation;
            public int RatioSpDeviation;
            public int RatioDiagonalsDeviation;
        }

        public SearchSet SearchSettings;
    }

    
    class CascadesWork
    {
        public delegate void Function(ref Data data);

        public List<Function>[] CreateCascades()
        {
            Algorithms alg = new Algorithms();
            var scasc = ProgramControl.Settings.Cascades;
            var casc = new List<Function>[scasc.Count];
            for (int i = 0; i < casc.Length; i++)
            {
                casc[i] = new List<Function>();
                for (int j = 0; j < scasc[i].Methods.Count; j++)
                {
                    switch (scasc[i].Methods[j])
                    {
                        case "AutoRegionGrowing":
                        {
                            casc[i].Add(alg.AutoRegionGrowing);
                        }
                            break;
                        case "DispersionAndExpectation":
                        {
                            casc[i].Add(alg.CalculateDispAndExpectation);
                        }
                            break;
                        case "CalculateRatioSP":
                        {
                            casc[i].Add(alg.CalculateRatioSp);
                        }
                            break;
                        case "CalculateRatioDiagonals":
                        {
                            casc[i].Add(alg.CalculateRatioDiagonals);
                        }
                            break;
                    }
                }
            }
            return casc;
        }
    }

    public static class XmlSerialization
    {
        public static void Serialize(string filename, Settings settings)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            TextWriter writer = new StreamWriter(filename);
            serializer.Serialize(writer, settings);
            writer.Close();
        }

        public static Settings Deserialize(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            serializer.UnknownNode += serializer_UnknownNode;
            serializer.UnknownAttribute += serializer_UnknownAttribute;
            FileStream fs = new FileStream(filename, FileMode.Open);
            return (Settings)serializer.Deserialize(fs);
        }

        private static void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            SysLog.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text, Codes.ERROR);
        }

        private static void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            XmlAttribute attr = e.Attr;
            SysLog.WriteLine("Unknown attribute " + attr.Name + "='" + attr.Value + "'", Codes.ERROR);
        }
    }
}
