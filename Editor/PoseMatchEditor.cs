using System;
using UnityEngine;
using UnityEditor;

namespace PhysAnim
{
    [CustomEditor(typeof(PoseMatch))]
    public class PoseMatchEditor : Editor
    {
        private SerializedProperty _profile;
        private SerializedProperty _state;
        private SerializedProperty _reference;
        private PoseMatch          _poseMatch;

        private void OnEnable()
        {
            _poseMatch = (PoseMatch)target;
            _profile = serializedObject.FindProperty("_profile");
            _reference = serializedObject.FindProperty("Reference");
        }

        public override void OnInspectorGUI()
        {
            if (_poseMatch == null)
                return;
            serializedObject.Update();
            EditorGUILayout.PropertyField(_profile);
            EditorGUILayout.PropertyField(_reference);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
