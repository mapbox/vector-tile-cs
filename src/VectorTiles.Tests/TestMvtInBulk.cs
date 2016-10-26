using System;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Mapbox.VectorTile;
using System.Collections;
using Mapbox.VectorTile.Geometry;

namespace VectorTiles.Tests
{
    [TestFixture]
    public class BulkMvtTests
    {

        private string fixturesPath;


        [OneTimeSetUp]
        protected void SetUp()
        {
            fixturesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "test", "mvt-fixtures", "fixtures", "valid");
        }

        [Test, Order(1)]
        public void FixturesPathExists()
        {
            Assert.True(Directory.Exists(fixturesPath), "MVT fixtures directory exists");
        }


        [Test, TestCaseSource(typeof(GetMVTs), "GetFixtureFileName")]
        public void AtLeastOneLayer(string fileName)
        {
            string fullFileName = Path.Combine(fixturesPath, fileName);
            Assert.True(File.Exists(fullFileName), "Vector tile exists");
            byte[] data = File.ReadAllBytes(fullFileName);
            VectorTile vt = VectorTileReader.Decode(0, 0, 0, data);
            Assert.GreaterOrEqual(vt.Layers.Count, 1, "At least one layer");
        }


        [Test, TestCaseSource(typeof(GetMVTs), "GetFixtureFileName")]
        public void IterateAllProperties(string fileName)
        {
            string fullFileName = Path.Combine(fixturesPath, fileName);
            Assert.True(File.Exists(fullFileName), "Vector tile exists");
            byte[] data = File.ReadAllBytes(fullFileName);
            VectorTile vt = VectorTileReader.Decode(0, 0, 0, data);
            foreach (var lyr in vt.Layers)
            {
                foreach (var feat in lyr.Features)
                {
                    var properties = feat.GetProperties();
                    foreach (var prop in properties)
                    {
                        Assert.IsInstanceOf<string>(prop.Key);
                        Debug.WriteLine(new { prop.Key, prop.Value });
                    }
                    foreach (var geom in feat.Geometry)
                    {
                        Debug.WriteLine(geom.Count);
                    }
                }
            }
        }
    }






    public class GetMVTs
    {
        public static IEnumerable GetFixtureFileName()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "test", "mvt-fixtures", "fixtures", "valid");

            foreach (var file in Directory.GetFiles(path))
            {
                //return file basename only to make test description more readable
                yield return new TestCaseData(Path.GetFileName(file));
            }
        }
    }





}