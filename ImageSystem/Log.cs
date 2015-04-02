using System;
using System.Collections.Generic;
using System.IO;

namespace UniversalSystem
{
    public enum Codes
    {
        ERROR, WARNING, WORK, RESULT, SYSTEM, EMPTY
    }

    class Log
    {
        private TextWriter tw;
        private string filename;

        public void InitLog(string fileName)
        {
            filename = fileName;
            tw = new StreamWriter(fileName);
            tw.WriteLine("[SYSTEM]:Start logging at {0:R}",DateTime.Now);
            SysLog.WriteLine(string.Format("File log \"{0}\" started",fileName), Codes.SYSTEM);
        }

        public void ClearLog()
        {
            File.Delete(filename);
            SysLog.WriteLine(string.Format("File log \"{0}\" cleared", filename), Codes.SYSTEM);
        }

        public void WriteLine(string line, Codes flag)
        {
            if (flag == Codes.EMPTY)
                tw.WriteLine(line);
            else
                tw.WriteLine("[{0}]:{1}", flag, line);
        }

        public void CloseLog()
        {
            tw.WriteLine("[SYSTEM]:Finish logging at {0:R}", DateTime.Now);
            tw.Close();
            SysLog.WriteLine(string.Format("File log \"{0}\" finished", filename), Codes.SYSTEM);
        }

        public void WriteSearchFolderLog(ref List<SearchData> data) 
        {
            WriteLine("Start search by folder", Codes.SYSTEM);
            foreach (var searchData in data)
            {
                WriteLine(string.Format("Kind:{0}", searchData.Kind), Codes.WORK);
                WriteLine(string.Format("Sort:{0}", searchData.Sort), Codes.WORK);
                WriteLine(string.Format("ColorPercent.R:{0:F2}%", searchData.ColorPercent.R), Codes.WORK);
                WriteLine(string.Format("ColorPercent.G:{0:F2}%", searchData.ColorPercent.G), Codes.WORK);
                WriteLine(string.Format("ColorPercent.B:{0:F2}%", searchData.ColorPercent.B), Codes.WORK);
                WriteLine(string.Format("RatioSPPercent:{0:F2}%", searchData.RatioSPPercent), Codes.WORK);
                WriteLine(string.Format("RatioDiagPercent:{0:F2}%", searchData.RatioDiagonalsPercent), Codes.WORK);
                WriteLine(string.Format("{0}", searchData.Result), Codes.RESULT);
                WriteLine("===================================", Codes.EMPTY);
            }
        }

        public void WriteSearchFileLog(ref List<SearchData> data)
        {
            WriteLine("Start search by one file", Codes.SYSTEM);
            foreach (var searchData in data)
            {
                WriteLine(string.Format("Kind:{0}", searchData.Kind), Codes.WORK);
                WriteLine(string.Format("Sort:{0}", searchData.Sort), Codes.WORK);
                WriteLine(string.Format("ColorPercent.R:{0:F2}%", searchData.ColorPercent.R), Codes.WORK);
                WriteLine(string.Format("ColorPercent.G:{0:F2}%", searchData.ColorPercent.G), Codes.WORK);
                WriteLine(string.Format("ColorPercent.B:{0:F2}%", searchData.ColorPercent.B), Codes.WORK);
                WriteLine(string.Format("RatioSPPercent:{0:F2}%", searchData.RatioSPPercent), Codes.WORK);
                WriteLine(string.Format("RatioDiagPercent:{0:F2}%", searchData.RatioDiagonalsPercent), Codes.WORK);
                WriteLine(string.Format("{0}", searchData.Result), Codes.RESULT);
                WriteLine("===================================", Codes.EMPTY);
            }
        }
    }

    public static class SysLog
    {
        
        public static void WriteLine(string line, Codes flag)
        {
            switch (flag)
            {
                case Codes.SYSTEM:
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("[{0}]:{1}", flag, line);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                    break;
                case Codes.WORK:
                {
                    Console.WriteLine("[{0}]:{1}", flag, line);
                }
                    break;
                case Codes.ERROR:
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[{0}]:{1}", flag, line);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                    break;
                    case Codes.WARNING:
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("[{0}]:{1}", flag, line);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                    break;
                    case Codes.RESULT:
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[{0}]:{1}", flag, line);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                    break;
            }
        }

        public static void WriteStep(ref int step, int count)
        {
            Console.WriteLine("[{0}]:Added[{1}/{2}]",Codes.WORK, step, count);
            step++;
        }
    }
}
