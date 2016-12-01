using System;
using System.ComponentModel;
//using System.IO.Compression;

namespace Mapbox.VectorTile.ExtensionMethods
{

    //DOES NOT WORK WITH UNITY
    ///// <summary>
    ///// inflates buffer, returns original buffer if not zipped
    ///// </summary>
    //public class UtilGzip {
    //	public static byte[] Inflate(byte[] buffer) {
    //		if (buffer[0] == 0x1f && buffer[1] == 0x8b) {
    //			using (GZipStream stream = new GZipStream(new MemoryStream(buffer), CompressionMode.Decompress)) {
    //				const int size = 4096;
    //				byte[] buf = new byte[size];
    //				using (MemoryStream memory = new MemoryStream()) {
    //					int count = 0;
    //					do {
    //						count = stream.Read(buf, 0, size);
    //						if (count > 0) {
    //							memory.Write(buf, 0, count);
    //						}
    //					}
    //					while (count > 0);
    //					buffer = memory.ToArray();
    //				}
    //			}
    //		}
    //		return buffer;
    //	}
    //}

    //public class UtilGzip {
    //	public static byte[] Inflate(byte[] buffer) {
    //		if (buffer[0] == 0x1f && buffer[1] == 0x8b) {
    //			using (GZipInputStream stream = new GZipInputStream(new MemoryStream(buffer))) {
    //				const int size = 4096;
    //				byte[] buf = new byte[size];
    //				using (MemoryStream memory = new MemoryStream()) {
    //					int count = 0;
    //					do {
    //						count = stream.Read(buf, 0, size);
    //						if (count > 0) {
    //							memory.Write(buf, 0, count);
    //						}
    //					}
    //					while (count > 0);
    //					buffer = memory.ToArray();
    //				}
    //			}
    //		}
    //		return buffer;
    //	}
    //}


    /// <summary>
    /// Extension method to extract the [Description] attribute from an Enum
    /// </summary>
    public static class EnumExtensions
    {
        public static string Description(this Enum value)
        {
            // variables  
            var enumType = value.GetType();
            var field = enumType.GetField(value.ToString());
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

            // return  
            return attributes.Length == 0 ? value.ToString() : ((DescriptionAttribute)attributes[0]).Description;
        }
    }
}
