﻿using System.Diagnostics;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ReproFeatureDatasetTopologyCreateAsyncIssue.Utilities;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue.UI
{
    internal class CreateNonDatasetFeaturesButton : Button
    {
        protected override async void OnClick()
        {
            var success = await QueuedTask.Run(() => FeatureHelper.GetNonDatasetFeatureLayers().GenerateRandomPolygons());
            Debug.WriteLine($"Created non-dataset features? {success}");
        }
    }
}
