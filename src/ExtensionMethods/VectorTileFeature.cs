using Mapbox.VectorTile.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

#if !NET20
using System.Linq;
#endif

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
#if NET20
				List<LatLng> partAsWgs84 = new List<LatLng>();
				foreach(var partGeom in part) {
					partAsWgs84.Add(partGeom.ToLngLat(zoom, tileColumn, tileRow, feature.Layer.Extent));
				}
				geometryAsWgs84.Add(partAsWgs84);
#else
				geometryAsWgs84.Add(
					part.Select(g => g.ToLngLat(zoom, tileColumn, tileRow, feature.Layer.Extent)).ToList()
				);
#endif
			}

			return geometryAsWgs84;
		}



	}
}
