#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace PhysAnim
{
    [CustomEditor(typeof(RagdollProfile))]
    public class RagdollProfileEditor : Editor
    {
        SerializedProperty Joints;
        SerializedProperty PoseMatch;
        SerializedProperty Damping;
        RagdollProfile     Profile;

        private void OnEnable()
        {
            Profile = (RagdollProfile)target;

            Joints = serializedObject.FindProperty("Joints");
            PoseMatch = serializedObject.FindProperty("PoseMatch");
            Damping = serializedObject.FindProperty("Damping");
            if (Profile.transform.TryGetComponent(out PoseMatch ps))
                Profile.PoseMatch = ps;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (GUILayout.Button("Convert character joints to configurable joints"))
            {
                if (Profile.PoseMatch.Reference == null)
                    throw new ArgumentException("Character joints can't be detected because the Reference's Root is not defined");
                CharacterJoint[] char_joints = Profile.PoseMatch.Reference.transform.GetComponentsInChildren<CharacterJoint>();
                foreach (CharacterJoint cj in char_joints)
                {
                    ConfigurableJoint new_j = PhysAnimUtilities.RecursiveFindChild(Profile.PoseMatch.Reference.transform, cj.name).gameObject.AddComponent<ConfigurableJoint>();
                    PhysAnimUtilities.ConvertCharJointToConfigurableJoint(cj, ref new_j);
                    DestroyImmediate(cj);
                }
            }
            if (GUILayout.Button("Auto-add joints"))
            {
                if (Profile.PoseMatch.Reference == null)
                    throw new ArgumentException("Character joints can't be detected because the Reference's Root is not defined");
                ConfigurableJoint[] joints = Profile.PoseMatch.Reference.transform.GetComponentsInChildren<ConfigurableJoint>();
                foreach (ConfigurableJoint j in joints)
                    Profile.Add(new Joint(j));
            }
            EditorGUILayout.PropertyField(PoseMatch);
            EditorGUILayout.PropertyField(Damping);
            EditorGUILayout.PropertyField(Joints);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif