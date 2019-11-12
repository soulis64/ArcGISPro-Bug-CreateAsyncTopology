using System.Diagnostics;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ReproFeatureDatasetTopologyCreateAsyncIssue.Utilities;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue.UI
{
    internal class CreateNonTopologyDatasetFeaturesButton : Button
    {
        protected override async void OnClick()
        {
            var success = await QueuedTask.Run(() => FeatureHelper.GetNonTopologyDatasetFeatureLayers().GenerateRandomPolygons());
            Debug.WriteLine($"Created non-topology dataset features? {success}");
        }
    }
}
