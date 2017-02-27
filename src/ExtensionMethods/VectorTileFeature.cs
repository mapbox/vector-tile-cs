using Mapbox.VectorTile.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Mapbox.VectorTile.ExtensionMethods {


	public static class VectorTileFeatureExtensions {


		/// <summary>Geometry in LatLng Coordinates</summary>
		public static List<List<LatLng>> GeometryAsWgs84(
			this VectorTileFeature feature
			, ulong zoom
			, ulong tileColumn
			, ulong tileRow
			) {

			List<List<LatLng>> geometryAsWgs84 = new List<List<LatLng>>();
			foreach(var part in feature.Geometry) {
				geometryAsWgs84.Add(
					part.Select(g => g.ToLngLat(zoom, tileColumn, tileRow, feature.Layer.Extent)).ToList()
				);
			}

			return geometryAsWgs84;
		}



	}
}
