using UnityEditor;
using UnityEngine;
using RengeGames.HealthBars.Extensions;

using Random = UnityEngine.Random;

namespace RengeGames.HealthBars.Editors {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RadialSegmentedHealthBar))]
    public class RadialSegmentedHealthBarEditor : Editor {
        #region serializedproperties

        private SerializedProperty parentName;
        private SerializedProperty hbName;
        private SerializedProperty usingSpriteRenderer;
        //private SerializedProperty forceBuiltInShader;
        private SerializedProperty updatesPerSecond;

        private SerializedProperty overlayColor;
        private SerializedProperty innerColor;
        private SerializedProperty borderColor;
        private SerializedProperty emptyColor;
        private SerializedProperty spaceColor;
        private SerializedProperty segmentCount;
        private SerializedProperty removeSegments;
        private SerializedProperty segmentSpacing;
        private SerializedProperty arc;
        private SerializedProperty radius;
        private SerializedProperty lineWidth;
        private SerializedProperty rotation;
        private SerializedProperty offset;
        private SerializedProperty borderWidth;
        private SerializedProperty borderSpacing;
        private SerializedProperty removeBorder;
        private SerializedProperty overlayNoiseEnabled;
        private SerializedProperty overlayNoiseScale;
        private SerializedProperty overlayNoiseStrength;
        private SerializedProperty overlayNoiseOffset;
        private SerializedProperty emptyNoiseEnabled;
        private SerializedProperty emptyNoiseScale;
        private SerializedProperty emptyNoiseStrength;
        private SerializedProperty emptyNoiseOffset;
        private SerializedProperty contentNoiseEnabled;
        private SerializedProperty contentNoiseScale;
        private SerializedProperty contentNoiseStrength;
        private SerializedProperty contentNoiseOffset;
        private SerializedProperty pulsateWhenLow;
        private SerializedProperty pulseColor;
        private SerializedProperty pulseSpeed;
        private SerializedProperty pulseActivationThreshold;
        private SerializedProperty overlayTextureEnabled;
        private SerializedProperty overlayTexture;
        private SerializedProperty overlayTextureOpacity;
        private SerializedProperty overlayTextureTiling;
        private SerializedProperty overlayTextureOffset;
        private SerializedProperty innerTextureEnabled;
        private SerializedProperty innerTexture;
        private SerializedProperty alignInnerTexture;
        private SerializedProperty innerTextureScaleWithSegments;
        private SerializedProperty innerTextureOpacity;
        private SerializedProperty innerTextureTiling;
        private SerializedProperty innerTextureOffset;
        private SerializedProperty borderTextureEnabled;
        private SerializedProperty borderTexture;
        private SerializedProperty alignBorderTexture;
        private SerializedProperty borderTextureScaleWithSegments;
        private SerializedProperty borderTextureOpacity;
        private SerializedProperty borderTextureTiling;
        private SerializedProperty borderTextureOffset;
        private SerializedProperty emptyTextureEnabled;
        private SerializedProperty emptyTexture;
        private SerializedProperty alignEmptyTexture;
        private SerializedProperty emptyTextureScaleWithSegments;
        private SerializedProperty emptyTextureOpacity;
        private SerializedProperty emptyTextureTiling;
        private SerializedProperty emptyTextureOffset;
        private SerializedProperty spaceTextureEnabled;
        private SerializedProperty spaceTexture;
        private SerializedProperty alignSpaceTexture;
        private SerializedProperty spaceTextureOpacity;
        private SerializedProperty spaceTextureTiling;
        private SerializedProperty spaceTextureOffset;
        private SerializedProperty innerGradient;
        private SerializedProperty innerGradientEnabled;
        private SerializedProperty valueAsGradientTimeInner;
        private SerializedProperty emptyGradient;
        private SerializedProperty emptyGradientEnabled;
        private SerializedProperty valueAsGradientTimeEmpty;
        private SerializedProperty variableWidthCurve;
        private SerializedProperty fillClockwise;

        #endregion

        private GUIStyle headerStyle;
        private bool generalFoldout = true;
        private bool overlayFoldout = false;
        private bool hbFoldout = true;
        private bool borderFoldout = false;
        private bool depletedFoldout = false;
        private bool emptyFoldout = false;

        private void OnEnable() {
            parentName = serializedObject.FindProperty("parentName");
            hbName = serializedObject.FindProperty("hbName");
            usingSpriteRenderer = serializedObject.FindProperty("usingSpriteRenderer");
            //forceBuiltInShader = serializedObject.FindProperty("forceBuiltInShader");
            updatesPerSecond = serializedObject.FindProperty("updatesPerSecond");



            overlayColor = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.OverlayColor);
            innerColor = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.InnerColor);
            borderColor = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.BorderColor);
            emptyColor = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.EmptyColor);
            spaceColor = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.SpaceColor);
            segmentCount = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.SegmentCount);
            removeSegments = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.RemoveSegments);
            segmentSpacing = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.SegmentSpacing);
            arc = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.Arc);
            radius = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.Radius);
            lineWidth = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.LineWidth);
            rotation = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.Rotation);
            offset = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.Offset);
            borderWidth = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.BorderWidth);
            borderSpacing = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.BorderSpacing);
            removeBorder = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.RemoveBorder);
            overlayNoiseEnabled = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.OverlayNoiseEnabled);
            overlayNoiseScale = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.OverlayNoiseScale);
            overlayNoiseStrength = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.OverlayNoiseStrength);
            overlayNoiseOffset = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.OverlayNoiseOffset);
            emptyNoiseEnabled = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.EmptyNoiseEnabled);
            emptyNoiseScale = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.EmptyNoiseScale);
            emptyNoiseStrength = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.EmptyNoiseStrength);
            emptyNoiseOffset = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.EmptyNoiseOffset);
            contentNoiseEnabled = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.ContentNoiseEnabled);
            contentNoiseScale = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.ContentNoiseScale);
            contentNoiseStrength = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.ContentNoiseStrength);
            contentNoiseOffset = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.ContentNoiseOffset);
            pulsateWhenLow = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.PulsateWhenLow);
            pulseColor = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.PulseColor);
            pulseSpeed = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.PulseSpeed);
            pulseActivationThreshold = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.PulseActivationThreshold);
            overlayTextureEnabled = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarKeywords.OverlayTextureEnabled);
            overlayTexture = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.OverlayTexture);
            overlayTextureOpacity = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.OverlayTextureOpacity);
            overlayTextureTiling = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.OverlayTextureTiling);
            overlayTextureOffset = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.OverlayTextureOffset);
            innerTextureEnabled = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarKeywords.InnerTextureEnabled);
            innerTexture = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.InnerTexture);
            alignInnerTexture = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.AlignInnerTexture);
            innerTextureScaleWithSegments = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.InnerTextureScaleWithSegments);
            innerTextureOpacity = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.InnerTextureOpacity);
            innerTextureTiling = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.InnerTextureTiling);
            innerTextureOffset = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.InnerTextureOffset);
            borderTextureEnabled = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarKeywords.BorderTextureEnabled);
            borderTexture = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.BorderTexture);
            alignBorderTexture = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.AlignBorderTexture);
            borderTextureScaleWithSegments = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.BorderTextureScaleWithSegments);
            borderTextureOpacity = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.BorderTextureOpacity);
            borderTextureTiling = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.BorderTextureTiling);
            borderTextureOffset = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.BorderTextureOffset);
            emptyTextureEnabled = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarKeywords.EmptyTextureEnabled);
            emptyTexture = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.EmptyTexture);
            alignEmptyTexture = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.AlignEmptyTexture);
            emptyTextureScaleWithSegments = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.EmptyTextureScaleWithSegments);
            emptyTextureOpacity = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.EmptyTextureOpacity);
            emptyTextureTiling = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.EmptyTextureTiling);
            emptyTextureOffset = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.EmptyTextureOffset);
            spaceTextureEnabled = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarKeywords.SpaceTextureEnabled);
            spaceTexture = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.SpaceTexture);
            alignSpaceTexture = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.AlignSpaceTexture);
            spaceTextureOpacity = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.SpaceTextureOpacity);
            spaceTextureTiling = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.SpaceTextureTiling);
            spaceTextureOffset = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.SpaceTextureOffset);
            innerGradient = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.InnerGradient);
            innerGradientEnabled = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.InnerGradientEnabled);
            valueAsGradientTimeInner = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.ValueAsGradientTimeInner);
            emptyGradient = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.EmptyGradient);
            emptyGradientEnabled = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.EmptyGradientEnabled);
            valueAsGradientTimeEmpty = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.ValueAsGradientTimeEmpty);
            variableWidthCurve = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.VariableWidthCurve);
            fillClockwise = FindSerializedPropertyFromAutoProperty(serializedObject, RadialHealthBarProperties.FillClockwise);
        }

        private SerializedProperty FindSerializedPropertyFromAutoProperty(SerializedObject obj, string propertyName) => obj.FindProperty($"<{propertyName}>k__BackingField");
        private SerializedProperty FindValueProperty(SerializedProperty parent) => parent.FindPropertyRelative("_value");
        private SerializedProperty FindDirtyProperty(SerializedProperty parent) => parent.FindPropertyRelative("_isDirty");

        public override void OnInspectorGUI() {
            serializedObject.Update();

            headerStyle = GUI.skin.label;
            headerStyle.fontStyle = FontStyle.Bold;

            GUILayout.Label(new GUIContent() { text = "Naming and Access", tooltip = "This section is used by the Manager, you can ignore this if you plan on not using it." }, headerStyle);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                EditorGUILayout.PropertyField(parentName, new GUIContent() { text = "Parent Name", tooltip = "The name of the parent for manager access. This name is free for you to choose." });
                EditorGUILayout.PropertyField(hbName, new GUIContent() { text = "Health Bar Name", tooltip = "The progress bar name for manager access" });
            }

            GUILayout.Space(10);

            GUILayout.Label("Data", headerStyle);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                EditorGUILayout.PropertyField(segmentCount, new GUIContent() { text = "Segment Count", tooltip = "The number of segments of the progress bar" });
                EditorGUILayout.PropertyField(removeSegments, new GUIContent() { text = "Remove Segments", tooltip = "The amount of segments removed from the progress bar. This is essentially the 'value'. There is a SetPercent() method you can use as well."});
                EditorGUILayout.PropertyField(updatesPerSecond, new GUIContent() { text = "FPS", tooltip = "How many times the progress bar updates every second. Default is 30fps"});
            }

            GUILayout.Space(10);
            GUILayout.Label("Appearance", headerStyle);

            generalFoldout = EditorGUILayout.Foldout(generalFoldout, "General Appearance");
            if (generalFoldout) {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                    EditorGUILayout.PropertyField(fillClockwise, new GUIContent("Fill Clockwise", "Change the direction the progress bar is filled from"));
                    EditorGUILayout.PropertyField(segmentSpacing, new GUIContent() { text = "Segment Spacing" , tooltip = "The spacing distance between segments"});
                    SliderPropertyField("Arc", "The arcing of the progress bar. Tip: combine with 'Rotation', 'Radius' and 'Offset' to achieve a more linear progress bar", arc, 0, 1);
                    EditorGUILayout.PropertyField(radius, new GUIContent() { text = "Radius", tooltip = "The radius of the progress bar" });
                    SliderPropertyField("Line Width", "The thickness of the progress bar", lineWidth, 0, 1);
                    EditorGUILayout.PropertyField(variableWidthCurve, new GUIContent() { text = "Line Width Curve", tooltip = "Change the thickness of the progress bar at different locations with this curve" });
                    SliderPropertyField("Rotation", "The rotation of the progress bar", rotation, 0, 360);
                    EditorGUILayout.PropertyField(offset, new GUIContent() { text = "Offset", tooltip = "The x and y offset of the progress bar" });
                }
            }

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                hbFoldout = EditorGUILayout.Foldout(hbFoldout, "Health Portion");
                EditorGUILayout.PropertyField(innerColor, new GUIContent() { text = "Health Color", tooltip = "The color of the value portion of the progress bar" });
            }
            EditorGUILayout.EndHorizontal();
            if (hbFoldout) {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                    EditorGUILayout.PropertyField(pulsateWhenLow, new GUIContent() { text = "Pulsate When Low", tooltip = "Make the progress bar pulsate when below a certain threshold value" });
                    if (FindValueProperty(pulsateWhenLow).boolValue) {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                            EditorGUILayout.PropertyField(pulseColor, new GUIContent("Pulse Color", "The color the progress bar should pulsate with"));
                            SliderPropertyField("Percent Health Activation", "The percent value of the progress bar at which pulsating starts", pulseActivationThreshold, 0, 1);
                            EditorGUILayout.PropertyField(pulseSpeed, new GUIContent() { text = "Pulse Speed", tooltip = "How fast the progress bar should pulsate" });
                        }
                    }

                    EditorGUILayout.PropertyField(innerTextureEnabled, new GUIContent() { text = "Use Texture" });
                    if (!innerTextureEnabled.hasMultipleDifferentValues && FindValueProperty(innerTextureEnabled).boolValue) {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                            //Texture stuff
                            EditorGUILayout.PropertyField(innerTexture, new GUIContent() { text = "Texture", tooltip = "The texture to be used for the value portion of the progress bar" });
                            EditorGUILayout.PropertyField(innerTextureScaleWithSegments, new GUIContent() { text = "Scale Texture with Segments", tooltip = "Make the texture tile with the number of segments. Two Segments = Texture repeats twice. Tip: The value in 'Texture Tiling' is added on top of this value" });
                            EditorGUILayout.PropertyField(alignInnerTexture, new GUIContent() { text = "Align Texture", tooltip = "Have the texture travel along the progress bar" });
                            SliderPropertyField("Texture Opacity", "The opacity of the texture. 0 = only color, 1 = full texture visibilty", innerTextureOpacity, 0, 1);
                            EditorGUILayout.PropertyField(innerTextureTiling, new GUIContent() { text = "Texture Tiling",  tooltip = "The tiling of the texture" });
                            EditorGUILayout.PropertyField(innerTextureOffset, new GUIContent() { text = "Texture Offset", tooltip = "The offset of the texture" });

                            //Gradient stuff
                            EditorGUILayout.PropertyField(innerGradientEnabled, new GUIContent() { text = "Use Gradient", tooltip = "Display a gradient on the value portion of the progress bar" });
                            if (!innerGradientEnabled.hasMultipleDifferentValues && FindValueProperty(innerGradientEnabled).boolValue) {
                                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                                    EditorGUILayout.PropertyField(innerGradient, new GUIContent() { text = "Gradient" , tooltip = "Choose a gradient"});
                                    EditorGUILayout.PropertyField(valueAsGradientTimeInner, new GUIContent() { text = "Value As Gradient Time", tooltip = "Use the progress bar value to sample the gradient" });
                                }
                            }

                        }
                    }

                    EditorGUILayout.PropertyField(contentNoiseEnabled, new GUIContent() { text = "Use Noise", tooltip = "Layer noise over the value portion of the progress bar" });
                    if (!contentNoiseEnabled.hasMultipleDifferentValues && FindValueProperty(contentNoiseEnabled).boolValue) {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                            EditorGUILayout.PropertyField(contentNoiseStrength, new GUIContent() { text = "Noise Strength", tooltip = "The intensity of the noise"});
                            EditorGUILayout.PropertyField(contentNoiseScale, new GUIContent() { text = "Noise Scale", tooltip = "The scale of the noise" });
                            EditorGUILayout.PropertyField(contentNoiseOffset, new GUIContent() { text = "Noise Offset", tooltip = "The offset of the noise" });
                        }
                    }
                }
            }

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                overlayFoldout = EditorGUILayout.Foldout(overlayFoldout, "Overlay Portion");
                EditorGUILayout.PropertyField(overlayColor, new GUIContent() { text = "Overlay Color", tooltip = "The color affecting the entire progress bar" });
            }
            EditorGUILayout.EndHorizontal();
            if (overlayFoldout) {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                    EditorGUILayout.PropertyField(overlayTextureEnabled, new GUIContent() { text = "Use Texture", tooltip = "Use a texture to overlay over the progress bar" });
                    if (FindValueProperty(overlayTextureEnabled).boolValue) {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                            EditorGUILayout.PropertyField(overlayTexture, new GUIContent() { text = "Texture", tooltip = "Select a texture" });
                            SliderPropertyField("Texture Opacity", "The opacity of the texture. 0 = only color, 1 = full texture visibilty", overlayTextureOpacity, 0, 1);
                            EditorGUILayout.PropertyField(overlayTextureTiling, new GUIContent() { text = "Texture Tiling" });
                            EditorGUILayout.PropertyField(overlayTextureOffset, new GUIContent() { text = "Texture Offset" });
                        }
                    }

                    EditorGUILayout.PropertyField(overlayNoiseEnabled, new GUIContent() { text = "Use Noise", tooltip = "Layer noise over the progress bar" });
                    if (FindValueProperty(overlayNoiseEnabled).boolValue) {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                            EditorGUILayout.PropertyField(overlayNoiseStrength, new GUIContent() { text = "Noise Strength", tooltip = "The intensity of the noise" });
                            EditorGUILayout.PropertyField(overlayNoiseScale, new GUIContent() { text = "Noise Scale", tooltip = "The scale of the noise" });
                            EditorGUILayout.PropertyField(overlayNoiseOffset, new GUIContent() { text = "Noise Offset", tooltip = "The offset of the noise" });
                        }
                    }
                }
            }

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                borderFoldout = EditorGUILayout.Foldout(borderFoldout, "Border");
                EditorGUILayout.PropertyField(borderColor, new GUIContent() { text = "Border Color", tooltip = "The color used for the border"});
            }
            EditorGUILayout.EndHorizontal();
            if (borderFoldout) {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                    SliderPropertyField("Border Width", "The width of the border. This affects the outside and inside border, not the space between segments. Refer to 'Border Spacing' for that.", borderWidth, 0, 1);
                    EditorGUILayout.PropertyField(borderSpacing, new GUIContent() { text = "Border Spacing", tooltip = "The border size between segments" });

                    EditorGUILayout.PropertyField(borderTextureEnabled, new GUIContent() { text = "Use Texture", tooltip = "Use a texture for the border" });
                    if (FindValueProperty(borderTextureEnabled).boolValue) {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                            EditorGUILayout.PropertyField(borderTexture, new GUIContent() { text = "Texture" });
                            EditorGUILayout.PropertyField(borderTextureScaleWithSegments, new GUIContent() { text = "Scale Texture with Segments", tooltip = "Make the texture tile with the number of segments. Two Segments = Texture repeats twice. Tip: The value in 'Texture Tiling' is added on top of this value" });
                            EditorGUILayout.PropertyField(alignBorderTexture, new GUIContent() { text = "Align Texture", tooltip = "Have the texture travel along the progress bar" });
                            SliderPropertyField("Texture Opacity", "The opacity of the texture. 0 = only color, 1 = full texture visibilty", borderTextureOpacity, 0, 1);
                            EditorGUILayout.PropertyField(borderTextureTiling, new GUIContent() { text = "Texture Tiling" });
                            EditorGUILayout.PropertyField(borderTextureOffset, new GUIContent() { text = "Texture Offset" });
                        }
                    }
                }
            }

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                depletedFoldout = EditorGUILayout.Foldout(depletedFoldout, "Depleted Portion");
                EditorGUILayout.PropertyField(emptyColor, new GUIContent() { text = "Depleted Color", tooltip = "The background color"});
            }
            EditorGUILayout.EndHorizontal();
            if (depletedFoldout) {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                    SliderPropertyField("Depleted Transparency", "How visible should the portion of the progress bar be that has been depleted", removeBorder, 0, 1);
                    EditorGUILayout.PropertyField(emptyTextureEnabled, new GUIContent() { text = "Use Texture", tooltip = "Use a texture for the background" });
                    if (FindValueProperty(emptyTextureEnabled).boolValue) {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                            EditorGUILayout.PropertyField(emptyTexture, new GUIContent() { text = "Texture" });
                            EditorGUILayout.PropertyField(emptyTextureScaleWithSegments, new GUIContent() { text = "Scale Texture with Segments", tooltip = "Make the texture tile with the number of segments. Two Segments = Texture repeats twice. Tip: The value in 'Texture Tiling' is added on top of this value" });
                            EditorGUILayout.PropertyField(alignEmptyTexture, new GUIContent() { text = "Align Texture", tooltip = "Have the texture travel along the progress bar" });
                            SliderPropertyField("Texture Opacity", "The opacity of the texture. 0 = only color, 1 = full texture visibilty", emptyTextureOpacity, 0, 1);
                            EditorGUILayout.PropertyField(emptyTextureTiling, new GUIContent() { text = "Texture Tiling" });
                            EditorGUILayout.PropertyField(emptyTextureOffset, new GUIContent() { text = "Texture Offset" });


                            //Gradient stuff
                            EditorGUILayout.PropertyField(emptyGradientEnabled, new GUIContent() { text = "Use Gradient", tooltip = "Display a gradient on the value portion of the progress bar" });
                            if (!emptyGradientEnabled.hasMultipleDifferentValues && FindValueProperty(emptyGradientEnabled).boolValue) {
                                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                                    EditorGUILayout.PropertyField(emptyGradient, new GUIContent() { text = "Gradient", tooltip = "Choose a gradient" });
                                    EditorGUILayout.PropertyField(valueAsGradientTimeEmpty, new GUIContent() { text = "Value As Gradient Time", tooltip = "Use the progress bar value to sample the gradient" });
                                }
                            }
                        }
                    }

                    EditorGUILayout.PropertyField(emptyNoiseEnabled, new GUIContent() { text = "Use Noise", tooltip = "Layer noise over the background of the progress bar" });
                    if (FindValueProperty(emptyNoiseEnabled).boolValue) {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                            EditorGUILayout.PropertyField(emptyNoiseStrength, new GUIContent() { text = "Noise Strength" });
                            EditorGUILayout.PropertyField(emptyNoiseScale, new GUIContent() { text = "Noise Scale" });
                            EditorGUILayout.PropertyField(emptyNoiseOffset, new GUIContent() { text = "Noise Offset" });
                        }
                    }
                }
            }

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                emptyFoldout = EditorGUILayout.Foldout(emptyFoldout, "Empty Space");
                EditorGUILayout.PropertyField(spaceColor, new GUIContent() { text = "Empty Space Color", tooltip = "Use a color for the space between segments. Tip: set the alpha to 0 to disable this section" });
            }
            EditorGUILayout.EndHorizontal();
            if (emptyFoldout) {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                    //FindValueProperty(spaceTextureEnabled).boolValue = EditorGUILayout.Toggle("Use Texture", FindValueProperty(spaceTextureEnabled).boolValue);
                    EditorGUILayout.PropertyField(spaceTextureEnabled, new GUIContent() { text = "Use Texture" , tooltip = "Use a texture in the space between segments"});
                    if (FindValueProperty(spaceTextureEnabled).boolValue) {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                            EditorGUILayout.PropertyField(spaceTexture, new GUIContent() { text = "Texture" });
                            EditorGUILayout.PropertyField(alignSpaceTexture, new GUIContent() { text = "Align Texture", tooltip = "Have the texture travel along the progress bar" });
                            SliderPropertyField("Texture Opacity", "The opacity of the texture. 0 = only color, 1 = full texture visibilty", spaceTextureOpacity, 0, 1);
                            EditorGUILayout.PropertyField(spaceTextureTiling, new GUIContent() { text = "Texture Tiling" });
                            EditorGUILayout.PropertyField(spaceTextureOffset, new GUIContent() { text = "Texture Offset" });
                        }
                    }
                }
            }

            EditorGUILayout.Separator();
            GUILayout.Label("Other", headerStyle);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                GUILayout.BeginVertical();
                {
                    if (GUILayout.Button(new GUIContent("Reset to defaults", "Reset all values to their defaults. This can be undone"))) {
                        Undo.RecordObjects(serializedObject.targetObjects, "Reset RSHB");
                        foreach (var serializedObjectTargetObject in serializedObject.targetObjects) {
                            (serializedObjectTargetObject as RadialSegmentedHealthBar)?.ResetPropertiesToDefault();
                        }
                    }

                    if (GUILayout.Button(new GUIContent("Duplicate Progress Bar"))) {
                        foreach (var serializedObjectTargetObject in serializedObject.targetObjects) {
                            if (serializedObjectTargetObject != null) {
                                var obj = serializedObjectTargetObject as RadialSegmentedHealthBar;
                                var newObj = GameObject.Instantiate(obj, obj.transform.parent);
                                //newObj.Refresh(true);
                            }
                        }
                    }

                    if (GUILayout.Button(new GUIContent("Randomize All", "Randomize some properties of the progress bar. This can be undone"))) {
                        Color c = new Color();
                        Undo.RecordObjects(serializedObject.targetObjects, "Randomize RSHB");
                        foreach (var serializedObjectTargetObject in serializedObject.targetObjects) {
                            if (serializedObjectTargetObject != null) {
                                var obj = serializedObjectTargetObject as RadialSegmentedHealthBar;

                                obj.SegmentCount.Value = Mathf.Round(Random.value * 10 + 1);
                                obj.RemoveSegments.Value = Mathf.Floor(Random.value * (obj.SegmentCount.Value / 2));
                                obj.SegmentSpacing.Value = Random.value * 0.1f;
                                obj.Arc.Value = Mathf.Round(Random.value * 5) / 10;
                                obj.Radius.Value = Random.Range(0.2f, 0.4f);
                                obj.LineWidth.Value = Random.Range(0.02f, 0.1f);
                                obj.Rotation.Value = Random.value * 360;
                                obj.InnerColor.Value = c.RandomColor();
                                obj.BorderWidth.Value = Random.value * 0.05f;
                                obj.BorderSpacing.Value = Random.Range(0, obj.BorderWidth.Value + obj.Arc.Value * .1f);
                                obj.BorderColor.Value = c.RandomColor();
                                obj.RemoveBorder.Value = Random.value;
                                obj.EmptyColor.Value = c.RandomColor();
                                obj.SpaceColor.Value = c.RandomColor(true);

                                EditorApplication.QueuePlayerLoopUpdate();
                            }
                        }
                    }
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                {
                    if (GUILayout.Button(new GUIContent("Refresh", "Refresh the component if you notice desync between the inspector and the scene"))) {
                        foreach (var serializedObjectTargetObject in serializedObject.targetObjects) {
                            if (serializedObjectTargetObject != null) {
                                var obj = serializedObjectTargetObject as RadialSegmentedHealthBar;
                                obj.Refresh(true);
                            }
                        }
                    }

                    GUILayout.BeginHorizontal();
                    {

                        if (GUILayout.Button(new GUIContent("Sprite", "Use a sprite renderer to diplay the progress bar."))) {
                            foreach (var serializedObjectTargetObject in serializedObject.targetObjects) {
                                if (serializedObjectTargetObject != null) {
                                    var obj = serializedObjectTargetObject as RadialSegmentedHealthBar;
                                    obj.UsingSpriteRenderer = true;
                                }
                            }
                        }

                        if (GUILayout.Button(new GUIContent("Image", "Use an image component to display the progress bar. Always use when in a canvas context"))) {
                            foreach (var serializedObjectTargetObject in serializedObject.targetObjects) {
                                if (serializedObjectTargetObject != null) {
                                    var obj = serializedObjectTargetObject as RadialSegmentedHealthBar;
                                    obj.UsingSpriteRenderer = false;
                                }
                            }
                        }
                    }
                    GUILayout.EndHorizontal();

                    string currentlyUsing = "-";
                    if (!usingSpriteRenderer.hasMultipleDifferentValues) {
                        if (usingSpriteRenderer.boolValue)
                            currentlyUsing = "Sprite Renderer";
                        else
                            currentlyUsing = "Image";
                    }
                    GUILayout.Label("Currently using: " + currentlyUsing);

                    // EditorGUI.BeginChangeCheck();
                    // EditorGUILayout.PropertyField(forceBuiltInShader, new GUIContent("Force Built In Shader", "Use this if you are having render issues in your canvas! Usually on by default if using a canvas"));
                    // if (EditorGUI.EndChangeCheck()) {
                    //     foreach (var serializedObjectTargetObject in serializedObject.targetObjects) {
                    //         if (serializedObjectTargetObject != null) {
                    //             var obj = serializedObjectTargetObject as RadialSegmentedHealthBar;
                    //             obj.ForceBuiltInShader = forceBuiltInShader.boolValue;
                    //         }
                    //     }
                    // }
                }
                GUILayout.EndVertical();
            }
            //GUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("Remember to include the namespace 'RengeGames.HealthBars' when using progress bars in your scripts!", MessageType.Info);

            serializedObject.ApplyModifiedProperties();

            //base.OnInspectorGUI();
        }

        void SliderPropertyField(string text, string tooltip, SerializedProperty property, float min, float max) {
            var valueProp = FindValueProperty(property);
            var dirtyProp = FindDirtyProperty(property);

            var floatVal = valueProp.floatValue;
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = valueProp.hasMultipleDifferentValues;
            floatVal = EditorGUILayout.Slider(new GUIContent(text, tooltip), floatVal, min, max);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck()) {
                valueProp.floatValue = floatVal;
                dirtyProp.boolValue = true;
            }
        }
    }
}