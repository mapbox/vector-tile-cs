using System;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Mapbox.VectorTile;
using System.Collections;
using Mapbox.VectorTile.Geometry;
using Mapbox.VectorTile.ExtensionMethods;


namespace VectorTiles.Tests {


	[TestFixture]
	public class BulkMvtTests {


		private string fixturesPath;


		[OneTimeSetUp]
		protected void SetUp() {
			fixturesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "test", "mvt-fixtures", "fixtures", "valid");
		}

		[Test, Order(1)]
		public void FixturesPathExists() {
			Assert.True(Directory.Exists(fixturesPath), "MVT fixtures directory exists");
		}


		[Test, TestCaseSource(typeof(GetMVTs), "GetFixtureFileName")]
		public void LazyDecoding(string fileName) {
			string fullFileName = Path.Combine(fixturesPath, fileName);
			Assert.True(File.Exists(fullFileName), "Vector tile exists");
			byte[] data = File.ReadAllBytes(fullFileName);
			VectorTile vt = new VectorTile(data);
			foreach(var layerName in vt.LayerNames()) {
				VectorTileLayer layer = vt.GetLayer(layerName);
				for(int i = 0; i < layer.FeatureCount(); i++) {
					VectorTileFeature<long> feat = layer.GetFeature<long>(i);
					var properties = feat.GetProperties();
					foreach(var prop in properties) {
						Assert.AreEqual(prop.Value, feat.GetValue(prop.Key), "Property values match");
					}
					foreach(var geomPart in feat.Geometry) {
						foreach(var coord in geomPart) {
							//TODO add Assert
						}
					}
				}
			}
			//string geojson = vt.ToGeoJson(0, 0, 0);
			//Assert.GreaterOrEqual(geojson.Length, 30, "geojson >= 30 chars");
		}


		[Test]
		public void DifferentPoint2dTypes() {
			string fullFileName = Path.Combine(fixturesPath, "Feature-single-linestring.mvt");
			byte[] data = File.ReadAllBytes(fullFileName);
			VectorTile vt = new VectorTile(data);
			foreach (var layerName in vt.LayerNames()) {
				VectorTileLayer layer = vt.GetLayer(layerName);
				for (int i = 0; i < layer.FeatureCount(); i++) {
//2 / 2
//2 / 10
//10 / 10
					VectorTileFeature<long> featLong = layer.GetFeature<long>(i);
					foreach (var geomPart in featLong.Geometry) {
						foreach (var coord in geomPart) {
							Debug.WriteLine("long: {0}/{1}", coord.X, coord.Y);
						}
						Assert.AreEqual(2L, geomPart[0].X);
						Assert.AreEqual(2L, geomPart[0].Y);
						Assert.AreEqual(2L, geomPart[1].X);
						Assert.AreEqual(10L, geomPart[1].Y);
						Assert.AreEqual(10L, geomPart[2].X);
						Assert.AreEqual(10L, geomPart[2].Y);
					}
					VectorTileFeature<int> featInt = layer.GetFeature<int>(i, null, 1.5f);
					foreach (var geomPart in featInt.Geometry) {
						foreach (var coord in geomPart) {
							Debug.WriteLine("integer: {0}/{1}",coord.X, coord.Y);
						}
						Assert.AreEqual(3, geomPart[0].X);
						Assert.AreEqual(3, geomPart[0].Y);
						Assert.AreEqual(3, geomPart[1].X);
						Assert.AreEqual(15, geomPart[1].Y);
						Assert.AreEqual(15, geomPart[2].X);
						Assert.AreEqual(15, geomPart[2].Y);
					}
					VectorTileFeature<float> featFloat = layer.GetFeature<float>(i, 0, 1.75f);
					foreach (var geomPart in featFloat.Geometry) {
						foreach (var coord in geomPart) {
							Debug.WriteLine("float: {0}/{1}", coord.X, coord.Y);
						}
						//TODO: verify what's going on, clipper seems to revert order
						//Assert.AreEqual(3.5f, geomPart[0].X);
						//Assert.AreEqual(3.5f, geomPart[0].Y);
						//Assert.AreEqual(3.5f, geomPart[1].X);
						//Assert.AreEqual(17.5f, geomPart[1].Y);
						//Assert.AreEqual(17.5f, geomPart[2].X);
						//Assert.AreEqual(17.5f, geomPart[2].Y);
					}

				}
			}
			//string geojson = vt.ToGeoJson(0, 0, 0);
			//Assert.GreaterOrEqual(geojson.Length, 30, "geojson >= 30 chars");
		}


