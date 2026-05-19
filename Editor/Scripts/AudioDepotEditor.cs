using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using KulibinSpace.AudioDepot;

[CustomEditor(typeof(AudioDepot))]
public class AudioDepotEditor : Editor {

    SerializedProperty clipsProp;
    ReorderableList list;

    void OnEnable () {
        clipsProp = serializedObject.FindProperty("clips");

        list = new ReorderableList(serializedObject, clipsProp, true, true, true, true);

        list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Audio Clips");
        };

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            SerializedProperty element = clipsProp.GetArrayElementAtIndex(index);

            SerializedProperty nameProp = element.FindPropertyRelative("name");
            SerializedProperty asideProp = element.FindPropertyRelative("aside");
            SerializedProperty volumeProp = element.FindPropertyRelative("volume");
            SerializedProperty clipProp = element.FindPropertyRelative("audioclip");
            SerializedProperty phasesProp = element.FindPropertyRelative("phases");

            rect.y += 2;

            // ==== Формируем заголовок ====
            string label = "";

            // имя
            label += string.IsNullOrEmpty(nameProp.stringValue)
                ? $"Clip {index}"
                : nameProp.stringValue;

            // Aside
            if (asideProp.boolValue) {
                label += " (Aside)";
            }

            // Volume
            label += $" | Vol {volumeProp.floatValue:0.##}";

            // Имя файла
            if (clipProp.objectReferenceValue != null) {
                string clipName = clipProp.objectReferenceValue.name;
                if (clipName.Length > 10) clipName = clipName.Substring(0, 10) + "...";
                label += $" | {clipName}";
            }

            // Foldout
            float foldoutOffset = 15f; // место под drag handle
            element.isExpanded = EditorGUI.Foldout(
                new Rect(rect.x + foldoutOffset, rect.y, rect.width - foldoutOffset, EditorGUIUtility.singleLineHeight),
                element.isExpanded,
                label,
                true
            );

            if (element.isExpanded) {
                EditorGUI.indentLevel++;

                float y = rect.y + EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight), nameProp);
                y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight), asideProp);
                y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight), volumeProp);
                y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight), clipProp);
                y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, EditorGUI.GetPropertyHeight(phasesProp)), phasesProp, true);

                EditorGUI.indentLevel--;
            }
        };

        list.elementHeightCallback = (int index) => {
            SerializedProperty element = clipsProp.GetArrayElementAtIndex(index);

            if (!element.isExpanded)
                return EditorGUIUtility.singleLineHeight + 4;

            SerializedProperty phasesProp = element.FindPropertyRelative("phases");

            float height = 0;
            height += EditorGUIUtility.singleLineHeight + 2; // foldout
            height += (EditorGUIUtility.singleLineHeight + 2) * 4; // поля
            height += EditorGUI.GetPropertyHeight(phasesProp);

            return height + 6;
        };

        // === Автоинкремент имени ===
        list.onAddCallback = (ReorderableList l) => {
            int index = clipsProp.arraySize;
            clipsProp.InsertArrayElementAtIndex(index);

            SerializedProperty element = clipsProp.GetArrayElementAtIndex(index);
            SerializedProperty nameProp = element.FindPropertyRelative("name");

            nameProp.stringValue = GenerateName(index);

            serializedObject.ApplyModifiedProperties();
        };
    }

    public override void OnInspectorGUI () {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "clips");

        list.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    string GenerateName (int index) {
        return $"clip_{index:00}";
    }
}
