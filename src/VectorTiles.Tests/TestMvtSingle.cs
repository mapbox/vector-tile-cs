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



	}
}
