using System;
using System.Collections.Generic;

namespace UniversalSystem
{
    class Search
    {
        private readonly List<SearchData> results;
        private readonly List<SearchData> etalons; 
        private readonly DbWorker dbWorker;

        public Search()
        {
            results = new List<SearchData>();
            etalons = new List<SearchData>();
            dbWorker = new DbWorker();
        }

        public List<SearchData> StartSearch(IEnumerable<SearchData> data, double dispDeviation, double expDeviation, double rDiagDeviation, double rSpDeviation)
        {
            results.Clear();
            ReadEtalons();
            foreach (SearchData sd in data)
            {
                foreach (Data d in sd.Data)
                {
                    bool flag = false;
                    foreach (SearchData t in etalons)
                    {
                        foreach (Data td in t.Data)
                        {
                            if (Vector3D.Abs(d.Dispersion - td.Dispersion) <= dispDeviation &&
                                Vector3D.Abs(d.Expectation - td.Expectation) <= expDeviation &&
                                Math.Abs(d.RatioDiagonals - td.RatioDiagonals) <= rDiagDeviation &&
                                Math.Abs(d.RatioSP - td.RatioSP) <= rSpDeviation)
                            {
                                var rslt = new SearchData
                                {
                                    Kind = t.Kind,
                                    Sort = t.Sort,
                                    RatioDiagonalsPercent = CalculatePercent(d.RatioDiagonals, td.RatioDiagonals),
                                    RatioSPPercent = CalculatePercent(d.RatioSP, td.RatioSP),
                                    ColorPercent = CalculateColorPercent(d.Dispersion, td.Dispersion),
                                    Result = true
                                };
                                var im = dbWorker.ReadImage(rslt.Kind, rslt.Sort);
                                rslt.Data.Add(new Data{Image = im});
                                results.Add(rslt);
                                flag = true;
                                break;
                            }
                            else
                            {
                                var rslt = new SearchData
                                {
                                    Data = t.Data,
                                    Kind = t.Kind,
                                    Sort = t.Sort,
                                    RatioDiagonalsPercent = CalculatePercent(d.RatioDiagonals, td.RatioDiagonals),
                                    RatioSPPercent = CalculatePercent(d.RatioSP, td.RatioSP),
                                    ColorPercent = CalculateColorPercent(d.Dispersion, td.Dispersion),
                                    Result = false
                                };
                                results.Add(rslt);
                            }
                        }
                        if(flag)
                            break;
                    }
                }
            }
            return results;
        }

        private void ReadEtalons()
        {
            dbWorker.ConnecToDb(@"DIMAPC\SQLEXPRESS");
            etalons.Clear();
            var kinds = dbWorker.ReadKinds();
            var sorts = new List<string>();
            var etalon = new SearchData();
            foreach (string kind in kinds)
            {
                sorts.Clear();
                sorts.AddRange(dbWorker.ReadSorts(kind));
                for (int i = 0; i < sorts.Count; i++)
                {
                    etalon = new SearchData
                    {
                        Data = dbWorker.ReadDescriptors(sorts[i], kind),
                        Kind = kind,
                        Sort = sorts[i]
                    };
                    etalons.Add(etalon);
                }
            }
        }

        private double CalculatePercent(double n1, double n2)
        {
            return n1*100/n2;
        }

        private Vector3D CalculateColorPercent(Vector3D v1, Vector3D v2)
        {
            return v1*100/v2;
        }
    }
}
