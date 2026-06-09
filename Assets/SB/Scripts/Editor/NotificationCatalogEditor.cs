using SB.App.Application;
using UnityEditor;
using UnityEngine;

namespace SB.App.Editor
{
    [CustomEditor(typeof(NotificationCatalog))]
    public sealed class NotificationCatalogEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("알림 규칙 편집 창 열기"))
                NotificationScheduleEditorWindow.Open();
        }
    }
}
