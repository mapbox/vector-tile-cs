using Mapbox.VectorTile;
using Mapbox.VectorTile.Geometry;
using System;
using System.IO;
#if WINDOWS_UWP
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using ATestClass = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using ATestMethod = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#else
using NUnit.Framework;
using ATestClass = NUnit.Framework.TestFixtureAttribute;
using ATestMethod = NUnit.Framework.TestAttribute;
#endif

namespace VectorTiles.Tests {


	[ATestClass]
	public class GeometryTests {


//		[ATestMethod]
//		public void GeometryObjects() {
//			LatLng ll = new LatLng() { Lng = 15, Lat = 48 };
//			Assert.AreEqual("48.000000/15.000000", ll.ToString(), "LatLng.ToString()");

//			Point2d<long> ptSE = new Point2d<long>(4096, 4096);
//			Assert.AreEqual("4096/4096", ptSE.ToString(), "Point2d.ToString()");

//			LatLng fromPtSE = ptSE.ToLngLat(0, 0, 0, 4096);
//			Assert.AreEqual(180, fromPtSE.Lng, "correct longitude");
//			Assert.AreEqual(-85.051128779806589, fromPtSE.Lat, "correct latitude");

//			ptSE.X = 4210;
//			fromPtSE = ptSE.ToLngLat(0, 0, 0, 4096);
//			Assert.AreEqual(190, fromPtSE.Lng, 0.02, "correct longitude - out of bounds");
//#if WINDOWS_UWP
//			Assert.ThrowsException<Exception>(() => { ptSE.ToLngLat(0, 0, 0, 04096, true); });
//#else
//			Assert.Throws(Is.InstanceOf<Exception>(), () => { ptSE.ToLngLat(0, 0, 0, 04096, true); });
//#endif

//			ptSE.X = 4096;
//			ptSE.Y = 4210;
//			fromPtSE = ptSE.ToLngLat(0, 0, 0, 4096);
//			Assert.AreEqual(-85.844, fromPtSE.Lat, 0.02, "correct latitude - out of bounds");
//#if WINDOWS_UWP
//			Assert.ThrowsException<Exception>(() => { ptSE.ToLngLat(0, 0, 0, 04096, true); });
//#else
//			Assert.Throws(Is.InstanceOf<Exception>(), () => { ptSE.ToLngLat(0, 0, 0, 04096, true); });
//#endif
//		}





	}

}