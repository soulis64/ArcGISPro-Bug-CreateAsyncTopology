using System;
using System.Collections.Generic;
using System.Windows;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue.Utilities
{
    public static class GeometryHelper
    {
        /// <summary>
        /// Creates 20 random polygons on the given feature layers. This method must be called on the MCT. Use QueuedTask.Run.
        /// </summary>
        /// <param name="featureLayers">The feature layers to create polygons on.</param>
        /// <returns>
        /// The success of the edit operation creating the polygons.
        /// </returns>
        public static bool GenerateRandomPolygons(this IReadOnlyList<FeatureLayer> featureLayers)
        {
            var randomGenerator = new Random();

            var editOperation = new EditOperation { Name = "Generate Polygons" };

            for (var i = 0; i < 20; i++)
            {
                var index = 0;
                
                if(featureLayers.Count >= 3)
                    index = i % 3;
                else if(featureLayers.Count == 2)
                    index = i % 2;

                var featureLayer = featureLayers[index];
                var featureClass = featureLayer.GetFeatureClass();

                var featureDataset = featureClass.GetFeatureDataset();
                
                var spatialReference = featureDataset?.GetDefinition().GetSpatialReference();
                
                if(spatialReference == null || spatialReference.IsUnknown) 
                    spatialReference = featureClass.GetDefinition().GetSpatialReference();

                if (spatialReference == null || spatialReference.IsUnknown)
                    throw new ArgumentException($"No valid spatial reference for feature class '{featureClass.GetName()}'");
                
                var extent = featureDataset?.GetExtent();

                if (extent == null || extent.IsEmpty || double.IsNaN(extent.Area) || extent.Area == 0)
                    extent = featureClass.GetExtent();

                if (extent == null || extent.IsEmpty || double.IsNaN(extent.Area) || extent.Area == 0)
                    extent = MapView.Active?.Map.CalculateFullExtent();

                if (extent == null || extent.IsEmpty || double.IsNaN(extent.Area) || extent.Area == 0)
                    throw new ArgumentException($"No valid extent for feature class '{featureClass.GetName()}'");

                var numOfVerts = randomGenerator.Next(3, 8);
                var verts = new List<MapPoint>();

                for (var j = 0; j < numOfVerts; j++)
                {
                    verts.Add(MapPointBuilder.CreateMapPoint(randomGenerator.NextCoordinate2D(extent), spatialReference));
                }

                var multipoint = MultipointBuilder.CreateMultipoint(verts, spatialReference);
                var polygon = GeometryEngine.Instance.ConvexHull(multipoint) as Polygon;

                editOperation.Create(featureLayer, polygon);
            }

            var success = editOperation.Execute();

            if(!success)
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Edit operation failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            return success;
        }
    }
}