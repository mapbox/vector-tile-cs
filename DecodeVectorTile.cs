using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

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

		public static void GetMVT() {

			var bufferedData = File.ReadAllBytes(@"sample.mvt");
			var tileReader = new PBFReader(bufferedData);

			while (tileReader.Next()) {

				Debug.WriteLine("[tileReader] tag:{0} val:{1}", tileReader.Tag, tileReader.Value);

				// get layer message in tile message
				if (tileReader.Tag == 3) {//3=layer ??? https://github.com/mapbox/vector-tile-spec/blob/master/2.1/vector_tile.proto#L75

					var layersData = tileReader.View();
					var layerReader = new PBFReader(layersData);
					List<byte[]> featureViews = new List<byte[]>();

					while (layerReader.Next()) {
						Debug.WriteLine("[layerReader] tag:{0} val:{1}", layerReader.Tag, layerReader.Value);

						switch (layerReader.Tag) {
							case 1:
								string lyrName = layerReader.GetString(layerReader.Value);
								Debug.WriteLine("Layer name:{0}", lyrName);
								break;
							//default:
							//	layerReader.Skip();
							//	break;
						}

						// get feature message in layer message
						//if (layerReader.Tag == 2) {//2=feature??? https://github.com/mapbox/vector-tile-spec/blob/master/2.1/vector_tile.proto#L60
						//	featureViews.Add(layerReader.View());
						//} else if (layerReader.Tag == 5) {
						//	//extent = layerReader.Varint();
						//} else {
						//	layerReader.Skip();
						//}
					}

					//foreach (PBFReader view in featureViews) {
					//	var featureReader = new PBFReader(view);
					//	while (featureReader.Next()) {
					//		if (featureReader._tag == 4) {
					//			//for f in _decode_array(feature_reader.get_packed_uint32(), extent):
					//			//    yield f
					//		} else {
					//			featureReader.Skip();
					//		}
					//	}
					//}
				} //else {
				//	tileReader.Skip();
				//}
			}
		}

		public class PBFReader {

			public ulong Tag { get; private set; }
			public ulong Value { get; private set; }
			public ulong Pos { get; private set; }

			private byte[] _buffer;
			private WireTypes _wireType = WireTypes.UNDEFINED;
			private ulong _length;

			public PBFReader(byte[] tileBuffer) {
				// Initialize
				_buffer = tileBuffer;
				_length = (ulong)_buffer.Length;
				Debug.WriteLine("[PBFReader] constructor, buffer length:{0}", _length);
			}

			public ulong Varint() {
				Debug.WriteLine("[Varint()]");
				// convert to base 128 varint
				// https://developers.google.com/protocol-buffers/docs/encoding
				//int result = 0;
				//int shift = 0;
				//while (1 == 1) {
				//	byte b = _buffer[Pos];
				//	result |= ((b & 0x7f) << shift);
				//	Pos++;
				//	Debug.WriteLine("[Varint()] pos:{0} result:{1} b:{2}", Pos, result, b);
				//	if (0 == (b & 0x80)) {
				//		return result;
				//	}
				//	shift += 7;
				//	if (shift >= 64) {
				//		throw new Exception("Too many bytes when decoding varint.");
				//	}
				//}
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
				Debug.WriteLine("[View()]");
				// return layer/feature subsections of the main stream
				if (Tag == 0) {
					throw new Exception("call next() before accessing field value");
				};
				if (_wireType != WireTypes.BYTES) {
					throw new Exception("not of type string, bytes or message");
				}
				ulong tmpPos = Pos;
				ulong skipBytes = Varint();
				Debug.WriteLine("[View()] skipBytes:{0}", skipBytes);
				SkipBytes(skipBytes);

				byte[] buf = new byte[skipBytes];
				Array.Copy(_buffer, (int)Pos - (int)skipBytes, buf, 0, (int)skipBytes);
				//Array.Copy(_buffer, (int)tmpPos, buf, 0, (int)skipBytes);
				Debug.WriteLine("[View()] returning array, length:{0}", buf.Length);
				return buf;
			}


			public string GetString(ulong length) {
				byte[] buf = new byte[length];
				Array.Copy(_buffer, (int)Pos, buf, 0, (int)length);
				Pos += length;
				return System.Text.Encoding.UTF8.GetString(buf);
			}

			public bool Next() {
				Debug.WriteLine("[Next()] pos:{0} len:{1}", Pos, _length);
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
				_wireType = (WireTypes)(Value & 0x07);
				Debug.WriteLine("[Next()] tag:{0} val:{1} wiretype:{2}", Tag, Value, _wireType);
				return true;
			}


			public void SkipVarint() {
				Debug.WriteLine("[SkipVarint()]");
				while (0 == (_buffer[Pos] & 0x80)) {
					Debug.WriteLine("[SkipVarint()] incrementing _pos, pos:{0} _buffer[_pos]:{1} (_buffer[_pos] & 0x80):{2}", Pos, _buffer[Pos], (_buffer[Pos] & 0x80));
					Pos++;
				}

				Debug.WriteLine("[SkipVarint()] pos:{0} _buffer[_pos]:{1} (_buffer[_pos] & 0x80):{2}", Pos, _buffer[Pos], (_buffer[Pos] & 0x80));
				if (Pos > _length) {
					throw new Exception("Truncated message.");
				}
			}


			public void SkipBytes(ulong skip) {
				string msg = string.Format("[SkipBytes()] skip:{0} pos:{1} len:{2}", skip, Pos, _length);
				Debug.WriteLine(msg);
				if (Pos + skip > _length) {
					throw new Exception(msg);
				}
				Pos += skip;
			}


			public ulong Skip() {
				Debug.WriteLine("[Skip()]");
				// return number of bytes to skip depending on wireType
				if (Tag == 0) {
					throw new Exception("call next() before calling skip()");
				}

				switch (_wireType) {
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
