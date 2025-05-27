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

            if (poseMatch == null)
                return;
            if (GUILayout.Button("Convert character joints to configurable joints"))
            {
                if (poseMatch.profile.Reference == null)
                    throw new ArgumentException("Character joints can't be detected because the Reference's Root is not defined");
                CharacterJoint[] char_joints = poseMatch.profile.Reference.transform.GetComponentsInChildren<CharacterJoint>();
                foreach (CharacterJoint cj in char_joints)
                {
                    ConfigurableJoint new_j = PhysAnimUtilities.RecursiveFindChild(poseMatch.profile.Reference.transform, cj.name).gameObject.AddComponent<ConfigurableJoint>();
                    PhysAnimUtilities.ConvertCharJointToConfigurableJoint(cj, ref new_j);
                    DestroyImmediate(cj);
                }
            }
            if (GUILayout.Button("Auto-add joints"))
            {
                if (poseMatch.profile.Reference == null)
                    throw new ArgumentException("Character joints can't be detected because the Reference's Root is not defined");
                ConfigurableJoint[] joints = poseMatch.profile.Reference.transform.GetComponentsInChildren<ConfigurableJoint>();
                foreach (ConfigurableJoint j in joints)
                {
                    poseMatch.profile.MotorJoints.Add(new MotorizedJoint(1.0f, j));
                    poseMatch.profile.KeyFramedJoints.Add(new KeyframedJoint(1.0f, j.transform));
                }
            }
            serializedObject.Update();
            EditorGUILayout.PropertyField(profile);
            EditorGUILayout.PropertyField(MotorDamping);
            EditorGUILayout.PropertyField(MotorStrength);
            EditorGUILayout.PropertyField(rag_state);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
