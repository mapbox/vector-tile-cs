using System;
using System.ComponentModel;
using System.IO;

namespace Mapbox.VectorTile.Util {

	/// <summary>
	/// Extension method to extract the [Description] attribute from an Enum
	/// </summary>
	public static class EnumExtensions {
		public static string Description(this Enum value) {
			// variables  
			var enumType = value.GetType();
			var field = enumType.GetField(value.ToString());
			var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

			// return  
			return attributes.Length == 0 ? value.ToString() : ((DescriptionAttribute)attributes[0]).Description;
		}
	}
}
