using System;
using System.ComponentModel;

namespace Mapbox.VectorTile.Geometry {

	public enum GeomType {
		UNKNOWN = 0,
		[Description("Point")]
		POINT = 1,
		[Description("LineString")]
		LINESTRING = 2,
		[Description("Polygon")]
		POLYGON = 3
	}
	
	public class LatLng {
		public double Lat { get; set; }
		public double Lng { get; set; }

		public override string ToString() {
			return string.Format(
				"{0:0.000000}/{1:0.000000}"
				, Lat
				, Lng);
		}
	}

	public class Point2d {

		public long X { get; set; }
		public long Y { get; set; }

		public LatLng ToLngLat(ulong z, ulong x, ulong y, ulong extent) {

			double size = (double)extent * Math.Pow(2, (double)z);
			double x0 = (double)extent * (double)x;
			double y0 = (double)extent * (double)y;

			double y2 = 180 - (Y + y0) * 360 / size;
			double lng = (X + x0) * 360 / size - 180;
			double lat = 360 / Math.PI * Math.Atan(Math.Exp(y2 * Math.PI / 180)) - 90;

			if (lng <= -180 || lng >= 180) {
				throw new Exception("Longitude out of range");
			}
			if (lat <= -90 || lat >= 90) {
				throw new Exception("Latitude out of range");
			}
			LatLng latLng = new LatLng() {
				Lat = lat,
				Lng = lng
			};

			return latLng;
		}

		public override string ToString() {
			return string.Format("{0}/{1}", X, Y);
		}
	}


}
