using Mapbox.VectorTile.ExtensionMethods;
using System;
using System.IO;


namespace Mapbox.VectorTile {


	public class DemoConsoleApp {


		public static int Main(string[] args) {

			if(args.Length != 1 && args.Length != 2) {
				Console.WriteLine("invalid number of arguments");
				return 1;
			}

			string vtIn = args[0];
			uint? clipBuffer = null;
			if(args.Length == 2) {
				clipBuffer = Convert.ToUInt32(args[1]);
			}
			if(!File.Exists(vtIn)) {
				Console.WriteLine($"file [{vtIn}] not found");
				return 1;
			}


			var bufferedData = File.ReadAllBytes(vtIn);
			ulong zoom;
			ulong tileCol;
			ulong tileRow;

			if(!parseArg(Path.GetFileName(vtIn), out zoom, out tileCol, out tileRow)) {
				return 1;
			}

			VectorTile tile = new VectorTile(bufferedData);

			Console.WriteLine(tile.ToGeoJson(zoom, tileCol, tileRow, clipBuffer));

			return 0;
		}


		private static bool parseArg(string fileName, out ulong zoom, out ulong tileCol, out ulong tileRow) {
			zoom = 0;
			tileCol = 0;
			tileRow = 0;

			string zxyTxt = fileName.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
			string[] zxy = zxyTxt.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if(zxy.Length != 3) {
				Console.WriteLine("invalid zoom, tileCol or tileRow [{0}]", zxyTxt);
				return false;
			}
			if(!ulong.TryParse(zxy[0], out zoom)) {
				Console.WriteLine($"could not parse zoom: {zxy[0]}");
				return false;
			}
			if(!ulong.TryParse(zxy[1], out tileCol)) {
				Console.WriteLine($"could not parse tileCol: {zxy[1]}");
				return false;
			}
			if(!ulong.TryParse(zxy[2], out tileRow)) {
				Console.WriteLine($"could not parse tileRow: {zxy[2]}");
				return false;
			}

			return true;
		}



	}
}
