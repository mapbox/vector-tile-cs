using Mapbox.VectorTile.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

#if !NET20
using System.Linq;
#endif

namespace Mapbox.VectorTile.ExtensionMethods
{


	public static class VectorTileFeatureExtensions
	{


		public static List<List<Point3d<T, T2>>> Geometry<T, T2>(
			this VectorTileFeature feature
			, string zProperty
			, uint? clipBuffer = null
			, float? scale = null
	)
		{
			if (feature.GeometryType != GeomType.POINT)
			{
				throw new Exception("Point3d only works with geometry type 'POINT'");
			}

			Dictionary<string, object> properties = feature.GetProperties();
			if (!properties.ContainsKey(zProperty))
			{
				throw new Exception(string.Format("No property [{0}]", zProperty));
			}

			List<List<Point2d<T>>> geom2d = feature.Geometry<T>(clipBuffer, scale);
			List<List<Point3d<T, T2>>> geom3d = new List<List<Point3d<T, T2>>>();

			foreach (var part2d in geom2d)
			{
				List<Point3d<T, T2>> part3d = new List<Point3d<T, T2>>();
				foreach (var pnt2d in part2d)
				{
					part3d.Add(new Point3d<T, T2>(
						pnt2d.X
						, pnt2d.Y
						, (T2)properties[zProperty]
						)
					);
				}
				geom3d.Add(part3d);
			}
			return geom3d;
		}


		/// <summary>
		/// >Geometry in LatLng coordinates instead of internal tile coordinates
		/// </summary>
		/// <param name="feature"></param>
		/// <param name="zoom">Zoom level of the tile</param>
		/// <param name="tileColumn">Column of the tile (OSM tile schema)</param>
		/// <param name="tileRow">Row of the tile (OSM tile schema)</param>
		/// <returns></returns>
		public static List<List<LatLng>> GeometryAsWgs84(
			this VectorTileFeature feature
			, ulong zoom
			, ulong tileColumn
			, ulong tileRow
			, uint? clipBuffer = null
			)
		{

			List<List<LatLng>> geometryAsWgs84 = new List<List<LatLng>>();
			foreach (var part in feature.Geometry<long>(clipBuffer, 1.0f))
			{
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
