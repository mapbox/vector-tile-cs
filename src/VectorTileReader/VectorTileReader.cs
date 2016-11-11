using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mapbox.VectorTile.Geometry;
using System.Globalization;
using System.Collections.ObjectModel;

namespace Mapbox.VectorTile
{


    public class VectorTileReader
    {

        public VectorTileReader(byte[] data)
        {
            if (data[0] == 0x1f && data[1] == 0x8b)
            {
                throw new Exception("tile data is zipped");
            }

            layers(data);
        }


        private Dictionary<string, byte[]> _Layers = new Dictionary<string, byte[]>();

        private void layers(byte[] data)
        {
            PbfReader tileReader = new PbfReader(data);
            while (tileReader.NextByte())
            {
                if (tileReader.Tag == (int)TileType.Layers)
                {
                    string name = null;
                    byte[] layerMessage = tileReader.View();
                    PbfReader layerView = new PbfReader(layerMessage);
                    while (layerView.NextByte())
                    {
                        if (layerView.Tag == (int)LayerType.Name)
                        {
                            ulong strLen = layerView.Varint();
                            name = layerView.GetString(strLen);
                        } else
                        {
                            layerView.Skip();
                        }
                    }
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new Exception("Layer missing name");
                    }
                    _Layers.Add(name, layerMessage);
                } else
                {
                    tileReader.Skip();
                }
            }
        }


        public ReadOnlyCollection<string> LayerNames()
        {
            return _Layers.Keys.ToList().AsReadOnly();
        }

        public VectorTileLayer GetLayer(string name)
        {
            if (!_Layers.ContainsKey(name))
            {
                return null;
            }

            return getLayer(_Layers[name]);
        }


        private VectorTileLayer getLayer(byte[] data)
        {
            VectorTileLayer layer = new VectorTileLayer();
            PbfReader layerReader = new PbfReader(data);
            while (layerReader.NextByte())
            {
                switch ((LayerType)layerReader.Tag)
                {
                    case LayerType.Version:
                        ulong version = layerReader.Varint();
                        layer.Version = version;
                        break;
                    case LayerType.Name:
                        ulong strLength = layerReader.Varint();
                        layer.Name = layerReader.GetString(strLength);
                        break;
                    case LayerType.Extent:
                        layer.Extent = layerReader.Varint();
                        break;
                    case LayerType.Keys:
                        byte[] keyBuffer = layerReader.View();
                        string key = Encoding.UTF8.GetString(keyBuffer);
                        layer.Keys.Add(key);
                        break;
                    case LayerType.Values:
                        byte[] valueBuffer = layerReader.View();
                        PbfReader valReader = new PbfReader(valueBuffer);
                        while (valReader.NextByte())
                        {
                            switch ((ValueType)valReader.Tag)
                            {
                                case ValueType.String:
                                    byte[] stringBuffer = valReader.View();
                                    string value = Encoding.UTF8.GetString(stringBuffer);
                                    layer.Values.Add(value);
                                    break;
                                case ValueType.Float:
                                    float snglVal = valReader.GetFloat();
                                    layer.Values.Add(snglVal);
                                    break;
                                case ValueType.Double:
                                    double dblVal = valReader.GetDouble();
                                    layer.Values.Add(dblVal);
                                    break;
                                case ValueType.Int:
                                    ulong i64 = valReader.Varint();
                                    layer.Values.Add(i64);
                                    break;
                                case ValueType.UInt:
                                    ulong u64 = valReader.Varint();
                                    layer.Values.Add(u64);
                                    break;
                                case ValueType.SInt:
                                    ulong s64 = valReader.Varint();
                                    layer.Values.Add(s64);
                                    break;
                                case ValueType.Bool:
                                    ulong b = valReader.Varint();
                                    layer.Values.Add(b == 1);
                                    break;
                                default:
                                    throw new Exception(string.Format(
                                        NumberFormatInfo.InvariantInfo
                                        , "NOT IMPLEMENTED valueReader.Tag:{0} valueReader.WireType:{1}"
                                        , valReader.Tag
                                        , valReader.WireType
                                    ));
                                    //uncomment the following lines when not throwing!!
                                    //valReader.Skip();
                                    //break;
                            }
                        }
                        break;
                    case LayerType.Features:
                        byte[] featureBuffer = layerReader.View();
                        PbfReader featureReader = new PbfReader(featureBuffer);
                        VectorTileFeature feat = new VectorTileFeature(layer);
                        bool geomTypeSet = false;
                        while (featureReader.NextByte())
                        {
                            switch ((FeatureType)featureReader.Tag)
                            {
                                case FeatureType.Id:
                                    feat.Id = featureReader.Varint();
                                    break;
                                case FeatureType.Tags:
                                    List<int> tags = featureReader.GetPackedUnit32().Select(t => (int)t).ToList();
                                    feat.Tags = tags;
                                    break;
                                case FeatureType.Type:
                                    int geomType = (int)featureReader.Varint();
                                    if (!Enum.IsDefined(typeof(GeomType), geomType))
                                    {
                                        throw new Exception("Unknown geometry type tag");
                                    }
                                    feat.GeometryType = (GeomType)geomType;
                                    geomTypeSet = true;
                                    break;
                                case FeatureType.Geometry:
                                    if (null != feat.GeometryOnTile)
                                    {
                                        throw new Exception("feature already has a geometry");
                                    }
                                    //get raw array of commands and coordinates
                                    List<uint> geometry = featureReader.GetPackedUnit32();
                                    //decode commands and coordinates
                                    List<List<Point2d>> geom = DecodeGeometry.GetGeometry(
                                        layer.Extent
                                        , tile.Zoom
                                        , tile.TileColumn
                                        , tile.TileRow
                                        , feat.GeometryType
                                        , geometry
                                    );
                                    feat.GeometryOnTile = geom;
                                    //convert tile coordinates to LatLnt
                                    List<List<LatLng>> geomAsLatLng = new List<List<LatLng>>();
                                    foreach (var part in geom)
                                    {
                                        geomAsLatLng.Add(
                                            part.Select(g => g.ToLngLat(zoom, tileCol, tileRow, layer.Extent)).ToList()
                                        );
                                    }
                                    feat.Geometry.AddRange(geomAsLatLng);
                                    break;
                                default:
                                    featureReader.Skip();
                                    break;
                            }
                        }

                        if (!geomTypeSet)
                        {
                            throw new Exception("feature missing geometry type");
                        }

                        layer.Features.Add(feat);
                        break;
                    default:
                        layerReader.Skip();
                        break;
                }
            }
            return layer;
        }


