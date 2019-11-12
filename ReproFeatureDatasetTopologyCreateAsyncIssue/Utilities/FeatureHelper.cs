using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue.Utilities
{
    internal static class FeatureHelper
    {
        /// <summary>
        /// Gets all feature layers from the active map view that are not children of the TopologyLayer
        /// </summary>
        ///
        /// <returns>
        /// All feature layers from the active map view that are not children of the TopologyLayer.
        /// </returns>
        private static IReadOnlyList<FeatureLayer> GetAllFeatureLayers()
        {
            var featureLayers = MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => !(fl.Parent is ArcGIS.Desktop.Internal.Mapping.TopologyLayer)).ToList();

            if (featureLayers == null || featureLayers.Count == 0)
                throw new GeodatabaseException("No feature layers found");

            return featureLayers;
        }

        /// <summary>
        /// Gets all dataset feature class definitions. This method must be called on the MCT or within a method that will be called on the MCT. Use QueuedTask.Run.
        /// </summary>
        /// <returns>
        /// All dataset feature class definitions.
        /// </returns>
        private static IReadOnlyList<FeatureClassDefinition> GetDatasetFeatureClassDefinitions()
        {
            using (var geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath))))
            using (var featureDatasetDefinition = geodatabase.GetDefinition<FeatureDatasetDefinition>("FeatureDataset"))
                return geodatabase.GetRelatedDefinitions(featureDatasetDefinition, DefinitionRelationshipType.DatasetInFeatureDataset).OfType<FeatureClassDefinition>().ToList();
        }

        /// <summary>
        /// Gets all dataset, including topology, feature layers from the active map view. This method must be called on the MCT. Use QueuedTask.Run.
        /// </summary>
        /// <returns>
        /// All dataset, including topology, feature layers from the active map view.
        /// </returns>
        public static IReadOnlyList<FeatureLayer> GetDatasetFeatureLayers()
        {
            var featureLayers = GetAllFeatureLayers();

            IReadOnlyList<FeatureClassDefinition> featureDatasetFeatureClassDefinitions = new List<FeatureClassDefinition>();
            try
            {
                featureDatasetFeatureClassDefinitions = GetDatasetFeatureClassDefinitions();

                return featureLayers.Where(fl => featureDatasetFeatureClassDefinitions.Any(featureDatasetFeatureClassDefinition =>
                {
                    using(var featureClass = fl.GetFeatureClass())
                    using(var featureClassDefinition = featureClass.GetDefinition())
                        return featureDatasetFeatureClassDefinition.GetName() == featureClassDefinition.GetName();
                })).ToList();
            }
            finally
            {
                foreach(var featureClassDefinition in featureDatasetFeatureClassDefinitions) 
                    featureClassDefinition?.Dispose();
            }
        }

        /// <summary>
        /// Gets all non-dataset feature layers from the active map view. This method must be called on the MCT. Use QueuedTask.Run.
        /// </summary>
        /// <returns>
        /// All non-dataset feature layers from the active map view.
        /// </returns>
        public static IReadOnlyList<FeatureLayer> GetNonDatasetFeatureLayers()
        {
            var datasetFeatureLayers = GetDatasetFeatureLayers();

            return GetAllFeatureLayers().Where(fl => !datasetFeatureLayers.Contains(fl)).ToList();
        }

        /// <summary>
        /// Gets all dataset, non-topology feature layers from the active map view. This method must be called on the MCT. Use QueuedTask.Run.
        /// </summary>
        /// <returns>
        /// All dataset, non-topology feature layers from the active map view.
        /// </returns>
        public static IReadOnlyList<FeatureLayer> GetNonTopologyDatasetFeatureLayers()
        {
            var datasetFeatureLayers = GetDatasetFeatureLayers();

            return datasetFeatureLayers.Where(fl => fl.Name.StartsWith("NonTopology")).ToList();
        }

        /// <summary>
        /// Gets all topology-included feature layers from the active map view. This method must be called on the MCT. Use QueuedTask.Run.
        /// </summary>
        /// <returns>
        /// All topology-included feature layers from the active map view.
        /// </returns>
        public static IReadOnlyList<FeatureLayer> GetTopologyFeatureLayers()
        {
            var datasetFeatureLayers = GetDatasetFeatureLayers();

            return datasetFeatureLayers.Where(fl => !fl.Name.StartsWith("NonTopology")).ToList();
        }
    }
}