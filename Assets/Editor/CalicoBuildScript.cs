using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Editor-only build helper (Assets/Editor/ is excluded from player builds
// automatically). Invoked via -executeMethod for a one-off Windows x64 build.
public static class CalicoBuildScript
{
    public static void BuildWindows64()
    {
        PlayerSettings.bundleVersion = "0.3.1 Alpha";

        var scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Windows/CalicoCombatLeague_0.3.1_Alpha/CalicoCombatLeague.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        Debug.Log($"BUILD_RESULT={summary.result} totalErrors={summary.totalErrors} totalWarnings={summary.totalWarnings} outputPath={summary.outputPath} sizeBytes={summary.totalSize}");

        if (summary.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }
}
