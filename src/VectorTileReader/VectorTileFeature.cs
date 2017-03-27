using System.Collections.Generic;
using Mapbox.VectorTile.Geometry;
using System;


namespace Mapbox.VectorTile {


	public class VectorTileFeature {


		/// <summary>
		/// Initialize VectorTileFeature
		/// </summary>
		/// <param name="layer">Parent <see cref="VectorTileLayer"/></param>
		public VectorTileFeature(VectorTileLayer layer) {
			_Layer = layer;
			Tags = new List<int>();
		}


		private VectorTileLayer _Layer;


		/// <summary>Id of this feature https://github.com/mapbox/vector-tile-spec/blob/master/2.1/vector_tile.proto#L32</summary>
		public ulong Id { get; set; }


		/// <summary>Parent <see cref="VectorTileLayer"/> this feature belongs too</summary>
		public VectorTileLayer Layer { get { return _Layer; } }


		/// <summary><see cref="GeomType"/> of this feature</summary>
		public GeomType GeometryType { get; set; }


		/// <summary>Geometry in internal tile coordinates</summary>
		public List<List<Point2d>> Geometry { get; set; }


		/// <summary>Tags to resolve properties https://github.com/mapbox/vector-tile-spec/tree/master/2.1#44-feature-attributes</summary>
		public List<int> Tags { get; set; }


		/// <summary>
		/// Get properties of this feature. Throws exception if there is an uneven number of feature tag ids
		/// </summary>
		/// <returns>Dictionary of this feature's properties</returns>
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


		/// <summary>
		/// Get property by name
		/// </summary>
		/// <param name="key">Name of the property to request</param>
		/// <returns>Value of the requested property</returns>
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
