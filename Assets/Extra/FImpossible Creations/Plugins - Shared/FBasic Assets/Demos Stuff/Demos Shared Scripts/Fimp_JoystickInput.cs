using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FIMSpace.Basics
{
    public class Fimp_JoystickInput : MonoBehaviour
    {
        public Image JoystickButton;
        public Image OptionalJoyBackStick;

        [Space(5)]
        public float DragDistanceLimit = 75f;

        [Space(5)]
        public float ValuePower = 1f;
        [FPD_FixedCurveWindow]
        public AnimationCurve Sensitivity = AnimationCurve.Linear(0.1f, 0f, 0.9f, 1f);
        public Vector2 ScaleOutput = Vector2.one;

        public Vector2 OutputValue { get; private set; }

        Vector2 joyPos = Vector2.zero;
        Vector2 sd_joyPos = Vector2.zero;

        private void Start()
        {
            if (JoystickButton == null) return;
            joyHandler = JoystickButton.gameObject.AddComponent<JoyHandler>().Initialize(this);
        }

        void Update()
        {
            if (JoystickButton == null) return;

            Vector2 targetPosition = Vector2.zero;

            if (isDragging)
            {
                targetPosition = (Input.mousePosition - startDragMousePosition);
                targetPosition /=  JoystickButton.transform.lossyScale.x;

                if (targetPosition.magnitude > DragDistanceLimit) targetPosition = targetPosition.normalized * DragDistanceLimit;

                OutputValue = new Vector2(
                    Mathf.Clamp(targetPosition.x / DragDistanceLimit, -1f, 1f),
                    Mathf.Clamp(targetPosition.y / DragDistanceLimit, -1f, 1f));

                Vector2 outVal = OutputValue;

                outVal.x = Sensitivity.Evaluate(Mathf.Abs(outVal.x));
                if (OutputValue.x < 0f) outVal.x *= -1f;
                outVal.y = Sensitivity.Evaluate(Mathf.Abs(outVal.y));
                if (OutputValue.y < 0f) outVal.y *= -1f;

                outVal.x *= ScaleOutput.x;
                outVal.y *= ScaleOutput.y;

                OutputValue = outVal * ValuePower;
            }
            else
            {
                OutputValue = Vector2.zero;
            }

            // End Dragging
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
            {
                isDragging = false;
            }

            joyPos = Vector2.SmoothDamp(joyPos, targetPosition, ref sd_joyPos, isDragging ? 0.005f : 0.03f, float.MaxValue, Time.unscaledDeltaTime);
            JoystickButton.rectTransform.anchoredPosition = joyPos;

            if (OptionalJoyBackStick)
            {
                if (joyPos != Vector2.zero)
                {
                    Quaternion look = Quaternion.LookRotation(new Vector3(joyPos.x, 0, joyPos.y));
                    OptionalJoyBackStick.rectTransform.rotation = Quaternion.Euler(0, 0, -look.eulerAngles.y);
                }

                float dist = Vector2.Distance(JoystickButton.rectTransform.anchoredPosition, Vector3.zero);
                var size = OptionalJoyBackStick.rectTransform.sizeDelta;
                size.y = dist;
                OptionalJoyBackStick.rectTransform.sizeDelta = size;

                Vector3 jPos = JoystickButton.rectTransform.anchoredPosition.normalized * -14f;
                OptionalJoyBackStick.rectTransform.anchoredPosition = jPos;
            }

        }

        bool isDragging = false;
        Vector3 startDragMousePosition = Vector3.zero;
        private void OnClick()
        {
            if (isDragging) return;

            isDragging = true;
            startDragMousePosition = Input.mousePosition;
        }


        #region Click Handler Class

        JoyHandler joyHandler;
        class JoyHandler : MonoBehaviour, IPointerDownHandler
        {
            Fimp_JoystickInput Parent;
            public JoyHandler Initialize(Fimp_JoystickInput parent) { Parent = parent; return this; }

            public void OnPointerDown(PointerEventData eventData)
            {
                Parent.OnClick();
            }
        }

        #endregion


        #region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor(typeof(Fimp_JoystickInput))]
        public class DEMO_LegsAnim_JoystickInputEditor : UnityEditor.Editor
        {
            public Fimp_JoystickInput Get { get { if (_get == null) _get = (Fimp_JoystickInput)target; return _get; } }
            private Fimp_JoystickInput _get;

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                serializedObject.Update();

                GUILayout.Space(4f);
                GUI.enabled = false;
                EditorGUILayout.Vector2Field("Output Value:", Get.OutputValue);
                GUI.enabled = true;

                serializedObject.ApplyModifiedProperties();
            }
        }

#endif

        #endregion


    }
}