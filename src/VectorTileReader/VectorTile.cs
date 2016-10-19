using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Mapbox.VectorTile.Geometry;
using Mapbox.VectorTile.Util;

namespace Mapbox.VectorTile {

	public class VectorTile {

		public VectorTile(
			ulong zoom
			, ulong tileColumn
			, ulong tileRow
		) {
			Zoom = zoom;
			TileColumn = tileColumn;
			TileRow = tileRow;
			Layers = new List<VectorTileLayer>();
		}

		public ulong Zoom { get; set; }
		public ulong TileColumn { get; set; }
		public ulong TileRow { get; set; }

		public List<VectorTileLayer> Layers { get; set; }
		
		public string ToGeoJson() {

			//to get '.' instead of ',' when using "string.format" with double/float and non-US system number format settings
			CultureInfo en_US = new CultureInfo("en-US");

			// escaping '{' '}' -> @"{{" "}}"
			//escaping '"' -> @""""
			string templateFeatureCollection = @"{{""type"":""FeatureCollection"",""features"":[{0}]}}";
			string templateFeature = @"{{""type"":""Feature"",""geometry"":{{""type"":""{0}"",""coordinates"":[{1}]}},""properties"":{2}}}";

			List<string> geojsonFeatures = new List<string>();

			foreach (var lyr in Layers) {

				foreach (var feat in lyr.Features) {

					if (feat.GeometryType == GeomType.UNKNOWN) { continue; }

					//resolve properties
					List<string> keyValue = new List<string>();
					for (int i = 0; i < feat.Tags.Count; i += 2) {
						string key = lyr.Keys[feat.Tags[i]];
						object val = lyr.Values[feat.Tags[i + 1]];
						keyValue.Add(string.Format(en_US, @"""{0}"":""{1}""", key, val));
					}

					//build geojson properties object from resolved properties
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
							en_US
							, templateFeature
							, geomType
							, geojsonCoords
							, geojsonProps
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


}
