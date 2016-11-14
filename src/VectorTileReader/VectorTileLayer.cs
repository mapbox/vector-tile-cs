using System.Collections.Generic;

namespace Mapbox.VectorTile
{


    public class VectorTileLayer
    {

        public VectorTileLayer()
        {
            Features = new List<VectorTileFeature>();
            FeaturesData = new List<byte[]>();
            Keys = new List<string>();
            Values = new List<object>();
        }

        public VectorTileLayer(byte[] data) : this()
        {
            Data = data;
        }

        public byte[] Data { get; private set; }

        public int FeatureCount()
        {
            return FeaturesData.Count;
        }

        public VectorTileFeature GetFeature(int feature)
        {
            VectorTileReader vtr = new VectorTileReader(null);
            return vtr.GetFeature(this, feature);
        }

        public string Name { get; set; }
        public ulong Version { get; set; }
        public ulong Extent { get; set; }
        public List<VectorTileFeature> Features { get; set; }
        public List<byte[]> FeaturesData { get; set; }
        public List<object> Values { get; set; }
        public List<string> Keys { get; set; }


    }

}
