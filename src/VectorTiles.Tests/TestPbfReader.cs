using Mapbox.VectorTile.Contants;
using NUnit.Framework;


namespace VectorTiles.Tests {


	[TestFixture]
	public class PbfReaderTests {

		[Test]
		public void Constants() {
			Assert.AreEqual(0, (int)WireTypes.VARINT, "WireTypes.VARINT");
			Assert.AreEqual(1, (int)WireTypes.FIXED64, "WireTypes.FIXED64");
			Assert.AreEqual(2, (int)WireTypes.BYTES, "WireTypes.BYTES");
			Assert.AreEqual(5, (int)WireTypes.FIXED32, "WireTypes.FIXED32");
			Assert.AreEqual(99, (int)WireTypes.UNDEFINED, "WireTypes.UNDEFINED");

			Assert.AreEqual(1, (int)Commands.MoveTo, "Commands.MoveTo");
			Assert.AreEqual(2, (int)Commands.LineTo, "Commands.LineTo");
			Assert.AreEqual(7, (int)Commands.ClosePath, "Commands.ClosePath");

			Assert.AreEqual(3, (int)TileType.Layers, "TileType.Layers");

			Assert.AreEqual(15, (int)LayerType.Version, "LayerType.Version");
			Assert.AreEqual(1, (int)LayerType.Name, "LayerType.Name");
			Assert.AreEqual(2, (int)LayerType.Features, "LayerType.Features");
			Assert.AreEqual(3, (int)LayerType.Keys, "LayerType.Keys");
			Assert.AreEqual(4, (int)LayerType.Values, "LayerType.Values");
			Assert.AreEqual(5, (int)LayerType.Extent, "LayerType.Extent");

			Assert.AreEqual(1, (int)FeatureType.Id, "FeatureType.Id");
			Assert.AreEqual(2, (int)FeatureType.Tags, "FeatureType.Tags");
			Assert.AreEqual(3, (int)FeatureType.Type, "FeatureType.Type");
			Assert.AreEqual(4, (int)FeatureType.Geometry, "FeatureType.Geometry");
			Assert.AreEqual(5, (int)FeatureType.Raster, "FeatureType.Raster");

			Assert.AreEqual(1, (int)ValueType.String, "ValueType.String");
			Assert.AreEqual(2, (int)ValueType.Float, "ValueType.Float");
			Assert.AreEqual(3, (int)ValueType.Double, "ValueType.Double");
			Assert.AreEqual(4, (int)ValueType.Int, "ValueType.Int");
			Assert.AreEqual(5, (int)ValueType.UInt, "ValueType.UInt");
			Assert.AreEqual(6, (int)ValueType.SInt, "ValueType.SInt");
			Assert.AreEqual(7, (int)ValueType.Bool, "ValueType.Bool");
		}





	}

}