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
    public class BulkInvalidMvtTests
    {

        private string fixturesPath;


        [OneTimeSetUp]
        protected void SetUp()
        {
            fixturesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "test", "mvt-fixtures", "fixtures", "invalid");
        }

        [Test, Order(1)]
        public void FixturesPathExists()
        {
            Assert.True(Directory.Exists(fixturesPath), "MVT fixtures directory exists");
        }


        [Test, TestCaseSource(typeof(GetMVTs), "GetInValidFixtureFileName")]
        public void Validate(string fileName)
        {
            string fullFileName = Path.Combine(fixturesPath, fileName);
            Assert.True(File.Exists(fullFileName), "Vector tile exists");
            byte[] data = File.ReadAllBytes(fullFileName);
            Assert.Throws(Is.InstanceOf<Exception>(), () =>
            {
                VectorTileReader vtr = new VectorTileReader(data);
                VectorTile vt = vtr.Decode(0, 0, 0, data);
                foreach (var lyr in vt.Layers)
                {
                    foreach (var feat in lyr.Features)
                    {
                        feat.GetProperties();
                    }
                }
                //vt.Validate();
            });
        }
    }






    public partial class GetMVTs
    {
        public static IEnumerable GetInValidFixtureFileName()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "test", "mvt-fixtures", "fixtures", "invalid");

            foreach (var file in Directory.GetFiles(path))
            {
                //return file basename only to make test description more readable
                yield return new TestCaseData(Path.GetFileName(file));
            }
        }
    }





}