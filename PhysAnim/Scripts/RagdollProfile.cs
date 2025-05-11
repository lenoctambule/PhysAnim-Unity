using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysAnim
{
    [Serializable]
    public struct MotorizedJoint
    {
        [Range(0, 1)]
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
    public struct KeyframedJoint
    {
        [Range(0, 1)]
        public float                Stiffness;
        public Transform            Limb;
        public Transform            RagdollLimb;

        public KeyframedJoint(float Stiffness, Transform Limb)
        {
            this.Stiffness      = Stiffness;
            this.Limb           = Limb;
            this.RagdollLimb    = null;
        }
    }

    [Serializable]
    public class RagdollProfile
    {
        public List<MotorizedJoint> MotorJoints;
        public List<KeyframedJoint> KeyFramedJoints;
        public PhysicMaterial       RagdollMaterial;
        public GameObject           Reference;
    }
}
