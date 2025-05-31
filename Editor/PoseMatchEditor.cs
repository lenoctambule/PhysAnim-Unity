using System;
using UnityEngine;
using UnityEditor;

namespace PhysAnim
{
    [CustomEditor(typeof(PoseMatch))]
    public class PoseMatchEditor : Editor
    {
        SerializedProperty Profile;
        SerializedProperty Damping;
        SerializedProperty MotorStrength;
        SerializedProperty State;

        private void OnEnable()
        {
            Profile = serializedObject.FindProperty("Profile");
            Damping = serializedObject.FindProperty("Damping");
            MotorStrength = serializedObject.FindProperty("MotorStrength");
            State = serializedObject.FindProperty("State");
        }

        public override void OnInspectorGUI()
        {
            PoseMatch poseMatch = (PoseMatch)target;

            if (poseMatch == null)
                return;
            if (GUILayout.Button("Convert character joints to configurable joints"))
            {
                if (poseMatch.Profile.Reference == null)
                    throw new ArgumentException("Character joints can't be detected because the Reference's Root is not defined");
                CharacterJoint[] char_joints = poseMatch.Profile.Reference.transform.GetComponentsInChildren<CharacterJoint>();
                foreach (CharacterJoint cj in char_joints)
                {
                    ConfigurableJoint new_j = PhysAnimUtilities.RecursiveFindChild(poseMatch.Profile.Reference.transform, cj.name).gameObject.AddComponent<ConfigurableJoint>();
                    PhysAnimUtilities.ConvertCharJointToConfigurableJoint(cj, ref new_j);
                    DestroyImmediate(cj);
                }
            }
            if (GUILayout.Button("Auto-add joints"))
            {
                if (poseMatch.Profile.Reference == null)
                    throw new ArgumentException("Character joints can't be detected because the Reference's Root is not defined");
                ConfigurableJoint[] joints = poseMatch.Profile.Reference.transform.GetComponentsInChildren<ConfigurableJoint>();
                foreach (ConfigurableJoint j in joints)
                {
                    poseMatch.Profile.AddMotor(new MotorizedJoint(1000.0f, j));
                    poseMatch.Profile.AddKeyframedJoint(new KeyframedJoint(1.0f, j.transform));
                }
            }
            serializedObject.Update();
            EditorGUILayout.PropertyField(Profile);
            EditorGUILayout.PropertyField(Damping);
            EditorGUILayout.PropertyField(State);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
