using UnityEngine;
using PhysAnim;

namespace PhysAnim
{
    public class BipedBalanceCalculator
    {
        public float CalculateTotalMass(RagdollProfile profile)
        {
            float totalMass = 0f;
            profile.MotorJoints.ForEach(
                   j => { totalMass += j.Joint.GetComponent<Rigidbody>().mass; }
                );
            return totalMass;
        }

        public Vector3 CalculateCenterOfMass(RagdollProfile profile)
        {
            Vector3 massCenter = Vector3.zero;
            float totalMass = CalculateTotalMass(profile);
            profile.MotorJoints.ForEach(
                   j => { massCenter += j.Joint.transform.position * j.Joint.GetComponent<Rigidbody>().mass; }
                );
            massCenter /= totalMass;

            return massCenter;
        }

    }
}
