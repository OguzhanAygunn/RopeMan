using UnityEngine;

namespace FIMSpace.Basics
{
    [DefaultExecutionOrder(10000)]
    public class Fimp_JoyCamera : MonoBehaviour
    {
        public Transform FollowObject;
        public float HeightOffset = 2f;
        public float DistanceOffset = 7f;
        public float SideOffset = 0f;

        [Space(5)]
        public Fimp_JoystickInput joystickInput;
        public Vector2 VerticalClamp = new Vector2(-40, 40);

        [Space(5)]
        [Range(0f, 1f)] public float FollowSpeed = 0.9f;
        [Range(0f, 1f)] public float RotationSpeed = 0.9f;

        [Space(5)]
        public EControl MouseControl = EControl.None;
        public float MouseControlSensitivity = 1f;

        public enum EControl
        {
            None,
            LockCursor,
            OnRMBHold
        }

        Vector3 _sd_camPos = Vector3.zero;

        Vector2 sphericalRotation = Vector2.zero;

        Vector2 targetSphericalRot = Vector2.zero;

        public Vector2 SetTargetSphericalRot
        {
            get { return targetSphericalRot; }
            set { targetSphericalRot = value; }
        }

        Vector2 _sd_sphRot = Vector2.zero;

        Vector3 followPosition = Vector3.zero;


        private void Start()
        {
            if (FollowObject == null) return;

            sphericalRotation = transform.eulerAngles;
            targetSphericalRot = sphericalRotation;
            followPosition = FollowObject.position;
        }

        private void LateUpdate()
        {
            if (FollowObject == null) return;

            float screenDelta = (( Screen.width + Screen.height ) / 2f) * 0.001f;
            screenDelta = ( MouseControlSensitivity / screenDelta ) * 0.25f;

            if (MouseControl == EControl.LockCursor)
            {
                if (Cursor.visible || Cursor.lockState == CursorLockMode.None) SwitchLockCursor(false);
                if (Input.GetMouseButtonDown(1)) SwitchLockCursor(true);
                if (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Tab)) SwitchLockCursor(false);

                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    float sze = ((Screen.width + Screen.height) / 2f) * 0.02f * screenDelta;

                    float powx = 1f;
                    float powy = 1f;
                    if( joystickInput )
                    {
                        powx = joystickInput.ValuePower * joystickInput.ScaleOutput.x;
                        powy = joystickInput.ValuePower * joystickInput.ScaleOutput.y;
                    }

                    targetSphericalRot.x -= Input.GetAxis( "Mouse Y" ) * sze * powx;
                    targetSphericalRot.y += Input.GetAxis( "Mouse X" ) * sze * powy;
                }
            }
            else if (MouseControl == EControl.OnRMBHold)
            {
                if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
                {
                    float sze = ((Screen.width + Screen.height) / 2f) * 0.02f * screenDelta;
                    targetSphericalRot.x -= Input.GetAxis("Mouse Y") * sze;
                    targetSphericalRot.y += Input.GetAxis("Mouse X") * sze;
                }
            }

            if( joystickInput )
            {
                targetSphericalRot.x -= joystickInput.OutputValue.y;
                targetSphericalRot.y += joystickInput.OutputValue.x;
            }

            targetSphericalRot.x = Mathf.Clamp(targetSphericalRot.x, VerticalClamp.x, VerticalClamp.y);

            if (RotationSpeed > 0.999f)
            {
                sphericalRotation = targetSphericalRot;
            }
            else
            {
                float dur = Mathf.Lerp(0.2f, 0.005f, RotationSpeed);
                sphericalRotation.x = Mathf.SmoothDampAngle(sphericalRotation.x, targetSphericalRot.x, ref _sd_sphRot.x, dur, 1000f, Time.unscaledDeltaTime);
                sphericalRotation.y = Mathf.SmoothDampAngle(sphericalRotation.y, targetSphericalRot.y, ref _sd_sphRot.y, dur, 1000f, Time.unscaledDeltaTime);
            }

            transform.rotation = Quaternion.Euler(sphericalRotation.x, sphericalRotation.y, 0f);

            if (FollowSpeed > 0.999f) followPosition = FollowObject.position;
            else
                followPosition = Vector3.SmoothDamp(followPosition, FollowObject.position, ref _sd_camPos, Mathf.Lerp(0.5f, 0.02f, FollowSpeed), 1000f, Time.unscaledDeltaTime);


            Vector3 targetPosition = followPosition;

            targetPosition += Vector3.up * HeightOffset;
            targetPosition += transform.right * SideOffset;
            targetPosition -= transform.forward * DistanceOffset;


            transform.position = targetPosition;
        }


        bool lockCursor = false;
        void SwitchLockCursor(bool lck)
        {
            if (lck == lockCursor) return;

            lockCursor = lck;

            if (lck)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }


    }
}