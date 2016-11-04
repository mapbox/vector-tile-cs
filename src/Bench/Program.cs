using Mapbox.VectorTile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ProfileDecoding
{
    class Program
    {
        static int Main(string[] args)
        {

            //ul 14/4680/6260
            //lr 14/4693/6274
            ulong zoom = 14;
            ulong minCol = 4680;
            ulong minRow = 6260;
            ulong maxCol = 4693;
            ulong maxRow = 6274;

            string fixturePath = Path.Combine("..", "bench", "mvt-bench-fixtures", "fixtures");
            if (!Directory.Exists(fixturePath))
            {
                Console.Error.WriteLine("fixture directory not found: [{0}]", fixturePath);
                return 1;
            }

            ulong nrOfTiles = (maxCol - minCol + 1) * (maxRow - minRow + 1);
            List<TileData> tiles = new List<TileData>((int)nrOfTiles);

            //https://a.tiles.mapbox.com/v4/mapbox.mapbox-terrain-v2,mapbox.mapbox-streets-v7/13/2343/3133.vector.pbf?access_token=
            using (GZipWebClient wc = new GZipWebClient())
            {
                for (ulong col = minCol; col <= maxCol; col++)
                {
                    for (ulong row = minRow; row <= maxRow; row++)
                    {
                        string fileName = string.Format("{0}-{1}-{2}.mvt", zoom, col, row);
                        fileName = Path.Combine(fixturePath, fileName);
                        if (!File.Exists(fileName))
                        {
                            Console.Error.WriteLine("fixture mvt not found: [{0}]", fileName);
                            return 1;
                        } else
                        {
                            tiles.Add(new TileData() {
                                zoom = zoom,
                                col = col,
                                row = row,
                                pbf = File.ReadAllBytes(fileName)
                            });
                        }
                    }
                }
            }

            Stopwatch stopWatch = new Stopwatch();
            List<long> elapsed = new List<long>();

            for (int i = 0; i <= 100; i++)
            {
                Console.Write(".");
                stopWatch.Start();
                foreach (var tile in tiles)
                {
                    VectorTileReader.Decode(tile.zoom, tile.col, tile.row, tile.pbf);
                }
                stopWatch.Stop();
                //skip first run
                if (i != 0)
                {
                    elapsed.Add(stopWatch.ElapsedMilliseconds);
                }
                stopWatch.Reset();
            }


            Console.WriteLine(
               "{0}{0}runs:{1}{0}tiles per run:{2}{0}min [ms]:{3}{0}max [ms]:{4}{0}avg [ms]:{5}{0}StdDev:{6:0.00}{0}overall [ms]:{7}",
               Environment.NewLine,
               elapsed.Count,
               tiles.Count,
               elapsed.Min(),
               elapsed.Max(),
               elapsed.Average(),
               StdDev(elapsed),
               elapsed.Sum()
               );


            return 0;
        }


        private static double StdDev(List<long> values)
        {
            double ret = 0;
            int count = values.Count();
            if (count > 1)
            {
                //Compute the Average
                double avg = values.Average();

                //Perform the Sum of (value-avg)^2
                double sum = values.Sum(d => (d - avg) * (d - avg));

                //Put it all together
                ret = Math.Sqrt(sum / count);
            }
            return ret;
        }


    }


    public struct TileData
    {
        public ulong zoom;
        public ulong col;
        public ulong row;
        public byte[] pbf;
    }


    public class GZipWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return request;
        }
    }

}
