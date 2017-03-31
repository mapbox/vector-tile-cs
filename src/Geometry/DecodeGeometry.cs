using System.Collections.Generic;
using Mapbox.VectorTile.Contants;

namespace Mapbox.VectorTile.Geometry {


	/// <summary>
	/// Decode tile geometries
	/// </summary>
	public static class DecodeGeometry {

		/// <summary>
		/// <para>returns a list of lists.</para>
		/// <para>If the root list contains one child list it is a single part feature</para>
		/// <para>and the child list contains the coordinate pairs.</para>
		/// <para>e.g. single part point:</para>
		/// <para> Parent list with one child list, child list contains one Pont2D</para>
		/// <para>If the root list contains several child lists, it is a multipart feature</para>
		/// <para>e.g. multipart or donut polygon:</para>
		/// <para> Parent list contains number of list equal to the number of parts.</para>
		/// <para> Each child list contains the corrdinates of this part.</para>
		/// </summary>
		/// <param name="extent">Tile extent</param>
		/// <param name="geomType">Geometry type</param>
		/// <param name="geometryCommands"></param>
		/// <param name="scale">NOT IMPLEMENTED HERE, <see cref="Scale{T}(List{List{Point2d{long}}})>"/> </param>
		/// <returns>List<List<Point2d>> of decoded geometries (in internal tile coordinates)</returns>
		public static List<List<Point2d<long>>> GetGeometry(
			ulong extent
			, GeomType geomType
			, List<uint> geometryCommands
			, float scale = 1.0f
		) {

			List<List<Point2d<long>>> geomOut = new List<List<Point2d<long>>>();
			List<Point2d<long>> geomTmp = new List<Point2d<long>>();
			long cursorX = 0;
			long cursorY = 0;
			long tileExtent = (long)extent;

			for (int i = 0; i < geometryCommands.Count; i++) {

				uint g = geometryCommands[i];
				Commands cmd = (Commands)(g & 0x7);
				uint cmdCount = g >> 3;

				if (cmd == Commands.MoveTo || cmd == Commands.LineTo) {
					for (int j = 0; j < cmdCount; j++) {
						Point2d<long> delta = zigzagDecode(geometryCommands[i + 1], geometryCommands[i + 2]);
						cursorX += delta.X;
						cursorY += delta.Y;
						i += 2;
						//end of part of multipart feature
						if (cmd == Commands.MoveTo && geomTmp.Count > 0) {
							geomOut.Add(geomTmp);
							geomTmp = new List<Point2d<long>>();
						}

						//Point2d pntTmp = new Point2d(cursorX, cursorY);
						Point2d<long> pntTmp = new Point2d<long>() {
							X = cursorX,
							Y = cursorY
						};
						geomTmp.Add(pntTmp);
					}
				}
				if (cmd == Commands.ClosePath) {
					if (geomType == GeomType.POLYGON && geomTmp.Count > 0) {
						geomTmp.Add(geomTmp[0]);
					}
				}
			}

			if (geomTmp.Count > 0) {
				geomOut.Add(geomTmp);
			}

			return geomOut;
		}


		public static List<List<Point2d<T>>> Scale<T>(
			List<List<Point2d<long>>> inGeom
			, float scale = 1.0f
		) {

			List<List<Point2d<T>>> outGeom = new List<List<Point2d<T>>>();
			foreach (var inPart in inGeom) {
				List<Point2d<T>> outPart = new List<Point2d<T>>();
				foreach (var inVertex in inPart) {
					Point2d<float> tmp = new Point2d<float>() {
						X = ((float)inVertex.X) * scale,
						Y = ((float)inVertex.Y) * scale
					};
					outPart.Add((Point2d<T>)tmp);
				}
				outGeom.Add(outPart);
			}

			return outGeom;
		}

		private static Point2d<long> zigzagDecode(long x, long y) {

			//TODO: verify speed improvements using
			// new Point2d(){X=x, Y=y} instead of
			// new Point3d(x, y);

			//return new Point2d(
			//    ((x >> 1) ^ (-(x & 1))),
			//    ((y >> 1) ^ (-(y & 1)))
			//);
			return new Point2d<long>() {
				X = ((x >> 1) ^ (-(x & 1))),
				Y = ((y >> 1) ^ (-(y & 1)))
			};
		}


	}
}
