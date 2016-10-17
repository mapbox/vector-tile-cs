using System;
using System.IO;
using Mapbox.VectorTile.Geometry;

namespace Mapbox.VectorTile {

	public class DemoConsoleApp {
		public static void Main() {

			var bufferedData = File.ReadAllBytes(@"14-8902-5666.vector.pbf");
			//var bufferedData = File.ReadAllBytes(@"sample.mvt");
			Tile tile = VectorTileReader.Decode(
				14
				, 8902
				, 5666
				, bufferedData
			);

			//foreach (var lyr in tile.Layers) {
			//	Console.WriteLine(
			//		"=== Layer:{0} Version:{1} Extent:{2} Features:{3}"
			//		, lyr.Name
			//		, lyr.Version
			//		, lyr.Extent
			//		, lyr.Features.Count
			//	);
			//	Console.WriteLine("Keys: " + string.Join(", ", lyr.Keys.ToArray()));
			//	foreach (var feat in lyr.Features) {
			//		Console.WriteLine("Feature ID:{0} GeometryType:{1}", feat.Id, feat.GeometryType);
			//		foreach (var geoms in feat.Geometry) {
			//			Console.WriteLine("Geometry:");
			//			foreach (var latLng in geoms) {
			//				Console.Write(latLng + " ");
			//			}
			//		}
			//	}
			//}

			Console.WriteLine(tile.ToGeoJson());
		}




	}
}
