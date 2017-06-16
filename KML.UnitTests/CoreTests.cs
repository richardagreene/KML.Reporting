using KML.Core;
using NUnit.Framework;
using SharpKml.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KML.UnitTests
{
    [TestFixture]
    public class CoreTests
    {
        [Test]
        public void GetPolygons()
        {
            DocumentData data = new DocumentData(@"C:\Development\KML\KML.UnitTests\National Territories v2.0.kml");
            Assert.IsTrue(data.Towns.Count > 0);
        }

        [TestCase(-31.87509722, 115.88166667)]  // bayswater
        [TestCase(-32.24545555555556, 136.59840277777778)]  // middle of nowhere
        [TestCase(-38.15322222222222, 145.31143611111114)]  // Melbourne, Casey

        public void FindIntersection(double lat, double lng)
        {
            SharpKml.Dom.Point point = new SharpKml.Dom.Point() { Coordinate = new SharpKml.Base.Vector(lat, lng) };

            DocumentData data = new DocumentData(@"C:\Development\KML\KML.UnitTests\National Territories v2.0.kml");
            Placemark placemark = data.Intersects(point);
            Assert.IsNotNull(placemark);
            Console.WriteLine($"town {placemark.Name}");
        }

        [TestCase(-44.93733888888889, 169.81687222222223)]  // bayswater

        public void Should_NotFind_Intersection(double lat, double lng)
        {
            SharpKml.Dom.Point point = new SharpKml.Dom.Point() { Coordinate = new SharpKml.Base.Vector(lat, lng) };

            DocumentData data = new DocumentData(@"C:\Development\KML\KML.UnitTests\National Territories v2.0.kml");
            Placemark placemark = data.Intersects(point);
            Assert.IsNull(placemark);
        }

        [Test]
        public void Should_Save_KML()
        {
            DocumentData data = new DocumentData(@"C:\Development\KML\KML.UnitTests\National Territories v2.0.kml");
            data.Save(@"C:\Development\KML\KML.UnitTests\National Territories v2.1.kml");
        }


        [Test]
        public void Should_Update_Description()
        {
            DocumentData data = new DocumentData(@"C:\Development\KML\KML.UnitTests\National Territories v2.0.kml");
            Assert.IsTrue(data.Towns.Count > 0);
            var town = data.Towns[0];
            town.ExtendedData.AddData(new Data() { Name = "Revenue", Value = "1" });
            data.Update(town);
            data.Save(@"C:\Development\KML\KML.UnitTests\National Territories v2.1.kml");
        }


    }
}
