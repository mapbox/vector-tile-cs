using System.Collections.Generic;

namespace Mapbox.VectorTile {


	public class VectorTileLayer {

		public VectorTileLayer() {
			Features = new List<VectorTileFeature>();
			Keys = new List<string>();
			Values = new List<object>();
		}

		public string Name { get; set; }
		public ulong Version { get; set; }
		public ulong Extent { get; set; }
		public List<VectorTileFeature> Features { get; set; }
		public List<object> Values { get; set; }
		public List<string> Keys { get; set; }
	}

}
