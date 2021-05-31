using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEngine;

/// <summary>
/// Gets the assembly definition file of each user script (MonoBehaviour) that is currently loaded
/// and checks if it supports the selected build platform. It generates warnings if that's not the case.
/// </summary>
public class AsmDefPlatformSupportCheck : IPreprocessBuildWithReport
{
	public int callbackOrder => 0;

#pragma warning disable CS0649
	class AsmDefData
	{
		public string name;
		public string[] includePlatforms;
		public string[] excludePlatforms;
	}
#pragma warning restore CS0649

	[MenuItem("Tools/Check if all .asmdef Support Current Platform")]
	public static void CheckAssemblySupport()
	{
		//you know what? fuck it. this can be useful even if it's just for the currently loaded scene, so let's do that.
		var asmDefPaths = GetAsmDefPathsForCurrentScene();

		BuildTarget currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
		AssemblyDefinitionPlatform[] asmDefPlatforms = CompilationPipeline.GetAssemblyDefinitionPlatforms();
		AssemblyDefinitionPlatform currentAsmDefPlatform = asmDefPlatforms.Single(def => def.BuildTarget == currentBuildTarget);

		foreach (string path in asmDefPaths)
		{
			using (FileStream fileStream = File.OpenRead(path))
			{
				using (var reader = new StreamReader(fileStream))
				{
					//usually asmdef files ain't too big, so let's do it the quick and hacky way.
					string json = reader.ReadToEnd();
					var data = JsonUtility.FromJson<AsmDefData>(json);

					//Only complain about assemblies that are not editor only (those probably have a good reason to be)
					if (!IsPlatformSupported(currentAsmDefPlatform, data) && !IsEditorOnlyAssembly(data))
					{
						Debug.LogWarning($"Assembly at {path} does not support currently selected build platform ({currentAsmDefPlatform.Name})");
					}
					//else
					//{
					//	Debug.Log($"Assembly at {path} supports the currently selected build platform ({currentAsmDefPlatform.Name})");
					//}
				}
			}
		}
	}

	private static HashSet<string> GetAsmDefPathsForCurrentScene()
	{
		//this returns all MonoBehaviours that are loaded, see:
		//https://docs.unity3d.com/2018.4/Documentation/ScriptReference/Resources.FindObjectsOfTypeAll.html
		MonoBehaviour[] scripts = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
		ScriptableObject[] scriptableObjects = Resources.FindObjectsOfTypeAll<ScriptableObject>();

		var asmDefPaths = new HashSet<string>();

		foreach (var script in scripts)
		{
			var monoScript = MonoScript.FromMonoBehaviour(script);
			string scriptPath = AssetDatabase.GetAssetPath(monoScript.GetInstanceID());
			string assemblyName = CompilationPipeline.GetAssemblyNameFromScriptPath(scriptPath);
			string asmDefPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assemblyName);

			if (asmDefPath != null)
			{
				asmDefPaths.Add(asmDefPath);
			}
		}

		//hacky support for ScriptableObject
		foreach (var obj in scriptableObjects)
		{
			//apparently this can fail for some ScriptableObjects ._.
			var monoScript = MonoScript.FromScriptableObject(obj);

			if (monoScript != null)
			{
				string scriptPath = AssetDatabase.GetAssetPath(monoScript.GetInstanceID());
				string assemblyName = CompilationPipeline.GetAssemblyNameFromScriptPath(scriptPath);
				string asmDefPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assemblyName);

				if (asmDefPath != null)
				{
					asmDefPaths.Add(asmDefPath);
				}
			}
		}

		return asmDefPaths;
	}

	private static bool IsPlatformSupported(AssemblyDefinitionPlatform currentPlatform, AsmDefData data)
	{
		bool supported = false;

		//so from what I've seen when deserializing with JsonUtility:
		// "includePlatforms": [] -> string[0]
		// "excludePlatforms": [] -> string[0]
		// if nothing is in the file -> null
		string[] includePlatforms = data.includePlatforms;
		string[] excludePlatforms = data.excludePlatforms;

		if (includePlatforms != null && includePlatforms.Length > 0)
		{
			//SingleOrDefault returns null if it doesn't find anything.
			string platformName = includePlatforms.SingleOrDefault(plat => plat == currentPlatform.Name);

			if (platformName != null)
			{
				supported = true;
			}
		}
		else if (excludePlatforms != null && excludePlatforms.Length > 0)
		{
			//if we find our platform here, it's not supported.
			//conversely, if we don't find it it is supported.
			string platformName = excludePlatforms.SingleOrDefault(plat => plat == currentPlatform.Name);

			if (platformName == null)
			{
				supported = true;
			}
		}
		else
		{
			//if we end up here, do we implicitly support all platforms? I guess we do.
			//Quick look in editor verifies: "any platform" is ticked when the file contains neither include nor exclude.
			supported = true;
		}

		return supported;
	}

	private static bool IsEditorOnlyAssembly(AsmDefData data)
	{
		string[] includePlatforms = data.includePlatforms;

		if (includePlatforms != null && includePlatforms.Length == 1 && includePlatforms[0] == "Editor")
		{
			return true;
		}

		return false;
	}

	public void OnPreprocessBuild(BuildReport report)
	{
		CheckAssemblySupport();
	}
}
