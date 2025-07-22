using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Scans all scripts and shows a warning if JsonUtility is used instead of RhitomataSerializer.
/// Requires Clear on Recompiple to be disabled in the Console settings, otherwise the warning won't show up!
/// </summary>
public class JsonUtilityScanner : AssetPostprocessor {
    private static readonly List<string> _forbiddenCodes = new() {
        "JsonUtility.ToJson",
        "JsonUtility.FromJson"
    };

    private static readonly string[] _ignorePaths = {
        "JsonUtilityScanner.cs",
        "Packages/com.unity"
    };

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] _, string[] __, string[] ___) {
        foreach (var path in importedAssets) {
            if (!path.EndsWith(".cs"))
                continue;

            if (IsIgnored(path))
                continue;

            var contents = File.ReadAllText(path);
            foreach (var match in _forbiddenCodes) {
                if (!contents.Contains(match)) continue;
                
                Debug.LogWarning($"JsonUtility usage detected in: {path}\nConsider using RhitomataSerializer instead.");
                break;
            }
        }
    }

    private static bool IsIgnored(string path) {
        foreach (var ignore in _ignorePaths) {
            if (path.Contains(ignore))
                return true;
        }
        return false;
    }
}