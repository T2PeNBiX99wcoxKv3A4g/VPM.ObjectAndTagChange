using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Refs: https://booth.pm/ja/items/6475280
namespace io.github.ykysnk.ObjectAndTagChange.Editor
{
    public class SetObjectAndTag : EditorWindow
    {
        private static readonly Dictionary<int, string> OriginalTags = new();
        private static readonly Dictionary<int, bool> WasActives = new();
        private const string EditorOnlyTag = "EditorOnly";
        private const string UntaggedTag = "Untagged";
        private const string MenuPath = "Tools/Set Object and Tag #e";
        private const string MenuPath2 = "GameObject/yky/Set Object and Tag";

        [MenuItem(MenuPath)]
        [MenuItem(MenuPath2)]
        private static void ToggleInactiveAndTag(MenuCommand menuCommand)
        {
            if (!ShouldExecute(menuCommand)) return;

            var selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length < 1)
            {
                Debug.LogWarning("オブジェクトが選択されていません。");
                return;
            }

            foreach (var obj in selectedObjects)
            {
                var id = obj.GetInstanceID();

                if (obj.activeSelf)
                {
                    if (!OriginalTags.ContainsKey(id))
                    {
                        OriginalTags.TryAdd(id, obj.tag);
                        WasActives.TryAdd(id, obj.activeSelf);
                    }

                    Undo.RecordObject(obj, "Change EditorOnly");
                    obj.SetActive(false);
                    obj.tag = EditorOnlyTag;
                }
                else
                {
                    var hasOriginalTag = OriginalTags.TryGetValue(id, out var originalTag);
                    var hasActive = WasActives.TryGetValue(id, out var wasActive);

                    Undo.RecordObject(obj, "Undo Tag");
                    obj.SetActive(!hasActive || wasActive);
                    if (obj.CompareTag(EditorOnlyTag))
                        obj.tag = hasOriginalTag && originalTag != EditorOnlyTag ? originalTag : UntaggedTag;

                    OriginalTags.Remove(id);
                    WasActives.Remove(id);
                }

                EditorUtility.SetDirty(obj);
            }
        }

        // Refs: https://discussions.unity.com/t/how-to-execute-menuitem-for-multiple-objects-once/91492/5
        private static bool ShouldExecute(MenuCommand menuCommand)
        {
            if (menuCommand.context == null) return true;
            return menuCommand.context == Selection.activeObject;
        }
    }
}