using System.Collections.Generic;
using Mapbox.VectorTile.Geometry;

namespace Mapbox.VectorTile
{

    public class VectorTileFeature
    {

        public VectorTileFeature()
        {
            Geometry = new List<List<LatLng>>();
        }

        public ulong Id { get; set; }
        public GeomType GeometryType { get; set; }
        public List<List<LatLng>> Geometry { get; set; }
        public List<int> Tags { get; set; }
    }







}
