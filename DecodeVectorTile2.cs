using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

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
			using (var bufferedData = new BinaryReader(File.Open(@"sample.mvt", FileMode.Open), Encoding.UTF8)) {
				var tileReader = new PBFReader(bufferedData);
				while (tileReader.Next()) {
					// see https://github.com/mapbox/vector-tile-spec/blob/master/2.1/vector_tile.proto
					// get Layer message in Tile message
					if (tileReader._tag == 3) {
						// get varint encoded length of embedded Layer message
						var layerLength = Convert.ToInt32(tileReader.Varint());
						var layerEndPos = tileReader._pos + layerLength;
						Console.WriteLine("Layer");
						while ((tileReader.Next()) && (tileReader._pos < layerEndPos)) {
							// get Layer name
							if (tileReader._tag == 1) {
								//var nameLength = Convert.ToInt32(tileReader.Varint());
								//Console.WriteLine(nameLength);
								var name = tileReader._buffer.ReadString();
								tileReader._pos += name.Length + 1;
								Console.WriteLine("name: {0}", name);
							}

							// get Layer extent
							if (tileReader._tag == 5) {
								var extent = tileReader.Varint();
								Console.WriteLine("extent: {0}", extent);
							}

							// get Layer keys
							if (tileReader._tag == 3) {
								var key = tileReader._buffer.ReadString();
								tileReader._pos += key.Length + 1;
								Console.WriteLine("key: {0}", key);
							}

							//if (tileReader._tag == 2) {
							//	if (tileReader._wireType != WireTypes.BYTES) {
							//		throw new Exception("wiretype must be BYTES");
							//	}

							//	int featuresLength = (int)tileReader.Varint();
							//	Console.WriteLine("FEATURES, length:{0}", featuresLength);
							//	tileReader._pos += featuresLength;
							//	//for (int i = 0; i < featuresLength; i++) {
							//	//	ulong coord = tileReader.Varint();
							//	//	Console.WriteLine("coord:{0}", coord);
							//	//}
							//}

							// get Layer values
							if (tileReader._tag == 4) {
								// get varint encoded length of embedded Value message
								var valueLength = Convert.ToInt32(tileReader.Varint());
								tileReader.Next();
								var valueTag = tileReader._tag;
								//Console.WriteLine("valueTag: {0}", valueTag);
								switch (valueTag) {
									case 1:
										string sval = tileReader._buffer.ReadString();
										Console.WriteLine("value: {0}", sval);
										break;
									case 2:
										float fval = tileReader._buffer.ReadSingle();
										Console.WriteLine("value: {0}", fval);
										break;
									case 3:
										double dval = tileReader._buffer.ReadDouble();
										Console.WriteLine("value: {0}", dval);
										break;
									case 4:
										Int64 i64val = tileReader._buffer.ReadInt64();
										Console.WriteLine("value: {0}", i64val);
										break;
									case 5:
										UInt64 ui64val = tileReader._buffer.ReadUInt64();
										Console.WriteLine("value: {0}", ui64val);
										break;
									//case 6:
									//	int64 si64val = tileReader._buffer.ReadUInt64();
									//	Console.WriteLine("value: {0}", ui64val);
									//	break;
									case 7:
										bool bval = tileReader._buffer.ReadBoolean();
										Console.WriteLine("value: {0}", bval);
										break;
										//default:
										//	throw new Exception(string.Format("'Value' with tag '{0}' not implemented", valueTag));
								}
							}
						}
						//} else {
						//    tileReader.Skip();
					}
				}
			}
		}

		public class PBFReader {

			public BinaryReader _buffer;
			public WireTypes _wireType = WireTypes.UNDEFINED;
			public long _length;
			public int _pos;
			public ulong _tag;
			public ulong _val;

			public PBFReader(BinaryReader tileBuffer) {
				// Initialize
				_buffer = tileBuffer;
				_length = _buffer.BaseStream.Length;
			}

			public ulong Varint() {
				// convert to base 128 varint
				// https://developers.google.com/protocol-buffers/docs/encoding
				int shift = 0;
				ulong result = 0;
				while (shift < 64) {
					byte b = _buffer.ReadByte();
					result |= (ulong)(b & 0x7F) << shift;
					_pos++;
					if ((b & 0x80) == 0) {
						return result;
					}
					shift += 7;
				}
				throw new System.ArgumentException("Invalid varint");
			}

			public bool Next() {
				// get and process the next byte in the buffer
				// return true until end of stream
				if (_pos >= 200) return false; //normally would be if (_pos >= _length)
				_val = this.Varint();
				_tag = _val >> 3;
				_wireType = (WireTypes)(_val & 0x07);
				Console.WriteLine("tag: {0}", _tag);
				return true;
			}

			public int Skip() {
				// return number of bytes to skip depending on wireType
				return 0;
			}
		}
	}
}
