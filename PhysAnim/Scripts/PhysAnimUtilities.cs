using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PhysAnim
{
    public static class PhysAnimUtilities
    {
        public static void ConvertCharJointToConfigurableJoint(CharacterJoint char_joint, ref ConfigurableJoint config_joint)
        {
            config_joint.anchor = char_joint.anchor;
            config_joint.connectedAnchor = char_joint.connectedAnchor;
            config_joint.connectedBody = char_joint.connectedBody;
            config_joint.autoConfigureConnectedAnchor = char_joint.autoConfigureConnectedAnchor;
            config_joint.axis = char_joint.axis;
            config_joint.secondaryAxis = char_joint.swingAxis;
            config_joint.lowAngularXLimit = char_joint.lowTwistLimit;
            config_joint.highAngularXLimit = char_joint.highTwistLimit;
            config_joint.angularYLimit = char_joint.swing1Limit;
            config_joint.angularZLimit = char_joint.swing2Limit;
            config_joint.xMotion = ConfigurableJointMotion.Locked;
            config_joint.yMotion = ConfigurableJointMotion.Locked;
            config_joint.zMotion = ConfigurableJointMotion.Locked;
            config_joint.angularXMotion = ConfigurableJointMotion.Limited;
            config_joint.angularYMotion = ConfigurableJointMotion.Limited;
            config_joint.angularZMotion = ConfigurableJointMotion.Limited;
            JointDrive jointDrive = new();
            jointDrive.positionSpring = 0f;
            jointDrive.maximumForce = float.MaxValue;
            config_joint.slerpDrive = jointDrive;
            config_joint.rotationDriveMode = RotationDriveMode.Slerp;
        }

        public static JointDrive ModifyJointDrive(float pos_spring, float pos_damp)
        {
            JointDrive res = new();
            res.positionSpring = pos_spring;
            res.maximumForce = float.MaxValue;
            res.positionDamper = pos_damp;
            return res;
        }

        public static JointDrive ModifyJointDrive(JointDrive joint, float pos_spring)
        {
            joint.positionSpring = pos_spring;
            joint.maximumForce = float.MaxValue;
            return joint;
        }

        public static Transform RecursiveFindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }
                else
                {
                    Transform found = RecursiveFindChild(child, childName);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }

        public static Quaternion SetTargetRotation(ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
        {
            Vector3 right = joint.axis;
            Vector3 forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;
            Quaternion worldToJointSpace = (forward != Vector3.zero) ? Quaternion.LookRotation(forward, up) : Quaternion.identity;
            Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);
            resultRotation *= Quaternion.Inverse(targetLocalRotation) * startLocalRotation;
            resultRotation *= worldToJointSpace;
            return resultRotation;
        }

    }
}
