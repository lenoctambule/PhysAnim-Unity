using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PhysAnim
{
    public enum StateRagdoll
    {
        Ragdolling,
        PartiallyKeyFramed,
    }

    public class PoseMatch : MonoBehaviour
    {

        public float MotorDamping;
        public float MotorStrength;
        public StateRagdoll rag_state;
        [SerializeField]
        public RagdollProfile profile;

        private GameObject _ragdoll;
        private Dictionary<Transform, MotorizedJoint> _cached_mjoints = new();
        private Dictionary<Transform, KeyframedJoint> _cached_kjoints = new();

        public GameObject GetReference() {
            return _ragdoll;
        }

        public MotorizedJoint getMotorizedJoint(Transform obj) {
            return _cached_mjoints[obj];
        }

        public KeyframedJoint getKeyframeJoint(Transform obj) {
            return _cached_kjoints[obj];
        }

        private void PartialRefMatch()
        {
            foreach (KeyframedJoint j in  profile.KeyFramedJoints)
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

        private void MotorMatch()
        {
            foreach (MotorizedJoint joint in profile.MotorJoints)
            {
                ConfigurableJoint cj    = joint.RagdollJoint;
                Transform target        = joint.Joint.transform;
                Rigidbody rb            = cj.transform.GetComponent<Rigidbody>();

                rb.isKinematic = false;
                rb.freezeRotation = false;
                cj.targetRotation = PhysAnimUtilities.SetTargetRotation(cj, target.localRotation, joint.StartRotation);
                cj.slerpDrive = PhysAnimUtilities.ModifyJointDrive(
                    MotorStrength * joint.Strength,
                    MotorDamping * joint.Strength);
            }
        }

        private void InitRef()
        {
            Animator    anim;
            Transform[] ref_transforms = profile.Reference.GetComponentsInChildren<Transform>();

            if (profile.Reference == null)
                throw new ArgumentException("No reference provided");
            _ragdoll = Instantiate(profile.Reference, profile.Reference.transform.position, profile.Reference.transform.rotation);
            _ragdoll.transform.SetParent(profile.Reference.transform.parent);
            _ragdoll.transform.position = profile.Reference.transform.position;
            _ragdoll.name = profile.Reference.name + "_ragdoll";
            if (profile.Reference.TryGetComponent(out anim))
                anim.enabled = true;
            if (_ragdoll.TryGetComponent(out anim))
                anim.enabled = false;
            foreach (Transform c in ref_transforms)
            {
                if (c.gameObject.TryGetComponent(out SkinnedMeshRenderer smr))
                    smr.enabled = false;
                if (c.gameObject.TryGetComponent(out Collider coll))
                    coll.enabled = false;
                if (c.gameObject.TryGetComponent(out Rigidbody rb))
                    rb.isKinematic = true;
            }
        }

        private void InitJoints()
        {
            for (int i = 0; i < profile.MotorJoints.Count; i++)
            {
                MotorizedJoint j = profile.MotorJoints[i];
    
                if (j.Joint != null)
                {
                    Transform ragdoll_joint = PhysAnimUtilities.RecursiveFindChild(_ragdoll.transform, j.Joint.transform.name);
                    if (ragdoll_joint.TryGetComponent<Collider>(out var col))
                        col.material = profile.RagdollMaterial;
                    j.RagdollJoint = ragdoll_joint.GetComponent<ConfigurableJoint>();
                    j.StartRotation = ragdoll_joint.localRotation;
                    _cached_mjoints[j.Joint.transform] = j;
                }
            }
            for (int i = 0; i < profile.KeyFramedJoints.Count; i++)
            {
                KeyframedJoint j = profile.KeyFramedJoints[i];

                if (!j.Limb.TryGetComponent(out Rigidbody rb))
                    Debug.LogError("Keyframed Joint "+ j.Limb.name + " does not have a Rigid Body");
                else if (j.Limb != null)
                {
                    Transform ragdoll_limb = PhysAnimUtilities.RecursiveFindChild(_ragdoll.transform, j.Limb.transform.name);
                    if (ragdoll_limb.TryGetComponent<Collider>(out var col))
                        col.material = profile.RagdollMaterial;
                    j.RagdollLimb = ragdoll_limb.transform;
                    _cached_kjoints[j.Limb.transform] = j;
                }
            }
        }

        private void OnEnable()
        {
            if (Physics.sleepThreshold != 0)
                Debug.LogWarning("The physics simulation sleep threshold is not zero. You may experience glitches.");
            if (profile.Reference)
            {
                InitRef();
                InitJoints();
            }
            else
                Debug.LogError(this + ": Reference is unassigned.");
        }

        private void OnDisable()
        {
            if (!profile.Reference)
                return;
            Collider[] ref_colliders = profile.Reference.GetComponentsInChildren<Collider>();
            SkinnedMeshRenderer[] ref_renderers = profile.Reference.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (Collider c in ref_colliders)
                c.enabled = true;
            foreach (SkinnedMeshRenderer r in ref_renderers)
                r.enabled = true;
            Destroy(_ragdoll);
        }

        private void OnValidate()
        {
            for (Transform p = this.transform; p && profile.Reference; p = p.parent)
            {
                if (profile.Reference.transform == p)
                {
                    profile.Reference = null;
                    Debug.LogError("Reference can't be self or one of parents.");
                    break;
                }
            }
        }

        private void StateHandler()
        {
            switch (rag_state)
            {
                case StateRagdoll.Ragdolling:
                    MotorMatch();
                    break;
                case StateRagdoll.PartiallyKeyFramed:
                    MotorMatch();
                    PartialRefMatch();
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (profile.Reference)
                StateHandler();
        }
    }
}
