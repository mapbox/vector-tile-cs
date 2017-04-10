# vector-tile-cs

C# library for decoding [`Mapbox Vector Tiles @ v2.x`](https://www.mapbox.com/vector-tiles/) ([vector tile specification](https://github.com/mapbox/vector-tile-spec)).

_**Decoding tiles created according to `Mapbox Vector Tile Specification @ v1.x` is not supported!**_

Available as nuget package: [![nuget.org](https://img.shields.io/nuget/v/Mapbox.VectorTile.svg)](https://www.nuget.org/packages/Mapbox.VectorTile)

Vector tile parsers in other languages:
* JavaScript: https://github.com/mapbox/vector-tile-js
* C++: https://github.com/mapbox/vector-tile

# Build status
master branch:
* Travis: [![Build Status](https://travis-ci.com/mapbox/vector-tile-cs.svg?token=hLpUd9oZwpjSs5JzfqFa&branch=master)](https://travis-ci.com/mapbox/vector-tile-cs)
* AppVeyor: [![Build status](https://ci.appveyor.com/api/projects/status/k8u69w8u13f7i0a7/branch/master?svg=true)](https://ci.appveyor.com/project/Mapbox/vector-tile-cs/branch/master)
* Coveralls: [![Coverage Status](https://coveralls.io/repos/github/mapbox/vector-tile-cs/badge.svg?branch=master&t=q2Lud9)](https://coveralls.io/github/mapbox/vector-tile-cs?branch=master)

# Depends

Native C# implementation - no dependencies.
`NUnit` and `NUnit3TestAdapter` for running tests - will get restored automatically when building the solution.

#Example

```c#
byte[] data = //raw unzipped vectortile
VectorTile vt = new VectorTile(data);
//get available layer names
foreach(var lyrName in vt.LayerNames()) {
    //get layer by name
    VectorTileLayer lyr = vt.GetLayer(lyrName);
    //iterate through all features
    for(int i = 0; i < lyr.FeatureCount(); i++) {
        Debug.WriteLine("{0} lyr:{1} feat:{2}", fileName, lyr.Name, i);
        //get the feature
        VectorTileFeature feat = lyr.GetFeature(i);
        //get feature properties
        var properties = feat.GetProperties();
        foreach(var prop in properties) {
            Debug.WriteLine("key:{0} value:{1}", prop.Key, prop.Value);
        }
        //or get property value if you already know the key
        //object value = feat.GetValue(prop.Key);
        //iterate through all geometry parts
        //requesting coordinates as ints
        foreach(var part in feat.Geometry<int>()) {
            //iterate through coordinates of the part
            foreach(var geom in part) {
                Debug.WriteLine("geom.X:{0} geom.Y:{1}", geom.X, geom.Y);
            }
        }
    }
}

```
