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

        public MotorizedJoint(float strength, ConfigurableJoint joint)
        {
            this.Strength = strength;
            this.Joint = joint;
        }
    }

    [Serializable]
    public struct KeyframedJoint
    {
        [Range(0, 1)]
        public float        Stiffness;
        public Transform    Limb;

        public KeyframedJoint(float Stiffness, Transform Limb)
        {
            this.Stiffness = Stiffness;
            this.Limb = Limb;
        }
    }

    [Serializable]
    public struct RagdollProfile
    {
        public List<MotorizedJoint> MotorJoints;
        public List<KeyframedJoint> KeyFramedJoints;
        public PhysicMaterial       RagdollMaterial;
        public GameObject           Reference;
    }
}
