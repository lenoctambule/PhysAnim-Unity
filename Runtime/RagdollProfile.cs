using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PhysAnim
{
    [Serializable]
    public class MotorizedJoint
    {
        public float                Strength;
        public ConfigurableJoint    Joint;

        [NonSerialized]
        public ConfigurableJoint    RagdollJoint;
        [NonSerialized]
        public Quaternion           StartRotation;      

        public MotorizedJoint(float strength, ConfigurableJoint joint)
        {
            this.Strength       = strength;
            this.Joint          = joint;
            this.RagdollJoint   = null;
            this.StartRotation  = Quaternion.identity;
        }
    }

    [Serializable]
    public class KeyframedJoint
    {
        [Range(0, 1)]
        public float                Stiffness;
        public Transform            Limb;
        [NonSerialized]
        public Transform            RagdollLimb;

        public KeyframedJoint(float Stiffness, Transform Limb)
        {
            this.Stiffness      = Stiffness;
            this.Limb           = Limb;
            this.RagdollLimb    = null;
        }
    }

    [AddComponentMenu("PhysAnim/Ragdoll Profile")]
    public class RagdollProfile : MonoBehaviour
    {
        public List<MotorizedJoint> MotorJoints;
        public List<KeyframedJoint> KeyFramedJoints;
        public PhysicMaterial       RagdollMaterial;
        public float                Damping;
        public PoseMatch            PoseMatch;

        private Dictionary<Transform, MotorizedJoint> _cached_mjoints = new();
        private Dictionary<Transform, KeyframedJoint> _cached_kjoints = new();

        public bool Add(MotorizedJoint mj) {
            foreach (MotorizedJoint j in MotorJoints)
            {
                if (j.Joint.transform == mj.Joint.transform)
                    return false;
            }
            MotorJoints.Add(mj);
            return true;
        }

        public bool Add(KeyframedJoint kj) {
            foreach (KeyframedJoint j in KeyFramedJoints)
            {
                if (j.Limb == kj.Limb)
                    return false;
            }
            KeyFramedJoints.Add(kj);
            return true;
        }

        public MotorizedJoint getMotorizedJoint(Transform obj) {
            return _cached_mjoints[obj];
        }

        public KeyframedJoint getKeyframeJoint(Transform obj) {
            return _cached_kjoints[obj];
        }

        public void Enable()
        {
            _cached_mjoints.Clear();
            _cached_kjoints.Clear();
            InitJoints();
        }

        private void InitJoints()
        {
            Transform ragdoll = PoseMatch.GetRagdoll().transform;

            foreach(MotorizedJoint j in MotorJoints)
            {    
                if (j.Joint != null)
                {
                    Transform ragdoll_joint = PhysAnimUtilities.RecursiveFindChild(ragdoll, j.Joint.transform.name);
                    if (ragdoll_joint.TryGetComponent<Collider>(out Collider col))
                        col.material = RagdollMaterial;
                    j.RagdollJoint = ragdoll_joint.GetComponent<ConfigurableJoint>();
                    j.StartRotation = ragdoll_joint.localRotation;
                    _cached_mjoints[j.Joint.transform] = j;
                }
            }
            foreach(KeyframedJoint j in KeyFramedJoints)
            {
                if (!j.Limb.TryGetComponent(out Rigidbody rb))
                    Debug.LogError("Keyframed Joint "+ j.Limb.name + " does not have a Rigid Body");
                else if (j.Limb != null)
                {
                    Transform ragdoll_limb = PhysAnimUtilities.RecursiveFindChild(PoseMatch.GetRagdoll().transform, j.Limb.transform.name);
                    if (ragdoll_limb.TryGetComponent<Collider>(out Collider col))
                        col.material = RagdollMaterial;
                    j.RagdollLimb = ragdoll_limb.transform;
                    _cached_kjoints[j.Limb.transform] = j;
                }
            }
        }
    }
}
