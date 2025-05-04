using FIMSpace.Basics;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [DefaultExecutionOrder(9999)]
    public class DEMO_FCameraAutoDirect : MonoBehaviour
    {
        public Fimp_JoyCamera CameraScript;
        public Fimp_JoystickInput OptionalMovementJoy;

        public float StartRotateAtVelocity = 0.1f;
        [Range(0f, 1f)] public float AdjustementSpeed = 0.5f;

        Rigidbody rig;
        Vector3 velocityOfTarget = Vector3.zero;
        Vector3 sd_velocity = Vector3.zero;
        Vector3 prePos = Vector3.zero;
        Vector2 sd_angleSmooth = Vector2.zero;

        float rotateBlend = 0f;
        float sd_rotateBlend = 0f;

        void Start()
        {
            if (CameraScript == null) return;
            if (CameraScript.FollowObject == null) return;

            prePos = CameraScript.FollowObject.position;
            rig = CameraScript.FollowObject.GetComponentInChildren<Rigidbody>();
        }

        void LateUpdate()
        {
            if (CameraScript == null) return;
            if (CameraScript.FollowObject == null) return;


            Vector3 currentVelocity;

            if (rig) currentVelocity = rig.velocity;
            else
                currentVelocity = CameraScript.FollowObject.position - prePos;


            prePos = CameraScript.FollowObject.position;

            velocityOfTarget = Vector3.SmoothDamp(velocityOfTarget, currentVelocity, ref sd_velocity, 1f, 10f, Time.unscaledDeltaTime);

            float tgt = 1f;
            if (OptionalMovementJoy != null) if (OptionalMovementJoy.OutputValue.sqrMagnitude < 0.1f) tgt = 0f;

            if (currentVelocity.magnitude > StartRotateAtVelocity)
                rotateBlend = Mathf.SmoothDamp(rotateBlend, tgt, ref sd_rotateBlend, 0.2f, 100f, Time.unscaledDeltaTime);
            else
                rotateBlend = Mathf.SmoothDamp(rotateBlend, 0f, ref sd_rotateBlend, 0.2f, 100f, Time.unscaledDeltaTime);


            if (rotateBlend > 0.001f)
            {
                Vector3 lookVelo = velocityOfTarget;
                lookVelo.y = 0f;

                if (lookVelo != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookVelo);
                    Vector3 angle = targetRotation.eulerAngles;
                    Vector2 targetAngles = new Vector2(angle.x, angle.y);

                    float dur = Mathf.Lerp(2f, 0.001f, AdjustementSpeed);

                    Vector2 setAngles = CameraScript.SetTargetSphericalRot;

                    //setAngles.x = Mathf.SmoothDampAngle(setAngles.x, targetAngles.x, ref sd_angleSmooth.x, dur, 10f, Time.unscaledDeltaTime);
                    setAngles.y = Mathf.SmoothDampAngle(setAngles.y, targetAngles.y, ref sd_angleSmooth.y, dur, 1000f, Time.unscaledDeltaTime);

                    setAngles.y = Mathf.Lerp(CameraScript.SetTargetSphericalRot.y, setAngles.y, rotateBlend);

                    CameraScript.SetTargetSphericalRot = setAngles;
                }
            }


        }

    }
}