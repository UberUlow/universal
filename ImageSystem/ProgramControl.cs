using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using UniversalSystem;

namespace ImageSystem
{
    class ProgramControl
    {
        private bool exit;
        private readonly Algorithms algorithms = new Algorithms();
        private Data data;
        private Log log;
        private readonly DbWorker dbw = new DbWorker();
        public static Settings Settings = new Settings();
        public void StartControl()
        {
            SysLog.WriteLine("Start system",Codes.SYSTEM);
            data = new Data();
            log = new Log();
            var command = new List<string>();
            while (!exit)
            {
                command.Clear();
                var readLine = Console.ReadLine();
                if (readLine != null) command.AddRange(readLine.Split(' '));
                try
                {
                    switch (command[0].ToLower())
                    {
                        case "search":
                        {
                            switch (command[1].ToLower())
                            {
                                case "file":
                                {
                                    SearchFile(command[2],command[3]);
                                }
                                    break;
                                case "folder":
                                {
                                    SearchFolder(command[2],command[3]);
                                }
                                    break;
                                default:
                                    SysLog.WriteLine("Wrong parameters.", Codes.ERROR);
                                    break;
                            }
                        }
                            break;
                        case "db":
                        {
                            switch (command[1].ToLower())
                            {
                                case "add":
                                {
                                    switch (command[2].ToLower())
                                    {
                                        case "folder":
                                        {
                                            AddToDbFolder(command[3]);
                                        }
                                            break;
                                        case "cam":
                                        {
                                            AddToDbCamera();
                                        }
                                            break;
                                        default:
                                            SysLog.WriteLine("Wrong parameters.", Codes.ERROR);
                                            break;
                                    }
                                }
                                    break;
                                default:
                                    SysLog.WriteLine("Wrong parameters.", Codes.ERROR);
                                    break;
                            }
                        }
                            break;
                        case "rg":
                        {
                            switch (command[1].ToLower())
                            {
                                case "file":
                                    RgFile(command[2], command[3]);
                                    break;
                                case "folder":
                                    RgFolder(command[2], command[3]);
                                    break;
                                default:
                                    SysLog.WriteLine("Wrong parameters.", Codes.ERROR);
                                    break;
                            }
                        }
                            break;
                        case "gist":
                        {
                            switch (command[1].ToLower())
                            {
                                case "file":
                                    GistogramFile(command[2], command[3]);
                                    break;
                                case "folder":
                                {
                                    GistogramFolder(command[2], command[3]);
                                }
                                    break;
                                default:
                                    SysLog.WriteLine("Wrong parameters.",Codes.ERROR);
                                    break;
                            }
                        }
                            break;
                        case "settings":
                            {
                                switch (command[1].ToLower())
                                {
                                    case "default":
                                    {
                                        DefaultSettings();
                                    }
                                        break;
                                    case "read":
                                    {
                                        ReadSettings();
                                    }
                                        break;
                                }
                            }
                            break;
                        case "clear":
                            Console.Clear();
                            break;
                        case "exit":
                            exit = true;
                            break;
                        default:
                            SysLog.WriteLine("Wrong command.",Codes.ERROR);
                            break;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    SysLog.WriteLine("Wrong count of parameters",Codes.ERROR);
                }
            }
        }

        private void DefaultSettings()
        {
            Settings = new Settings();
            var aRg = new Settings.ARg {PercentMin = 15, PercentMax = 40, Deviation = 125};
            Settings.AutoRegionGrowing = aRg;
            Settings.Cascades = new List<Settings.Cascade>();
            var rg = new Settings.Rg {Deviation = 125};
            Settings.RegionGrowing = rg;
            var dae = new Settings.DispAndExp {Percent = 15};
            Settings.DispersionAndExpectation = dae;
            var ss = new Settings.SearchSet
            {
                DispersionDeviation = 25,
                ExpectationDeviation = 25,
                RatioDiagonalsDeviation = 25,
                RatioSpDeviation = 25
            };
            Settings.SearchSettings = ss;
            Settings.Cascade kaskade;
            kaskade.Level = 1;
            kaskade.Methods = new List<string> {"AutoRegionGrowing", "DispersionAndExpectation"};
            kaskade.Name = "Color";
            Settings.Cascades.Add(kaskade);
            kaskade.Level = 2;
            kaskade.Methods = new List<string> { "CalculateRatioSP", "CalculateRatioDiagonals" };
            kaskade.Name = "Form";
            Settings.Cascades.Add(kaskade);
            XmlSerialization.Serialize("Settings.xml", Settings);
            SysLog.WriteLine("Default settings restored",Codes.RESULT);
        }

        private void ReadSettings()
        {
            Settings = XmlSerialization.Deserialize("Settings.xml");
            SysLog.WriteLine("Settings are readed",Codes.RESULT);
        }

        private void SearchFile(string pathFrom, string pathTo)
        {
            log.InitLog("SearchLog.txt");
            var ss = Settings.SearchSettings;
            var search = new Search();
            data.Image = new Bitmap(pathFrom);
            data.ImageMatrix.CreateMatrix(data.Image);
            algorithms.CalculateDispAndExpectation(ref data);
            algorithms.AutoRegionGrowing(ref data);
            algorithms.CalculateRatioDiagonals(ref data);
            algorithms.CalculateRatioSp(ref data);
            var sdata = new SearchData {Data = new List<Data> {data}};
            var results = search.StartSearch(new List<SearchData> {sdata},ss.DispersionDeviation,ss.ExpectationDeviation,ss.RatioDiagonalsDeviation,ss.RatioSpDeviation );
            if(results.Count !=0)
                results[0].Data[0].Image.Save(pathTo);
            log.WriteSearchFileLog(ref results);
            Console.WriteLine("Done!");
            log.CloseLog();
        }

        private void SearchFolder(string pathFrom, string pathTo)
        {
            log.InitLog("SearchLog.txt");
            var ss = Settings.SearchSettings;
            var search = new Search();
            SysLog.WriteLine("Get filelist", Codes.SYSTEM);
            var dirf = new DirectoryInfo(pathFrom);
            var dirt = new DirectoryInfo(pathTo);
            if (!dirt.Exists)
                dirt.Create();
            var files = dirf.GetFiles("*.jpg");
            var sdata = new SearchData {Data = new List<Data>()};
            SysLog.WriteLine("Calculating descriptors",Codes.SYSTEM);
            int count = 1;
            foreach (FileInfo t in files)
            {
                data = new Data();
                data.ImageMatrix.CreateMatrix(new Bitmap(t.FullName));
                algorithms.CalculateDispAndExpectation(ref data);
                algorithms.AutoRegionGrowing(ref data);
                algorithms.CalculateRatioDiagonals(ref data);
                algorithms.CalculateRatioSp(ref data);
                sdata.Data.Add(data);
                SysLog.WriteStep(ref count,files.Length);    
            }
            SysLog.WriteLine("Calculation of descriptors is completed",Codes.SYSTEM);
            SysLog.WriteLine("Starting search", Codes.SYSTEM);
            var results = search.StartSearch(new List<SearchData> { sdata }, ss.DispersionDeviation, ss.ExpectationDeviation, ss.RatioDiagonalsDeviation, ss.RatioSpDeviation);
            SysLog.WriteLine("Search is finished",Codes.SYSTEM);
            log.WriteSearchFolderLog(ref results);
            int c = 1;
            SysLog.WriteLine("Writting result images to file system",Codes.SYSTEM);
            var rslt = results.Where(r => r.Result).Select(s => s);
            rslt.All(s => s.Data.All(w =>
            {
                w.Image.Save(Path.Combine(pathTo, string.Format("{0}{1}_{2}.jpg",c,s.Kind.Trim(),s.Sort.Trim())));
                c++;
                return true;
            }));
            SysLog.WriteLine("Done!",Codes.RESULT);
            log.CloseLog();
        }

        private void AddToDbFolder(string pathFrom)
        {
            var dir = new DirectoryInfo(pathFrom);
            var files = dir.GetFiles("*.jpg");
            Console.Write("Enter kind: ");
            var kind = Console.ReadLine().Trim();
            Console.Write("Enter sort: ");
            var sort = Console.ReadLine().Trim();
            if(!dbw.IsConnected)
                dbw.ConnecToDb(@"DIMAPC\SQLEXPRESS");
            SysLog.WriteLine("Connected to database",Codes.SYSTEM);
            var kinds = dbw.ReadKinds();
            if (kinds == null || !kinds.Any(k => k.Trim().Equals(kind)))
                dbw.AddKind(kind);
            var sorts = dbw.ReadSorts(kind);
            if (sorts == null || !sorts.Any(s => s.Trim().Equals(sort)))
                dbw.AddSort(sort, kind);
            SysLog.WriteLine("Starting adding files to database",Codes.SYSTEM);
            var count = 1;
            foreach (var fileInfo in files)
            {
                data = new Data {Image = new Bitmap(fileInfo.FullName)};
                data.ImageMatrix.CreateMatrix(data.Image);
                algorithms.CalculateDispAndExpectation(ref data);
                algorithms.AutoRegionGrowing(ref data);
                algorithms.CalculateRatioDiagonals(ref data);
                algorithms.CalculateRatioSp(ref data);
                dbw.AddImage(data.Image, kind, sort);
                dbw.AddDescriptors(ref data, kind, sort);
                SysLog.WriteStep(ref count,files.Length);
            }
            SysLog.WriteLine("Done!",Codes.RESULT);
        }

        public void AddToDbCamera()
        {
            Console.Write("Enter kind: ");
            var kind = Console.ReadLine();
            Console.Write("Enter sort: ");
            var sort = Console.ReadLine();
            var capture = new Capture(CaptureType.ANY);
            var im = capture.QueryFrame();
            im.Save("test.jpg");
        }

        private void GistogramFile(string pathFrom, string pathTo)
        {
            data = new Data {Image = new Bitmap(pathFrom)};
            data.ImageMatrix.CreateMatrix(data.Image);
            algorithms.CalculateDispAndExpectation(ref data);
            algorithms.AutoRegionGrowing(ref data);
            algorithms.CreateGistogram(ref data);
            algorithms.DrawGistogram(ref data, pathTo);
        }

        private void GistogramFolder(string pathFrom, string pathTo)
        {
            SysLog.WriteLine("Starting gistogram builder algorithm",Codes.SYSTEM);
            var dirf = new DirectoryInfo(pathFrom);
            var dirt = new DirectoryInfo(pathTo);
            if (!dirt.Exists)
                dirt.Create();
            var files = dirf.GetFiles("*.jpg");
            var step = 1;
            foreach (FileInfo t in files)
            {
                GistogramFile(t.FullName, Path.Combine(pathTo, t.Name));
                SysLog.WriteStep(ref step,files.Length);
            }
            SysLog.WriteLine("All done!",Codes.RESULT);
        }

        private void RgFile(string pathFrom, string pathTo)
        {
            data = new Data {Image = new Bitmap(pathFrom)};
            data.ImageMatrix.CreateMatrix(data.Image);
            algorithms.CalculateDispAndExpectation(ref data);
            algorithms.AutoRegionGrowing(ref data);
            algorithms.LogicToImageMatrix(ref data, Color.BlueViolet);
            data.Image = data.ImageMatrix.TranslateToBitmap();
            data.Image.Save(pathTo);
        }

        private void RgFolder(string pathFrom, string pathTo)
        {
            SysLog.WriteLine("Starting Region Growing algorithm",Codes.SYSTEM);
            var dirf = new DirectoryInfo(pathFrom);
            var dirt = new DirectoryInfo(pathTo);
            if (!dirt.Exists)
                dirt.Create();
            var files = dirf.GetFiles("*.jpg");
            var step = 1;
            foreach (FileInfo t in files)
            {
                RgFile(t.FullName, Path.Combine(pathTo, t.Name));
                SysLog.WriteStep(ref step,files.Length);
            }
            SysLog.WriteLine("All done!",Codes.RESULT);
        }
    }
}
