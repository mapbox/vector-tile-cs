using Mapbox.VectorTile.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Mapbox.VectorTile
{


	public class DemoConsoleApp
	{


		public static int Main(string[] args)
		{

			bool validate = true;
			string vtIn = string.Empty;
			uint? clipBuffer = null;
			bool outGeoJson = false;
			ulong? zoom = null;
			ulong? tileCol = null;
			ulong? tileRow = null;

			for (int i = 0; i < args.Length; i++)
			{
				string argLow = args[i].ToLower();
				if (argLow.Contains("vt:"))
				{
					vtIn = argLow.Replace("vt:", "");
				}
				else if (argLow.Contains("validate:"))
				{
					validate = Convert.ToBoolean(argLow.Replace("validate:", ""));
				}
				else if (argLow.Contains("clip:"))
				{
					clipBuffer = Convert.ToUInt32(argLow.Replace("clip:", ""));
				}
				else if (argLow.Contains("out:"))
				{
					outGeoJson = argLow.Replace("out:", "").Equals("geojson");
				}
				else if (argLow.Contains("tileid:"))
				{
					parseArg(argLow.Replace("tileid:", ""), out zoom, out tileCol, out tileRow);
				}
			}

			if (!File.Exists(vtIn))
			{
				Console.WriteLine($"file [{vtIn}] not found");
				usage();
				return 1;
			}

			// z-x-y weren't passed via parameters, try to get them from file name
			if (!zoom.HasValue || !tileCol.HasValue || !tileRow.HasValue)
			{
				if (!parseArg(Path.GetFileName(vtIn), out zoom, out tileCol, out tileRow))
				{
					usage();
					return 1;
				}
			}

			var bufferedData = File.ReadAllBytes(vtIn);

			VectorTile tile = new VectorTile(bufferedData, validate);

			if (outGeoJson)
			{
				Console.WriteLine(tile.ToGeoJson(zoom.Value, tileCol.Value, tileRow.Value, clipBuffer));
			}
			else
			{
				foreach (string lyrName in tile.LayerNames())
				{
					VectorTileLayer lyr = tile.GetLayer(lyrName);
					Console.WriteLine(string.Format("------------ {0} ---------", lyrName));
					//if (lyrName != "building") { continue; }
					int featCnt = lyr.FeatureCount();
					for (int i = 0; i < featCnt; i++)
					{
						VectorTileFeature feat = lyr.GetFeature(i, clipBuffer);
						Console.WriteLine(string.Format("feature {0}: {1}", i, feat.GeometryType));
						Dictionary<string, object> props = feat.GetProperties();
						foreach (var prop in props)
						{
							Console.WriteLine(string.Format("   {0}\t : {1}", prop.Key, prop.Value));
						}
						if (feat.GeometryType == Geometry.GeomType.XYZ)
						{
							StringBuilder sb = new StringBuilder();

							//List<int> xyz = feat.XYZ();
							//int extent = (int)lyr.Extent;
							//int extentSquared = extent * extent;
							//// get actual tile data
							//int n = 0;
							//for (int y = 0; y < extent; y++)
							//{
							//	for (int x = 0; x < extent; x++)
							//	{
							//		sb.Append(((float)xyz[n] / 4f).ToString("0.00", NumberFormatInfo.InvariantInfo).PadLeft(8));
							//		n++;
							//	}
							//	sb.Append(Environment.NewLine);
							//}

							//// get bleed data
							//extent = 258;
							//int cnter = 0;
							//for (int b = n; b < xyz.Count; b++)
							//{
							//	if (0 != cnter && 0 == cnter % extent)
							//	{
							//		sb.Append(Environment.NewLine);
							//	}
							//	sb.Append(((float)xyz[b] / 4f).ToString("0.00", NumberFormatInfo.InvariantInfo).PadLeft(8));
							//	cnter++;
							//}

							int[,] z = feat.XYZ();
							int rows = z.GetLength(0);
							int cols = z.GetLength(1);
							for (int y = 0; y < rows; y++)
							{
								for (int x = 0; x < cols; x++)
								{
									sb.Append(((float)z[x,y] / 4f).ToString("0.00", NumberFormatInfo.InvariantInfo).PadLeft(8));
								}
								sb.Append(Environment.NewLine);
							}

							File.WriteAllText($"{vtIn}.txt", sb.ToString(), new UTF8Encoding(false));
						}
					}
				}
			}

			return 0;
		}

		private static void usage()
		{

			Console.WriteLine("");
			Console.WriteLine("DemoConsoleApp.exe vt:<tile.mvt> <optional parameters>");
			Console.WriteLine("");
			Console.WriteLine("* vt:<path/to/vector/tile.mvt> or vt:<path/to/<z>-<x>-<y>.tile.mvt>");
			Console.WriteLine("");
			Console.WriteLine("* tileid:<z>-<x>-<y>      required: if tile id is not contained within the file name");
			Console.WriteLine("* clip:<buffer>           optional: clip geometries extending beyond tile border, default: no clipping");
			Console.WriteLine("* out:<geojson|metadata>  optional: ouput either GeoJson or some metadata, default:metadata");
			Console.WriteLine("* validate:<true|false>   optional: turn validation on/off, default: on");
			Console.WriteLine("");
			Console.WriteLine("");
		}

		private static bool parseArg(string fileName, out ulong? zoom, out ulong? tileCol, out ulong? tileRow)
		{
			zoom = null;
			tileCol = null;
			tileRow = null;

			string zxyTxt = fileName.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
			string[] zxy = zxyTxt.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (zxy.Length != 3)
			{
				Console.WriteLine("invalid zoom, tileCol or tileRow [{0}]", zxyTxt);
				return false;
			}

			ulong z;
			if (!ulong.TryParse(zxy[0], out z))
			{
				Console.WriteLine($"could not parse zoom: {zxy[0]}");
				return false;
			}
			zoom = z;

			ulong x;
			if (!ulong.TryParse(zxy[1], out x))
			{
				Console.WriteLine($"could not parse tileCol: {zxy[1]}");
				return false;
			}
			tileCol = x;

			ulong y;
			if (!ulong.TryParse(zxy[2], out y))
			{
				Console.WriteLine($"could not parse tileRow: {zxy[2]}");
				return false;
			}
			tileRow = y;

			return true;
		}



	}
}
