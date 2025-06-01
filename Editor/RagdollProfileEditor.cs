#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace PhysAnim
{
    [CustomEditor(typeof(RagdollProfile))]
    public class RagdollProfileEditor : Editor
    {
        private SerializedProperty _joints;
        private SerializedProperty _damping;
        private RagdollProfile     _profile;

        private void OnEnable()
        {
            _profile = (RagdollProfile)target;
            _joints = serializedObject.FindProperty("Joints");
            _damping = serializedObject.FindProperty("Damping");
        }

        public override void OnInspectorGUI()
        {
            PoseMatch ps;

            serializedObject.Update();
            if (!_profile.TryGetComponent(out ps))
            {
                EditorGUILayout.HelpBox("Can't access auto-add and joint conversion when there are no Pose Match Component.",
                                        MessageType.Warning);
                GUI.enabled = false;
            }
            if (GUILayout.Button("Convert character joints to configurable joints"))
            {
                if (ps.Reference == null)
                    Debug.LogError("Character joints can't be detected because the Reference's Root is not defined");
                else
                {
                    CharacterJoint[] char_joints = ps.Reference.transform.GetComponentsInChildren<CharacterJoint>();
                    foreach (CharacterJoint cj in char_joints)
                    {
                        ConfigurableJoint new_j = PhysAnimUtilities.RecursiveFindChild(ps.Reference.transform, cj.name).gameObject.AddComponent<ConfigurableJoint>();
                        PhysAnimUtilities.ConvertCharJointToConfigurableJoint(cj, ref new_j);
                        DestroyImmediate(cj);
                    }
                }
            }
            if (GUILayout.Button("Auto-add joints"))
            {
                if (ps.Reference == null)
                    Debug.LogError("Joints can't be detected because the Reference's Root is not defined");
                else
                {
                    ConfigurableJoint[] joints = ps.Reference.transform.GetComponentsInChildren<ConfigurableJoint>();
                    foreach (ConfigurableJoint j in joints)
                        _profile.Add(new Joint(j));
                }
            }
            GUI.enabled = true;
            EditorGUILayout.PropertyField(_damping);
            EditorGUILayout.PropertyField(_joints);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif