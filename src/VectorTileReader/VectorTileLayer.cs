using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mapbox.VectorTile {


	[DebuggerDisplay("Layer {Name}")]
	public class VectorTileLayer {


		public VectorTileLayer() {
			_FeaturesData = new List<byte[]>();
			Keys = new List<string>();
			Values = new List<object>();
		}


		public VectorTileLayer(byte[] data) : this() {
			Data = data;
		}


		public byte[] Data { get; private set; }


		public int FeatureCount() {
			return _FeaturesData.Count;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="feature"></param>
		/// <param name="clippBuffer">
		/// <para>'null': returns the geometries unaltered as they are in the vector tile. </para>
		/// <para>Any value >=0 clips a border with the size around the tile. </para>
		/// <para>These are not pixels but the same units as the 'extent' of the layer. </para>
		/// </param>
		/// <returns></returns>
		public VectorTileFeature GetFeature(int feature, uint? clippBuffer = null) {
			return VectorTileReader.GetFeature(this, _FeaturesData[feature], true, clippBuffer);
		}


		public void AddFeatureData(byte[] data) {
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
