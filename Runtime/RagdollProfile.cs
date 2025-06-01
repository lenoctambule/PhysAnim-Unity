using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PhysAnim
{
    public enum Mode
    {
        Local,
        Global,
        Global_and_Local, 
    }

    [Serializable]
    public class Joint
    {
        public ConfigurableJoint    Target;
        public Mode                 Mode;
        [NonSerialized]
        public ConfigurableJoint    RagdollJoint;

        [NonSerialized]
        public Quaternion           StartRotation;  
        public float                Strength;

        [Range(0, 1)]
        public float                Stiffness;

        public Joint(ConfigurableJoint target)
        {
            this.Target = target;
            this.Mode   = Mode.Local;
            this.Strength = 1000.0f;
            this.Stiffness = 1.0f;
        }
    }

    [AddComponentMenu("PhysAnim/Ragdoll Profile")]
    public class RagdollProfile : MonoBehaviour
    {
        public List<Joint>          Joints;
        public PhysicMaterial       RagdollMaterial;
        public float                Damping;
        public PoseMatch            PoseMatch;

        private Dictionary<Transform, Joint> _cached_joints = new();

        public bool Add(Joint joint)
        {
            foreach (Joint j in Joints)
            {
                if (j.Target.transform == joint.Target.transform)
                    return false;
            }
            Joints.Add(joint);
            return true;
        }

        public Joint getJoint(Transform obj) {
            return _cached_joints[obj];
        }

        public void Enable()
        {
            _cached_joints.Clear();
            if (!PoseMatch.GetRagdoll())
                return;
            InitJoints();
        }

        private void InitJoints()
        {
            Transform ragdoll = PoseMatch.GetRagdoll().transform;

            foreach (Joint j in Joints)
            {
                if (j.Target != null)
                {
                    Transform ragdoll_joint = PhysAnimUtilities.RecursiveFindChild(ragdoll, j.Target.transform.name);
                    if (ragdoll_joint.TryGetComponent<Collider>(out Collider col))
                        col.material = RagdollMaterial;
                    j.RagdollJoint = ragdoll_joint.GetComponent<ConfigurableJoint>();
                    j.StartRotation = ragdoll_joint.localRotation;
                    _cached_joints[j.Target.transform] = j; 
                }
            }
        }
    }
}
