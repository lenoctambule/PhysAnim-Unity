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

    [Serializable]
    public class RagdollProfile
    {
        public List<MotorizedJoint> MotorJoints;
        public List<KeyframedJoint> KeyFramedJoints;
        public PhysicMaterial       RagdollMaterial;
        public GameObject           Reference;

        public bool AddMotor(MotorizedJoint mj) {
            foreach (MotorizedJoint j in MotorJoints)
            {
                if (j.Joint.transform == mj.Joint.transform)
                    return false;
            }
            MotorJoints.Add(mj);
            return true;
        }

        public bool AddKeyframedJoint(KeyframedJoint kj) {
            foreach (KeyframedJoint j in KeyFramedJoints)
            {
                if (j.Limb == kj.Limb)
                    return false;
            }
            KeyFramedJoints.Add(kj);
            return true;
        }
    }
}
