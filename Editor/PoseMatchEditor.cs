using System;
using UnityEngine;
using UnityEditor;

namespace PhysAnim
{
    [CustomEditor(typeof(PoseMatch))]
    public class PoseMatchEditor : Editor
    {
        SerializedProperty Profile;
        SerializedProperty State;
        SerializedProperty Reference;

        private void OnEnable()
        {
            Profile = serializedObject.FindProperty("Profile");
            State = serializedObject.FindProperty("State");
            Reference = serializedObject.FindProperty("Reference");
        }

        public override void OnInspectorGUI()
        {
            PoseMatch poseMatch = (PoseMatch)target;

            if (poseMatch == null)
                return;
            serializedObject.Update();
            EditorGUILayout.PropertyField(Profile);
            EditorGUILayout.PropertyField(State);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
