using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Useful {
    public static string GetRelativePath(string filePath, string rootDirPath) {
        var pathUri = new Uri(filePath);
        // Folders must end in a slash
        if (!rootDirPath.EndsWith(Path.DirectorySeparatorChar)) {
            rootDirPath += Path.DirectorySeparatorChar;
        }

        var folderUri = new Uri(rootDirPath);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar)).Replace('\\', '/');
    }

    public static Vector2 GetLocalDelta(Transform relativeTo, PointerEventData eventData) =>
        GetLocalDelta(relativeTo as RectTransform, eventData);

    public static Vector2 GetLocalDelta(RectTransform relativeTo, PointerEventData eventData) =>
        GetLocalPoint(relativeTo, eventData.position) - GetLocalPoint(relativeTo, eventData.position - eventData.delta);

    public static Vector2 GetLocalPoint(Transform relativeTo, Vector2 screenPosition) =>
        GetLocalPoint(relativeTo as RectTransform, screenPosition);

    public static Vector2 GetLocalPoint(RectTransform relativeTo, Vector2 screenPosition) {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(relativeTo, screenPosition, null, out var localPoint);
        return localPoint;
    }

    // Borrowed from JANOARG
    public static bool ContainsPointer(this RectTransform rt, PointerEventData eventData) =>
        RectTransformUtility.RectangleContainsScreenPoint(rt, eventData.pressPosition, eventData.pressEventCamera);

    public static bool ToLocalPos(this RectTransform rt, PointerEventData eventData, out Vector2 pos) =>
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.pressPosition, eventData.pressEventCamera, out pos);
}
