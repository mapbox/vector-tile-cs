using Mapbox.VectorTile;
using Mapbox.VectorTile.ExtensionMethods;
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
			fixturesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "test", "mvt-fixtures", "fixtures", "valid");
		}


		[Test]
		public void FeatureSinglePoint()
		{

			byte[] data = File.ReadAllBytes(Path.Combine(fixturesPath, "Feature-single-point.mvt"));
			VectorTile vt = new VectorTile(data);
			Assert.AreEqual(1, vt.LayerNames().Count, "one layer");
			VectorTileLayer lyr = vt.GetLayer(vt.LayerNames()[0]);
			Assert.AreEqual("layer_name", lyr.Name, "Layer name");
			Assert.AreEqual(1, lyr.FeatureCount(), "Feature count");
			VectorTileFeature feat = lyr.GetFeature(0);
			Assert.AreEqual(GeomType.POINT, feat.GeometryType, "Geometry type");
			Assert.AreEqual(123, feat.Id, "id");
			Dictionary<string, object> properties = feat.GetProperties();
			Assert.AreEqual("world", properties["hello"]);
			Assert.AreEqual("world", feat.GetValue("hello"));
		}

		[Test]
		public void Point3d()
		{
			VectorTileFeature feat = getFeature("Value-single-double-point.mvt");
			List<List<Point3d<int, double>>> zDouble = feat.Geometry<int, double>("double");
			Assert.AreEqual(8.999999999, zDouble[0][0].Z, 0.00001, "Wrong Z value (double)");

			feat = getFeature("Value-single-float-point.mvt");
			List<List<Point3d<int, float>>> zFloat = feat.Geometry<int, float>("float");
			Assert.AreEqual(9.000023f, zFloat[0][0].Z, 0.00001, "Wrong Z value (float)");

			feat = getFeature("Value-single-int64-point.mvt");
			List<List<Point3d<int, long>>> zLong = feat.Geometry<int, long>("int64");
			Assert.AreEqual(9223372036854775807L, zLong[0][0].Z, "Wrong Z value (long)");
		}


		private VectorTileFeature getFeature(string mvtName)
		{
			byte[] data = File.ReadAllBytes(Path.Combine(fixturesPath, mvtName));
			VectorTile vt = new VectorTile(data);
			VectorTileLayer lyr = vt.GetLayer(vt.LayerNames()[0]);
			return lyr.GetFeature(0);
		}
	}
}
