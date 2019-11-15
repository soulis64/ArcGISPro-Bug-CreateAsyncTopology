# ArcGISPro-Bug-CreateAsyncTopology
MRE for the adding features to feature classes in a topology in a project newly created by Project.CreateAsync().

## Branches
 - The master branch contains the MRE
 - The Workaround_CreateTopoOnCreateProject branch contains a workaround by creating a topology when creating a new project
 
## The Bug
In a project created using Project.CreateAsync() and a project template that includes a file geodatabase with a topology, adding features to feature classes that are part of the topology via an EditOperation fails. This MRE was created based code for an internal project, so I am unsure if the projections or fdb changes cause the issue, so they are included in the code, but can be easily toggled off/commented out/removed in CreateNewProjectWindow.CreateNewProject().

**Note**: The bug appears to occur intermittently, so if it does not appear in first project created, try creating another to trigger it. Additionally, the bug is only present when the project is newly created. Closing ArcGIS Pro and reopening the project allows features to be created.

## Reproduction Steps
### Given (included in the MRE)
 - A file geodatabase with a topology containing at least 1 feature class
 - A project template that includes the file geodatabase
 - An add-in that contains tools for: 
   - Creating a project using Project.CreateAsync() and the project template
   - Creating features for the topology-contained feature class using EditOperation.Create()

### Steps
1. Click the 'Create New Project' button in the 'Repro Issue' ribbon tab
1. Enter a project name, pick a destination and coordinate system
1. Click the 'Create Non-Dataset Features' button
   - Result: Features are created for the non-dataset feature class (NonDatasetFeatureClass)
1. Click the 'Create Non-Topology Dataset Features' button
   - Result: Features are created for the non-topology dataset feature class (NonTopologyFeatureClass)
1. Click the 'Create Topology Features' button
   - Expected: Features are created for the topology feature classes (ParentFeatureClass and ChildFeatureClass)
   - Actual: The edit operation fails

## Relevant Specs, Versions, etc.
 - OS Name: Microsoft Windows 10 Pro
 - Version: 10.0.18362 Build 18362
 - Processor: Intel(R) Core(TM) i7-8750H CPU @ 2.20GHz, 2208 Mhz, 6 Core(s), 12 Logical Processor(s)
 - Installed Physical Memory (RAM): 16.0 GB
 - GPU: NVIDIA GeForce GTX 1070
 - ArcGIS Pro 2.4.1
 - MRE is targeting .NET 4.6.2

## The Workaround
The workaround functions by creating the topology after the new project is created, which seems to circumvent the issue.