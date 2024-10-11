# Spatial Awareness

Hololight Stream supports [Unity's Meshing Subsystem](https://docs.unity3d.com/es/2020.2/Manual/xrsdk-meshing.html) to retrieve mesh data from the HoloLens 2 client. This mesh data can be retrieved and displayed using [MRTK Spatial Awareness System](https://docs.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/features/spatial-awareness/spatial-awareness-getting-started?view=mrtkunity-2021-05) or [AR Foundations Mesh Manager](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/features/meshing.html).

Additionally to Meshing, Hololight Stream also supports [Plane Detection](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/features/plane-detection.html). The retrieved plane data can be used and displayed together with mesh data by [MRTK Scene Understanding Observer](#enabling-mrtk-scene-understanding).

## Getting Started

Before enabling Spatial Awareness, follow the getting started of [Getting Started](../README.md#getting-started) and add the [MRTK Toolkit](mrtk2_extension.md).

### Enabling MRTK Spatial Awareness

1. In the Unity Scene, select `Mixed Reality Toolkit`
2. On the `Inspector`, check the `Spatial Awareness->Enable Spatial Awareness System` checkbox
3. Set the `Spatial Awareness System Type` to `MixedRealitySpatialAwarenessSystem`
4. Set the profile to `HololightStreamSpatialAwarenessSystemProfile`

#### Additional information

For more information regarding the how to edit the observer settings, see [MRTK Spatial Awareness System](https://docs.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/features/spatial-awareness/spatial-awareness-getting-started?view=mrtkunity-2021-05).

> :warning: **When using custom profiles for the Spatial Awareness system and the Spatial Awareness observer,** it is important to create the correct profile types, as MRTK shows all profiles that use the same base profile type as possible candidates.

##### Performance Considerations

The `HololightStreamSpatialAwarenessSystemProfile` by default uses the `XR SDK Spatial Observer` with its default settings. These settings can be edited in order to specify the functionality of the spatial observer. The following are performance considerations when editing the settings:

- `Update Interval` - The update interval will determine how often the observer's center location is updated, in non-satationary observer mode. When using a lower rate, the meshes will be updated more often and the application will be less performant.
- `Observer Extents` - A larger bounding volume will display meshes in a larger area. If moving between rooms often, it is beneficial to set a larger volume to avoid constantly adding and removing meshes from the space. If remaining in a single location, smaller volumes are often more performant.
- `Level of Detail` - This will set the amount of triangles/cubic meter for the mesh generation. A finer mesh will reduce the performance of the application. Setting the value to custom will use the value set in `Triangles/Cubic Meter`. This is a value between 0.0-1.0, with 1.0 being the highest triangles/cubic meter.
- `Display Option` - The display option determines how meshes should be displayed. The visible setting will generate display of meshes in the space, however it will be the least performant of the settings. When possible, disable this option and use the meshes for occlusion only.

#### Limitations

1. Unity currently only supports the `Axis Aligned Cube` bounding volume. Any other observer shape's will not be allowed.
2. `Recalculate Normals` must be enabled to allow Unity to calculate the mesh normals. Without this, the generation will have unexpected behaviour.
3. If using a custom `Level of Detail`, the Unity will only accept a value between 0.0 - 1.0 for `Triangles/Cubic Meter`.
4. `Level of Detail` setting is not applicable to iOS Client.

### Enabling MRTK Scene Understanding

1. In the Unity Scene, select `Mixed Reality Toolkit`
2. On the `Inspector`, check the `Spatial Awareness->Enable Spatial Awareness System` checkbox
3. Set the `Spatial Awareness System Type` to `MixedRealitySpatialAwarenessSystem`
4. Set the profile to `HololightStreamSceneUnderstandingSystemProfile`

#### Additional information

For more information regarding the how to edit the scene understanding observer settings, see [MRTK Scene Understanding Observer](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk2/features/spatial-awareness/scene-understanding?view=mrtkunity-2021-05). Hololight Stream uses its own Scene Understanding Observer instead of Windows Mixed Reality Scene Understanding Observer, so observer needs to be adjusted in the MRTK examples.

> :warning: **When using custom profiles for the Spatial Awareness system and the Scene Understanding observer,** it is important to create the correct profile types, as MRTK shows all profiles that use the same base profile type as possible candidates. HololightStreamSceneUnderstandingObserver uses its own custom profile type, so it needs to be checked carefully.

##### Performance Considerations

The `HololightStreamSceneUnderstandingSystemProfile` by default uses the `Hololight Stream Scene Understanding Observer` with it's default settings. These settings can be edited in order to specify the functionality of the spatial observer. The following are performance considerations when editing the settings:

- `Update Interval` - The update interval will determine how often the observer's center location is updated, in non-satationary observer mode. When using a lower rate, the meshes will be updated more often and the application will be less performant.
- `Query Radius` - A larger radius will display meshes in a larger area. If moving between rooms often, it is beneficial to set a larger radius to avoid constantly adding and removing meshes from the space. If remaining in a single location, a smaller radius is often more performant.
- `World Mesh Level of Detail` - This will set the amount of triangles/cubic meter for the mesh generation. A finer mesh will reduce the performance of the application.

#### Limitations

1. `World Mesh Level of Detail` does not support custom values.
2. `World Mesh Level of Detail` setting is not applicable to iOS Client.
