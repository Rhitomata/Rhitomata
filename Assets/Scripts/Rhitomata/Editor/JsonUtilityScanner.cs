using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Scans all scripts and shows a warning if JsonUtility is used instead of RhitomataSerializer.
/// Requires Clear on Recompiple to be disabled in the Console settings, otherwise the warning won't show up!
/// </summary>
public class JsonUtilityScanner : AssetPostprocessor {
    static List<string> forbiddenCodes = new() {
        "JsonUtility.ToJson",
        "JsonUtility.FromJson"
    };

    static readonly string[] ignorePaths = new[] {
        "JsonUtilityScanner.cs"
    };

    static void OnPostprocessAllAssets(string[] importedAssets, string[] _, string[] __, string[] ___) {
        foreach (string path in importedAssets) {
            if (!path.EndsWith(".cs"))
                continue;

            if (IsIgnored(path))
                continue;

            string contents = File.ReadAllText(path);
            foreach (var match in forbiddenCodes) {
                if (contents.Contains(match)) {
                    Debug.LogWarning($"JsonUtility usage detected in: {path}\nConsider using RhitomataSerializer instead.");
                    break;
                }
            }
        }
    }

    static bool IsIgnored(string path) {
        foreach (var ignore in ignorePaths) {
            if (path.Contains(ignore))
                return true;
        }
        return false;
    }
}