using System.Collections.Generic;
using Mapbox.VectorTile.Geometry;
using System;


namespace Mapbox.VectorTile {


	public class VectorTileFeature {


		/// <summary>
		/// Initialize VectorTileFeature
		/// </summary>
		/// <param name="layer">Parent <see cref="VectorTileLayer"/></param>
		public VectorTileFeature(VectorTileLayer layer, uint? clipBuffer = null, float scale = 1.0f) {
			_layer = layer;
			_clipBuffer = clipBuffer;
			_scale = scale;
			Tags = new List<int>();
		}


		private VectorTileLayer _layer;
		// TODO: how to cache without using object
		// may a dictionary with parameters clip and scale as key to keep different requests fast
		private object _cachedGeometry;
		private uint? _clipBuffer;
		private float? _scale;
		private float? _previousScale; //cache previous scale to not return


		/// <summary>Id of this feature https://github.com/mapbox/vector-tile-spec/blob/master/2.1/vector_tile.proto#L32</summary>
		public ulong Id { get; set; }


		/// <summary>Parent <see cref="VectorTileLayer"/> this feature belongs too</summary>
		public VectorTileLayer Layer { get { return _layer; } }


		/// <summary><see cref="GeomType"/> of this feature</summary>
		public GeomType GeometryType { get; set; }


		/// <summary>Geometry in internal tile coordinates</summary>
		public List<uint> GeometryCommands { get; set; }


		public List<List<Point2d<T>>> Geometry<T>(
			uint? clipBuffer = null
			, float? scale = null
		) {

			// parameters passed to this method override parameters passed to the constructor
			if (_clipBuffer.HasValue && !clipBuffer.HasValue) { clipBuffer = _clipBuffer; }
			if (_scale.HasValue && !scale.HasValue) { scale = _scale; }

			// TODO: how to cache 'finalGeom' without making whole class generic???
			// and without using an object (boxing) ???
			List<List<Point2d<T>>> finalGeom = _cachedGeometry as List<List<Point2d<T>>>;
			if (null != finalGeom && scale==_previousScale) {
				return finalGeom;
			}

			//decode commands and coordinates
			List<List<Point2d<long>>> geom = DecodeGeometry.GetGeometry(
				_layer.Extent
				, GeometryType
				, GeometryCommands
				, scale.Value
			);
			if (clipBuffer.HasValue) {
				geom = UtilGeom.ClipGeometries(geom, GeometryType, (long)_layer.Extent, clipBuffer.Value, scale.Value);
			}

			//HACK: use 'Scale' to convert to <T> too
			finalGeom = DecodeGeometry.Scale<T>(geom, scale.Value);

			//set field needed for next iteration
			_previousScale = scale;
			_cachedGeometry = finalGeom;

			return finalGeom;
		}

		/// <summary>Tags to resolve properties https://github.com/mapbox/vector-tile-spec/tree/master/2.1#44-feature-attributes</summary>
		public List<int> Tags { get; set; }


		/// <summary>
		/// Get properties of this feature. Throws exception if there is an uneven number of feature tag ids
		/// </summary>
		/// <returns>Dictionary of this feature's properties</returns>
		public Dictionary<string, object> GetProperties() {

			if (0 != Tags.Count % 2) {
				throw new Exception(string.Format("Layer [{0}]: uneven number of feature tag ids", _layer.Name));
			}
			Dictionary<string, object> properties = new Dictionary<string, object>();
			for (int i = 0; i < Tags.Count; i += 2) {
				properties.Add(_layer.Keys[Tags[i]], _layer.Values[Tags[i + 1]]);
			}
			return properties;
		}


		/// <summary>
		/// Get property by name
		/// </summary>
		/// <param name="key">Name of the property to request</param>
		/// <returns>Value of the requested property</returns>
		public object GetValue(string key) {

			var idxKey = _layer.Keys.IndexOf(key);
			if (-1 == idxKey) {
				throw new Exception(string.Format("Key [{0}] does not exist", key));
			}

			for (int i = 0; i < Tags.Count; i++) {
				if (idxKey == Tags[i]) {
					return _layer.Values[Tags[i + 1]];
				}
			}
			return null;
		}



	}
}
