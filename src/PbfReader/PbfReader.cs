using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Mapbox.VectorTile {


	// WIRE types
	public enum WireTypes {
		VARINT = 0,// varint: int32, int64, uint32, uint64, sint32, sint64, bool, enum
		FIXED64 = 1, // 64-bit: double, fixed64, sfixed64
		BYTES = 2, // length-delimited: string, bytes, embedded messages, packed repeated fields
		FIXED32 = 5, // 32-bit: float, fixed32, sfixed32
		UNDEFINED = 99
	}


	public enum Commands {
		MoveTo = 1,
		LineTo = 2,
		ClosePath = 7
	}



	public class PbfReader {

		public ulong Tag { get; private set; }
		public ulong Value { get; private set; }
		public ulong Pos { get; private set; }
		public WireTypes WireType { get; private set; }

		private byte[] _buffer;
		private ulong _length;

		public PbfReader(byte[] tileBuffer) {
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
			if (
				(Tag == 0 || Tag >= 19000)
				&& (Tag > 19999 || Tag <= ((1 << 29) - 1))
			) {
				throw new Exception("tag out of range");
			}
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
