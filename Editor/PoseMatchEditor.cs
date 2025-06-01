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
            PoseMatch poseMatch = (PoseMatch)target;

            Profile = serializedObject.FindProperty("_profile");
            State = serializedObject.FindProperty("State");
            Reference = serializedObject.FindProperty("Reference");

            PoseMatch[] l = poseMatch.transform.GetComponentsInChildren<PoseMatch>();
            if (l.Length > 1)
            {
                EditorUtility.DisplayDialog("The component PoseMatch can't be added because" + poseMatch.gameObject + "already contains the same component.", "Cancel" ,"");
                DestroyImmediate(poseMatch);
            }
        }

        public override void OnInspectorGUI()
        {
            PoseMatch poseMatch = (PoseMatch)target;

            if (poseMatch == null)
                return;
            serializedObject.Update();
            EditorGUILayout.PropertyField(Profile);
            EditorGUILayout.PropertyField(State);
            EditorGUILayout.PropertyField(Reference);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
