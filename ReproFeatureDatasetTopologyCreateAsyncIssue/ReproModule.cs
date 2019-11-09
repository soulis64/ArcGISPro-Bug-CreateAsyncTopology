using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue
{
    internal class ReproModule : Module
    {
        private static ReproModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static ReproModule Current
        {
            get
            {
                return _this ?? (_this = (ReproModule)FrameworkApplication.FindModule("ReproFeatureDatasetTopologyCreateAsyncIssue_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
