using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mapbox.VectorTile.Geometry;
using Mapbox.VectorTile.Util;

namespace Mapbox.VectorTile {


	public class VectorTileReader {

		public static VectorTile Decode(
			ulong zoom
			, ulong tileCol
			, ulong tileRow
			, byte[] bufferedData
		) {

			var tileReader = new PbfReader(bufferedData);
			VectorTile tile = new VectorTile(zoom, tileCol, tileRow);

			while (tileReader.NextByte()) {
				if (tileReader.Tag == 3) { //layer
					VectorTileLayer layer = new VectorTileLayer();
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
											layer.Values.Add(value);
											break;
										case 2: //float
											float snglVal = valReader.GetFloat();
											layer.Values.Add(snglVal);
											break;
										case 3: //double
											double dblVal = valReader.GetDouble();
											layer.Values.Add(dblVal);
											break;
										case 4: //int64
											ulong i64 = valReader.Varint();
											layer.Values.Add(i64);
											break;
										default:
											throw new Exception(string.Format(
												"NOT IMPLEMENTED valueReader.Tag:{0} valueReader.WireType:{1}"
												, valReader.Tag
												, valReader.WireType
											));
											//uncomment the following lines when not throwing!!
											//valReader.Skip();
											//break;
									}
								}
								break;
							case 2: //features
								byte[] featureBuffer = layerReader.View();
								PbfReader featureReader = new PbfReader(featureBuffer);
								VectorTileFeature feat = new VectorTileFeature();
								while (featureReader.NextByte()) {
									switch (featureReader.Tag) {
										case 1: //id
											feat.Id = featureReader.Varint();
											break;
										case 2: //tags 
											List<int> tags = featureReader.GetPackedUnit32().Select(t => (int)t).ToList();
											feat.Tags = tags;
											break;
										case 3://geomtype
											feat.GeometryType = (GeomType)featureReader.Varint();
											break;
										case 4: //geometry
											//get raw array of commands and coordinates
											List<UInt32> geometry = featureReader.GetPackedUnit32();
											//decode commands and coordinates
											List<List<Point2d>> geom = DecodeGeometry.GetGeometry(
												layer.Extent
												, tile.Zoom
												, tile.TileColumn
												, tile.TileRow
												, feat.GeometryType
												, geometry
											);
											//convert tile coordinates to LatLnt
											List<List<LatLng>> geomAsLatLng = new List<List<LatLng>>();
											foreach (var part in geom) {
												geomAsLatLng.Add(
													part.Select(g => g.ToLngLat(zoom, tileCol, tileRow, layer.Extent)).ToList()
												);
											}
											feat.Geometry.AddRange(geomAsLatLng);
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



	}
}
