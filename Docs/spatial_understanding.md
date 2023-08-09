# Spatial Awareness

ISAR supports [Unity's Meshing Subsystem](https://docs.unity3d.com/es/2020.2/Manual/xrsdk-meshing.html) to retrieve mesh data from the HoloLens 2 client. This mesh data can be retrieved and displayed using [MRTK Spatial Awareness System](https://docs.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/features/spatial-awareness/spatial-awareness-getting-started?view=mrtkunity-2021-05) or [AR Foundations Mesh Manager](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.1/manual/mesh-manager.html).

## Getting Started

Before enabling Spatial Awareness, follow the getting started of [Getting Started](README.md#getting-started) and add the [MRTK Toolkit](mrtkextension.md).

### Enabling MRTK Spatial Awareness

1. In the Unity Scene, select `Mixed Reality Toolkit`
2. On the `Inspector`, check the `Spatial Awareness->Enable Spatial Awareness System` checkbox
3. Set the `Spatial Awareness System Type` to `MixedRealitySpatialAwarenessSystem`
4. Set the profile to `IsarXRSDKSpatialAwarenessSystemProfile`

## Additional information

For more information regarding the how to edit the observer settings, see [MRTK Spatial Awareness System](https://docs.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/features/spatial-awareness/spatial-awareness-getting-started?view=mrtkunity-2021-05).

### Performance Considerations
The `IsarXRSDKSpatialAwarenessSystemProfile` by default uses the `XR SDK Spatial Observer` with it's default settings. These settings can be edited in order to specify the functionality of the spatial observer. The following is performance consideration when edited the settings:
- `Update Interval` - The update interval will determine how often the observers center location is updated, in non-satationary observer mode. When using a lower rate, the meshes will be updated more often and the application will be less performant.
- `Observer Extents` - A larger bounding volume will display meshes in a larger area. If moving between rooms often, it is beneficial to set a larger volume to avoid constantly adding and removing meshes from the space. If remaining in a single location, smaller volumes are often more performant.
- `Level of Detail` - This will set the amount of triangles/cubic meter for the mesh generation. A finer mesh will reduce the performance of the application. Setting the value to custom will use the value set in `Triangles/Cubic Meter`. This is a value between 0.0-1.0, with 1.0 being the highest triangles/cubic meter.
- `Display Option` - The display option determines how meshes should be displayed. The visible setting will generate display of meshes in the space, however it will be the least performant of the settings. When possible, disable this option and use the meshes for occlusion only.

## Limitations

1. Unity currently only supports the `Axis Aligned Cube` bounding volume. Any other observer shape's will not be allowed.
2. `Recalculate Normals` must be enabled to allow Unity to calculate the mesh normals. Without this, the generation will have unexpected behaviour.
3. If using a custom `Level of Detail`, the Unity will only accept a value between 0.0 - 1.0 for `Triangles/Cubic Meter`.

