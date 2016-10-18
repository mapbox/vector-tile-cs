using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Globalization;

namespace Mapbox.VectorTile.Geometry {

	public class Feature {

		public Feature() {
			Geometry = new List<List<LatLng>>();
		}

		public ulong Id { get; set; }
		public GeomType GeometryType { get; set; }
		public List<List<LatLng>> Geometry { get; set; }
		public List<int> Tags { get; set; }
	}


	public class Layer {

		public Layer() {
			Features = new List<Feature>();
			Keys = new List<string>();
			Values = new List<object>();
		}

		public string Name { get; set; }
		public ulong Version { get; set; }
		public ulong Extent { get; set; }
		public List<Feature> Features { get; set; }
		public List<object> Values { get; set; }
		public List<string> Keys { get; set; }
	}


	public class Tile {

		public Tile(
			ulong zoom
			, ulong tileColumn
			, ulong tileRow
		) {
			Zoom = zoom;
			TileColumn = tileColumn;
			TileRow = tileRow;
			Layers = new List<Layer>();
		}

		public ulong Zoom { get; set; }
		public ulong TileColumn { get; set; }
		public ulong TileRow { get; set; }

		public List<Layer> Layers { get; set; }
		
		public string ToGeoJson() {

			//to get '.' instead of ',' when using "string.format" with double/float
			CultureInfo en_US = new CultureInfo("en-US");

			// escaping '{' '}' -> @"{{" "}}"
			//escaping '"' -> @""""
			string templateFeatureCollection = @"{{""type"":""FeatureCollection"",""features"":[{0}]}}";
			string templateFeature = @"{{""type"":""Feature"",""geometry"":{{""type"":""{0}"",""coordinates"":[{1}]}},""properties"":{2}}}";

			List<string> geojsonFeatures = new List<string>();

			foreach (var lyr in Layers) {

				foreach (var feat in lyr.Features) {

					//if (feat.GeometryType != GeomType.POLYGON) { continue; }
					if (feat.GeometryType == GeomType.UNKNOWN) { continue; }

					//resolve properties
					List<string> keyValue = new List<string>();
					for (int i = 0; i < feat.Tags.Count; i += 2) {
						string key = lyr.Keys[feat.Tags[i]];
						object val = lyr.Values[feat.Tags[i + 1]];
						keyValue.Add(string.Format(en_US, @"""{0}"":""{1}""", key, val));
					}

					string geojsonProps = string.Format(
						@"{{""id"":{0},""lyr"":""{1}"",{2}}}"
						, feat.Id
						, lyr.Name
						, string.Join(",", keyValue.ToArray())
					);

					//work through geometries
					string geojsonCoords = "";
					string geomType = feat.GeometryType.Description();

					//multipart
					if (feat.Geometry.Count > 1) {
						switch (feat.GeometryType) {
							case GeomType.POINT:
								geomType = "MultiPoint";
								geojsonCoords = string.Join(
									","
									, feat.Geometry
										.SelectMany((List<LatLng> g) => g)
										.Select(g => string.Format(en_US, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
								);
								break;
							case GeomType.LINESTRING:
								geomType = "MultiLineString";
								List<string> parts = new List<string>();
								foreach (var part in feat.Geometry) {
									parts.Add("[" + string.Join(
									","
									, part.Select(g => string.Format(en_US, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
									) + "]");
								}
								geojsonCoords = string.Join(",", parts.ToArray());
								break;
							case GeomType.POLYGON:
								geomType = "MultiPolygon";
								List<string> partsMP = new List<string>();
								foreach (var part in feat.Geometry) {
									partsMP.Add("[" + string.Join(
									","
									, part.Select(g => string.Format(en_US, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
									) + "]");
								}
								geojsonCoords = "[" + string.Join(",", partsMP.ToArray()) + "]";
								break;
							default:
								break;
						}
					} else { //singlepart
						switch (feat.GeometryType) {
							case GeomType.POINT:
								geojsonCoords = string.Format(en_US, "{0},{1}", feat.Geometry[0][0].Lng, feat.Geometry[0][0].Lat);
								break;
							case GeomType.LINESTRING:
								geojsonCoords = string.Join(
									","
									, feat.Geometry[0].Select(g => string.Format(en_US, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
								);
								break;
							case GeomType.POLYGON:
								geojsonCoords = "[" + string.Join(
									","
									, feat.Geometry[0].Select(g => string.Format(en_US, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
								) + "]";
								break;
							default:
								break;
						}
					}

					geojsonFeatures.Add(
						string.Format(
							templateFeature
						, geomType
						, geojsonCoords
						, geojsonProps
						, 1
						)
					);
				}
			}

			string geoJsonFeatColl = string.Format(
				templateFeatureCollection
				, string.Join(",", geojsonFeatures.ToArray())
			);

			return geoJsonFeatColl;
		}

	}



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
