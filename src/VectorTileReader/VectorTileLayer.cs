using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mapbox.VectorTile
{


    [DebuggerDisplay("Layer {Name}")]
    public class VectorTileLayer
    {

        public VectorTileLayer()
        {
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
            return VectorTileReader.GetFeature(this, _FeaturesData[feature]);
        }

        public void AddFeatureData(byte[] data)
        {
            _FeaturesData.Add(data);
        }

        public string Name { get; set; }

        public ulong Version { get; set; }

        public ulong Extent { get; set; }

        private List<byte[]> _FeaturesData { get; set; }

        /// <summary>
        /// TODO: switch to 'dynamic' when Unity supports .Net 4.5
        /// </summary>
        public List<object> Values { get; set; }

        public List<string> Keys { get; set; }


    }

}
