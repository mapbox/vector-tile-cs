using Mapbox.VectorTile;
using Mapbox.VectorTile.Geometry;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace VectorTiles.Tests
{
    [TestFixture]
    public class SingleMvtTests
    {

        private string fixturesPath;

        [OneTimeSetUp]
        protected void SetUp()
        {
            fixturesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "test", "mvt-fixtures", "fixtures", "valid");
        }


        [Test]
        public void FeatureSinglePoint()
        {

            byte[] data = File.ReadAllBytes(Path.Combine(fixturesPath, "Feature-single-point.mvt"));
            VectorTileReader vtr = new VectorTileReader(data);
            VectorTile vt = vtr.Decode(0, 0, 0, data);
            Assert.AreEqual(1, vt.Layers.Count, "one layer");
            VectorTileLayer lyr = vt.Layers[0];
            Assert.AreEqual("layer_name", lyr.Name, "Layer name");
            Assert.AreEqual(1, lyr.Features.Count, "Feature count");
            VectorTileFeature feat = lyr.Features[0];
            Assert.AreEqual(GeomType.POINT, feat.GeometryType, "Geometry type");
            Assert.AreEqual(123, feat.Id, "id");
            Dictionary<string, object> properties = feat.GetProperties();
            Assert.AreEqual("world", properties["hello"]);
            Assert.AreEqual("world", feat.GetValue("hello"));
        }





    }
}
