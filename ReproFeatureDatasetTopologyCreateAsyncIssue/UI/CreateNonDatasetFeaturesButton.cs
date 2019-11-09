using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ReproFeatureDatasetTopologyCreateAsyncIssue.Utilities;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue.UI
{
    internal class CreateNonDatasetFeaturesButton : Button
    {
        protected override async void OnClick()
        {
            var featureLayers = await GetNonDatasetFeatures();
            await featureLayers.GenerateRandomPolygons();
        }

        private Task<IReadOnlyList<FeatureLayer>> GetNonDatasetFeatures()
        {
            return QueuedTask.Run(() =>
            {
                var featureLayers = MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.EndsWith("Class", StringComparison.OrdinalIgnoreCase)).ToList();

                if (featureLayers == null || featureLayers.Count == 0)
                    throw new GeodatabaseException("No feature layers found");

                using (var geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath))))
                {
                    var fcDefinitions = geodatabase.GetDefinitions<FeatureClassDefinition>();
                    try
                    {
                        using (var featureDatasetDefinition = geodatabase.GetDefinition<FeatureDatasetDefinition>("FeatureDataset"))
                        {
                            var featureDatasetFeatureClassDefinitions = geodatabase.GetRelatedDefinitions(featureDatasetDefinition, DefinitionRelationshipType.DatasetInFeatureDataset).OfType<FeatureClassDefinition>();

                            featureLayers = featureLayers.Where(fl => featureDatasetFeatureClassDefinitions.All(featureDatasetFeatureClassDefinition =>
                            {
                                using (var featureClass = fl.GetFeatureClass())
                                using (var featureClassDefinition = featureClass.GetDefinition())
                                    return featureDatasetFeatureClassDefinition != featureClassDefinition;
                            })).ToList();
                        }
                    }
                    finally
                    {
                        foreach (var fcDefinition in fcDefinitions)
                        {
                            fcDefinition?.Dispose();
                        }
                    }
                }

                return (IReadOnlyList<FeatureLayer>)featureLayers;
            });
        }
    }
}
