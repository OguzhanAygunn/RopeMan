#if UNITY_EDITOR

using UnityEditor;
using FIMSpace.FEditor;

#endif

using FIMSpace.FGenerating;
using UnityEngine;
using UnityEngine.Events;
using static FIMSpace.FProceduralAnimation.RAF_ReposeOnFall;
using System;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_FallGetUpAnimate : RagdollAnimatorFeatureBase
    {
        protected FUniversalVariable springPowerOnFallV;
        protected FUniversalVariable durationV;
        protected FUniversalVariable fallClipNameV;
        protected FUniversalVariable fallTransitionV;
        protected FUniversalVariable fallStateLayerV;
        protected FUniversalVariable onFallEventV;
        protected FUniversalVariable getUpFacedownClipNameV;
        protected FUniversalVariable getUpFromBackClipNameV;
        protected FUniversalVariable getUpCrossfadeV;
        protected FUniversalVariable getUpAnimatorLayer;
        protected FUniversalVariable getUpEventV;
        protected FUniversalVariable ragdolledPropertyV;
        protected FUniversalVariable repositionBaseTransformV;
        protected FUniversalVariable findRigidbodyV;
        protected FUniversalVariable bodyVelocityV;
        protected FUniversalVariable supportGetupRestoreV;
        protected FUniversalVariable getupRestoreClipStateV;

        //FUniversalVariable ragdollStandupBlendDuration;
        protected FUniversalVariable modeV;

        protected int _ragProperty = -1;
        protected int _fallClipState = -1;
        protected int _getupFaceState = -1;
        protected int _getupBackState = -1;
        protected int _h_velocity = -1;
        protected int _restoreState = -1;

        /// <summary> USe if getup animation should start at different normalized time than zero </summary>
        [NonSerialized] public float ClipTimePlayOffset = 0f;

        public override bool OnInit()
        {
            springPowerOnFallV = InitializedWith.RequestVariable("Springs On Fall:", 250f);
            durationV = InitializedWith.RequestVariable("Change Duration:", 0.15f);
            fallClipNameV = InitializedWith.RequestVariable("Fall Animation:", "Animator State Name");
            fallTransitionV = InitializedWith.RequestVariable("Fall Crossfade Duration:", 0.2f);
            fallStateLayerV = InitializedWith.RequestVariable("Layer:", 0);
            onFallEventV = InitializedWith.RequestVariable("Use On Fall Event:", false);
            getUpFacedownClipNameV = InitializedWith.RequestVariable("Get Up Face Down:", "Get Up Face Down");
            getUpFromBackClipNameV = InitializedWith.RequestVariable("Get Up From Back:", "Get Up From Back");
            getUpCrossfadeV = InitializedWith.RequestVariable("Get Up Crossfade:", 0f);
            getUpAnimatorLayer = InitializedWith.RequestVariable("GetUpLayer", 0);
            getUpEventV = InitializedWith.RequestVariable("Use Get Up Event:", false);
            ragdolledPropertyV = InitializedWith.RequestVariable("Set Bool Property On Fall:", "Ragdolled");

            supportGetupRestoreV = InitializedWith.RequestVariable("Support Standing Restore:", false);
            getupRestoreClipStateV = InitializedWith.RequestVariable("On Restore Animation:", "");

            repositionBaseTransformV = InitializedWith.RequestVariable("Reposition Base Transform:", true);
            findRigidbodyV = InitializedWith.RequestVariable("Find Character Rigidbody:", false);
            bodyVelocityV = InitializedWith.RequestVariable("Body Velocity Property:", "");
            modeV = InitializedWith.RequestVariable("Reposition Mode:", 0);

            ParentRagdollHandler.AddToOnFallModeSwitchActions(OnFallStateChange);
            ParentRagdollHandler.AddToLateUpdateLoop(UpdateFeature);

            RefreshHashes();

            return base.OnInit();
        }

        public override void OnDestroyFeature()
        {
            ParentRagdollHandler.AddToOnFallModeSwitchActions(OnFallStateChange);
            ParentRagdollHandler.RemoveFromLateUpdateLoop(UpdateFeature);
        }

        private void OnFallStateChange()
        {
            if (InitializedWith.Enabled == false) return;

            if (ParentRagdollHandler.AnimatingMode == RagdollHandler.EAnimatingMode.Standing)
            {
                ApplyOnGetUpSwitches();
                if (getUpEventV.GetBool()) Helper.customEventsList[1].Invoke(); // On Get Up - Like Mover Enable
            }
            else if (ParentRagdollHandler.IsFallingOrSleep)
            {
                ApplyOnFallSwitches();
                if (onFallEventV.GetBool()) Helper.customEventsList[0].Invoke(); // On Get Up - Like Mover Disable
            }
        }

        public void RefreshHashes()
        {
            CalculateHash(fallClipNameV, ref _fallClipState);
            CalculateHash(ragdolledPropertyV, ref _ragProperty);
            CalculateHash(getUpFacedownClipNameV, ref _getupFaceState);
            CalculateHash(getUpFromBackClipNameV, ref _getupBackState);
            CalculateHash(bodyVelocityV, ref _h_velocity);
            CalculateHash(getupRestoreClipStateV, ref _restoreState);
        }

        private void CalculateHash(FUniversalVariable variable, ref int hash)
        {
            if (string.IsNullOrWhiteSpace(variable.GetString())) hash = -1;
            else hash = Animator.StringToHash(variable.GetString());
        }

        /// <summary> Provide for custom get up point </summary>
        public RaycastHit groundHit = new RaycastHit();

        protected Rigidbody characterRigidbody = null;
        protected ERagdollGetUpType getupType = ERagdollGetUpType.None;

        public void ApplyOnFallSwitches()
        {
            var handler = ParentRagdollHandler;

            getupType = ERagdollGetUpType.None;
            groundHit = new RaycastHit();

            PlayOnFallAnimation(handler);

            // Rest of the calculations in Update loop
        }

        public void ApplyOnGetUpSwitches()
        {
            var handler = ParentRagdollHandler;
            var baseT = handler.GetBaseTransform();

            if (repositionBaseTransformV.GetBool())
            {
                #region Rigidbody Controller Search for Reposition

                if (findRigidbodyV.GetBool() && characterRigidbody == null)
                {
                    characterRigidbody = baseT.GetComponent<Rigidbody>();
                    if (characterRigidbody == null) characterRigidbody = baseT.GetComponentInParent<Rigidbody>();
                    if (characterRigidbody == null) characterRigidbody = baseT.GetComponentInChildren<Rigidbody>();
                }

                #endregion Rigidbody Controller Search for Reposition


                bool restored = false;
                if (supportGetupRestoreV.GetBool())
                {
                    if (handler.GetUpCall_StandingRestore)
                    {
                        EBaseTransformRepose reposeMode = (EBaseTransformRepose)modeV.GetInt();

                        if (groundHit.transform == null)
                        {
                            groundHit = new RaycastHit();
                            Vector3 pos = RAF_ReposeOnFall.GetReposePosition(handler, EBaseTransformRepose.AnchorToFootPosition);
                            groundHit.point = pos;
                        }

                        baseT.position = groundHit.point;

                        var anchor = handler.GetAnchorBoneController;
                        baseT.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(anchor.PhysicalDummyBone.rotation * anchor.LocalForward, Vector3.up), Vector3.up);

                        if (characterRigidbody)
                        {
                            characterRigidbody.position = baseT.position;
                            characterRigidbody.rotation = baseT.rotation;
                        }

                        restored = true;
                    }
                }

                if (!restored)
                {
                    EBaseTransformRepose reposeMode = (EBaseTransformRepose)modeV.GetInt();

                    if (groundHit.transform == null)
                    {
                        groundHit = new RaycastHit();
                        Vector3 pos = RAF_ReposeOnFall.GetReposePosition(handler, reposeMode);
                        groundHit.point = pos;
                    }

                    baseT.position = groundHit.point;
                    baseT.rotation = handler.User_GetMappedRotationHipsToLegsMiddle();

                    if (characterRigidbody)
                    {
                        characterRigidbody.position = baseT.position;
                        characterRigidbody.rotation = baseT.rotation;
                    }
                }
            }

            if (handler.Mecanim)
            {
                if (_ragProperty != -1) handler.Mecanim.SetBool(_ragProperty, false);
                PlayGetUpAnimation(handler);
            }

            //handler.User_TransitionToStandingMode( ragdollStandupBlendDuration.GetFloat(), ragdollStandupBlendDuration.GetFloat() * 0.7f, getUpCrossfadeV.GetFloat() > 0f ? 0.1f : 0f, 0f );
        }

        protected void CallGetUpAnimation(RagdollHandler handler, int getupHash)
        {
            if (getUpCrossfadeV.GetFloat() <= 0f)
                handler.Mecanim.CrossFadeInFixedTime(getupHash, 0f, getUpAnimatorLayer.GetInt(), ClipTimePlayOffset); // Beware, it's crossfading standing animation into lying animation
            else
                handler.Mecanim.CrossFadeInFixedTime(getupHash, getUpCrossfadeV.GetFloat(), getUpAnimatorLayer.GetInt(), ClipTimePlayOffset); // Beware, it's crossfading standing animation into lying animation
        }

        protected virtual void PlayGetUpAnimation(RagdollHandler handler)
        {
            if (supportGetupRestoreV.GetBool())
            {
                // Restore to standing state from ragdolled but standing character
                if (handler.GetUpCall_StandingRestore)
                {
                    if (_restoreState != -1) CallGetUpAnimation(handler, _restoreState);
                    return;
                }
            }

            if (_getupBackState != -1 || _getupFaceState != -1)
            {
                if (getupType == ERagdollGetUpType.None) getupType = handler.User_CanGetUpByRotation(false);

                int getupHash;

                if (_getupBackState == -1 && _getupFaceState != -1) getupHash = _getupFaceState;
                else if (_getupBackState != -1 && _getupFaceState == -1) getupHash = _getupBackState;
                else
                {
                    if (getupType == ERagdollGetUpType.FromFacedown) getupHash = _getupFaceState;
                    else getupHash = _getupBackState;
                }

                if (getupHash != -1)
                {
                    CallGetUpAnimation(handler, getupHash);
                }
            }
        }

        protected virtual void PlayOnFallAnimation(RagdollHandler handler)
        {
            if (handler.Mecanim)
            {
                if (_ragProperty != -1) handler.Mecanim.SetBool(_ragProperty, true);

                if (_fallClipState != -1)
                {
                    if (fallTransitionV.GetFloat() <= 0f)
                        handler.Mecanim.Play(_fallClipState, fallStateLayerV.GetInt()); // Beware, it's crossfading standing animation into lying animation
                    else
                        handler.Mecanim.CrossFadeInFixedTime(_fallClipState, fallTransitionV.GetFloat(), fallStateLayerV.GetInt(), ClipTimePlayOffset); // Beware, it's crossfading standing animation into lying animation
                }
            }
        }

        public void UpdateFeature()
        {
            if (InitializedWith.Enabled == false) return;

            var handler = ParentRagdollHandler;

            float? preValue = handler.OverrideSpringsValueOnFall;

            if (handler.AnimatingMode == RagdollHandler.EAnimatingMode.Standing)
            {
                if (handler.OverrideSpringsValueOnFall != null)
                {
                    SmoothChangeSpringsValueOnFall(handler.SpringsValue, durationV.GetFloat());
                    if (handler.OverrideSpringsValueOnFall == handler.SpringsValue) handler.OverrideSpringsValueOnFall = null;
                }
            }
            else if (handler.IsFallingOrSleep) // Falling mode
            {
                if (springPowerOnFallV.GetFloat() > 0f)
                {
                    if (handler.OverrideSpringsValueOnFall == null) handler.OverrideSpringsValueOnFall = handler.GetCurrentMainSpringsValue;
                    SmoothChangeSpringsValueOnFall(springPowerOnFallV.GetFloat(), durationV.GetFloat());
                }
            }

            if (preValue != handler.OverrideSpringsValueOnFall)
            {
                handler.User_UpdateJointsPlayParameters(false);
            }

            if (_h_velocity != -1 && handler.Mecanim)
            {
                float magn = ParentRagdollHandler.User_GetChainBonesVelocity(ERagdollChainType.Core).magnitude;
                float newVal = ParentRagdollHandler.Mecanim.GetFloat(_h_velocity);
                newVal = Mathf.SmoothDamp(newVal, magn, ref _sdVelo, 0.125f, 10000f, ParentRagdollHandler.Delta);
                ParentRagdollHandler.Mecanim.SetFloat(_h_velocity, newVal);
            }
        }

        private float _sd = 0f;
        private float _sdVelo = 0f;

        private void SmoothChangeSpringsValueOnFall(float to, float duration)
        {
            var handler = ParentRagdollHandler;

            handler.OverrideSpringsValueOnFall = Mathf.SmoothDamp(handler.OverrideSpringsValueOnFall.Value,
                to, ref _sd, duration, 10000000f, handler.Delta);

            if (Mathf.Abs(handler.OverrideSpringsValueOnFall.Value - to) < 0.1f) handler.OverrideSpringsValueOnFall = to;
        }

        #region Prepare Events

        private bool RefreshHelperEvents(RagdollAnimatorFeatureHelper helper)
        {
            bool changed = false;
            if (helper.customEventsList == null)
            {
                helper.customEventsList = new System.Collections.Generic.List<UnityEvent>();
                changed = true;
            }

            while (helper.customEventsList.Count < 2) { helper.customEventsList.Add(new UnityEvent()); changed = true; }

            return changed;
        }

        #endregion Prepare Events

