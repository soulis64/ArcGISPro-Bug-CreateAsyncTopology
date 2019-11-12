using System.Diagnostics;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ReproFeatureDatasetTopologyCreateAsyncIssue.Utilities;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue.UI
{
    internal class CreateTopologyFeaturesButton : Button
    {
        protected override async void OnClick()
        {
            var success = await QueuedTask.Run(() => FeatureHelper.GetTopologyFeatureLayers().GenerateRandomPolygons());
            Debug.WriteLine($"Created topology features? {success}");
        }
    }
}
