using SharpKml.Base;
using SharpKml.Dom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using SharpKml.Engine;
using System.Xml;
using System.Diagnostics;
using System.Xml.Linq;

namespace KML.Core
{
    public class DocumentData
    {
        string _filename = String.Empty;
        Parser _parser = new Parser();
        Kml _data = new Kml();

        public List<Placemark> Towns = new List<Placemark>();

        public DocumentData(string kmlFilename)
        {
            _filename = kmlFilename;
            GetPlacemarks(kmlFilename);
        }


        /// <summary>
        /// Open a file and load KML data
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private void GetPlacemarks(string filename)
        {
            // open the KML
            _parser.ParseString(File.ReadAllText(filename), false);
            _data = (Kml)_parser.Root;
            Feature feature = _data.Feature;
            SharpKml.Dom.Container c = (SharpKml.Dom.Container)feature;
            List<Placemark> placemarks = new List<Placemark>();
            foreach (Feature f in c.Features)
                ExtractPlacemarks(f, placemarks);
            // set the towns
            Towns = placemarks;
        }

        public Placemark Intersects(Point point)
        {
            foreach (var town in Towns)
            {
                foreach (var boundry in GetPloygons(town))
                {
                    List<System.Numerics.Vector2> points = new List<Vector2>();
                    if (boundry == null) continue;
                    var coordinates = boundry.OuterBoundary.LinearRing.Coordinates;
                    foreach (var p in coordinates)
                        points.Add(new Vector2(float.Parse(p.Latitude.ToString()), float.Parse(p.Longitude.ToString())));

                    if (CheckCollision(points.ToArray(), new Vector2(float.Parse(point.Coordinate.Latitude.ToString()), float.Parse(point.Coordinate.Longitude.ToString()))))
                        return town;
                }
            }
            return null;
        }

        public void Save(string filename)
        {
            KmlFile kmlfile = KmlFile.Create(_data, false);
            using (var stream = System.IO.File.OpenWrite(filename))
            {
                kmlfile.Save(stream);
            }
        }

        /// <summary>
        /// Update a selected placemarker
        /// </summary>
        /// <param name="town"></param>
        public void Update(Placemark town)
        {
            var KmlFile = XDocument.Load(_filename);

            XNamespace KmlNamespace = "http://www.opengis.net/kml/2.2";

            // find the Placemarks in the Photos folder
            IEnumerable<XElement> Placemarks = KmlFile.Element(KmlNamespace + "kml").Element(KmlNamespace + "Document").Element(KmlNamespace + "Folder").Elements(KmlNamespace + "Placemark");

            foreach (XElement p in Placemarks)
            {

                XmlDocument doc = new XmlDocument();
                var value = String.Format("{0}<br>Revenue:{1}", p.Element(KmlNamespace + "description").Value, 100);
                p.Element(KmlNamespace + "description").Value = doc.CreateCDataSection(value).OuterXml;
            }
            KmlFile.Save(@"C:\Development\KML\KML.UnitTests\update.kml");
        }

        private static void ExtractPlacemarks(Feature feature, List<Placemark> placemarks)
        {
            // Is the passed in value a Placemark?
            Placemark placemark = feature as Placemark;
            if (placemark != null)
                placemarks.Add(placemark);
            else
            {
                // Is it a Container, as the Container might have a child Placemark?
                SharpKml.Dom.Container container = feature as SharpKml.Dom.Container;
                if (container != null)
                    // Check each Feature to see if it's a Placemark or another Container
                    foreach (var f in container.Features)
                        ExtractPlacemarks(f, placemarks);
            }
        }

        private List<Polygon> GetPloygons(Placemark town)
        {
            List<Polygon> polygon = new List<Polygon>();
            Console.WriteLine($"working ....{town.Name}");

            var multiBoundry = town.Geometry as MultipleGeometry;
            if (multiBoundry == null)
            {   // simple polygon 
                polygon.Add(town.Geometry as Polygon);
            }
            else
            {
                foreach (var boundry in multiBoundry.Geometry)
                    polygon.Add(boundry as Polygon);
            }
            return polygon;
        }

        private bool CheckCollision(Vector2[] Points, Vector2 Position)
        {
            double MinX = Points.Min(a => a.X);
            double MinY = Points.Min(a => a.Y);
            double MaxX = Points.Max(a => a.X);
            double MaxY = Points.Max(a => a.Y);

            if (Position.X < MinX || Position.X > MaxX || Position.Y < MinY || Position.Y > MaxY)
                return false;

            int I = 0;
            int J = Points.Count() - 1;
            bool IsMatch = false;

            for (; I < Points.Count(); J = I++)
            {
                //When the position is right on a point, count it as a match.
                if (Points[I].X == Position.X && Points[I].Y == Position.Y)
                    return true;
                if (Points[J].X == Position.X && Points[J].Y == Position.Y)
                    return true;

                //When the position is on a horizontal or vertical line, count it as a match.
                if (Points[I].X == Points[J].X && Position.X == Points[I].X && Position.Y >= Math.Min(Points[I].Y, Points[J].Y) && Position.Y <= Math.Max(Points[I].Y, Points[J].Y))
                    return true;
                if (Points[I].Y == Points[J].Y && Position.Y == Points[I].Y && Position.X >= Math.Min(Points[I].X, Points[J].X) && Position.X <= Math.Max(Points[I].X, Points[J].X))
                    return true;

                if (((Points[I].Y > Position.Y) != (Points[J].Y > Position.Y)) && (Position.X < (Points[J].X - Points[I].X) * (Position.Y - Points[I].Y) / (Points[J].Y - Points[I].Y) + Points[I].X))
                {
                    IsMatch = !IsMatch;
                }
            }

            return IsMatch;
        }
    }
}