		[Test, TestCaseSource(typeof(GetMVTs), "GetFixtureFileName")]
		public void AtLeastOneLayer(string fileName) {
			string fullFileName = Path.Combine(fixturesPath, fileName);
			Assert.True(File.Exists(fullFileName), "Vector tile exists");
			byte[] data = File.ReadAllBytes(fullFileName);
			VectorTile vt = new VectorTile(data);
			Assert.GreaterOrEqual(vt.LayerNames().Count, 1, "At least one layer");
			//string geojson = vt.ToGeoJson(0, 0, 0);
			//Assert.GreaterOrEqual(geojson.Length, 30, "geojson >= 30 chars");
			foreach(var lyrName in vt.LayerNames()) {
				VectorTileLayer lyr = vt.GetLayer(lyrName);
				for(int i = 0; i < lyr.FeatureCount(); i++) {
					Debug.WriteLine("{0} lyr:{1} feat:{2}", fileName, lyr.Name, i);
					VectorTileFeature<long> feat = lyr.GetFeature<long>(i);
					long extent = (long)lyr.Extent;
					foreach(var part in feat.Geometry) {
						foreach(var geom in part) {
							if(geom.X < 0 || geom.Y < 0 || geom.X > extent || geom.Y > extent) {
								Debug.WriteLine("{0} lyr:{1} feat:{2} x:{3} y:{4}", fileName, lyr.Name, i, geom.X, geom.Y);
							}
						}
					}
				}
			}
		}


		[Test, TestCaseSource(typeof(GetMVTs), "GetFixtureFileName")]
		public void ClipHardAtTileBoundary(string fileName) {
			string fullFileName = Path.Combine(fixturesPath, fileName);
			Assert.True(File.Exists(fullFileName), "Vector tile exists");
			byte[] data = File.ReadAllBytes(fullFileName);
			VectorTile vt = new VectorTile(data);
			Assert.GreaterOrEqual(vt.LayerNames().Count, 1, "At least one layer");
			//string geojson = vt.ToGeoJson(0, 0, 0);
			//Assert.GreaterOrEqual(geojson.Length, 30, "geojson >= 30 chars");
			foreach(var lyrName in vt.LayerNames()) {
				VectorTileLayer lyr = vt.GetLayer(lyrName);
				for(int i = 0; i < lyr.FeatureCount(); i++) {
					Debug.WriteLine("{0} lyr:{1} feat:{2}", fileName, lyr.Name, i);
					VectorTileFeature<long> feat = lyr.GetFeature<long>(i, 0);
					long extent = (long)lyr.Extent;
					foreach(var part in feat.Geometry) {
						foreach(var geom in part) {
							Assert.GreaterOrEqual(geom.X, 0, "geom.X equal or greater 0");
							Assert.GreaterOrEqual(geom.Y, 0, "geom.Y eqaul or greater 0");
							Assert.LessOrEqual(geom.X, extent, "geom.X less or equal extent");
							Assert.LessOrEqual(geom.Y, extent, "geom.Y less or equal extent");
						}
					}
				}
			}
		}


		[Test, TestCaseSource(typeof(GetMVTs), "GetFixtureFileName")]
		public void IterateAllProperties(string fileName) {
			string fullFileName = Path.Combine(fixturesPath, fileName);
			Assert.True(File.Exists(fullFileName), "Vector tile exists");
			byte[] data = File.ReadAllBytes(fullFileName);
			VectorTile vt = new VectorTile(data);
			foreach(var layerName in vt.LayerNames()) {
				var layer = vt.GetLayer(layerName);
				for(int i = 0; i < layer.FeatureCount(); i++) {
					var feat = layer.GetFeature<long>(i);
					var properties = feat.GetProperties();
					foreach(var prop in properties) {
						Assert.IsInstanceOf<string>(prop.Key);
					}
				}
			}
		}
	}



	public partial class GetMVTs {
		public static IEnumerable GetFixtureFileName() {
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "test", "mvt-fixtures", "fixtures", "valid");

			foreach(var file in Directory.GetFiles(path)) {
				//return file basename only to make test description more readable
				yield return new TestCaseData(Path.GetFileName(file));
			}
		}
	}





}