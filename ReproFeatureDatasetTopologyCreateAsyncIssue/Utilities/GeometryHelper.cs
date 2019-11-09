using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue.Utilities
{
    public static class GeometryHelper
    {
        public static Task GenerateRandomPolygons(this IReadOnlyList<FeatureLayer> featureLayers)
        {
            return QueuedTask.Run(() =>
            {
                var randomGenerator = new Random();

                var editOperation = new EditOperation { Name = "Generate Polygons" };

                for (var i = 0; i < 20; i++)
                {
                    var featureLayer = featureLayers[i % 3];
                    var featureClass = featureLayer.GetFeatureClass();
                    var spatialReference = featureClass.GetDefinition().GetSpatialReference();

                    var numOfVerts = randomGenerator.Next(3, 8);
                    var verts = new List<MapPoint>();

                    for (var j = 0; j < numOfVerts; j++)
                    {
                        verts.Add(MapPointBuilder.CreateMapPoint(randomGenerator.NextCoordinate2D(MapView.Active.Extent), spatialReference));
                    }

                    var polyline = PolylineBuilder.CreatePolyline(verts, spatialReference);
                    var polygon = GeometryEngine.Instance.ConvexHull(polyline) as Polygon;

                    editOperation.Create(featureLayer, polygon);
                }

                return editOperation.Execute();
            });
        }
    }
}