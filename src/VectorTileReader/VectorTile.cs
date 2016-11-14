using System.Collections.Generic;
using System.Linq;
using Mapbox.VectorTile.Geometry;
using System.Globalization;
using System;

namespace Mapbox.VectorTile
{

    public class VectorTile
    {

        public VectorTile(
            ulong zoom
            , ulong tileColumn
            , ulong tileRow
        )
        {
            Zoom = zoom;
            TileColumn = tileColumn;
            TileRow = tileRow;
            Layers = new List<VectorTileLayer>();
        }

        public ulong Zoom { get; set; }
        public ulong TileColumn { get; set; }
        public ulong TileRow { get; set; }

        public List<VectorTileLayer> Layers { get; set; }

        public string ToGeoJson()
        {

            //to get '.' instead of ',' when using "string.format" with double/float and non-US system number format settings
            //CultureInfo en_US = new CultureInfo("en-US");

            // escaping '{' '}' -> @"{{" "}}"
            //escaping '"' -> @""""
            string templateFeatureCollection = @"{{""type"":""FeatureCollection"",""features"":[{0}]}}";
            string templateFeature = @"{{""type"":""Feature"",""geometry"":{{""type"":""{0}"",""coordinates"":[{1}]}},""properties"":{2}}}";

            List<string> geojsonFeatures = new List<string>();

            foreach (var lyr in Layers)
            {

                foreach (var feat in lyr.Features)
                {

                    if (feat.GeometryType == GeomType.UNKNOWN) { continue; }

                    //resolve properties
                    List<string> keyValue = new List<string>();
                    for (int i = 0; i < feat.Tags.Count; i += 2)
                    {
                        string key = lyr.Keys[feat.Tags[i]];
                        object val = lyr.Values[feat.Tags[i + 1]];
                        keyValue.Add(string.Format(NumberFormatInfo.InvariantInfo, @"""{0}"":""{1}""", key, val));
                    }

                    //build geojson properties object from resolved properties
                    string geojsonProps = string.Format(
                        NumberFormatInfo.InvariantInfo
                        , @"{{""id"":{0},""lyr"":""{1}""{2}{3}}}"
                        , feat.Id
                        , lyr.Name
                        , keyValue.Count > 0 ? "," : ""
                        , string.Join(",", keyValue.ToArray())
                    );

                    //work through geometries
                    string geojsonCoords = "";
                    string geomType = feat.GeometryType.Description();

                    //multipart
                    List<List<LatLng>> geomWgs84 = feat.GeometryAsWgs84(this);
                    if (geomWgs84.Count > 1)
                    {
                        switch (feat.GeometryType)
                        {
                            case GeomType.POINT:
                                geomType = "MultiPoint";
                                geojsonCoords = string.Join(
                                    ","
                                    , geomWgs84
                                        .SelectMany((List<LatLng> g) => g)
                                        .Select(g => string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
                                );
                                break;
                            case GeomType.LINESTRING:
                                geomType = "MultiLineString";
                                List<string> parts = new List<string>();
                                foreach (var part in geomWgs84)
                                {
                                    parts.Add("[" + string.Join(
                                    ","
                                    , part.Select(g => string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
                                    ) + "]");
                                }
                                geojsonCoords = string.Join(",", parts.ToArray());
                                break;
                            case GeomType.POLYGON:
                                geomType = "MultiPolygon";
                                List<string> partsMP = new List<string>();
                                foreach (var part in geomWgs84)
                                {
                                    partsMP.Add("[" + string.Join(
                                    ","
                                    , part.Select(g => string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
                                    ) + "]");
                                }
                                geojsonCoords = "[" + string.Join(",", partsMP.ToArray()) + "]";
                                break;
                            default:
                                break;
                        }
                    } else
                    { //singlepart
                        switch (feat.GeometryType)
                        {
                            case GeomType.POINT:
                                geojsonCoords = string.Format(NumberFormatInfo.InvariantInfo, "{0},{1}", geomWgs84[0][0].Lng, geomWgs84[0][0].Lat);
                                break;
                            case GeomType.LINESTRING:
                                geojsonCoords = string.Join(
                                    ","
                                    , geomWgs84[0].Select(g => string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
                                );
                                break;
                            case GeomType.POLYGON:
                                geojsonCoords = "[" + string.Join(
                                    ","
                                    , geomWgs84[0].Select(g => string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
                                ) + "]";
                                break;
                            default:
                                break;
                        }
                    }

                    geojsonFeatures.Add(
                        string.Format(
                            NumberFormatInfo.InvariantInfo
                            , templateFeature
                            , geomType
                            , geojsonCoords
                            , geojsonProps
                        )
                    );
                }
            }

            string geoJsonFeatColl = string.Format(
                NumberFormatInfo.InvariantInfo
                , templateFeatureCollection
                , string.Join(",", geojsonFeatures.ToArray())
            );

            return geoJsonFeatColl;
        }




        public static VectorTile DecodeFully(
            ulong zoom
            , ulong tileCol
            , ulong tileRow
            , byte[] data
           )
        {
            VectorTileReader vtr = new VectorTileReader(data);
            return vtr.Decode(zoom, tileCol, tileRow, data);
        }
    }


}
