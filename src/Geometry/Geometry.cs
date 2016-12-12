using System;
using System.ComponentModel;
using System.Globalization;

namespace Mapbox.VectorTile.Geometry
{

    public enum GeomType
    {
        UNKNOWN = 0,
        [Description("Point")]
        POINT = 1,
        [Description("LineString")]
        LINESTRING = 2,
        [Description("Polygon")]
        POLYGON = 3
    }

    public struct LatLng
    {
        public double Lat { get; set; }
        public double Lng { get; set; }

        public override string ToString()
        {
            return string.Format(
                NumberFormatInfo.InvariantInfo
                , "{0:0.000000}/{1:0.000000}"
                , Lat
                , Lng);
        }
    }

    public struct Point2d
    {

        public Point2d(long x, long y)
        {
            X = x;
            Y = y;
        }

        public long X; //performance: field instead of property
        public long Y; //performance: field instead of property

        public LatLng ToLngLat(ulong z, ulong x, ulong y, ulong extent, bool checkLatLngMax = false)
        {

            double size = (double)extent * Math.Pow(2, (double)z);
            double x0 = (double)extent * (double)x;
            double y0 = (double)extent * (double)y;

            double y2 = 180 - (Y + y0) * 360 / size;
            double lng = (X + x0) * 360 / size - 180;
            double lat = 360 / Math.PI * Math.Atan(Math.Exp(y2 * Math.PI / 180)) - 90;

            if (checkLatLngMax)
            {
                if (lng < -180 || lng > 180)
                {
                    throw new ArgumentOutOfRangeException("Longitude out of range");
                }
                if (lat < -85.051128779806589 || lat > 85.051128779806589)
                {
                    throw new ArgumentOutOfRangeException("Latitude out of range");
                }
            }

            LatLng latLng = new LatLng()
            {
                Lat = lat,
                Lng = lng
            };

            return latLng;
        }

        public override string ToString()
        {
            return string.Format(NumberFormatInfo.InvariantInfo, "{0}/{1}", X, Y);
        }
    }


}
