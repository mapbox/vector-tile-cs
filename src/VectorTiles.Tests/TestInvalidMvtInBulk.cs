using System;
using System.IO;
using Mapbox.VectorTile;
using System.Collections;
#if WINDOWS_UWP
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using ATestClass = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using ATestMethod = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using ATestClassSetup = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.ClassInitializeAttribute; //run once per class
using ATestSetup = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute; //run before every test
using ATestDataSource = Microsoft.VisualStudio.TestTools.UnitTesting.DataSourceAttribute;
#else
using NUnit.Framework;
using ATestClass = NUnit.Framework.TestFixtureAttribute;
using ATestMethod = NUnit.Framework.TestAttribute;
using ATestClassSetup = NUnit.Framework.OneTimeSetUpAttribute;
using ATestDataSource = NUnit.Framework.TestCaseSourceAttribute;
#endif


namespace VectorTiles.Tests {


	[ATestClass]
	public class BulkInvalidMvtTests {

		private string _fixturesPath;
		public static string _executingFolder = AppDomain.CurrentDomain.BaseDirectory;


		[ATestClassSetup]
		protected void SetUp() {
			_fixturesPath = Path.Combine(_executingFolder, "..", "..", "..", "test", "mvt-fixtures", "fixtures", "invalid");
		}

		[ATestMethod, Order(1)]
		public void FixturesPathExists() {
			Assert.True(Directory.Exists(_fixturesPath), "MVT fixtures directory exists");
		}


		[ATestMethod, ATestDataSource(typeof(GetMVTs), "GetInValidFixtureFileName")]
		public void Validate(string fileName) {
			string fullFileName = Path.Combine(_fixturesPath, fileName);
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
			string path = Path.Combine(BulkInvalidMvtTests._executingFolder, "..", "..", "..", "test", "mvt-fixtures", "fixtures", "invalid");

			foreach(var file in Directory.GetFiles(path)) {
				//return file basename only to make test description more readable
				yield return new TestCaseData(Path.GetFileName(file));
			}
		}
	}





}