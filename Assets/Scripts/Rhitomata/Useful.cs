using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Useful
{
    public static string GetRelativePath(string filePath, string rootDirPath)
    {
        var pathUri = new Uri(filePath);
        // Folders must end in a slash
        if (!rootDirPath.EndsWith(Path.DirectorySeparatorChar))
        {
            rootDirPath += Path.DirectorySeparatorChar;
        }

        var folderUri = new Uri(rootDirPath);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar)).Replace('\\', '/');
    }

    // Borrowed from JANOARG
    public static bool ContainsPointer(this RectTransform rt, PointerEventData eventData) =>
        RectTransformUtility.RectangleContainsScreenPoint(rt, eventData.pressPosition, eventData.pressEventCamera);
        
    public static bool ToLocalPos(this RectTransform rt,  PointerEventData eventData, out Vector2 pos) =>
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.pressPosition, eventData.pressEventCamera, out pos);
}
