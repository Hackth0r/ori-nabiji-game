using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public sealed class ProductionBuildValidator : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new BuildFailedException("No enabled scenes are configured in Build Settings.");

        if (string.IsNullOrWhiteSpace(PlayerSettings.productName) ||
            PlayerSettings.productName == "TA-Casual-Farm")
            throw new BuildFailedException("Set a production product name before building.");

        if (string.IsNullOrWhiteSpace(PlayerSettings.companyName) ||
            PlayerSettings.companyName == "DefaultCompany")
            throw new BuildFailedException("Set a production company name before building.");

        string identifier = PlayerSettings.GetApplicationIdentifier(report.summary.platformGroup);
        if (string.IsNullOrWhiteSpace(identifier) ||
            identifier.Contains("DefaultCompany", StringComparison.OrdinalIgnoreCase))
            throw new BuildFailedException("Set a production application identifier before building.");

        if ((report.summary.options & BuildOptions.Development) != 0)
            throw new BuildFailedException("Production builds must not use the Development option.");
    }
}
