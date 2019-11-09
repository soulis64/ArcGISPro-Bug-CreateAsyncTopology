using System.Windows;
using ArcGIS.Desktop.Framework.Contracts;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue.UI
{
    internal class CreateNewProjectButton : Button
    {
        private CreateNewProjectWindow _createNewProjectWindow;

        protected override async void OnClick()
        {
            //already open?
            if (_createNewProjectWindow != null)
                return;
            _createNewProjectWindow = new CreateNewProjectWindow
            {
                Owner = Application.Current.MainWindow
            };
            
            _createNewProjectWindow.Closed += (o, e) => { _createNewProjectWindow = null; };

            await _createNewProjectWindow.LoadData();

            //uncomment for modal
            _createNewProjectWindow.ShowDialog();
        }
    }
}
