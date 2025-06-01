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

    [DisallowMultipleComponent]
    [AddComponentMenu("PhysAnim/Pose Match")]
    public class PoseMatch : MonoBehaviour
    {
        public GameObject Reference;

        private GameObject _ragdoll;
        [SerializeField]
        private RagdollProfile _profile;
        private RagdollProfile _track_profile;

        public RagdollProfile Profile
        {
            get => _profile;
            set { (_profile = value).Enable(); }
        }

        public GameObject GetRagdoll() {
            return _ragdoll;
        }

        private void InitRef()
        {
            if (Reference == null)
                throw new ArgumentException("No reference provided");
            _ragdoll = Instantiate(Reference, Reference.transform.position, Reference.transform.rotation);
            _ragdoll.transform.SetParent(Reference.transform.parent);
            _ragdoll.transform.position = Reference.transform.position;
            _ragdoll.name = "[ragdoll]" + Reference.name;
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
                _profile.Enable();
            }
            else
                Debug.LogError(this + ": Reference is unassigned.");
        }

        private void OnDisable() {
            if (!Reference)
                return;
            Transform[] transforms = Reference.GetComponentsInChildren<Transform>();

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
            if (_track_profile != Profile)
            {
                _track_profile = Profile;
                if (_ragdoll)
                    Profile.Enable();
            }
        }

        private void GlobalMatch(Joint j)
        {
            if (j.Target && j.RagdollJoint && j.Stiffness != 0.0f )
            {
                Rigidbody rb = j.RagdollJoint.gameObject.GetComponent<Rigidbody>();
                Vector3 desiredVelocity = (j.Target.transform.position - rb.position) / Time.deltaTime;
                rb.freezeRotation = true;
                rb.velocity = rb.velocity * (1.0f - j.Stiffness) + desiredVelocity * j.Stiffness;
                rb.MoveRotation(j.Target.transform.rotation);
            }
        }

        private void LocalMatch(Joint j)
        {
            j.RagdollJoint.targetRotation = PhysAnimUtilities.SetTargetRotation(j.RagdollJoint,
                                                                                j.Target.transform.localRotation,
                                                                                j.StartRotation);
            j.RagdollJoint.slerpDrive = PhysAnimUtilities.ModifyJointDrive(j.Strength, _profile.Damping);
        }

        private void StateHandler()
        {
            foreach(Joint j in _profile.Joints)
            {
                switch (j.Mode)
                {
                    case Mode.Local:
                        LocalMatch(j);
                        break;
                    case Mode.Global:
                        GlobalMatch(j);
                        break;
                    case Mode.Global_and_Local:
                        // TODO: Mix of both local and global matching
                        GlobalMatch(j);
                        break;
                }
            }

        }

        private void FixedUpdate()
        {
            if (Reference)
                StateHandler();
        }
    }
}
