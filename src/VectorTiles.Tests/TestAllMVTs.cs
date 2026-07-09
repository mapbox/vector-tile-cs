using System;
using System.IO;
using Mapbox.VectorTile;
using System.Collections;
using Newtonsoft.Json.Linq;
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


namespace VectorTiles.Tests
{


	[ATestClass]
	public class BulkMvtTests
	{

		private string _fixturesPath;
		public static string _executingFolder = AppDomain.CurrentDomain.BaseDirectory;


		[ATestClassSetup]
		protected void SetUp()
		{
			_fixturesPath = Path.Combine(_executingFolder, "..", "test", "mvt-fixtures", "fixtures");
		}

		[ATestMethod, Order(1)]
		public void FixturesPathExists()
		{
			Assert.True(Directory.Exists(_fixturesPath), "MVT fixtures directory exists");
		}


		[ATestMethod, ATestDataSource(typeof(GetMVTs), "GetValidFixtureFileNames")]
		public void ValidMvt(string fileName)
		{
			Assert.True(File.Exists(fileName), "Vector tile exists");
			byte[] data = File.ReadAllBytes(fileName);
			Assert.DoesNotThrow(() =>
		   {
			   VectorTile vt = new VectorTile(data);
			   foreach (var layerName in vt.LayerNames())
			   {
				   var layer = vt.GetLayer(layerName);
				   for (int i = 0; i < layer.FeatureCount(); i++)
				   {
					   var feat = layer.GetFeature(i);
					   feat.GetProperties();
				   }
			   }
		   });
		}


		[ATestMethod, ATestDataSource(typeof(GetMVTs), "GetInvalidFixtureFileNames")]
		public void InvalidMvt(string fileName)
		{
			Assert.True(File.Exists(fileName), "Vector tile exists");
			byte[] data = File.ReadAllBytes(fileName);
			bool didThrow = true;
			Assert.Throws(Is.InstanceOf<Exception>(), () =>
			{
				VectorTile vt = new VectorTile(data);
				foreach (var layerName in vt.LayerNames())
				{
					var layer = vt.GetLayer(layerName);
					for (int i = 0; i < layer.FeatureCount(); i++)
					{
						var feat = layer.GetFeature(i);
						feat.GetProperties();
					}
				}
				didThrow = false;
			});
			Assert.True(didThrow);
		}


	}




	public partial class GetMVTs
	{
		public static IEnumerable GetValidFixtureFileNames()
		{
			foreach (var testCase in getFixtureFileNames(true))
			{
				yield return testCase;
			}
		}

		public static IEnumerable GetInvalidFixtureFileNames()
		{
			foreach (var testCase in getFixtureFileNames(false))
			{
				yield return testCase;
			}
		}

		private static IEnumerable getFixtureFileNames(bool valid)
		{
			string path = Path.Combine(BulkMvtTests._executingFolder, "..", "test", "mvt-fixtures", "fixtures");

			foreach (var fixtureDir in Directory.GetDirectories(path))
			{
				string infoJson = Path.Combine(fixtureDir, "info.json");
				if (!File.Exists(infoJson)) { continue; }
				dynamic info = JObject.Parse(File.ReadAllText(infoJson));
				if (info.validity.v2 != valid) { continue; }
				yield return new TestCaseData(Path.Combine(fixtureDir, "tile.mvt"));
			}
		}


	}





}