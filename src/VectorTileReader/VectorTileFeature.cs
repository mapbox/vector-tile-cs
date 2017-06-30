using System.Collections.Generic;
using Mapbox.VectorTile.Geometry;
using System;
using System.Diagnostics;

namespace Mapbox.VectorTile
{


	public class VectorTileFeature
	{


		/// <summary>
		/// Initialize VectorTileFeature
		/// </summary>
		/// <param name="layer">Parent <see cref="VectorTileLayer"/></param>
		public VectorTileFeature(VectorTileLayer layer, uint? clipBuffer = null, float scale = 1.0f)
		{
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


		public List<int> XYZraw { get; set; }


		private int triangleAlgorithm(int left, int up, int upLeft)
		{
			return left + up - upLeft;
		}


		//public List<int> XYZ()
		public int[,] XYZ()
		{
			if (GeometryType != GeomType.XYZ)
			{
				throw new Exception("Feature has is of type GEOMETRY, no XYZ available");
			}

			int extent = (int)Layer.Extent;
			int[,] asMatrix = new int[extent + 2, extent + 2];

			int n = 0;
			for (int y = 1; y <= extent; y++)
			{
				for (int x = 1; x <= extent; x++)
				{
					asMatrix[x, y] = XYZraw[n];
					n++;
				}
			}


			for (int y = 1; y <= extent; y++)
			{
				for (int x = 1; x <= extent; x++)
				{
					int value = asMatrix[x, y];
					int left = x > 1 ? asMatrix[x - 1, y] : 0;
					int up = y > 1 ? asMatrix[x, y - 1] : 0;
					int upLeft = x > 1 && y > 1 ? asMatrix[x - 1, y - 1] : 0;
					asMatrix[x, y] = value + triangleAlgorithm(left, up, upLeft);
				}
			}

			int bleedCnt = 0;
			int extentSquared = extent * extent;
			for (int i = extentSquared; i < XYZraw.Count; i++)
			{
				bleedCnt++;
			}
			Debug.WriteLine($"bleed count: {bleedCnt}, extent: {extent}, extentSquared: {extentSquared}, XYZRaw-extentSquared: {XYZraw.Count - extentSquared}");

			//return asMatrix;
			return decodeBleed(asMatrix, extent);
		}


		private int[,] decodeBleed(int[,] tileMatrix, int extent)
		{
			//int[,] bleedMatrix = new int[extent + 2, extent + 2];

			int x = 0;
			int y = 0;
			int prev = 0;
			int idxRaw = extent * extent;

			// left column
			while (y <= extent)
			{
				//bleedMatrix[x, y] = prev = XYZraw[idxRaw] + prev;
				tileMatrix[x, y] = prev = XYZraw[idxRaw] + prev;
				y++; idxRaw++;
			}

			// bottom row
			while (x <= extent)
			{
				//bleedMatrix[x, y] = prev = XYZraw[idxRaw] + prev;
				tileMatrix[x, y] = prev = XYZraw[idxRaw] + prev;
				x++; idxRaw++;
			}

			// right column
			while (y > 0)
			{
				//bleedMatrix[x, y] = prev = XYZraw[idxRaw] + prev;
				tileMatrix[x, y] = prev = XYZraw[idxRaw] + prev;
				y--; idxRaw++;
			}
			// top row
			while (x > 0)
			{
				//bleedMatrix[x, y] = prev = XYZraw[idxRaw] + prev;
				tileMatrix[x, y] = prev = XYZraw[idxRaw] + prev;
				x--; idxRaw++;
			}

			//return bleedMatrix;
			return tileMatrix;
		}


		public List<List<Point2d<T>>> Geometry<T>(
			uint? clipBuffer = null
			, float? scale = null
		)
		{

			if (GeometryType == GeomType.XYZ)
			{
				throw new Exception("Feature is of type XYZ, no geometry available");

				//int[,] asMatrix = XYZ();
				//int width = asMatrix.GetLength(0);
				//int height = asMatrix.GetLength(1);

				//List<List<Point2d<T>>> xyzGeom = new List<List<Point2d<T>>>();
				//for (int y = 0; y < height; y++)
				//{
				//	List<Point2d<T>> row = new List<Point2d<T>>();
				//	for (int x = 0; x < width; x++)
				//	{
				//		T xt = (T)(object)x;
				//		T yt = (T)(object)y;
				//		Point2d<T> pnt = new Point2d<T>(xt, yt);
				//	}
				//}

				//return xyzGeom;
			}

			// parameters passed to this method override parameters passed to the constructor
			if (_clipBuffer.HasValue && !clipBuffer.HasValue) { clipBuffer = _clipBuffer; }
			if (_scale.HasValue && !scale.HasValue) { scale = _scale; }

			// TODO: how to cache 'finalGeom' without making whole class generic???
			// and without using an object (boxing) ???
			List<List<Point2d<T>>> finalGeom = _cachedGeometry as List<List<Point2d<T>>>;
			if (null != finalGeom && scale == _previousScale)
			{
				return finalGeom;
			}

			//decode commands and coordinates
			List<List<Point2d<long>>> geom = DecodeGeometry.GetGeometry(
				_layer.Extent
				, GeometryType
				, GeometryCommands
				, scale.Value
			);
			if (clipBuffer.HasValue)
			{
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
		public Dictionary<string, object> GetProperties()
		{

			if (0 != Tags.Count % 2)
			{
				throw new Exception(string.Format("Layer [{0}]: uneven number of feature tag ids", _layer.Name));
			}
			Dictionary<string, object> properties = new Dictionary<string, object>();
			for (int i = 0; i < Tags.Count; i += 2)
			{
				properties.Add(_layer.Keys[Tags[i]], _layer.Values[Tags[i + 1]]);
			}
			return properties;
		}


		/// <summary>
		/// Get property by name
		/// </summary>
		/// <param name="key">Name of the property to request</param>
		/// <returns>Value of the requested property</returns>
		public object GetValue(string key)
		{

			var idxKey = _layer.Keys.IndexOf(key);
			if (-1 == idxKey)
			{
				throw new Exception(string.Format("Key [{0}] does not exist", key));
			}

			for (int i = 0; i < Tags.Count; i++)
			{
				if (idxKey == Tags[i])
				{
					return _layer.Values[Tags[i + 1]];
				}
			}
			return null;
		}



	}
}
