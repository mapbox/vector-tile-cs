using System;
using System.IO;
using System.Collections.Generic;

namespace Mapbox.VectorTile {

    public class Decode {

        public static void require(bool cond) {
            if (!cond) {
                throw new Exception("values not equal");
            }
        }

        public static int Main(string[] args) {

            if (args.Length != 1) {
                Console.WriteLine("invalid number of arguments");
                return 1;
            }

            string vtIn = args[0];
            if (!File.Exists(vtIn)) {
                Console.WriteLine("file [{0}] not found", vtIn);
                return 1;
            }

            ulong zoom = 0;
            ulong tileCol = 0;
            ulong tileRow = 0;

            var bufferedData = File.ReadAllBytes(vtIn);

            VectorTile tile = VectorTileReader.Decode(
(ulong)zoom
                , (ulong)tileCol
                , (ulong)tileRow
                , (byte[])bufferedData
            );

            require(tile.Layers.Count == 1);
            List<VectorTileFeature> feats = tile.Layers[0].Features;
            require(feats.Count == 1);
            VectorTileFeature feat = feats[0];
            require(feat.GeometryType == Mapbox.VectorTile.Geometry.GeomType.POINT);
            List<List<Mapbox.VectorTile.Geometry.LatLng>> geometry = feat.Geometry;
            require(geometry.Count == 1);
            require(geometry.Count == 1);
            List<Mapbox.VectorTile.Geometry.LatLng> coord = geometry[0];
            require(coord.Count == 1);
            Mapbox.VectorTile.Geometry.LatLng pt = coord[0];
            require((pt.Lat - 84.920545) < .001);
            require((pt.Lng - -177.802734) < .001);
            return 0;
        }

    }
}
