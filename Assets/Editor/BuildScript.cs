using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildScript
{
    [MenuItem("Build/Build Android APK")]
    public static void BuildAndroidAPK()
    {
        string buildPath = "Build/Android";
        Directory.CreateDirectory(buildPath);

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/SampleScene.unity" };
        buildPlayerOptions.locationPathName = buildPath + "/VRRunner.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.targetGroup = BuildTargetGroup.Android;
        buildPlayerOptions.options = BuildOptions.None;

        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Android APK build completed!");
    }
}