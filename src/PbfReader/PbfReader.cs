using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;



namespace Mapbox.VectorTile
{

    public struct DataView
    {
        public ulong start;
        public ulong end;
    }


    public class PbfReader
    {

        public int Tag { get; private set; }
        public ulong Value { get; private set; }
        public ulong Pos { get; private set; }
        public WireTypes WireType { get; private set; }

        private byte[] _buffer;
        private ulong _length;

        public PbfReader(byte[] tileBuffer)
        {
            // Initialize
            _buffer = tileBuffer;
            _length = (ulong)_buffer.Length;
            WireType = WireTypes.UNDEFINED;
        }

        public ulong Varint()
        {
            // convert to base 128 varint
            // https://developers.google.com/protocol-buffers/docs/encoding
            int shift = 0;
            ulong result = 0;
            while (shift < 64)
            {
                byte b = _buffer[Pos];
                result |= (ulong)(b & 0x7F) << shift;
                Pos++;
                if ((b & 0x80) == 0)
                {
                    return result;
                }
                shift += 7;
            }
            throw new System.ArgumentException("Invalid varint");

        }


        public byte[] View()
        {
            // return layer/feature subsections of the main stream
            if (Tag == 0)
            {
                throw new Exception("call next() before accessing field value");
            };
            if (WireType != WireTypes.BYTES)
            {
                throw new Exception("not of type string, bytes or message");
            }

            ulong tmpPos = Pos;
            ulong skipBytes = Varint();
            SkipBytes(skipBytes);

            byte[] buf = new byte[skipBytes];
            Array.Copy(_buffer, (int)Pos - (int)skipBytes, buf, 0, (int)skipBytes);

            return buf;
        }


        public List<uint> GetPackedUnit32()
        {
            List<uint> values = new List<uint>(200);
            ulong sizeInByte = Varint();
            ulong end = Pos + sizeInByte;
            while (Pos < end)
            {
                ulong val = Varint();
                values.Add((uint)val);
            }
            return values;
        }

        public double GetDouble()
        {
            byte[] buf = new byte[8];
            Array.Copy(_buffer, (int)Pos, buf, 0, 8);
            Pos += 8;
            double dblVal = BitConverter.ToDouble(buf, 0);
            return dblVal;
        }

        public float GetFloat()
        {
            byte[] buf = new byte[4];
            Array.Copy(_buffer, (int)Pos, buf, 0, 4);
            Pos += 4;
            float snglVal = BitConverter.ToSingle(buf, 0);
            return snglVal;
        }


        public string GetString(ulong length)
        {
            byte[] buf = new byte[length];
            Array.Copy(_buffer, (int)Pos, buf, 0, (int)length);
            Pos += length;
            return Encoding.UTF8.GetString(buf);
        }

        public bool NextByte()
        {
            if (Pos >= _length)
            {
                return false;
            }
            // get and process the next byte in the buffer
            // return true until end of stream
            Value = Varint();
            Tag = (int)Value >> 3;
            if (
                (Tag == 0 || Tag >= 19000)
                && (Tag > 19999 || Tag <= ((1 << 29) - 1))
            )
            {
                throw new Exception("tag out of range");
            }
            WireType = (WireTypes)(Value & 0x07);
            return true;
        }


        public void SkipVarint()
        {
            Varint();
            //while (0 == (_buffer[Pos] & 0x80))
            //{
            //    Pos++;
            //    if (Pos >= _length)
            //    {
            //        throw new Exception("Truncated message.");
            //    }
            //}

            //if (Pos > _length)
            //{
            //    throw new Exception("Truncated message.");
            //}
        }


        public void SkipBytes(ulong skip)
        {
            if (Pos + skip > _length)
            {
                string msg = string.Format(NumberFormatInfo.InvariantInfo, "[SkipBytes()] skip:{0} pos:{1} len:{2}", skip, Pos, _length);
                throw new Exception(msg);
            }
            Pos += skip;
        }


        public ulong Skip()
        {
            if (Tag == 0)
            {
                throw new Exception("call next() before calling skip()");
            }

            switch (WireType)
            {
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
