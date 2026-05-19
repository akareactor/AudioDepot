using UnityEditor;
using UnityEngine;

namespace KulibinSpace.AudioDepot {

    [CustomPropertyDrawer(typeof(AudioclipPhase))]
    public class AudioclipPhaseDrawer : PropertyDrawer {

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

            SerializedProperty timeProp = property.FindPropertyRelative("time");
            SerializedProperty durationProp = property.FindPropertyRelative("duration");

            float labelWidth = 40f;
            float spacing = 5f;

            Rect r = position;
            r.height = EditorGUIUtility.singleLineHeight;

            // time
            Rect timeLabel = new Rect(r.x, r.y, labelWidth, r.height);
            Rect timeField = new Rect(r.x + labelWidth, r.y, (r.width / 2) - labelWidth - spacing, r.height);

            // duration
            Rect durLabel = new Rect(r.x + r.width / 2, r.y, labelWidth, r.height);
            Rect durField = new Rect(durLabel.x + labelWidth, r.y, (r.width / 2) - labelWidth, r.height);

            EditorGUI.LabelField(timeLabel, "Time");
            EditorGUI.PropertyField(timeField, timeProp, GUIContent.none);

            EditorGUI.LabelField(durLabel, "Dur");
            EditorGUI.PropertyField(durField, durationProp, GUIContent.none);
        }

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}
