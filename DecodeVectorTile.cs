using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace MVT.DecoderWilly {

	public class Decode {
		public static void Main() {
			GetMVT();
		}

		// WIRE types
		public enum WireTypes {
			VARINT = 0,// varint: int32, int64, uint32, uint64, sint32, sint64, bool, enum
			FIXED64 = 1, // 64-bit: double, fixed64, sfixed64
			BYTES = 2, // length-delimited: string, bytes, embedded messages, packed repeated fields
			FIXED32 = 5, // 32-bit: float, fixed32, sfixed32
			UNDEFINED = 99
		}

		public enum GeomType {
			UNKNOWN = 0,
			POINT = 1,
			LINESTRING = 2,
			POLYGON = 3
		}

		public enum Commands {
			MoveTo = 1,
			LineTo = 2,
			ClosePath = 7
		}

		public static void GetMVT() {

			var bufferedData = File.ReadAllBytes(@"14-8902-5666.vector.pbf");
			//var bufferedData = File.ReadAllBytes(@"sample.mvt");
			var tileReader = new PBFReader(bufferedData);

			while (tileReader.NextByte()) {
				//Debug.WriteLine("[tileReader] tag:{0} val:{1}", tileReader.Tag, tileReader.Value);
				//layers
				if (tileReader.Tag == 3) {
					Console.WriteLine("--------------------- layer -----------");
					byte[] layer = tileReader.View();
					PBFReader layerReader = new PBFReader(layer);
					ulong extent = 0;
					while (layerReader.NextByte()) {
						switch (layerReader.Tag) {
							case 15: //version
								ulong version = layerReader.Varint();
								Console.WriteLine("version: {0}", version);
								break;
							case 1: //layer name
								ulong strLength = layerReader.Varint();
								string layerName = layerReader.GetString(strLength);
								Console.WriteLine("layer name: " + layerName);
								//layerReader.Skip();
								break;
							case 5: //extent
								extent = layerReader.Varint();
								Console.WriteLine("extent: {0}", extent);
								break;
							case 3: //keys
								byte[] keyBuffer = layerReader.View();
								string key = Encoding.UTF8.GetString(keyBuffer);
								Console.WriteLine("key: " + key);
								break;
							case 4: //values
								byte[] valueBuffer = layerReader.View();
								PBFReader valReader = new PBFReader(valueBuffer);
								while (valReader.NextByte()) {
									switch (valReader.Tag) {
										case 1: //string
											byte[] stringBuffer = valReader.View();
											string value = Encoding.UTF8.GetString(stringBuffer);
											Console.WriteLine("value: " + value);
											break;
										case 2: //float
											float snglVal = valReader.GetFloat();
											Console.WriteLine("value: {0}", snglVal);
											break;
										case 3: //double
											double dblVal = valReader.GetDouble();
											Console.WriteLine("value: {0}", dblVal);
											break;
										case 4: //int64
											ulong i64 = valReader.Varint();
											Console.WriteLine("value: " + i64);
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
								PBFReader featureReader = new PBFReader(featureBuffer);
								GeomType geomType = GeomType.UNKNOWN;
								while (featureReader.NextByte()) {
									switch (featureReader.Tag) {
										case 1: //id
											ulong id = featureReader.Varint();
											Console.WriteLine("id:{0}", id);
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
											geomType = (GeomType)featureReader.Varint();
											Console.WriteLine("geomType:{0}", geomType);
											break;
										case 4: //geometry
											List<UInt32> geometry = featureReader.GetPackedUnit32();
											Console.WriteLine("geometry RAW: {0}", string.Join(",", geometry.ToArray()));
											ulong zoom = 14;
											ulong tileCol = 8902;
											ulong tileRow = 5666;
											DecodeGeometry dg = new DecodeGeometry(
												extent
												, zoom
												, tileCol
												, tileRow
												, geomType
												, geometry
											);
											List<Point2d> geom = dg.GetGeometry();
											Console.WriteLine("geometry DECODED tile coords: {0}", string.Join(",", geom.Select(g => g.ToString()).ToArray()));
											Console.WriteLine("geometry DECODED LatLng: {0}", string.Join(",", geom.Select(g => g.ToLngLat(zoom, tileCol, tileRow, extent).ToString()).ToArray()));
											break;
										default:
											featureReader.Skip();
											break;
									}
								}
								break;
							default:
								layerReader.Skip();
								break;
						}
					}
				} else {
					tileReader.Skip();
				}
			}
		}


		public class LatLng {
			public double Lat { get; set; }
			public double Lng { get; set; }

			public override string ToString() {
				return string.Format(
					"{0:0.000000}/{1:0.000000}"
					, Lat
					,Lng);
			}
		}

		public class Point2d {

			public long X { get; set; }
			public long Y { get; set; }

			public LatLng ToLngLat(ulong z, ulong x, ulong y, ulong extent) {

				double size = (double)extent * Math.Pow(2, (double)z);
				double x0 = (double)extent * (double)x;
				double y0 = (double)extent * (double)y;

				double y2 = 180 - (Y + y0) * 360 / size;
				double lng = (X + x0) * 360 / size - 180;
				double lat = 360 / Math.PI * Math.Atan(Math.Exp(y2 * Math.PI / 180)) - 90;

				LatLng latLng = new LatLng() {
					Lat = lat,
					Lng = lng
				};

				return latLng;
			}

			public override string ToString() {
				return string.Format("{0}/{1}", X, Y);
			}
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


		public class PBFReader {

			public ulong Tag { get; private set; }
			public ulong Value { get; private set; }
			public ulong Pos { get; private set; }
			public WireTypes WireType { get; private set; }

			private byte[] _buffer;
			private ulong _length;

			public PBFReader(byte[] tileBuffer) {
				// Initialize
				_buffer = tileBuffer;
				_length = (ulong)_buffer.Length;
				WireType = WireTypes.UNDEFINED;
				//Debug.WriteLine("[PBFReader] constructor, buffer length:{0}", _length);
			}

			public ulong Varint() {
				//Debug.WriteLine("[Varint()]");
				// convert to base 128 varint
				// https://developers.google.com/protocol-buffers/docs/encoding
				int shift = 0;
				ulong result = 0;
				while (shift < 64) {
					byte b = _buffer[Pos];
					result |= (ulong)(b & 0x7F) << shift;
					Pos++;
					if ((b & 0x80) == 0) {
						return result;
					}
					shift += 7;
				}
				throw new System.ArgumentException("Invalid varint");

			}


			public byte[] View() {
				//Debug.WriteLine("[View()]");
				// return layer/feature subsections of the main stream
				if (Tag == 0) {
					throw new Exception("call next() before accessing field value");
				};
				if (WireType != WireTypes.BYTES) {
					throw new Exception("not of type string, bytes or message");
				}
				ulong tmpPos = Pos;
				ulong skipBytes = Varint();
				//Debug.WriteLine("[View()] skipBytes:{0}", skipBytes);
				SkipBytes(skipBytes);

				byte[] buf = new byte[skipBytes];
				Array.Copy(_buffer, (int)Pos - (int)skipBytes, buf, 0, (int)skipBytes);
				//Array.Copy(_buffer, (int)tmpPos, buf, 0, (int)skipBytes);
				//Debug.WriteLine("[View()] returning array, length:{0}", buf.Length);
				return buf;
			}


			public List<UInt32> GetPackedUnit32() {
				List<UInt32> values = new List<uint>();
				ulong sizeInByte = Varint();
				ulong end = Pos + sizeInByte;
				while (Pos < end) {
					ulong val = Varint();
					values.Add((UInt32)val);
				}
				return values;
			}

			public double GetDouble() {
				byte[] buf = new byte[8];
				Array.Copy(_buffer, (int)Pos, buf, 0, 8);
				Pos += 8;
				double dblVal = BitConverter.ToDouble(buf, 0);
				return dblVal;
			}

			public float GetFloat() {
				byte[] buf = new byte[4];
				Array.Copy(_buffer, (int)Pos, buf, 0, 4);
				Pos += 4;
				float snglVal = BitConverter.ToSingle(buf, 0);
				return snglVal;
			}


			public string GetString(ulong length) {
				byte[] buf = new byte[length];
				Array.Copy(_buffer, (int)Pos, buf, 0, (int)length);
				Pos += length;
				return Encoding.UTF8.GetString(buf);
			}

			public bool NextByte() {
				//Debug.WriteLine("[Next()] pos:{0} len:{1}", Pos, _length);
				if (Pos >= _length) {
					return false;
				}
				// get and process the next byte in the buffer
				// return true until end of stream
				Value = Varint();
				Tag = Value >> 3;
				Debug.Assert(
					(Tag > 0 && Tag < 19000)
					|| (Tag > 19999 && Tag <= ((1 << 29) - 1)
					), "tag out of range");
				WireType = (WireTypes)(Value & 0x07);
				//Debug.WriteLine("[Next()] tag:{0} val:{1} wiretype:{2}", Tag, Value, WireType);
				return true;
			}


			public void SkipVarint() {
				//Debug.WriteLine("[SkipVarint()]");
				while (0 == (_buffer[Pos] & 0x80)) {
					//Debug.WriteLine("[SkipVarint()] incrementing _pos, pos:{0} _buffer[_pos]:{1} (_buffer[_pos] & 0x80):{2}", Pos, _buffer[Pos], (_buffer[Pos] & 0x80));
					Pos++;
				}

				//Debug.WriteLine("[SkipVarint()] pos:{0} _buffer[_pos]:{1} (_buffer[_pos] & 0x80):{2}", Pos, _buffer[Pos], (_buffer[Pos] & 0x80));
				if (Pos > _length) {
					throw new Exception("Truncated message.");
				}
			}


			public void SkipBytes(ulong skip) {
				string msg = string.Format("[SkipBytes()] skip:{0} pos:{1} len:{2}", skip, Pos, _length);
				//Debug.WriteLine(msg);
				if (Pos + skip > _length) {
					throw new Exception(msg);
				}
				Pos += skip;
			}


			public ulong Skip() {
				//Debug.WriteLine("[Skip()]");
				// return number of bytes to skip depending on wireType
				if (Tag == 0) {
					throw new Exception("call next() before calling skip()");
				}

				switch (WireType) {
					case WireTypes.VARINT:
						SkipVarint();
						break;
					case WireTypes.BYTES:
						SkipBytes(Varint());
						break;
					case WireTypes.FIXED32:
						SkipBytes(4);
						break;
					case WireTypes.FIXED64:
						SkipBytes(8);
						break;
					case WireTypes.UNDEFINED:
						throw new Exception("undefined wire type");
					default:
						throw new Exception("unknown wire type");
				}

				return Pos;
			}


		}
	}
}
