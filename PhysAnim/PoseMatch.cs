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
        [Range(0, 1)]
        public float KeyframeStiffness;
        public StateRagdoll rag_state;

        private readonly Dictionary<Transform, KeyValuePair<ConfigurableJoint, MotorizedJoint>> _jointref_dict = new();
        private readonly Dictionary<ConfigurableJoint, Quaternion> _startRotations = new();
        private GameObject _ragdoll;

        private void ConvertToConfigurableJoint(CharacterJoint j)
        {
            ConfigurableJoint new_j = PhysAnimUtilities.RecursiveFindChild(_ragdoll.transform, j.name).gameObject.AddComponent<ConfigurableJoint>();
            PhysAnimUtilities.ConvertCharJointToConfigurableJoint(j, new_j);
            Destroy(j);
        }

        public GameObject GetReference()
        {
            return _ragdoll;
        }

        public Vector3 GetMeanDelta()
        {
            Vector3 delta = new(0, 0, 0);

            profile.KeyFramedJoints.ForEach(
                j =>
                {
                    Transform refBone = PhysAnimUtilities.RecursiveFindChild(_ragdoll.transform, j.Limb.name);
                    Transform physBone = _jointref_dict[refBone].Key.transform;
                    delta += refBone.position - physBone.position;
                });

            return delta / profile.KeyFramedJoints.Count;
        }

        public Vector3 GetDelta(Transform bone)
        {

            if (profile.KeyFramedJoints.Find(b => b.Limb == bone).Limb != null) 
                throw new System.Exception("The object you're trying to get the Delta from is not a PhysAnimBone");
            Transform refBone = PhysAnimUtilities.RecursiveFindChild(_ragdoll.transform, bone.name);
            Transform physBone = _jointref_dict[refBone].Key.transform;

            return refBone.position - physBone.position;
        }

        private void FullRefMatch()
        {
            foreach (KeyValuePair<Transform, KeyValuePair<ConfigurableJoint, MotorizedJoint>> entry in _jointref_dict)
            {
                Rigidbody j = entry.Value.Key.transform.GetComponent<Rigidbody>();
                j.isKinematic = false;

                Transform r = entry.Key;
                j.freezeRotation = true;
                j.velocity = (r.position - j.position) * 50;
                j.MoveRotation(r.rotation);
            }
        }

        private void PartialRefMatch()
        {
            MotorMatch();
            profile.KeyFramedJoints.ForEach(
                j =>
                {
                    
                    if (j.Limb != null && _jointref_dict.ContainsKey(j.Limb))
                    {
                        Transform refBone = j.Limb;
                        Rigidbody rb = _jointref_dict[refBone].Key.transform.GetComponent<Rigidbody>();
                        Debug.DrawRay(refBone.position, refBone.up * 0.2f, Color.red);
                        rb.isKinematic = false;
                        rb.freezeRotation = true;
                        Vector3 desiredVelocity = (refBone.position - rb.position) / Time.deltaTime;
                        rb.velocity = rb.velocity * (1.0f - j.Stiffness) + desiredVelocity * j.Stiffness;
                        rb.MoveRotation(refBone.rotation);
                    }
                });
        }

        private void MotorMatch()
        {
            foreach (KeyValuePair<Transform, KeyValuePair<ConfigurableJoint, MotorizedJoint>> entry in _jointref_dict)
            {
                ConfigurableJoint j = entry.Value.Key;
                Transform t = entry.Key;
                Rigidbody rb = j.transform.GetComponent<Rigidbody>();

                rb.isKinematic = false;
                rb.freezeRotation = false;
                j.targetRotation = PhysAnimUtilities.SetTargetRotation(j, t.localRotation, _startRotations[j]);
                j.slerpDrive = PhysAnimUtilities.ModifyJointDrive(
                    MotorStrength * profile.MotorJoints.Find(x => x.Joint == entry.Value.Value.Joint).Strength,
                    MotorDamping * profile.MotorJoints.Find(x => x.Joint == entry.Value.Value.Joint).Strength);//entry.Value.Value.Power);
            }
        }

        private void InitRef()
        {
            Animator anim;

            if (profile.Reference == null) throw new ArgumentException("No reference provided");
            _ragdoll = Instantiate(profile.Reference, profile.Reference.transform.position, profile.Reference.transform.rotation);
            _ragdoll.transform.SetParent(profile.Reference.transform.parent);
            _ragdoll.transform.position = profile.Reference.transform.position;
            _ragdoll.name = profile.Reference.name + "_ragdoll";
            if (profile.Reference.TryGetComponent<Animator>(out anim))
                anim.enabled = true;
            if (_ragdoll.TryGetComponent<Animator>(out anim))
                anim.enabled = true;

            Transform[] ref_transforms = profile.Reference.GetComponentsInChildren<Transform>();
            foreach (Transform c in ref_transforms)
            {
                if (c.gameObject.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer smr))
                    smr.enabled = false;
                if (c.gameObject.TryGetComponent<Collider>(out Collider coll))
                    coll.enabled = false;
                if (c.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
                    rb.isKinematic = true;
            }
        }

        private void InitJoints()
        {
            profile.MotorJoints.ForEach(
                j =>
                {
                    if (j.Joint != null)
                    {
                        Transform ragdoll_clonedjoint = PhysAnimUtilities.RecursiveFindChild(_ragdoll.transform, j.Joint.transform.name);
                        ConvertToConfigurableJoint(ragdoll_clonedjoint.GetComponent<CharacterJoint>());
                        ragdoll_clonedjoint.GetComponent<Collider>().material = profile.RagdollMaterial;
                        try
                        {
                            _jointref_dict.Add(j.Joint.transform,
                                new KeyValuePair<ConfigurableJoint, MotorizedJoint>(
                                    ragdoll_clonedjoint.GetComponent<ConfigurableJoint>(), j));
                        }
                        catch
                        {
                            Debug.LogError("No configurable joint found in clone.");
                        }
                        _startRotations.Add(ragdoll_clonedjoint.GetComponent<ConfigurableJoint>(), ragdoll_clonedjoint.localRotation);
                    }
                    else
                        Debug.LogWarning("Joint wasn't provided for an element of the list of MotorizedJoint");
                });
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
            _startRotations.Clear();
            _jointref_dict.Clear();
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
