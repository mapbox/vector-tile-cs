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
			using (var bufferedData = new BinaryReader(File.Open(@"sample.mvt", FileMode.Open))) {
				var tileReader = new PBFReader(bufferedData);
				while (tileReader.Next()) {
					// get layer message in tile message
					Debug.WriteLine("tileReader._tag: {0}", tileReader._tag);
					if (tileReader._tag == 3) {
						var layersData = tileReader.View();
						var layerReader = new PBFReader(layersData);
						//var extent = new byte[];
						//PBFReader[] featureViews = new PBFReader[];

						while (layerReader.Next()) {
							Debug.WriteLine("layerReader._tag: {0}", layerReader._tag);
							// get feature message in layer message
							if (layerReader._tag == 2) {
								//featureViews.append(layerReader.View())
							} else if (layerReader._tag == 5) {
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
		}

		public class PBFReader {

			public BinaryReader _buffer;
			public WireTypes _wireType = WireTypes.UNDEFINED;
			public long _length;
			public int _pos = 0;
			public int _tag;
			public int _val;

			public PBFReader(BinaryReader tileBuffer) {
				// Initialize
				_buffer = tileBuffer;
				_length = _buffer.BaseStream.Length;
			}

			public int Varint() {
				// convert to base 128 varint
				// https://developers.google.com/protocol-buffers/docs/encoding
				int mask = (1 << 64) - 1;
				//int result_type = long;
				int result = 0;
				int shift = 0;
				while (1 == 1) {
					byte[] buf = new byte[1];
					_buffer.BaseStream.Seek(_pos, SeekOrigin.Begin);
					int bytesRead = _buffer.Read(buf, 0, 1);
					int b = buf[0];
					result |= ((b & 0x7f) << shift);
					_pos++;
					if (0 == (b & 0x80)) {
						//result &= mask;
						return result;
					}
					shift += 7;
					if (shift >= 64) {
						throw new Exception("Too many bytes when decoding varint.");
					}
				}
			}

			public BinaryReader View() {
				// return layer/feature subsections of the main stream
				if (_tag == 0) {
					throw new Exception("call next() before accessing field value");
				};
				if (_wireType != WireTypes.BYTES) {
					throw new Exception("not of type string, bytes or message");
				}
				int skipBytes = Varint();
				SkipBytes(skipBytes);

				_buffer.BaseStream.Seek(_pos - skipBytes, SeekOrigin.Begin);
				byte[] buf = new byte[skipBytes];
				_buffer.Read(buf, 0, skipBytes);
				MemoryStream ms = new MemoryStream(buf);
				return new BinaryReader(ms);
			}

			public bool Next() {
				Debug.WriteLine("Next({0})", _pos);
				if (_pos >= _length) {
					return false;
				}
				// get and process the next byte in the buffer
				// return true until end of stream
				_val = Varint();
				_tag = _val >> 3;
				_wireType = (WireTypes)(_val & 0x07);
				return true;
			}


			public void SkipVarint() {
				_buffer.BaseStream.Seek(_pos, SeekOrigin.Begin);

				while (0 == (_buffer.ReadByte() & 0x80)) {
					_pos++;
				}
				if (_pos > _length) {
					throw new Exception("Truncated message.");
				}
			}


			public void SkipBytes(int skip) {
				if (_pos + skip > _length) {
					throw new Exception(string.Format(
						"skip:{0} pos:{1} len:{2}"
						, skip
						, _pos
						, _length
						));
				}
				_pos += skip;
			}

			public int Skip() {
				// return number of bytes to skip depending on wireType
				if (_tag == 0) {
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