        public VectorTile Decode(
            ulong zoom
            , ulong tileCol
            , ulong tileRow
            , byte[] _Data
        )
        {


            var tileReader = new PbfReader(_Data);
            VectorTile tile = new VectorTile(zoom, tileCol, tileRow);

            while (tileReader.NextByte())
            {
                if (tileReader.Tag != 3)
                {
                    throw new Exception("unknown tile tag");
                }
                if (tileReader.Tag == (int)TileType.Layers)
                {
                    byte[] layerBuffer = tileReader.View();
                    VectorTileLayer layer = getLayer(layerBuffer);
                    tile.Layers.Add(layer);

                    //PbfReader layerReader = new PbfReader(layerBuffer);
                    //while (layerReader.NextByte())
                    //{
                    //    switch ((LayerType)layerReader.Tag)
                    //    {
                    //        case LayerType.Version:
                    //            ulong version = layerReader.Varint();
                    //            layer.Version = version;
                    //            break;
                    //        case LayerType.Name:
                    //            ulong strLength = layerReader.Varint();
                    //            layer.Name = layerReader.GetString(strLength);
                    //            break;
                    //        case LayerType.Extent:
                    //            layer.Extent = layerReader.Varint();
                    //            break;
                    //        case LayerType.Keys:
                    //            byte[] keyBuffer = layerReader.View();
                    //            string key = Encoding.UTF8.GetString(keyBuffer);
                    //            layer.Keys.Add(key);
                    //            break;
                    //        case LayerType.Values:
                    //            byte[] valueBuffer = layerReader.View();
                    //            PbfReader valReader = new PbfReader(valueBuffer);
                    //            while (valReader.NextByte())
                    //            {
                    //                switch ((ValueType)valReader.Tag)
                    //                {
                    //                    case ValueType.String:
                    //                        byte[] stringBuffer = valReader.View();
                    //                        string value = Encoding.UTF8.GetString(stringBuffer);
                    //                        layer.Values.Add(value);
                    //                        break;
                    //                    case ValueType.Float:
                    //                        float snglVal = valReader.GetFloat();
                    //                        layer.Values.Add(snglVal);
                    //                        break;
                    //                    case ValueType.Double:
                    //                        double dblVal = valReader.GetDouble();
                    //                        layer.Values.Add(dblVal);
                    //                        break;
                    //                    case ValueType.Int:
                    //                        ulong i64 = valReader.Varint();
                    //                        layer.Values.Add(i64);
                    //                        break;
                    //                    case ValueType.UInt:
                    //                        ulong u64 = valReader.Varint();
                    //                        layer.Values.Add(u64);
                    //                        break;
                    //                    case ValueType.SInt:
                    //                        ulong s64 = valReader.Varint();
                    //                        layer.Values.Add(s64);
                    //                        break;
                    //                    case ValueType.Bool:
                    //                        ulong b = valReader.Varint();
                    //                        layer.Values.Add(b == 1);
                    //                        break;
                    //                    default:
                    //                        throw new Exception(string.Format(
                    //                            NumberFormatInfo.InvariantInfo
                    //                            , "NOT IMPLEMENTED valueReader.Tag:{0} valueReader.WireType:{1}"
                    //                            , valReader.Tag
                    //                            , valReader.WireType
                    //                        ));
                    //                        //uncomment the following lines when not throwing!!
                    //                        //valReader.Skip();
                    //                        //break;
                    //                }
                    //            }
                    //            break;
                    //        case LayerType.Features:
                    //            byte[] featureBuffer = layerReader.View();
                    //            PbfReader featureReader = new PbfReader(featureBuffer);
                    //            VectorTileFeature feat = new VectorTileFeature(layer);
                    //            bool geomTypeSet = false;
                    //            while (featureReader.NextByte())
                    //            {
                    //                switch ((FeatureType)featureReader.Tag)
                    //                {
                    //                    case FeatureType.Id:
                    //                        feat.Id = featureReader.Varint();
                    //                        break;
                    //                    case FeatureType.Tags:
                    //                        List<int> tags = featureReader.GetPackedUnit32().Select(t => (int)t).ToList();
                    //                        feat.Tags = tags;
                    //                        break;
                    //                    case FeatureType.Type:
                    //                        int geomType = (int)featureReader.Varint();
                    //                        if (!Enum.IsDefined(typeof(GeomType), geomType))
                    //                        {
                    //                            throw new Exception("Unknown geometry type tag");
                    //                        }
                    //                        feat.GeometryType = (GeomType)geomType;
                    //                        geomTypeSet = true;
                    //                        break;
                    //                    case FeatureType.Geometry:
                    //                        if (null != feat.GeometryOnTile)
                    //                        {
                    //                            throw new Exception("feature already has a geometry");
                    //                        }
                    //                        //get raw array of commands and coordinates
                    //                        List<uint> geometry = featureReader.GetPackedUnit32();
                    //                        //decode commands and coordinates
                    //                        List<List<Point2d>> geom = DecodeGeometry.GetGeometry(
                    //                            layer.Extent
                    //                            , tile.Zoom
                    //                            , tile.TileColumn
                    //                            , tile.TileRow
                    //                            , feat.GeometryType
                    //                            , geometry
                    //                        );
                    //                        feat.GeometryOnTile = geom;
                    //                        //convert tile coordinates to LatLnt
                    //                        List<List<LatLng>> geomAsLatLng = new List<List<LatLng>>();
                    //                        foreach (var part in geom)
                    //                        {
                    //                            geomAsLatLng.Add(
                    //                                part.Select(g => g.ToLngLat(zoom, tileCol, tileRow, layer.Extent)).ToList()
                    //                            );
                    //                        }
                    //                        feat.Geometry.AddRange(geomAsLatLng);
                    //                        break;
                    //                    default:
                    //                        featureReader.Skip();
                    //                        break;
                    //                }
                    //            }

                    //            if (!geomTypeSet)
                    //            {
                    //                throw new Exception("feature missing geometry type");
                    //            }

                    //            layer.Features.Add(feat);
                    //            break;
                    //        default:
                    //            layerReader.Skip();
                    //            break;
                    //    }
                    //}

                } else
                {
                    tileReader.Skip();
                }
            }

            return tile;
        }



    }
}
