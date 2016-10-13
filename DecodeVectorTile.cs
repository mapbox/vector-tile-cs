using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace MVT.Decoder {

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

						// get feature message in layer message
						if (layerReader.Tag == 2) {//2=feature??? https://github.com/mapbox/vector-tile-spec/blob/master/2.1/vector_tile.proto#L60
							featureViews.Add(layerReader.View());
						} else if (layerReader.Tag == 5) {
							//extent = layerReader.Varint();
						} else {
							layerReader.Skip();
						}
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
				} else {
					tileReader.Skip();
				}
			}
		}

		public class PBFReader {

			public int Tag { get; private set; }
			public int Value { get; private set; }

			private byte[] _buffer;
			private WireTypes _wireType = WireTypes.UNDEFINED;
			private long _length;
			private int _pos = 0;

			public PBFReader(byte[] tileBuffer) {
				// Initialize
				_buffer = tileBuffer;
				_length = _buffer.Length;
			}

			public int Varint() {
				// convert to base 128 varint
				// https://developers.google.com/protocol-buffers/docs/encoding
				int result = 0;
				int shift = 0;
				while (1 == 1) {
					byte b = _buffer[_pos];
					result |= ((b & 0x7f) << shift);
					_pos++;
					if (0 == (b & 0x80)) {
						return result;
					}
					shift += 7;
					if (shift >= 64) {
						throw new Exception("Too many bytes when decoding varint.");
					}
				}
			}

			public byte[] View() {
				// return layer/feature subsections of the main stream
				if (Tag == 0) {
					throw new Exception("call next() before accessing field value");
				};
				if (_wireType != WireTypes.BYTES) {
					throw new Exception("not of type string, bytes or message");
				}
				int skipBytes = Varint();
				SkipBytes(skipBytes);

				byte[] buf = new byte[skipBytes];
				Array.Copy(_buffer, _pos - skipBytes, buf, 0, skipBytes);
				return buf;
			}

			public bool Next() {
				Debug.WriteLine("[Next()] pos:{0} len:{1}", _pos, _length);
				if (_pos >= _length) {
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
				while (0 == (_buffer[_pos] & 0x80)) {
					Debug.WriteLine("[SkipVarint()] incrementing _pos, pos:{0} (_buffer[_pos] & 0x80):{1}", _pos, (_buffer[_pos] & 0x80));
					_pos++;
				}
				Debug.WriteLine("[SkipVarint()] pos:{0}, (_buffer[_pos] & 0x80):{1}", _pos, (_buffer[_pos] & 0x80));
				if (_pos > _length) {
					throw new Exception("Truncated message.");
				}
			}


			public void SkipBytes(int skip) {
				string msg = string.Format("[SkipBytes()] skip:{0} pos:{1} len:{2}", skip, _pos, _length);
				Debug.WriteLine(msg);
				if (_pos + skip > _length) {
					throw new Exception(msg);
				}
				_pos += skip;
			}

			public int Skip() {
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

				return _pos;
			}


		}
	}
}
