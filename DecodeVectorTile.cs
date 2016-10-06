using System;
using System.IO;
using System.Collections.Generic;

namespace MVT.Decoder
{

	public class Decode
	{
		private static FileInfo gzipFileName = new FileInfo(@"C:\Users\mateo_000\Documents\Mapbox\code\decoder\12667.mvt");
		public static void Main() {
			GetMVT();
		}

		// WIRE types
		Dictionary<string, int> wireTypes = new Dictionary<string, int> {
			{"VARINT", 0}, // varint: int32, int64, uint32, uint64, sint32, sint64, bool, enum
			{"FIXED64", 1}, // 64-bit: double, fixed64, sfixed64
			{"BYTES", 2}, // length-delimited: string, bytes, embedded messages, packed repeated fields
			{"FIXED32", 5} // 32-bit: float, fixed32, sfixed32
		};

		public static void GetMVT() {
			using (var bufferedData = new BinaryReader(File.Open(@"C:\Users\mateo_000\Documents\Mapbox\code\decoder\12667.mvt", FileMode.Open))) {
				var tileReader = new PBFReader(bufferedData);
				while (tileReader.Next()) {
					// get layer message in tile message
					if (tileReader._tag == 3) {
			            var layersData = tileReader.View();
			            var layerReader = new PBFReader(layersData);
			            //var extent = new byte[];
			            //featureViews = []

			            while (layerReader.Next()) {
			            	// get feature message in layer message
			            	if (layerReader._tag == 2) {
			                    //featureViews.append(layerReader.View())
			            	} else if (layerReader._tag == 5) {
			                    //extent = layerReader.Varint();
			            	} else {
			                    layerReader.Skip();
			            	}
			            }

			            foreach (PBFReader view in featureViews) {
			                var featureReader = new PBFReader(view);
			                while (featureReader.Next()) {
			                    if (featureReader._tag == 4) {
			                        //for f in _decode_array(feature_reader.get_packed_uint32(), extent):
			                        //    yield f
			                    } else {
			                        featureReader.Skip();
			                    }
			                }
			            }
			        } else {
			        	tileReader.Skip();
			        }
				}
			}
		}

		public class PBFReader {

			public BinaryReader _buffer;
			public int _wireType = 99;
			public long _length;
			public int _pos;
			public int _tag;
			public int _val;

			public PBFReader(BinaryReader tileBuffer)
			{
			  	// Initialize
		        _buffer = tileBuffer;
		        _length = _buffer.BaseStream.Length;
			}

			public byte[] Varint() {
				// convert to base 128 varint
				// https://developers.google.com/protocol-buffers/docs/encoding
			}

			public BinaryReader View() {
				// return layer/feature subsections of the main stream
			}

			public bool Next() {
				// get and process the next byte in the buffer
				// return true until end of stream
			}

			public int Skip() {
				// return number of bytes to skip depending on wireType
			}
		}
	}
}