#if UNITY_EDITOR

        public override string Editor_FeatureDescription => "Providing helper operations for basic character Get Up mechanics, so you don't need to write as much custom code for it, just set Ragdoll Animating Mode as Falling / Standing.";

        public override void Editor_InspectorGUI(SerializedProperty handlerProp, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper)
        {
            EditorGUI.BeginChangeCheck();

            #region Define Unity Events

            int featureIdx = -1;
            for (int i = 0; i < ragdollHandler.ExtraFeatures.Count; i++) if (ragdollHandler.ExtraFeatures[i] == helper) { featureIdx = i; break; }

            if (featureIdx == -1)
            {
                EditorGUILayout.HelpBox("Something went wrong with identifying feature in ragdoll handler", UnityEditor.MessageType.None);
                return;
            }

            if (RefreshHelperEvents(helper))
            {
                EditorUtility.SetDirty(handlerProp.serializedObject.targetObject);
                handlerProp.serializedObject.ApplyModifiedProperties();
                handlerProp.serializedObject.Update();
            }

            var sp = handlerProp.FindPropertyRelative("ExtraFeatures").GetArrayElementAtIndex(featureIdx);
            sp = sp.FindPropertyRelative("customEventsList");

            #endregion Define Unity Events

            if (ragdollHandler.Mecanim == null)
            {
                EditorGUILayout.HelpBox("Not found Mecanim Animator assigned to the Ragdoll Animator. Assign it to display animator related parameters.\nTriggered when AnimatingMode is changed from Fall to Standing.", UnityEditor.MessageType.None);
            }

            RAF_AutoGetUp getUpH = ragdollHandler.GetExtraFeature<RAF_AutoGetUp>();
            if (getUpH == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Consider using 'Auto Get Up' ragdoll feature in combination with this feature.", MessageType.Info);
                EditorGUILayout.EndHorizontal();
            }

            getUpAnimatorLayer = helper.RequestVariable("GetUpLayer", 0);

            #region Fall Properties

            GUILayout.Space(4);
            EditorGUILayout.LabelField("On Fall Properties:", EditorStyles.boldLabel);
            GUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 120;

            var springPowerOnFallV = helper.RequestVariable("Springs On Fall:", 250f);
            float spring = springPowerOnFallV.GetFloat();
            spring = EditorGUILayout.FloatField(new GUIContent("  Springs On Fall:", EditorGUIUtility.IconContent("SpringJoint Icon").image), spring);
            if (spring < 0f) spring = 0f;
            springPowerOnFallV.SetValue(spring);

            GUILayout.Space(8);

            if (springPowerOnFallV.GetFloat() > 0f)
            {
                EditorGUIUtility.labelWidth = 59;
                var durationV = helper.RequestVariable("Change Duration:", 0.6f);
                durationV._GUI_DisplayNameReplace = "Duration:";
                durationV.AssignTooltip("Springs Value on fall mode transition duration");
                durationV.SetMinMaxSlider(0.0f, 2f);
                durationV.Editor_DisplayVariableGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("Not using", MessageType.None);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;

            GUILayout.Space(4);

            if (ragdollHandler.Mecanim != null)
            {
                EditorGUI_DrawFallClipField(helper, ragdollHandler, handlerProp);

                if (EditorGUI_CanDrawFallClipDetails(helper))
                {
                    EditorGUILayout.BeginHorizontal();

                    var fallTransitionV = helper.RequestVariable("Fall Crossfade Duration:", 0.2f);
                    var fallStateLayerV = helper.RequestVariable("Layer:", 0);

                    EditorGUIUtility.labelWidth = 68;
                    float crs = fallTransitionV.GetFloat();
                    crs = EditorGUILayout.FloatField(new GUIContent("Crossfade:", "Animation State Crossfade Duration"), crs);
                    if (crs < 0f) crs = 0f; fallTransitionV.SetValue(crs);
                    EditorGUIUtility.labelWidth = 42;
                    GUILayout.Space(8);
                    fallStateLayerV.AssignTooltip("Set Layer to play animation state on");
                    fallStateLayerV.Editor_DisplayVariableGUI();

                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(6);
            }

            EditorGUIUtility.labelWidth = 0;

            #endregion Fall Properties

            #region Use On Fall Event

            var onFallEventV = helper.RequestVariable("Use On Fall Event:", false);
            bool useFallEvent = onFallEventV.GetBool();
            useFallEvent = EditorGUILayout.Toggle(new GUIContent("  Use On Fall Event:", EditorGUIUtility.IconContent("EventSystem Icon").image), useFallEvent);
            onFallEventV.SetValue(useFallEvent);

            if (onFallEventV.GetBool())
            {
                EditorGUILayout.PropertyField(sp.GetArrayElementAtIndex(0));
            }

            #endregion Use On Fall Event

            FGUI_Inspector.DrawUILineCommon(12);

            #region Get Up Properties

            EditorGUILayout.LabelField("On Get Up Properties:", EditorStyles.boldLabel);
            GUILayout.Space(2);

            var getUpCrossfadeV = helper.RequestVariable("Get Up Crossfade:", 0f);

            EditorGUI_DrawGetUpFaceDownClipField(helper, ragdollHandler, handlerProp);

            EditorGUI_DrawGetUpFromBackClipField(helper, ragdollHandler, handlerProp);

            if (EditorGUI_CanDrawGetupClipDetails(helper))
            {
                EditorGUILayout.BeginHorizontal();

                float getUpCrossfade = getUpCrossfadeV.GetFloat();
                EditorGUIUtility.labelWidth = 68;
                getUpCrossfade = EditorGUILayout.FloatField(new GUIContent("Crossfade:", "Get Up Animation State Crossfade duration in seconds"), getUpCrossfade);
                getUpCrossfade = Mathf.Clamp01(getUpCrossfade);
                getUpCrossfadeV.SetValue(getUpCrossfade);

                GUILayout.Space(8);

                int getUpLayer = getUpAnimatorLayer.GetInt();
                EditorGUIUtility.labelWidth = 86;
                getUpLayer = EditorGUILayout.IntField(new GUIContent("Get Up Layer:", "Get up Animation States animator layer to play states on"), getUpLayer);
                if (getUpLayer < 0) getUpLayer = 0;
                getUpAnimatorLayer.SetValue(getUpLayer);

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(4);
            EditorGUIUtility.labelWidth = 190;

            var repositionBaseTransformV = helper.RequestVariable("Reposition Base Transform:", true);
            repositionBaseTransformV.Icon = EditorGUIUtility.IconContent("Transform Icon").image;
            repositionBaseTransformV.AssignTooltip("Changing position of Ragdoll Animator's Base Transform to match get up position");
            repositionBaseTransformV.Editor_DisplayVariableGUI();

            if (repositionBaseTransformV.GetBool())
            {
                GUILayout.Space(2);

                var modeV = helper.RequestVariable("Reposition Mode:", 1);
                EBaseTransformRepose reposeM = (EBaseTransformRepose)modeV.GetInt();
                EditorGUI.BeginChangeCheck();
                EditorGUIUtility.labelWidth = 114;
                reposeM = (EBaseTransformRepose)EditorGUILayout.EnumPopup(new GUIContent("Reposition Mode:", "Choose option which fits to your Get Up animation origin. Some get up animations can start at hips center, some are starting at feet position."), reposeM);
                modeV.SetValue((int)reposeM);
                if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(handlerProp.serializedObject.targetObject);

                GUILayout.Space(2);

                EditorGUIUtility.labelWidth = 160;
                var findRigidbodyV = helper.RequestVariable("Find Character Rigidbody:", false);
                findRigidbodyV.AssignTooltip("Finding and repositioning base transform character movement rigidbody if found");
                findRigidbodyV.Editor_DisplayVariableGUI();
            }

            EditorGUIUtility.labelWidth = 0;

            var supportGetupRestoreV = helper.RequestVariable("Support Standing Restore:", false);
            supportGetupRestoreV.AssignTooltip("Use if you detect character standing on both legs during falling-ragdolled state. (check Auto Get Up Extra Feature)");
            supportGetupRestoreV.Editor_DisplayVariableGUI();

            if (supportGetupRestoreV.GetBool())
            {
                EditorGUI.indentLevel++;

                EditorGUI_DrawRestoreBallanceClipField(helper, ragdollHandler, handlerProp);

                EditorGUI.indentLevel--;
            }

            #endregion Get Up Properties

            #region Use Get Up Event

            GUILayout.Space(8);

            var getUpEventV = helper.RequestVariable("Use Get Up Event:", false);
            bool useGetUpEvent = getUpEventV.GetBool();
            useGetUpEvent = EditorGUILayout.Toggle(new GUIContent("  Use Get Up Event:", EditorGUIUtility.IconContent("EventSystem Icon").image), useGetUpEvent);
            getUpEventV.SetValue(useGetUpEvent);

            if (getUpEventV.GetBool())
            {
                EditorGUILayout.PropertyField(sp.GetArrayElementAtIndex(1));
            }

            #endregion Use Get Up Event

            FGUI_Inspector.DrawUILineCommon(12);

            EditorGUILayout.LabelField("Other Properties:", EditorStyles.boldLabel);
            GUILayout.Space(2);

            if (ragdollHandler.Mecanim)
            {
                // Animator Property Set

                var propertyV = helper.RequestVariable("Set Bool Property On Fall:", "Ragdolled");
                propertyV.AssignTooltip("Changing bool Animator property on fall -> setting it true. When standing up, changing it to false.");
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 152;
                propertyV.Editor_DisplayVariableGUI();

                if (string.IsNullOrWhiteSpace(propertyV.GetString())) EditorGUILayout.HelpBox("Changing bool Animator property", MessageType.None);
                EditorGUILayout.EndHorizontal();

                var bodyVelocityV = helper.RequestVariable("Body Velocity Property:", "");
                bodyVelocityV.AssignTooltip("Assigning ragdoll animator's body velocity value, to animator float property");
                bodyVelocityV.Editor_DisplayVariableGUI();
            }

            EditorGUIUtility.labelWidth = 0;

            #region End GUI

            GUILayout.Space(4);

            if (EditorGUI.EndChangeCheck())
            {
                if (handlerProp != null && handlerProp.serializedObject != null && handlerProp.serializedObject.targetObject != null) EditorUtility.SetDirty(handlerProp.serializedObject.targetObject);

                if (ragdollHandler.WasInitialized)
                {
                    RefreshHashes();
                }
            }

            #endregion End GUI
        }

        public static void ClipStateSelector(RagdollHandler handler, FUniversalVariable variable, int layer, SerializedProperty toDirty)
        {
            if (layer < 0) return;

            if (handler.Mecanim && handler.Mecanim.runtimeAnimatorController)
            {
                var animInternal = handler.Mecanim.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;

                if (animInternal != null && layer < animInternal.layers.Length)
                {
                    if (GUILayout.Button(new GUIContent(" >", EditorGUIUtility.IconContent("AnimatorController Icon").image), EditorStyles.label, GUILayout.Height(18), GUILayout.Width(30)))
                    {
                        GenericMenu menu = new GenericMenu();
                        var layerStates = animInternal.layers[layer];

                        foreach (var item in layerStates.stateMachine.states)
                        {
                            string stateName = item.state.name;
                            menu.AddItem(new GUIContent(stateName), false, () => { variable.SetValue(stateName); EditorUtility.SetDirty(toDirty.serializedObject.targetObject); });
                        }

                        menu.ShowAsContext();
                    }
                }
            }
        }

        public static void AnimatorPropertySelector(RagdollHandler handler, FUniversalVariable variable, SerializedProperty toDirty)
        {
            if (handler.Mecanim && handler.Mecanim.runtimeAnimatorController)
            {
                if (GUILayout.Button(new GUIContent(" >", EditorGUIUtility.IconContent("AnimatorController Icon").image), EditorStyles.label, GUILayout.Height(18), GUILayout.Width(30)))
                {
                    GenericMenu menu = new GenericMenu();

                    for (int p = 0; p < handler.Mecanim.parameterCount; p++)
                    {
                        string propName = handler.Mecanim.GetParameter(p).name;
                        menu.AddItem(new GUIContent(propName), false, () => { variable.SetValue(propName); EditorUtility.SetDirty(toDirty.serializedObject.targetObject); });
                    }

                    menu.ShowAsContext();
                }
            }
        }

        public static void AnimatorLayerSelector(RagdollHandler handler, FUniversalVariable variable, SerializedProperty toDirty)
        {
            if (handler.Mecanim && handler.Mecanim.runtimeAnimatorController)
            {
                if (GUILayout.Button(new GUIContent(" >", EditorGUIUtility.IconContent("AnimatorController Icon").image), EditorStyles.label, GUILayout.Height(18), GUILayout.Width(30)))
                {
                    GenericMenu menu = new GenericMenu();

                    for (int p = 0; p < handler.Mecanim.layerCount; p++)
                    {
                        string layerName = handler.Mecanim.GetLayerName(p);
                        menu.AddItem(new GUIContent(layerName), false, () => { variable.SetValue(layerName); EditorUtility.SetDirty(toDirty.serializedObject.targetObject); });
                    }

                    menu.ShowAsContext();
                }
            }
        }

        protected virtual bool EditorGUI_CanDrawGetupClipDetails(RagdollAnimatorFeatureHelper helper)
        {
            return ((string.IsNullOrWhiteSpace(getUpFacedownClipNameV.GetString()) == false) || string.IsNullOrWhiteSpace(getUpFromBackClipNameV.GetString()) == false);
        }

        protected virtual bool EditorGUI_CanDrawFallClipDetails(RagdollAnimatorFeatureHelper helper)
        {
            return (string.IsNullOrWhiteSpace(fallClipNameV.GetString()) == false);
        }

        protected virtual void EditorGUI_DrawFallClipField(RagdollAnimatorFeatureHelper helper, RagdollHandler ragdollHandler, SerializedProperty handlerProp)
        {
            fallClipNameV = helper.RequestVariable("Fall Animation:", "Animator State Name");
            fallClipNameV.AssignTooltip("Left empty to not use this part of the extra feature");

            EditorGUIUtility.labelWidth = 102;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Animator Icon").image), EditorStyles.label, GUILayout.Height(18), GUILayout.Width(20))) { }

            fallClipNameV.Editor_DisplayVariableGUI();
            ClipStateSelector(ragdollHandler, fallClipNameV, getUpAnimatorLayer.GetInt(), handlerProp);
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void EditorGUI_DrawGetUpFaceDownClipField(RagdollAnimatorFeatureHelper helper, RagdollHandler ragdollHandler, SerializedProperty handlerProp)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Animator Icon").image), EditorStyles.label, GUILayout.Height(18), GUILayout.Width(20))) { }

            getUpFacedownClipNameV = helper.RequestVariable("Get Up Face Down:", "Get Up Face Down");
            EditorGUIUtility.labelWidth = 126;
            getUpFacedownClipNameV.AssignTooltip("Get up face down animator state name, to play on character transition to standing mode");

            getUpFacedownClipNameV.Editor_DisplayVariableGUI();
            ClipStateSelector(ragdollHandler, getUpFacedownClipNameV, getUpAnimatorLayer.GetInt(), handlerProp);

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void EditorGUI_DrawGetUpFromBackClipField(RagdollAnimatorFeatureHelper helper, RagdollHandler ragdollHandler, SerializedProperty handlerProp)
        {
            getUpFromBackClipNameV = helper.RequestVariable("Get Up From Back:", "Get Up From Back");
            getUpFromBackClipNameV.AssignTooltip("Get up from character's back animator state name, to play on character transition to standing mode");

            EditorGUILayout.BeginHorizontal();

            getUpFromBackClipNameV.Editor_DisplayVariableGUI();
            ClipStateSelector(ragdollHandler, getUpFromBackClipNameV, getUpAnimatorLayer.GetInt(), handlerProp);

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void EditorGUI_DrawRestoreBallanceClipField(RagdollAnimatorFeatureHelper helper, RagdollHandler ragdollHandler, SerializedProperty handlerProp)
        {
            getupRestoreClipStateV = helper.RequestVariable("On Restore Animation:", "");
            getupRestoreClipStateV.AssignTooltip("Animation state to be played (on the same layer and crossfade time as get up animations)");

            EditorGUILayout.BeginHorizontal();

            getupRestoreClipStateV.Editor_DisplayVariableGUI();
            ClipStateSelector(ragdollHandler, getupRestoreClipStateV, getUpAnimatorLayer.GetInt(), handlerProp);

            EditorGUILayout.EndHorizontal();
        }

#endif

    }
}