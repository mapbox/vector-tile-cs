using System;
using NUnit.Framework;
using System.IO;
using Mapbox.VectorTile;
using System.Collections;


namespace VectorTiles.Tests {


	[TestFixture]
	public class BulkInvalidMvtTests {

		private string fixturesPath;


		[OneTimeSetUp]
		protected void SetUp() {
			fixturesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "test", "mvt-fixtures", "fixtures", "invalid");
		}

		[Test, Order(1)]
		public void FixturesPathExists() {
			Assert.True(Directory.Exists(fixturesPath), "MVT fixtures directory exists");
		}


		[Test, TestCaseSource(typeof(GetMVTs), "GetInValidFixtureFileName")]
		public void Validate(string fileName) {
			string fullFileName = Path.Combine(fixturesPath, fileName);
			Assert.True(File.Exists(fullFileName), "Vector tile exists");
			byte[] data = File.ReadAllBytes(fullFileName);
			Assert.Throws(Is.InstanceOf<Exception>(), () => {
				VectorTile vt = new VectorTile(data);
				foreach(var layerName in vt.LayerNames()) {
					var layer = vt.GetLayer(layerName);
					for(int i = 0; i < layer.FeatureCount(); i++) {
						var feat = layer.GetFeature(i);
						feat.GetProperties();
					}
				}
			});
		}
	}

	public partial class GetMVTs {
		public static IEnumerable GetInValidFixtureFileName() {
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "test", "mvt-fixtures", "fixtures", "invalid");

			foreach(var file in Directory.GetFiles(path)) {
				//return file basename only to make test description more readable
				yield return new TestCaseData(Path.GetFileName(file));
			}
		}
	}





}