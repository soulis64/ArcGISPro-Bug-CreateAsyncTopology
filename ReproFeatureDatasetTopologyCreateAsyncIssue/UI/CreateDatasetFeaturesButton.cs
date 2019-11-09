using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ReproFeatureDatasetTopologyCreateAsyncIssue.Utilities;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue.UI
{
    internal class CreateDatasetFeaturesButton : Button
    {
        protected override async void OnClick()
        {
            var featureLayers = GetDatasetFeatures();
            await featureLayers.GenerateRandomPolygons();
        }

        private IReadOnlyList<FeatureLayer> GetDatasetFeatures()
        {
            var featureLayers = MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.EndsWith("Class", StringComparison.OrdinalIgnoreCase)).ToList();

            if(featureLayers == null || featureLayers.Count == 0)
                throw new GeodatabaseException("No feature layers found");

            return featureLayers;
        }
    }
}
