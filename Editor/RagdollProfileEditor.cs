using System;
using UnityEngine;
using UnityEditor;

namespace PhysAnim
{
    [CustomEditor(typeof(RagdollProfile))]
    public class RagdollProfileEditor : Editor
    {
        SerializedProperty MotorJoints;
        SerializedProperty KeyFramedJoints;
        SerializedProperty PoseMatch;
        SerializedProperty Damping;

        private void OnEnable()
        {
            RagdollProfile profile = (RagdollProfile)target;

            PoseMatch = serializedObject.FindProperty("PoseMatch");
            MotorJoints = serializedObject.FindProperty("MotorJoints");
            KeyFramedJoints = serializedObject.FindProperty("KeyFramedJoints");
            Damping = serializedObject.FindProperty("Damping");
            if (profile.transform.TryGetComponent(out PoseMatch ps))
                profile.PoseMatch = ps;
            else
            {
                profile.PoseMatch = profile.gameObject.AddComponent<PoseMatch>();
                profile.PoseMatch.Profile = profile;
            }
        }

        public override void OnInspectorGUI()
        {
            RagdollProfile profile = (RagdollProfile)target;

            serializedObject.Update();
            if (GUILayout.Button("Convert character joints to configurable joints"))
            {
                if (profile.PoseMatch.Reference == null)
                    throw new ArgumentException("Character joints can't be detected because the Reference's Root is not defined");
                CharacterJoint[] char_joints = profile.PoseMatch.Reference.transform.GetComponentsInChildren<CharacterJoint>();
                foreach (CharacterJoint cj in char_joints)
                {
                    ConfigurableJoint new_j = PhysAnimUtilities.RecursiveFindChild(profile.PoseMatch.Reference.transform, cj.name).gameObject.AddComponent<ConfigurableJoint>();
                    PhysAnimUtilities.ConvertCharJointToConfigurableJoint(cj, ref new_j);
                    DestroyImmediate(cj);
                }
            }
            if (GUILayout.Button("Auto-add joints"))
            {
                if (profile.PoseMatch.Reference == null)
                    throw new ArgumentException("Character joints can't be detected because the Reference's Root is not defined");
                ConfigurableJoint[] joints = profile.PoseMatch.Reference.transform.GetComponentsInChildren<ConfigurableJoint>();
                foreach (ConfigurableJoint j in joints)
                {
                    profile.Add(new MotorizedJoint(1000.0f, j));
                    profile.Add(new KeyframedJoint(1.0f, j.transform));
                }
            }
            EditorGUILayout.PropertyField(MotorJoints);
            EditorGUILayout.PropertyField(KeyFramedJoints);
            EditorGUILayout.PropertyField(Damping);
            serializedObject.ApplyModifiedProperties();

        }
    }
}