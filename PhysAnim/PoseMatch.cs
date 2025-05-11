using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysAnim
{
    public enum StateRagdoll
    {
        Ragdolling,
        FullyKeyFramed,
        PartiallyKeyFramed,
    }

    public class PoseMatch : MonoBehaviour
    {
        public RagdollProfile profile;
        public float MotorDamping;
        public float MotorStrength;
        public StateRagdoll rag_state;

        private GameObject _ragdoll;

        public GameObject GetReference()
        {
            return _ragdoll;
        }

        private void FullRefMatch()
        {
            foreach (KeyframedJoint j in  profile.KeyFramedJoints)
            {
                if (j.Limb != null)
                {
                    Rigidbody rb = j.RagdollLimb.GetComponent<Rigidbody>();
                    Transform refBone = j.Limb;

                    rb.isKinematic = false;
                    rb.freezeRotation = true;
                    rb.velocity = (refBone.position - rb.position) * 50;
                    rb.MoveRotation(refBone.rotation);
                }
            }
            foreach (MotorizedJoint j in profile.MotorJoints)
            {
                    Rigidbody rb = j.RagdollJoint.GetComponent<Rigidbody>();
                    Transform refBone = j.Joint.transform;

                    rb.isKinematic = false;
                    rb.freezeRotation = true;
                    rb.velocity = (refBone.position - rb.position) * 50;
                    rb.MoveRotation(refBone.rotation);
            }
        }

        private void PartialRefMatch()
        {
            MotorMatch();
            foreach (KeyframedJoint j in  profile.KeyFramedJoints)
            {
                if (j.Limb != null && j.Stiffness != 0.0f)
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
                anim.enabled = true;
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
                    ragdoll_joint.GetComponent<Collider>().material = profile.RagdollMaterial;
                    j.RagdollJoint = ragdoll_joint.GetComponent<ConfigurableJoint>();
                    j.StartRotation = ragdoll_joint.localRotation;
                    profile.MotorJoints[i] = j;
                }
            }
            for (int i = 0; i < profile.KeyFramedJoints.Count; i++)
            {
                KeyframedJoint j = profile.KeyFramedJoints[i];

                if (j.Limb != null)
                {
                    Transform ragdoll_limb = PhysAnimUtilities.RecursiveFindChild(_ragdoll.transform, j.Limb.transform.name);
                    ragdoll_limb.GetComponent<Collider>().material = profile.RagdollMaterial;
                    j.RagdollLimb = ragdoll_limb.transform;
                    profile.KeyFramedJoints[i] = j;
                }
            }
        }

        private void OnEnable()
        {
            if (Physics.sleepThreshold != 0)
                Debug.Log("The physics simulation sleep threshold is not zero. You may experience glitches.");
            InitRef();
            InitJoints();
        }

        private void OnDisable()
        {
            Collider[] ref_colliders = profile.Reference.GetComponentsInChildren<Collider>();
            SkinnedMeshRenderer[] ref_renderers = profile.Reference.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (Collider c in ref_colliders)
                c.enabled = true;
            foreach (SkinnedMeshRenderer r in ref_renderers)
                r.enabled = true;
            Destroy(_ragdoll);
        }

        private void StateHandler()
        {
            switch (rag_state)
            {
                case StateRagdoll.Ragdolling:
                    MotorMatch();
                    break;
                case StateRagdoll.FullyKeyFramed:
                    FullRefMatch();
                    break;
                case StateRagdoll.PartiallyKeyFramed:
                    PartialRefMatch();
                    break;
            }
        }

        private void FixedUpdate()
        {
            StateHandler();
        }
    }
}
