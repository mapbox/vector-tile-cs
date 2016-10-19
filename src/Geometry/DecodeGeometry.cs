using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mapbox.VectorTile.Geometry {


/// <summary>
/// Decode tile geometries
/// </summary>
public static class DecodeGeometry {

		/// <summary>
		/// <para>return a list of lists.</para>
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
		/// <param name="zoom">Zoom level</param>
		/// <param name="tileColumn">Tile column</param>
		/// <param name="tileRow">Tile row</param>
		/// <param name="geomType">Geometry type</param>
		/// <param name="geometry"></param>
		/// <returns>List<List<Point2d>></returns>
		public static List<List<Point2d>> GetGeometry(
			ulong extent
			, ulong zoom
			, ulong tileColumn
			, ulong tileRow
			, GeomType geomType
			, List<UInt32> geometry
		) {

			List<List<Point2d>> geomOut = new List<List<Point2d>>();
			List<Point2d> geomTmp = new List<Point2d>();
			long cursorX = 0;
			long cursorY = 0;

			for (int i = 0; i < geometry.Count; i++) {

				uint g = geometry[i];
				Commands cmd = (Commands)(g & 0x7);
				uint cmdCount = g >> 3;

				if (cmd == Commands.MoveTo || cmd == Commands.LineTo) {
					for (int j = 0; j < cmdCount; j++) {
						Point2d delta = zigzagDecode(geometry[i + 1], geometry[i + 2]);
						cursorX += delta.X;
						cursorY += delta.Y;
						i += 2;
						//end of part of multipart feature
						if (cmd == Commands.MoveTo && geomTmp.Count > 0) {
							geomOut.Add(geomTmp);
							geomTmp = new List<Point2d>();
						}
						geomTmp.Add(new Point2d() { X = cursorX, Y = cursorY });
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


		private static Point2d zigzagDecode(long x, long y) {

			return new Point2d() {
				X = ((x >> 1) ^ (-(x & 1))),
				Y = ((y >> 1) ^ (-(y & 1)))
			};
		}
	}
}
