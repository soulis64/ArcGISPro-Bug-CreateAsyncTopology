using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ActiproSoftware.Windows.Extensions;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ReproFeatureDatasetTopologyCreateAsyncIssue.Utilities;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue.UI
{
    /// <summary>
    ///     Interaction logic for CreateNewProjectWindow.xaml
    /// </summary>
    public partial class CreateNewProjectWindow : ProWindow, INotifyPropertyChanged
    {
        #region  Fields

        private string _newProjectName = "New Project";
        private string _projectFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private SpatialReference _projectSpatialReference;
        private bool _useTopologyTemplate;

        #endregion

        #region  Properties

        private static string TemplatePath => Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().CodeBase).Replace("file:\\", "").Replace("file:///", ""), "data");

        private static string TopologyTemplateFile => Path.Combine(TemplatePath, "MRE_Template.aptx");
        private static string NoTopologyTemplateFile => Path.Combine(TemplatePath, "NoTopo_MRE_Template.aptx");

        private static string TopologyGeodatabaseName => "MRE_Project.gdb";
        private static string NoTopologyGeodatabaseName => "NoTopo_MRE_Project.gdb";

        private static string FeatureDatasetName => "FeatureDataset";

        public string NewProjectName
        {
            get => _newProjectName;
            set
            {
                if (_newProjectName == value)
                    return;

                _newProjectName = value;
                OnPropertyChanged();
            }
        }

        public string ProjectFolder
        {
            get => _projectFolder;
            set
            {
                if (_projectFolder == value)
                    return;

                _projectFolder = value;
                OnPropertyChanged();
            }
        }

        public SpatialReference ProjectSpatialReference
        {
            get => _projectSpatialReference;
            set
            {
                if (_projectSpatialReference == value)
                    return;

                _projectSpatialReference = value;
                OnPropertyChanged();
            }
        }

        public bool UseTopologyTemplate
        {
            get => _useTopologyTemplate;
            set
            {
                if (_useTopologyTemplate == value)
                    return;

                _useTopologyTemplate = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SpatialReference> UtmSpatialReferences { get; } = new ObservableCollection<SpatialReference>();

        private string DefaultGeodatabasePath => Path.Combine(ProjectFolder, NewProjectName, UseTopologyTemplate ? TopologyGeodatabaseName : NoTopologyGeodatabaseName);

        #endregion

        #region  Methods

        public CreateNewProjectWindow()
        {
            InitializeComponent();
        }

        public async Task LoadData()
        {
            ProjectFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ArcGIS", "Projects");

            // Get the list of UTM based spatial references
            var utms = await QueuedTask.Run(() =>
            {
                var utmSRs = new List<SpatialReference>();
                //UTM84s for northern hemisphere
                for (var id = 32601; id <= 32660; id++)
                    utmSRs.Add(SpatialReferenceBuilder.CreateSpatialReference(id));

                //UTM84s for southern hemisphere
                for (var id = 32701; id <= 32760; id++)
                    utmSRs.Add(SpatialReferenceBuilder.CreateSpatialReference(id));

                return utmSRs;
            });
            UtmSpatialReferences.AddRange(utms);

            if (UtmSpatialReferences.Count > 0)
                cboCoordinateSystem.SelectedIndex = 0;
        }

        private async Task CreateNewProject()
        {
            if (await CreateProject())
            {
                await SetDefaultGeodatabase();

                await UpdateCoordinateSystem();

                await UpdateMapProjection();

                await RemoveProjectCreatedDatabase();

                await DeleteProjectCreatedDatabase();

                await ZoomToExtents();

                if (!UseTopologyTemplate)
                    await CreateFullTopology();

                await Project.Current.SaveAsync();
            }
        }

        private async Task CreateFullTopology()
        {
            await CreateTopology();

            await AddFeatureClassesToTopology();

            await CreateTopologyRules();
        }

        private async Task CreateTopology()
        {
            var success = await QueuedTask.Run(() =>
            {
                using (var featureDataset = FeatureHelper.GetFeatureDataset(FeatureDatasetName))
                    return TopologyManager.Default.CreateTopology(featureDataset);
            });

            if (success)
                Debug.WriteLine($"Topology {TopologyManager.Default.TopologyName} created.");
            else
                Debug.WriteLine($"FAIL: Could not create topology {FeatureDatasetName}_Topology.");
        }

        private async Task AddFeatureClassesToTopology()
        {
            var topologyLayers = await QueuedTask.Run(FeatureHelper.GetTopologyFeatureLayers);

            foreach (var topologyLayer in topologyLayers)
            {
                var success = await QueuedTask.Run(() => TopologyManager.Default.AddFeatureClassToTopology(topologyLayer.GetFeatureClass()));

                if (success)
                    Debug.WriteLine($"Feature class {topologyLayer.Name} added to topology {TopologyManager.Default.TopologyName}");
                else
                    Debug.WriteLine($"FAIL: Could not add feature class {topologyLayer.Name} to topology {TopologyManager.Default.TopologyName}");
            }
        }

        private async Task CreateTopologyRules()
        {
            var topologyLayers = await QueuedTask.Run(FeatureHelper.GetTopologyFeatureLayers);

            foreach(var topologyLayer in topologyLayers)
            {
                var mustNotHaveGapsSuccess = await QueuedTask.Run(() => TopologyManager.Default.AddRuleToTopology(TopologyRules.Polygon.MustNotHaveGaps, topologyLayer.GetFeatureClass()));

                if (mustNotHaveGapsSuccess)
                    Debug.WriteLine($"Topology rule '{TopologyRules.Polygon.MustNotHaveGaps}' added for feature class '{topologyLayer.Name}'");
                else
                    Debug.WriteLine($"FAIL: Could not add topology rule '{TopologyRules.Polygon.MustNotHaveGaps}' for feature class '{topologyLayer.Name}'");
            }
            
            var parentLayer = topologyLayers.First(tl => tl.Name.StartsWith("Parent", StringComparison.OrdinalIgnoreCase));
            var childLayer = topologyLayers.First(tl => tl.Name.StartsWith("Child", StringComparison.OrdinalIgnoreCase));


        }

        private async Task RemoveProjectCreatedDatabase()
        {
            var esriGeodatabaseName = NewProjectName + ".gdb";

            GDBProjectItem geodatabaseItemToRemove = null;

            var gdbProjectItems = Project.Current.GetItems<GDBProjectItem>();

            await QueuedTask.Run(() =>
            {
                foreach (var gdbProjectItem in gdbProjectItems)
                {
                    using (var datastore = gdbProjectItem.GetDatastore())
                    {
                        //Unsupported datastores (non File GDB and non Enterprise GDB) will be of type UnknownDatastore
                        if (datastore is UnknownDatastore)
                            continue;

                        if (string.Compare(gdbProjectItem.Name, esriGeodatabaseName, comparisonType: StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            geodatabaseItemToRemove = gdbProjectItem;
                            break;
                        }
                    }
                }
            });

            //Found it so remove it
            if (geodatabaseItemToRemove != null)
            {
                var success = await QueuedTask.Run(() => Project.Current.RemoveItem(geodatabaseItemToRemove));

                if (success)
                    Debug.WriteLine($"File geodatabase {geodatabaseItemToRemove} removed.");
                else
                    Debug.WriteLine($"FAIL: Could not remove file geodatabase {geodatabaseItemToRemove}.");
            }
        }

        private async Task DeleteProjectCreatedDatabase()
        {
            var esriGeodatabaseName = NewProjectName + ".gdb";
            var esriGeodatabaseFullPath = Path.Combine(ProjectFolder, NewProjectName, esriGeodatabaseName);

            var args = await QueuedTask.Run(() => Geoprocessing.MakeValueArray(esriGeodatabaseFullPath));

            var success = !(await Geoprocessing.ExecuteToolAsync("management.Delete", args)).IsFailed;

            if (success)
                Debug.WriteLine($"File geodatabase {esriGeodatabaseName} deleted");
            else
                Debug.WriteLine($"FAIL: Could not delete file geodatabase {esriGeodatabaseName}");
        }

        private async Task<bool> CreateProject()
        {
            var createProjectSettings = new CreateProjectSettings
            {
                Name = NewProjectName,
                LocationPath = ProjectFolder,
                TemplatePath = UseTopologyTemplate ? TopologyTemplateFile : NoTopologyTemplateFile
            };

            var success = await Project.CreateAsync(createProjectSettings) != null;

            if (success)
                Debug.WriteLine($"Project '{NewProjectName}' created. Has pre-made topology? {UseTopologyTemplate}");
            else
                Debug.WriteLine($"FAIL: Could not create project '{NewProjectName}'");

            return success;
        }

        private async Task SetDefaultGeodatabase()
        {
            await QueuedTask.Run(() => { Project.Current.SetDefaultGeoDatabasePath(DefaultGeodatabasePath); });
            Debug.WriteLine($"Default geodatabase updated. New path: {Project.Current.DefaultGeodatabasePath} ");
        }

        private async Task UpdateCoordinateSystem()
        {
            // Update for all the feature classes in the geodatabase
            Debug.WriteLine($"Update Feature Dataset projection success? {await UpdateFeatureDatasetProjection()}");

            foreach (var successTuple in await UpdateFeatureClassProjection())
                Debug.WriteLine($"Update {successTuple.FeatureClassName} projection success? {successTuple.Success}");
        }

        private async Task<bool> UpdateFeatureDatasetProjection()
        {
            var gpArgs = await QueuedTask.Run(() =>
            {
                using (var geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath))))
                using (var featureDataset = geodatabase.OpenDataset<FeatureDataset>(FeatureDatasetName))
                    return Geoprocessing.MakeValueArray(featureDataset, ProjectSpatialReference);
            });

            var success = !(await Geoprocessing.ExecuteToolAsync("management.DefineProjection", gpArgs, null, null, null, GPExecuteToolFlags.None)).IsFailed;

            if (success)
                Debug.WriteLine($"Updated Feature Dataset '{FeatureDatasetName}' projection");
            else
                Debug.WriteLine($"FAIL: Could not update project for Feature Dataset '{FeatureDatasetName}'");

            return success;
        }

        private async Task<List<(bool Success, string FeatureClassName)>> UpdateFeatureClassProjection()
        {
            var successList = new List<(bool Success, string FeatureClassName)>();

            var gpArgs = await QueuedTask.Run(() =>
            {
                var valueArrays = new List<IReadOnlyList<string>>();

                using (var geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath))))
                {
                    var fcDefinitions = geodatabase.GetDefinitions<FeatureClassDefinition>();
                    try
                    {
                        foreach (var featureClassDefinition in fcDefinitions)
                            using (var featureClass = geodatabase.OpenDataset<FeatureClass>(featureClassDefinition.GetName()))
                                valueArrays.Add(Geoprocessing.MakeValueArray(featureClass, ProjectSpatialReference));
                    }
                    finally
                    {
                        foreach (var fcDefinition in fcDefinitions)
                            fcDefinition?.Dispose();
                    }
                }

                return valueArrays;
            });

            foreach (var gpArg in gpArgs)
            {
                var success = !(await Geoprocessing.ExecuteToolAsync("management.DefineProjection", gpArg, null, null, null, GPExecuteToolFlags.None)).IsFailed;

                successList.Add((success, gpArg[0]));
            }

            return successList;
        }

        private async Task UpdateMapProjection()
        {
            var mapProjectItems = Project.Current.GetItems<MapProjectItem>().ToList();

            var results = await QueuedTask.Run(() =>
            {
                var createMapPaneTasks = new List<Task<IMapPane>>();

                foreach (var mapProjectItem in mapProjectItems)
                {
                    var map = mapProjectItem.GetMap();
                    map.SetSpatialReference(ProjectSpatialReference);
                    Debug.WriteLine($"Spatial reference for map {map.Name} updated: {ProjectSpatialReference.Name}");
                    createMapPaneTasks.Add(FrameworkApplication.Panes.CreateMapPaneAsync(mapProjectItem.GetMap()));
                }

                return Task.WhenAll(createMapPaneTasks);
            });

            foreach (var result in results)
            {
                Debug.WriteLine($"Map {result.Caption} opened.");
            }
        }

        private async Task ZoomToExtents()
        {
            while (MapView.Active == null || !MapView.Active.IsReady)
                await Task.Delay(25); // This should never execute

            await QueuedTask.Run(() => MapView.Active.ZoomTo(MapView.Active.Map.CalculateFullExtent()));
        }

        #endregion

        #region Events

        private void btnFolder_Click(object sender, RoutedEventArgs e)
        {
            var openItemDialog = new OpenItemDialog
            {
                InitialLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                MultiSelect = false,
                Title = "Select project folder",
                Filter = ItemFilters.folders
            };

            if (openItemDialog.ShowDialog() != true)
                return;

            if (openItemDialog.Items.Any())
                ProjectFolder = openItemDialog.Items.First().Path;
        }

        private async void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                this.IsEnabled = false;
                await CreateNewProject();
                this.IsEnabled = true;

                DialogResult = true;
                this.Close();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}