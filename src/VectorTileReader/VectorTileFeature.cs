using System.Collections.Generic;
using Mapbox.VectorTile.Geometry;
using System.Linq;
using System;

namespace Mapbox.VectorTile
{

    public class VectorTileFeature
    {

        /// <summary></summary>
        public ulong Id { get; set; }
        public GeomType GeometryType { get; set; }
        /// <summary>Geometry in Tile Coordinates</summary>
        public List<List<Point2d>> Geometry { get; set; }
        /// <summary>Tags to resolve Properties</summary>
        public List<int> Tags { get; set; }


        private VectorTileLayer _Layer;


        public VectorTileFeature(VectorTileLayer layer)
        {
            _Layer = layer;
            Tags = new List<int>();
        }


        public Dictionary<string, object> GetProperties()
        {

            if (0 != Tags.Count % 2)
            {
                throw new System.Exception("uneven number of feature tag ids");
            }
            Dictionary<string, object> properties = new Dictionary<string, object>();
            for (int i = 0; i < Tags.Count; i += 2)
            {
                properties.Add(_Layer.Keys[Tags[i]], _Layer.Values[Tags[i + 1]]);
            }
            return properties;
        }


        public object GetValue(string key)
        {

            var idxKey = _Layer.Keys.IndexOf(key);
            if (-1 == idxKey)
            {
                throw new System.Exception("key does not exist");
            }

            for (int i = 0; i < Tags.Count; i++)
            {
                if (idxKey == Tags[i])
                {
                    return _Layer.Values[Tags[i + 1]];
                }
            }
            return null;
        }


        private List<List<LatLng>> _GeometryAsWgs84 = null;
        [Obsolete("This is a convenience method during early development and will be deprecated. Future clients will have to convert themselves.")]
        /// <summary>Geometry in LatLng Coordinates</summary>
        public List<List<LatLng>> GeometryAsWgs84(ulong zoom, ulong tileColumn, ulong tileRow)
        {

            if (null != _GeometryAsWgs84)
            {
                return _GeometryAsWgs84;
            }

            _GeometryAsWgs84 = new List<List<LatLng>>();
            foreach (var part in Geometry)
            {
                _GeometryAsWgs84.Add(
                    part.Select(g => g.ToLngLat(zoom, tileColumn, tileRow, _Layer.Extent)).ToList()
                );
            }

            return _GeometryAsWgs84;
        }

    }







}
