using System.Collections.Generic;
using System.Diagnostics;

namespace Mapbox.VectorTile
{


    [DebuggerDisplay("Layer {Name}")]
    public class VectorTileLayer
    {

        public VectorTileLayer()
        {
            Features = new List<VectorTileFeature>();
            _FeaturesData = new List<byte[]>();
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
            return _FeaturesData.Count;
        }

        public VectorTileFeature GetFeature(int feature)
        {
            VectorTileReader vtr = new VectorTileReader();
            return vtr.GetFeature(this, feature);
        }

        public void AddFeatureData(byte[] data)
        {
            _FeaturesData.Add(data);
        }

        public byte[] GetFeatureData(int idxFeature)
        {
            return _FeaturesData[idxFeature];
        }

        public string Name { get; set; }
        public ulong Version { get; set; }
        public ulong Extent { get; set; }
        public List<VectorTileFeature> Features { get; set; }
        private List<byte[]> _FeaturesData { get; set; }
        public List<object> Values { get; set; }
        public List<string> Keys { get; set; }


    }

}
