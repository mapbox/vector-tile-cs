using Mapbox.VectorTile;
using Mapbox.VectorTile.Geometry;
using NUnit.Framework;
using System;
using System.IO;

namespace VectorTiles.Tests
{
    [TestFixture]
    public class GeometryTests
    {

        [Test]
        public void GeometryObjects()
        {
            LatLng ll = new LatLng() { Lng = 15, Lat = 48 };
            Assert.AreEqual("48.000000/15.000000", ll.ToString(), "LatLng.ToString()");

            Point2d ptSE = new Point2d(4096, 4096);
            Assert.AreEqual("4096/4096", ptSE.ToString(), "Point2d.ToString()");

            LatLng fromPtSE = ptSE.ToLngLat(0, 0, 0, 4096);
            Assert.AreEqual(180, fromPtSE.Lng, "correct longitude");
            Assert.AreEqual(-85.051128779806589, fromPtSE.Lat, "correct latitude");

            ptSE.X = 4210;
            fromPtSE = ptSE.ToLngLat(0, 0, 0, 4096);
            Assert.AreEqual(190, fromPtSE.Lng, 0.02, "correct longitude - out of bounds");
            Assert.Throws(Is.InstanceOf<Exception>(), () => { ptSE.ToLngLat(0, 0, 0, 04096, true); });

            ptSE.X = 4096;
            ptSE.Y = 4210;
            fromPtSE = ptSE.ToLngLat(0, 0, 0, 4096);
            Assert.AreEqual(-85.844, fromPtSE.Lat, 0.02, "correct latitude - out of bounds");
            Assert.Throws(Is.InstanceOf<Exception>(), () => { ptSE.ToLngLat(0, 0, 0, 04096, true); });
        }





    }

}