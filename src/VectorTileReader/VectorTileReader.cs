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

        public VectorTileReader(byte[] data = null)
        {
            if (null != data)
            {
                if (data[0] == 0x1f && data[1] == 0x8b)
                {
                    throw new Exception("Tile data is zipped");
                }

                layers(data);
            }
        }


        private Dictionary<string, byte[]> _Layers = new Dictionary<string, byte[]>();

        private void layers(byte[] data)
        {
            PbfReader tileReader = new PbfReader(data);
            while (tileReader.NextByte())
            {
                if (!Enum.IsDefined(typeof(TileType), tileReader.Tag))
                {
                    throw new Exception("Unknown tile tag: " + tileReader.Tag);
                }
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
                    if (_Layers.ContainsKey(name))
                    {
                        throw new Exception("Duplicate layer names: " + name);
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
            VectorTileLayer layer = new VectorTileLayer(data);
            PbfReader layerReader = new PbfReader(layer.Data);
            while (layerReader.NextByte())
            {
                int layerType = layerReader.Tag;
                if (!Enum.IsDefined(typeof(LayerType), layerType))
                {
                    throw new Exception("Unknown layer type: " + layerType);
                }
                switch ((LayerType)layerType)
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
                        layer.AddFeatureData(layerReader.View());
                        break;
                    default:
                        layerReader.Skip();
                        break;
                }
            }

            if (string.IsNullOrEmpty(layer.Name))
            {
                throw new Exception("Layer has no name");
            }
            if (0 == layer.Version)
            {
                throw new Exception("Layer has no version: " + layer.Name);
            }
            if (2 != layer.Version)
            {
                throw new Exception("Layer has invalid version: " + layer.Name);
            }
            if (0 == layer.Extent)
            {
                throw new Exception("Layer has no extent: " + layer.Name);
            }
            if (0 == layer.FeatureCount())
            {
                throw new Exception("Layer has no features: " + layer.Name);
            }
            //if (layer.Keys.Count != layer.Values.Count)
            //{
            //    throw new Exception(string.Format(
            //        "Number of keys and values does not match, layer:{0} keys:{1} values:{2} "
            //        , layer.Name
            //        , layer.Keys.Count
            //        , layer.Values.Count
            //        )
            //    );
            //}

            return layer;
        }


        public VectorTileFeature GetFeature(VectorTileLayer layer, int idxFeature)
        {

            byte[] data = layer.GetFeatureData(idxFeature);

            PbfReader featureReader = new PbfReader(data);
            VectorTileFeature feat = new VectorTileFeature(layer);
            bool geomTypeSet = false;
            while (featureReader.NextByte())
            {
                int featureType = featureReader.Tag;
                if (!Enum.IsDefined(typeof(FeatureType), featureType))
                {
                    throw new Exception(layer.Name + ", unknown feature type: " + featureType);
                }
                switch ((FeatureType)featureType)
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
                            throw new Exception(layer.Name + ", unknown geometry type tag " + geomType);
                        }
                        feat.GeometryType = (GeomType)geomType;
                        geomTypeSet = true;
                        break;
                    case FeatureType.Geometry:
                        if (null != feat.Geometry)
                        {
                            throw new Exception(layer.Name + ", feature already has a geometry");
                        }
                        //get raw array of commands and coordinates
                        List<uint> geometryCommands = featureReader.GetPackedUnit32();
                        //decode commands and coordinates
                        List<List<Point2d>> geom = DecodeGeometry.GetGeometry(
                            layer.Extent
                            , feat.GeometryType
                            , geometryCommands
                        );
                        feat.Geometry = geom;
                        break;
                    default:
                        featureReader.Skip();
                        break;
                }
            }

            if (!geomTypeSet)
            {
                throw new Exception(layer.Name + ", feature missing geometry type");
            }
            if (null == feat.Geometry)
            {
                throw new Exception(layer.Name + ", feature has no geometry");
            }

            return feat;
        }


        public static VectorTile Decode(byte[] data)
        {

            VectorTileReader vtr = new VectorTileReader(data);
            VectorTile tile = new VectorTile(data);

            foreach (var layerName in vtr.LayerNames())
            {
                VectorTileLayer layer = vtr.GetLayer(layerName);
                for (int i = 0; i < layer.FeatureCount(); i++)
                {
                    VectorTileFeature feature = vtr.GetFeature(layer, i);
                    layer.Features.Add(feature);
                }
                tile.Layers.Add(layer);
            }
            return tile;
        }



    }
}
