using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using Mapbox.VectorTile.Geometry;

namespace Mapbox.VectorTile {


	public class VectorTileReader {



		public static Tile Decode(
			ulong zoom
			, ulong tileCol
			, ulong tileRow
			, byte[] bufferedData
		) {
			var tileReader = new PbfReader(bufferedData);
			Tile tile = new Tile(zoom, tileCol, tileRow);

			while (tileReader.NextByte()) {
				//Debug.WriteLine("[tileReader] tag:{0} val:{1}", tileReader.Tag, tileReader.Value);
				//layers
				if (tileReader.Tag == 3) {
					Layer layer = new Layer();
					byte[] layerBuffer = tileReader.View();
					PbfReader layerReader = new PbfReader(layerBuffer);
					while (layerReader.NextByte()) {
						switch (layerReader.Tag) {
							case 15: //version
								ulong version = layerReader.Varint();
								layer.Version = version;
								break;
							case 1: //layer name
								ulong strLength = layerReader.Varint();
								layer.Name = layerReader.GetString(strLength);
								break;
							case 5: //extent
								layer.Extent = layerReader.Varint();
								break;
							case 3: //keys
								byte[] keyBuffer = layerReader.View();
								string key = Encoding.UTF8.GetString(keyBuffer);
								layer.Keys.Add(key);
								break;
							case 4: //values
								byte[] valueBuffer = layerReader.View();
								PbfReader valReader = new PbfReader(valueBuffer);
								while (valReader.NextByte()) {
									switch (valReader.Tag) {
										case 1: //string
											byte[] stringBuffer = valReader.View();
											string value = Encoding.UTF8.GetString(stringBuffer);
											break;
										case 2: //float
											float snglVal = valReader.GetFloat();
											break;
										case 3: //double
											double dblVal = valReader.GetDouble();
											break;
										case 4: //int64
											ulong i64 = valReader.Varint();
											break;
										default:
											Console.WriteLine(
												"{0}!!!!!!!!!!!!!!!!!{0}NOT IMPLEMENTED{0}valReader.Tag:{1} valReader.WireType:{2}{0}!!!!!!!!!!!!!!!!!{0}"
												, Environment.NewLine
												, valReader.Tag
												, valReader.WireType
											);
											valReader.Skip();
											break;
									}
								}
								break;
							case 2: //features
								byte[] featureBuffer = layerReader.View();
								PbfReader featureReader = new PbfReader(featureBuffer);
								Feature feat = new Feature();
								while (featureReader.NextByte()) {
									switch (featureReader.Tag) {
										case 1: //id
											feat.Id = featureReader.Varint();
											break;
										case 2: //tags - not working yet
											featureReader.View();
											//byte[] tagsBuffer = featureReader.View();
											//PBFReader tagReader = new PBFReader(tagsBuffer);
											//while (tagReader.NextByte()) {
											//	Console.WriteLine("{0} {1}", tagReader.Tag, tagReader.WireType);
											//	tagReader.Skip();
											//}
											break;
										case 3://geomtype
											feat.GeometryType = (GeomType)featureReader.Varint();
											break;
										case 4: //geometry
											List<UInt32> geometry = featureReader.GetPackedUnit32();
											DecodeGeometry dg = new DecodeGeometry(
												layer.Extent
												, tile.Zoom
												, tile.TileColumn
												, tile.TileRow
												, feat.GeometryType
												, geometry
											);
											List<Point2d> geom = dg.GetGeometry();
											//Console.WriteLine("geometry DECODED tile coords: {0}", string.Join(",", geom.Select(g => g.ToString()).ToArray()));
											//Console.WriteLine("geometry DECODED LatLng: {0}", string.Join(",", geom.Select(g => g.ToLngLat(zoom, tileCol, tileRow, layer.Extent).ToString()).ToArray()));
											List<LatLng> geomAsLatLng = geom.Select(g => g.ToLngLat(zoom, tileCol, tileRow, layer.Extent)).ToList();
											feat.Geometry.Add(geomAsLatLng);
											break;
										default:
											featureReader.Skip();
											break;
									}
								}

								layer.Features.Add(feat);
								break;
							default:
								layerReader.Skip();
								break;
						}
					}

					tile.Layers.Add(layer);
				} else {
					tileReader.Skip();
				}
			}

			return tile;
		}


		public class DecodeGeometry {

			public DecodeGeometry(
				ulong extent
				, ulong zoom
				, ulong tileColumn
				, ulong tileRow
				, GeomType geomType
				, List<UInt32> geometry
			) {

				_Zoom = zoom;
				_TileColumn = tileColumn;
				_TileRow = tileRow;
				_GeomType = geomType;
				_Geometry = geometry;
			}

			private ulong _Zoom;
			private ulong _TileColumn;
			private ulong _TileRow;
			private GeomType _GeomType;
			private List<UInt32> _Geometry;

			public List<Point2d> GetGeometry() {

				List<Point2d> outGeom = new List<Point2d>();
				long cursorX = 0;
				long cursorY = 0;

				for (int i = 0; i < _Geometry.Count; i++) {

					uint g = _Geometry[i];
					Commands cmd = (Commands)(g & 0x7);
					uint cmdCount = g >> 3;

					if (cmd == Commands.MoveTo || cmd == Commands.LineTo) {
						for (int j = 0; j < cmdCount; j++) {
							Point2d pt = zigzag(_Geometry[i + 1], _Geometry[i + 2]);
							cursorX += pt.X;
							cursorY += pt.Y;
							i += 2;
							//TODO calculate real coords
							outGeom.Add(pt);
						}
					}
					if (cmd == Commands.ClosePath) {
						break;
					}

				}

				if (_GeomType == GeomType.POLYGON && outGeom.Count > 0) {
					outGeom.Add(outGeom[0]);
				}

				return outGeom;
			}


			private Point2d zigzag(long x, long y) {

				return new Point2d() {
					X = ((x >> 1) ^ (-(x & 1))),
					Y = ((y >> 1) ^ (-(y & 1)))
				};
			}
		}





	}
}
