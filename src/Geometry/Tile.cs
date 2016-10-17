using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapbox.VectorTile;
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
	}


	public class Layer {

		public Layer() {
			Features = new List<Feature>();
			Keys = new List<string>();
		}

		public string Name { get; set; }
		public ulong Version { get; set; }
		public ulong Extent { get; set; }

		public List<Feature> Features { get; set; }
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

			CultureInfo en_US = new CultureInfo("en-US");
			// escaping '{' '}' -> @"{{" "}}"
			//escaping '"' -> @""""
			string templateFeatureCollection = @"{{""type"":""FeatureCollection"",""features"":[{0}]}}";
			string templateFeature = @"{{""type"":""Feature"",""geometry"":{{""type"":""{0}"",""coordinates"":[{1}]}},""properties"":{{""props"":""{2}""}}}}";

			List<string> geojsonFeatures = new List<string>();

			foreach (var lyr in Layers) {

				//Console.WriteLine(
				//	"=== Layer:{0} Version:{1} Extent:{2} Features:{3}"
				//	, lyr.Name
				//	, lyr.Version
				//	, lyr.Extent
				//	, lyr.Features.Count
				//);
				//Console.WriteLine("Keys: " + string.Join(", ", lyr.Keys.ToArray()));

				foreach (var feat in lyr.Features) {

					if (feat.Id == 0) { continue; }

					if (feat.GeometryType != GeomType.POINT) { continue; }
					//if (feat.GeometryType != GeomType.LINESTRING) { continue; }
					//if (feat.GeometryType != GeomType.POLYGON) { continue; }

					string geojsonProps = string.Format("fid:{0} lyr:{1} key;{2}", feat.Id, lyr.Name, string.Join(",", lyr.Keys));
					string geojsonCoords = "";
					string geomType = feat.GeometryType.Description();
					foreach (var geoms in feat.Geometry) {
						switch (feat.GeometryType) {
							case GeomType.UNKNOWN:
								break;
							case GeomType.POINT:
								if (geoms.Count == 1) {
									geojsonCoords = string.Format(en_US, "{0},{1}", geoms[0].Lng, geoms[0].Lat);
								} else {
									geomType = "MultiPoint";
									geojsonCoords = string.Join(
										","
										, geoms.Select(g => string.Format(en_US, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
									);
								}
								break;
							case GeomType.LINESTRING:
								//geomType = "MultiLineString";
								break;
							case GeomType.POLYGON:
								geomType = "MultiPolygon";
								break;
							default:
								break;
						}
						//geojsonCoords = string.Join(
						//	","
						//	, geoms.Select(g => string.Format(en_US, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
						//);
					}

					//templateFeature = @"""{0}"" wie geht's ""{1}""";
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

			//geojsonFeatures = geojsonFeatures.Skip(100).Take(10).ToList();
			//geojsonFeatures = geojsonFeatures.Take(20).ToList();

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
