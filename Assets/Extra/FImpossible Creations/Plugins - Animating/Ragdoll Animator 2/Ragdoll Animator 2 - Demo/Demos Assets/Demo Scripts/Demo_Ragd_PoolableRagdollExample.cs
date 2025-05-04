using FIMSpace.FProceduralAnimation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_PoolableRagdollExample : MonoBehaviour, IRagdollAnimator2Receiver
    {
        public Animator Mecanim;
        public RagdollAnimator2 Ragdoll;
        public List<AnimationClip> PlayRandom = new List<AnimationClip>();

        #region Playable animation clip play stuff

        // Playable - variables for random animation clip play
        PlayableGraph graph;
        AnimationPlayableOutput baseOutput;
        AnimationLayerMixerPlayable mixer;
        AnimationClipPlayable clipPlayable;

        public void ResetAnimationOnStart()
        {
            // Prepare playable graph for a basic clip play
            graph = PlayableGraph.Create(name);
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            baseOutput = AnimationPlayableOutput.Create(graph, "Test", Mecanim);
            mixer = AnimationLayerMixerPlayable.Create(graph, 1);
            baseOutput.SetSourcePlayable(mixer);

            CreatePlayableClip(PlayRandom[Random.Range(0, PlayRandom.Count)]);
            graph.Play();
        }

        void CreatePlayableClip(AnimationClip clip)
        {
            if (clipPlayable.IsValid()) clipPlayable.Destroy();

            mixer.DisconnectInput(0);
            clipPlayable = AnimationClipPlayable.Create(graph, clip);
            clipPlayable.SetApplyFootIK(true);

            mixer.ConnectInput(0, clipPlayable, 0);
            mixer.SetInputWeight(0, 1f);
        }

        #endregion

        public Vector3 startPosition { get; private set; }

        [Space(4)]
        public Vector3 moveRange = new Vector3(1f, 0f, 1f);
        public float TrigoSpeed = 2f;

        float trigoTime1;
        float trigoTime2;

        void Start()
        {
            ResetOnStart();
        }

        public void ResetOnStart()
        {
            isDead = false;

            startPosition = transform.position; // Set origin position to be shifted using sin and cos functions

            trigoTime1 = Random.Range(-10000f, 100000f);
            trigoTime2 = Random.Range(-10000f, 100000f);

            SetPosition();
            ResetAnimationOnStart();

            Ragdoll.Handler.Initialize(Ragdoll, Ragdoll.gameObject); // In case if it wasn't initialized yet

            // Object pool respawning operations
            Ragdoll.Handler.ApplyTPoseOnModel(); // Refresh full body pose
            Ragdoll.User_WarpRefresh(); // Refresh physics after instant teleport

            // Restore stand mode
            Ragdoll.RA2Event_SwitchToStand();
        }

        void LateUpdate()
        {
            trigoTime1 += Time.deltaTime * TrigoSpeed;
            trigoTime2 += Time.deltaTime * TrigoSpeed;
            SetPosition();
        }

        void SetPosition()
        {
            Vector3 tPos = new Vector3(0f, 0f, 0f);

            tPos.x = Mathf.Sin(trigoTime1) * moveRange.x;
            tPos.z = Mathf.Cos(trigoTime2) * moveRange.z;

            Vector3 newPos = startPosition + tPos;

            Vector3 dir = newPos - transform.position;
            if (dir != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 4f);

            transform.position = newPos;
        }

        [Space(4)]
        public string TagToGetHitOn = "Finish";
        public float StartFallOnHitPower = 4f;
        public float HitImpulse = 8f;
        public void RagdollAnimator2_OnCollisionEnterEvent(RA2BoneCollisionHandler hitted, Collision mainCollision)
        {
            if (mainCollision.gameObject.CompareTag(TagToGetHitOn) == false) return;

            if (mainCollision.impulse.magnitude > StartFallOnHitPower)
            {
                hitted.ParentHandler.User_SwitchFallState();
                hitted.ParentHandler.User_AddBoneImpact(hitted.BoneSettings, mainCollision.relativeVelocity.normalized * HitImpulse, 0.04f);
                if (!isDead) StartCoroutine(IEBackToPool());
                isDead = true;
            }
        }

        bool isDead = false;
        IEnumerator IEBackToPool()
        {
            yield return new WaitForSeconds(2f);
            Demo_Ragd_ObjectPoolingManager.Get.GiveBackObject(gameObject);
            yield break;
        }

    }
}