using System;
using NUnit.Framework;
using System.IO;
using System.Linq;
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
			foreach (var layerName in vt.LayerNames()) {
				VectorTileLayer layer = vt.GetLayer(layerName);
				for (int i = 0; i < layer.FeatureCount(); i++) {
					VectorTileFeature feat = layer.GetFeature(i);
					var properties = feat.GetProperties();
					foreach (var prop in properties) {
						Assert.AreEqual(prop.Value, feat.GetValue(prop.Key), "Property values match");
					}
					foreach (var geomPart in feat.Geometry<int>()) {
						foreach (var coord in geomPart) {
							//TODO add Assert
						}
					}
				}
			}
			string geojson = vt.ToGeoJson(0, 0, 0);
			Assert.GreaterOrEqual(geojson.Length, 30, "geojson >= 30 chars");
		}


		[Test]
		public void DifferentPoint2dTypesAndScaling() {
			string fullFileName = Path.Combine(fixturesPath, "Feature-single-linestring.mvt");
			byte[] data = File.ReadAllBytes(fullFileName);
			VectorTile vt = new VectorTile(data);
			foreach (var layerName in vt.LayerNames()) {
				VectorTileLayer layer = vt.GetLayer(layerName);
				for (int i = 0; i < layer.FeatureCount(); i++) {

					VectorTileFeature featLong = layer.GetFeature(i);
					foreach (var geomPart in featLong.Geometry<long>()) {
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

					// don't clip, as this might change order of vertices
					// test 'scale' on the VectorTileFeature constructor
					VectorTileFeature featInt = layer.GetFeature(i, null, 1.5f);
					foreach (var geomPart in featInt.Geometry<int>()) {
						foreach (var coord in geomPart) {
							Debug.WriteLine("integer: {0}/{1}", coord.X, coord.Y);
						}
						Assert.AreEqual(3, geomPart[0].X);
						Assert.AreEqual(3, geomPart[0].Y);
						Assert.AreEqual(3, geomPart[1].X);
						Assert.AreEqual(15, geomPart[1].Y);
						Assert.AreEqual(15, geomPart[2].X);
						Assert.AreEqual(15, geomPart[2].Y);
					}

					// don't clip, as this might change order of vertices
					VectorTileFeature featFloat = layer.GetFeature(i);
					// test 'scale' on the Geometry method
					foreach (var geomPart in featFloat.Geometry<float>(null, 2.0f)) {
						foreach (var coord in geomPart) {
							Debug.WriteLine("float: {0}/{1}", coord.X, coord.Y);
						}
						Assert.AreEqual(4f, geomPart[0].X);
						Assert.AreEqual(4f, geomPart[0].Y);
						Assert.AreEqual(4f, geomPart[1].X);
						Assert.AreEqual(20f, geomPart[1].Y);
						Assert.AreEqual(20f, geomPart[2].X);
						Assert.AreEqual(20f, geomPart[2].Y);
					}

				}
			}
			string geojson = vt.ToGeoJson(0, 0, 0);
			Assert.GreaterOrEqual(geojson.Length, 30, "geojson >= 30 chars");
		}



		[Test, TestCaseSource(typeof(GetMVTs), "GetFixtureFileName")]
		public void Scaling(string fileName) {
			float[] scales = new float[] { 1.5f, 2.25f, 5.75f, 197.3f };
			string fullFileName = Path.Combine(fixturesPath, fileName);
			byte[] data = File.ReadAllBytes(fullFileName);
			VectorTile vt = new VectorTile(data);
			foreach (var lyrName in vt.LayerNames()) {
				VectorTileLayer lyr = vt.GetLayer(lyrName);
				int featCnt = lyr.FeatureCount();
				for (int idxFeat = 0; idxFeat < featCnt; idxFeat++) {
					VectorTileFeature feat = lyr.GetFeature(idxFeat);
					List<List<Point2d<int>>> rawParts = feat.Geometry<int>();
					for (int idxPart = 0; idxPart < rawParts.Count; idxPart++) {
						List<Point2d<int>> rawGeom = rawParts[idxPart];
						foreach (var scale in scales) {
							List<List<Point2d<float>>> scaledParts = feat.Geometry<float>(null, scale);
							List<Point2d<float>> scaledGeom = scaledParts[idxPart];
							for (int idxVertex = 0; idxVertex < rawGeom.Count; idxVertex++) {
								Point2d<int> rawVertex = rawGeom[idxVertex];
								Point2d<float> scaledVertex = scaledGeom[idxVertex];
								Assert.AreEqual(scale * (float)rawVertex.X, scaledVertex.X, $"{fileName}, feature[{idxFeat}], geometry part[{idxPart}], vertex[{idxVertex}], scale[{scale}]: X does not match");
								Assert.AreEqual(scale * (float)rawVertex.Y, scaledVertex.Y, $"{fileName}, feature[{idxFeat}], geometry part[{idxPart}], vertex[{idxVertex}], scale[{scale}]: Y does not match");
							}
						}
					}
				}

			}
		}

		/// <summary>
		/// This test assumes that the features do *NOT* extend beyong the tile extent!!!
		/// It will fail otherwise.
		/// </summary>
		/// <param name="fileName"></param>
		[Test, TestCaseSource(typeof(GetMVTs), "GetFixtureFileName")]
		public void Clipping(string fileName) {
			string fullFileName = Path.Combine(fixturesPath, fileName);
			Assert.True(File.Exists(fullFileName), "Vector tile exists");
			byte[] data = File.ReadAllBytes(fullFileName);
			VectorTile vt = new VectorTile(data);
			foreach (var lyrName in vt.LayerNames()) {
				VectorTileLayer lyr = vt.GetLayer(lyrName);
				for (int i = 0; i < lyr.FeatureCount(); i++) {
					VectorTileFeature feat = lyr.GetFeature(i);
					//skip features with unknown geometry type
					if (feat.GeometryType == GeomType.UNKNOWN) { continue; }
					List<List<Point2d<long>>> geomRaw = feat.Geometry<long>();
					List<List<Point2d<long>>> geomClipped = feat.Geometry<long>(0);
					for (int j = 0; j < geomRaw.Count; j++) {
						List<Point2d<long>> part = geomRaw[j];
						List<Point2d<long>> partClipped = geomClipped[j];
						// Workaround to compare parts as clipping may or may not change the order of vertices
						// This only works if no actual clipping is done
						Assert.False(part.Except(partClipped).Any(), $"{fileName}, feature[{i}], geometry part[{j}]: geometry parts don't match after clipping");
					}
				}
			}
		}


		[Test, TestCaseSource(typeof(GetMVTs), "GetFixtureFileName")]
		public void AtLeastOneLayer(string fileName) {
			string fullFileName = Path.Combine(fixturesPath, fileName);
			Assert.True(File.Exists(fullFileName), "Vector tile exists");
			byte[] data = File.ReadAllBytes(fullFileName);
			VectorTile vt = new VectorTile(data);
			Assert.GreaterOrEqual(vt.LayerNames().Count, 1, "At least one layer");
			string geojson = vt.ToGeoJson(0, 0, 0);
			Assert.GreaterOrEqual(geojson.Length, 30, "geojson >= 30 chars");
			foreach (var lyrName in vt.LayerNames()) {
				VectorTileLayer lyr = vt.GetLayer(lyrName);
				for (int i = 0; i < lyr.FeatureCount(); i++) {
					Debug.WriteLine("{0} lyr:{1} feat:{2}", fileName, lyr.Name, i);
					VectorTileFeature feat = lyr.GetFeature(i);
					long extent = (long)lyr.Extent;
					foreach (var part in feat.Geometry<long>()) {
						foreach (var geom in part) {
							if (geom.X < 0 || geom.Y < 0 || geom.X > extent || geom.Y > extent) {
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
			string geojson = vt.ToGeoJson(0, 0, 0);
			Assert.GreaterOrEqual(geojson.Length, 30, "geojson >= 30 chars");
			foreach (var lyrName in vt.LayerNames()) {
				VectorTileLayer lyr = vt.GetLayer(lyrName);
				for (int i = 0; i < lyr.FeatureCount(); i++) {
					Debug.WriteLine("{0} lyr:{1} feat:{2}", fileName, lyr.Name, i);
					VectorTileFeature feat = lyr.GetFeature(i, 0);
					long extent = (long)lyr.Extent;
					foreach (var part in feat.Geometry<int>()) {
						foreach (var geom in part) {
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
			foreach (var layerName in vt.LayerNames()) {
				var layer = vt.GetLayer(layerName);
				for (int i = 0; i < layer.FeatureCount(); i++) {
					var feat = layer.GetFeature(i);
					var properties = feat.GetProperties();
					foreach (var prop in properties) {
						Assert.IsInstanceOf<string>(prop.Key);
					}
				}
			}
		}
	}



	public partial class GetMVTs {
		public static IEnumerable GetFixtureFileName() {
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "test", "mvt-fixtures", "fixtures", "valid");

			foreach (var file in Directory.GetFiles(path)) {
				//return file basename only to make test description more readable
				yield return new TestCaseData(Path.GetFileName(file));
			}
		}
	}





}