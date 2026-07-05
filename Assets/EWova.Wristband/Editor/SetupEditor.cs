using UnityEditor;

namespace EWova.Wristband.Editor
{
    [CustomEditor(typeof(Setup))]
    internal class SetupEditor : UnityEditor.Editor
    {
        private SerializedProperty _fallbackFeatures;
        private SerializedProperty _offlineMode;
        private SerializedProperty _editorTestFeatures;

        private void OnEnable()
        {
            _fallbackFeatures   = serializedObject.FindProperty("m_fallbackFeatures");
            _offlineMode        = serializedObject.FindProperty("m_editorOfflineMode");
            _editorTestFeatures = serializedObject.FindProperty("m_editorTestFeatures");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_fallbackFeatures);

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(_offlineMode);

            if (_offlineMode.boolValue)
                EditorGUILayout.PropertyField(_editorTestFeatures);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
