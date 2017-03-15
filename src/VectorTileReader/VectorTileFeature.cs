using System.Collections.Generic;
using Mapbox.VectorTile.Geometry;
using System;

namespace Mapbox.VectorTile {

	public class VectorTileFeature {

		public VectorTileFeature(VectorTileLayer layer) {
			_Layer = layer;
			Tags = new List<int>();
		}


		private VectorTileLayer _Layer;


		/// <summary></summary>
		public ulong Id { get; set; }

		/// <summary>Layer this feature belongs too</summary>
		public VectorTileLayer Layer { get { return _Layer; } }

		/// <summary></summary>
		public GeomType GeometryType { get; set; }

		/// <summary>Geometry in Tile Coordinates</summary>
		public List<List<Point2d>> Geometry { get; set; }

		/// <summary>Tags to resolve Properties</summary>
		public List<int> Tags { get; set; }


		public Dictionary<string, object> GetProperties() {

			if (0 != Tags.Count % 2) {
				throw new Exception(string.Format("Layer [{0}]: uneven number of feature tag ids", _Layer.Name));
			}
			Dictionary<string, object> properties = new Dictionary<string, object>();
			for (int i = 0; i < Tags.Count; i += 2) {
				properties.Add(_Layer.Keys[Tags[i]], _Layer.Values[Tags[i + 1]]);
			}
			return properties;
		}


		public object GetValue(string key) {

			var idxKey = _Layer.Keys.IndexOf(key);
			if (-1 == idxKey) {
				throw new Exception(string.Format("Key [{0}] does not exist", key));
			}

			for (int i = 0; i < Tags.Count; i++) {
				if (idxKey == Tags[i]) {
					return _Layer.Values[Tags[i + 1]];
				}
			}
			return null;
		}



	}
}
