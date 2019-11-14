using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Geoprocessing;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue.Utilities
{
    /// <summary>
    /// Provides wrappers for the topology geoprocessing tools. Also associates a created topology with the manager.
    /// </summary>
    public class TopologyManager
    {
        private static readonly string CreateTopologyTool = "CreateTopology_management";
        private static readonly string AddFeatureClassToTopologyTool = "AddFeatureClassToTopology_management";
        private static readonly string AddRuleToTopologyTool = "AddRuleToTopology_management";

        private static TopologyManager _topologyManager;

        public static TopologyManager Default => _topologyManager ?? (_topologyManager = new TopologyManager());

        /// <summary>
        /// True if a topology is associated with this manager.
        /// </summary>
        public bool IsTopologyCreated => !string.IsNullOrWhiteSpace(TopologyName) && !string.IsNullOrWhiteSpace(TopologyPath);

        /// <summary>
        /// The name of the associated topology.
        /// </summary>
        public string TopologyName { get; private set; }

        /// <summary>
        /// The full path of the associated topology.
        /// </summary>
        public string TopologyPath { get; private set; }

        /// <summary>
        /// Creates a topology. The topology will not contain any feature classes or rules. See
        /// https://pro.arcgis.com/en/pro-app/tool-reference/data-management/create-topology.htm.
        /// This method must be called on the MCT. Use QueuedTask.Run.
        /// </summary>
        /// <param name="featureDataset">
        /// The feature dataset in which the topology will be created.
        /// </param>
        /// <param name="topologyName">
        /// The name of the topology to be created. This name must be unique across the entire
        /// geodatabase. Default value is "{featureDataset}_Topology".
        /// </param>
        /// <param name="clusterTolerance">
        /// The cluster tolerance to be set on the topology. The larger the value, the more
        /// likely vertices will be to cluster together. Default value is based on the
        /// dataset's spatial reference.
        /// </param>
        /// <returns>
        /// True if the tool executed successfully, false if it was cancelled.
        /// </returns>
        public async Task<bool> CreateTopology(FeatureDataset featureDataset, string topologyName = null, double? clusterTolerance = null)
        {
            if (IsTopologyCreated)
                throw new InvalidOperationException("This instance of topology manager already has a topology associated with it.");

            if (featureDataset == null)
                throw new ArgumentNullException(nameof(featureDataset));

            if (string.IsNullOrWhiteSpace(topologyName))
                topologyName = $"{featureDataset.GetName()}_Topology";

            var datasetPath = Path.Combine(featureDataset.GetDatastore().GetPath().LocalPath, featureDataset.GetName());
            var args = clusterTolerance == null ? Geoprocessing.MakeValueArray(featureDataset, topologyName) : Geoprocessing.MakeValueArray(featureDataset, topologyName, clusterTolerance);
            
            var result = await Geoprocessing.ExecuteToolAsync(CreateTopologyTool, args);

            if (!result.IsFailed)
            {
                TopologyName = topologyName;
                TopologyPath = Path.Combine(datasetPath, topologyName);

                return true;
            }

            Geoprocessing.ShowMessageBox(result.Messages, "Create Topology Error", GPMessageBoxStyle.Error);

            TopologyName = null;
            return false;
        }

        /// <summary>
        /// Adds a feature class to a topology. See
        /// https://pro.arcgis.com/en/pro-app/tool-reference/data-management/add-feature-class-to-topology.htm.
        /// This method must be called on the MCT. Use QueuedTask.Run.
        /// </summary>
        /// <param name="featureClassPath">
        /// The path of the feature class to add to the topology. The feature class must be in the
        /// same feature dataset as the topology.
        /// </param>
        /// <param name="xyRank">
        /// The relative degree of positional accuracy associated with vertices of features in the
        /// feature class versus those in other feature classes participating in the topology. The
        /// feature class with the highest accuracy should get a higher rank (lower number, for
        /// example, 1) than a feature class which is known to be less accurate.
        /// </param>
        /// <param name="zRank">
        /// Feature classes that are z-aware have elevation values embedded in their geometry for
        /// each vertex. By setting a z rank, you can influence how vertices with accurate
        /// z-values are snapped or clustered with vertices that contain less accurate z
        /// measurements.
        /// </param>
        /// <returns>
        /// True if the tool executed successfully, false if it was cancelled.
        /// </returns>
        public async Task<bool> AddFeatureClassToTopology(FeatureClass featureClass, int xyRank = 1, int zRank = 1)
        {
            if (!IsTopologyCreated)
                throw new InvalidOperationException("This instance of topology manager does not have an associated topology. Ensure a topology is created first.");

            if (featureClass == null)
                throw new ArgumentNullException(nameof(featureClass));

            if (xyRank < 1)
                throw new ArgumentOutOfRangeException(nameof(xyRank), "XY rank must be a positive, non-zero integer.");

            if (zRank < 1)
                throw new ArgumentOutOfRangeException(nameof(zRank), "Z rank must be a positive, non-zero integer.");

            var args = Geoprocessing.MakeValueArray(TopologyPath, featureClass, xyRank, zRank);

            var result = await Geoprocessing.ExecuteToolAsync(AddFeatureClassToTopologyTool, args);

            if (!result.IsFailed)
                return true;

            Geoprocessing.ShowMessageBox(result.Messages, "Add Feature Class to Topology Error", GPMessageBoxStyle.Error);

            return false;
        }

        /// <summary>
        /// Adds a new rule to a topology. See
        /// https://pro.arcgis.com/en/pro-app/tool-reference/data-management/add-rule-to-topology.htm.
        /// This method must be called on the MCT. Use QueuedTask.Run.
        /// </summary>
        /// <param name="ruleType">
        /// The topology rule to be added. See <see cref="TopologyRules"/>.
        /// </param>
        /// <param name="featureClass1">
        /// The input or origin feature class.
        /// </param>
        /// <param name="subtype1">
        /// The subtype for the input or origin feature class. Enter the subtype's description
        /// (not the code). If subtypes do not exist on the origin feature class, or you want the
        /// rule to be applied to all subtypes in the feature class, leave this blank.
        /// </param>
        /// <param name="featureClass2">
        /// The destination feature class for the topology rule. Not required for every rule.
        /// </param>
        /// <param name="subtype2">
        /// The subtype for the destination feature class. Enter the subtype's description (not the
        /// code). If subtypes do not exist on the origin feature class, or you want the rule to be
        /// applied to all subtypes in the feature class, leave this blank.
        /// </param>
        /// <returns>
        /// True if the tool executed successfully, false if it was cancelled.
        /// </returns>
        public async Task<bool> AddRuleToTopology(string ruleType, FeatureClass featureClass1, string subtype1 = null, FeatureClass featureClass2 = null, string subtype2 = null)
        {
            if (!IsTopologyCreated)
                throw new InvalidOperationException("This instance of topology manager does not have an associated topology. Ensure a topology is created first.");

            if (!TopologyRules.AllRules.Contains(ruleType))
                throw new ArgumentException($"{ruleType} is an invalid rule type", nameof(ruleType));

            var argList = new List<object> { TopologyPath, ruleType, featureClass1 };

            if(subtype1 != null)
            {
                argList.Add(subtype1);

                if(featureClass2 != null)
                {
                    argList.Add(featureClass2);

                    if (!string.IsNullOrWhiteSpace(subtype2))
                        argList.Add(subtype2);
                }
            }
            
            if(subtype1 == null && featureClass2 != null)
            {
                subtype1 = string.Empty;
                argList.Add(subtype1);
                argList.Add(featureClass2);
                if(!string.IsNullOrWhiteSpace(subtype2))
                    argList.Add(subtype2);
            }

            var args = Geoprocessing.MakeValueArray(argList.ToArray());

            var result = await Geoprocessing.ExecuteToolAsync(AddRuleToTopologyTool, args);

            if (!result.IsFailed)
                return true;

            Geoprocessing.ShowMessageBox(result.Messages, "Add Feature Class to Topology Error", GPMessageBoxStyle.Error);

            return false;
        }
    }
}