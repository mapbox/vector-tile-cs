using System;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Mapbox.VectorTile;
using System.Collections;

namespace VectorTiles.Tests
{
    [TestFixture]
    public class TestData
    {

        private string fixturesPath;


        [OneTimeSetUp]
        protected void SetUp()
        {
            fixturesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "test", "mvt-fixtures", "fixtures", "valid");
        }

        [Test]
        public void FixturesPathExists()
        {
            Assert.True(Directory.Exists(fixturesPath));
        }


        [Test, TestCaseSource(typeof(GetMVTs),"GetFixtureFileName")]
        public void TestAllFixtures(string fileName)
        {
            Assert.True(File.Exists(fileName));
            byte[] data = File.ReadAllBytes(fileName);
            VectorTile vt = VectorTileReader.Decode(0, 0, 0, data);
            Assert.GreaterOrEqual(vt.Layers.Count, 1);
        }


    }


    public class GetMVTs
    {
        public static IEnumerable GetFixtureFileName()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "test", "mvt-fixtures", "fixtures", "valid");

            foreach (var file in Directory.GetFiles(path))
            {
                yield return new TestCaseData( file);
            }
        }
    }





}