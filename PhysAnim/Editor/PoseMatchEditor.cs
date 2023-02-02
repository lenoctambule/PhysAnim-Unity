using System;
using UnityEngine;
using UnityEditor;


namespace PhysAnim
{
    [CustomEditor(typeof(PoseMatch))]
    public class PoseMatchEditor : Editor
    {
        SerializedProperty profile;
        SerializedProperty MotorDamping;
        SerializedProperty MotorStrength;
        SerializedProperty rag_state;
        private void OnEnable()
        {
            profile = serializedObject.FindProperty("profile");
            MotorDamping = serializedObject.FindProperty("MotorDamping");
            MotorStrength = serializedObject.FindProperty("MotorStrength");
            rag_state = serializedObject.FindProperty("rag_state");
            
        }

        public override void OnInspectorGUI()
        {
            
            PoseMatch poseMatch = (PoseMatch)target;
            if (poseMatch == null) return;
            
            if (GUILayout.Button("Auto-detect and add character joints"))
            {
                if (poseMatch.profile.Reference == null) throw new ArgumentException("Character joints can't be detected because the Reference's Root is not defined");
                CharacterJoint[] char_joints = poseMatch.profile.Reference.transform.GetComponentsInChildren<CharacterJoint>();
                foreach (CharacterJoint j in char_joints)
                    poseMatch.profile.MotorJoints.Add(new MotorizedJoint(1.0f, j));
            }

            serializedObject.Update();
            EditorGUILayout.PropertyField(profile);
            EditorGUILayout.PropertyField(MotorDamping);
            EditorGUILayout.PropertyField(MotorStrength);
            EditorGUILayout.PropertyField(rag_state);
            serializedObject.ApplyModifiedProperties();

            //base.OnInspectorGUI();
        }
    }
}
