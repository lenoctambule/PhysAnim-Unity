using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PhysAnim
{
    public static class PhysAnimUtilities
    {
        public static ConfigurableJoint ConvertCharJointToConfigurableJoint(CharacterJoint j, ConfigurableJoint new_j)
        {
            new_j.anchor = j.anchor;
            new_j.connectedAnchor = j.connectedAnchor;
            new_j.connectedBody = j.connectedBody;
            new_j.autoConfigureConnectedAnchor = j.autoConfigureConnectedAnchor;
            new_j.axis = j.axis;
            new_j.secondaryAxis = j.swingAxis;
            new_j.lowAngularXLimit = j.lowTwistLimit;
            new_j.highAngularXLimit = j.highTwistLimit;
            new_j.angularYLimit = j.swing1Limit;
            new_j.angularZLimit = j.swing2Limit;
            new_j.xMotion = ConfigurableJointMotion.Locked;
            new_j.yMotion = ConfigurableJointMotion.Locked;
            new_j.zMotion = ConfigurableJointMotion.Locked;
            new_j.angularXMotion = ConfigurableJointMotion.Limited;
            new_j.angularYMotion = ConfigurableJointMotion.Limited;
            new_j.angularZMotion = ConfigurableJointMotion.Limited;
            JointDrive jointDrive = new();
            jointDrive.positionSpring = 0f;
            jointDrive.maximumForce = float.MaxValue;
            new_j.slerpDrive = jointDrive;
            new_j.rotationDriveMode = RotationDriveMode.Slerp;

            return new_j;
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
