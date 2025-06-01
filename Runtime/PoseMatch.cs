using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PhysAnim
{
    public enum StateRagdoll
    {
        Local,
        LocalAndGlobal,
    }

    [AddComponentMenu("PhysAnim/Pose Match")]
    public class PoseMatch : MonoBehaviour
    {

        public StateRagdoll State;
        [SerializeField]
        public RagdollProfile Profile;
        public GameObject Reference;
        private GameObject _ragdoll;

        public GameObject GetRagdoll() {
            return _ragdoll;
        }

        private void GlobalMatch()
        {
            foreach (KeyframedJoint j in  Profile.KeyFramedJoints)
            {
                if (j.Limb && j.RagdollLimb && j.Stiffness != 0.0f )
                {
                    Transform refBone = j.Limb;
                    Rigidbody rb = j.RagdollLimb.GetComponent<Rigidbody>();
                    Vector3 desiredVelocity = (refBone.position - rb.position) / Time.deltaTime;
                    rb.isKinematic = false;
                    rb.freezeRotation = true;
                    rb.velocity = rb.velocity * (1.0f - j.Stiffness) + desiredVelocity * j.Stiffness;
                    rb.MoveRotation(refBone.rotation);
                }
            }
        }

        private void LocalMatch()
        {
            foreach (MotorizedJoint joint in Profile.MotorJoints)
            {
                ConfigurableJoint cj    = joint.RagdollJoint;
                Transform target        = joint.Joint.transform;
                Rigidbody rb            = cj.transform.GetComponent<Rigidbody>();

                rb.isKinematic = false;
                rb.freezeRotation = false;
                cj.targetRotation = PhysAnimUtilities.SetTargetRotation(cj, target.localRotation, joint.StartRotation);
                cj.slerpDrive = PhysAnimUtilities.ModifyJointDrive(joint.Strength,Profile.Damping);
            }
        }

        private void InitRef()
        {
            if (Reference == null)
                throw new ArgumentException("No reference provided");
            _ragdoll = Instantiate(Reference, Reference.transform.position, Reference.transform.rotation);
            _ragdoll.transform.SetParent(Reference.transform.parent);
            _ragdoll.transform.position = Reference.transform.position;
            _ragdoll.name = Reference.name + "_ragdoll";
            foreach (Transform c in Reference.GetComponentsInChildren<Transform>())
            {
                if (c.gameObject.TryGetComponent(out SkinnedMeshRenderer smr))
                    smr.enabled = false;
                if (c.gameObject.TryGetComponent(out Collider coll))
                    coll.enabled = false;
                if (c.gameObject.TryGetComponent(out Rigidbody rb))
                    rb.isKinematic = true;
            }
            foreach (Rigidbody rb in _ragdoll.GetComponentsInChildren<Rigidbody>())
                rb.isKinematic = false;
        }

        private void OnEnable()
        {
            if (Physics.sleepThreshold != 0)
                Debug.LogWarning("The physics simulation sleep threshold is not zero. You may experience glitches.");
            if (Reference)
            {
                InitRef();
                Profile.Enable();
            }
            else
                Debug.LogError(this + ": Reference is unassigned.");
        }

        private void OnDisable() {
            Transform[] transforms = Reference.GetComponentsInChildren<Transform>();

            if (!Reference)
                return;
            if (Reference.TryGetComponent(out Animator anim))
                anim.enabled = true;
            foreach (Transform c in transforms)
            {
                if (c.gameObject.TryGetComponent(out SkinnedMeshRenderer smr))
                    smr.enabled = true;
                if (c.gameObject.TryGetComponent(out Collider coll))
                    coll.enabled = true;
            }
            Destroy(_ragdoll);
        }

        private void OnValidate()
        {
            for (Transform p = this.transform; p && Reference; p = p.parent)
            {
                if (Reference.transform == p)
                {
                    Reference = null;
                    Debug.LogError("Reference can't be self or one of parents.");
                    break;
                }
            }
        }

        private void StateHandler()
        {
            switch (State)
            {
                case StateRagdoll.Local:
                    LocalMatch();
                    break;
                case StateRagdoll.LocalAndGlobal:
                    LocalMatch();
                    GlobalMatch();
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (Reference)
                StateHandler();
        }
    }
}
