using Pancake.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI.Editor
{
    [CustomEditor(typeof(UIPopup), true)]
    [CanEditMultipleObjects]
    public class UIPopupEditor : UnityEditor.Editor
    {
        protected const int DEFAULT_LABEL_WIDTH = 110;
        protected static readonly string[] PopupMotionType = {"Scale", "Position", "PositionAndScale"};
        private SerializedProperty _canvas;
        private SerializedProperty _graphicRaycaster;
        private SerializedProperty _backgroundTransform;
        private SerializedProperty _backgroundCanvas;
        private SerializedProperty _backgroundGraphicRaycaster;
        private SerializedProperty _backgroundCanvasGroup;
        private SerializedProperty _containerTransform;
        private SerializedProperty _containerCanvas;
        private SerializedProperty _containerGraphicRaycaster;
        private SerializedProperty _containerCanvasGroup;
        private SerializedProperty _closeButtons;
        private SerializedProperty _closeByBackButton;
        private SerializedProperty _closeByClickBackground;
        private SerializedProperty _closeByClickContainer;
        private SerializedProperty _ignoreTimeScale;
        private SerializedProperty _motionAffectDisplay;
        private SerializedProperty _motionAffectHide;
        private SerializedProperty _positionToHide;
        private SerializedProperty _positionToDisplay;
        private SerializedProperty _positionFromDisplay;
        private SerializedProperty _interpolatorDisplay;
        private SerializedProperty _interpolatorHide;
        private SerializedProperty _endValueHide;
        private SerializedProperty _endValueDisplay;
        private SerializedProperty _durationHide;
        private SerializedProperty _durationDisplay;

        protected UIPopup popup;

        protected virtual void OnEnable()
        {
            popup = target as UIPopup;
            _closeByBackButton = serializedObject.FindProperty("closeByBackButton");
            _closeByClickBackground = serializedObject.FindProperty("closeByClickBackground");
            _closeByClickContainer = serializedObject.FindProperty("closeByClickContainer");
            _ignoreTimeScale = serializedObject.FindProperty("ignoreTimeScale");
            _closeButtons = serializedObject.FindProperty("closeButtons");
            _motionAffectDisplay = serializedObject.FindProperty("motionAffectDisplay");
            _motionAffectHide = serializedObject.FindProperty("motionAffectHide");
            _positionToHide = serializedObject.FindProperty("positionToHide");
            _positionToDisplay = serializedObject.FindProperty("positionToDisplay");
            _positionFromDisplay = serializedObject.FindProperty("positionFromDisplay");
            _containerTransform = serializedObject.FindProperty("containerTransform");
            _containerCanvas = serializedObject.FindProperty("containerCanvas");
            _containerGraphicRaycaster = serializedObject.FindProperty("containerGraphicRaycaster");
            _containerCanvasGroup = serializedObject.FindProperty("containerCanvasGroup");
            _interpolatorDisplay = serializedObject.FindProperty("interpolatorDisplay");
            _interpolatorHide = serializedObject.FindProperty("interpolatorHide");
            _endValueHide = serializedObject.FindProperty("endValueHide");
            _endValueDisplay = serializedObject.FindProperty("endValueDisplay");
            _durationHide = serializedObject.FindProperty("durationHide");
            _durationDisplay = serializedObject.FindProperty("durationDisplay");
            _canvas = serializedObject.FindProperty("canvas");
            _graphicRaycaster = serializedObject.FindProperty("graphicRaycaster");
            _backgroundTransform = serializedObject.FindProperty("backgroundTransform");
            _backgroundCanvas = serializedObject.FindProperty("backgroundCanvas");
            _backgroundGraphicRaycaster = serializedObject.FindProperty("backgroundGraphicRaycaster");
            _backgroundCanvasGroup = serializedObject.FindProperty("backgroundCanvasGroup");
        }

        public override void OnInspectorGUI()
        {
            var prevColor = GUI.color;
            serializedObject.Update();
            EditorGUIUtility.labelWidth = 110;

            Uniform.DrawUppercaseSection("UIPOPUP_CLOSE", "CLOSE BY", DrawCloseSetting);

            void DrawCloseSetting()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Back Button", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _closeByBackButton.boolValue = GUILayout.Toggle(_closeByBackButton.boolValue, "");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Background", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _closeByClickBackground.boolValue = GUILayout.Toggle(_closeByClickBackground.boolValue, "");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Container", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _closeByClickContainer.boolValue = GUILayout.Toggle(_closeByClickContainer.boolValue, "");
                EditorGUILayout.EndHorizontal();

#pragma warning disable 612
                if (_backgroundTransform != null && _backgroundTransform.objectReferenceValue != null)
                {
                    var bg = _backgroundTransform.objectReferenceValue as RectTransform;
                    if (bg != null)
                    {
                        bg.TryGetComponent<Button>(out var btn);
                        if (popup.CloseByClickBackground)
                        {
                            if (btn == null) btn = AddBlankButtonComponent(bg.gameObject);
                            if (!popup.CloseButtons.Contains(btn)) popup.CloseButtons.Add(btn);
                        }
                        else
                        {
                            if (btn != null)
                            {
                                DestroyImmediate(btn);
                                _closeButtons?.RemoveEmptyArrayElements();
                            }
                        }
                    }
                }

                if (_containerTransform != null && _containerTransform.objectReferenceValue != null)
                {
                    var container = _containerTransform.objectReferenceValue as RectTransform;
                    if (container != null)
                    {
                        container.TryGetComponent<Button>(out var btn);
                        if (popup.CloseByClickContainer)
                        {
                            if (btn == null) btn = AddBlankButtonComponent(container.gameObject);
                            if (!popup.CloseButtons.Contains(btn)) popup.CloseButtons.Add(btn);
                        }
                        else
                        {
                            if (btn != null)
                            {
                                DestroyImmediate(btn);
                                _closeButtons?.RemoveEmptyArrayElements();
                            }
                        }
                    }
                }
#pragma warning restore 612
            }

            Uniform.DrawUppercaseSection("UIPOPUP_SETTING_DISPLAY", "DISPLAY", DrawDisplaySetting);

            void DrawDisplaySetting()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Type", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _motionAffectDisplay.enumValueIndex = EditorGUILayout.Popup(_motionAffectDisplay.enumValueIndex, PopupMotionType);
                EditorGUILayout.EndHorizontal();
                if (_motionAffectDisplay.enumValueIndex == (int) EMotionAffect.Position || _motionAffectDisplay.enumValueIndex == (int) EMotionAffect.PositionAndScale)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    if (GUILayout.Button("Save From", GUILayout.Width(90)))
                    {
                        _positionFromDisplay.vector2Value = ((Transform) _containerTransform.objectReferenceValue).localPosition;
                        ((Transform) _containerTransform.objectReferenceValue).localPosition = Vector3.zero;
                    }

                    if (GUILayout.Button("Save To", GUILayout.Width(90)))
                    {
                        _positionToDisplay.vector2Value = ((Transform) _containerTransform.objectReferenceValue).localPosition;
                        ((Transform) _containerTransform.objectReferenceValue).localPosition = Vector3.zero;
                    }

                    if (GUILayout.Button("Clear", GUILayout.Width(90)))
                    {
                        _positionFromDisplay.vector2Value = Vector2.zero;
                        _positionToDisplay.vector2Value = Vector2.zero;
                        ((Transform) _containerTransform.objectReferenceValue).localPosition = Vector3.zero;
                    }

                    EditorGUILayout.EndHorizontal();

                    GUI.enabled = false;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("   From", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    popup.positionToHide = EditorGUILayout.Vector2Field("", _positionFromDisplay.vector2Value, GUILayout.Height(18));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("   To", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    popup.positionToDisplay = EditorGUILayout.Vector2Field("", _positionToDisplay.vector2Value, GUILayout.Height(18));
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Interpolator"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.PropertyField(_interpolatorDisplay, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                if (_motionAffectDisplay.enumValueIndex == (int) EMotionAffect.Scale || _motionAffectDisplay.enumValueIndex == (int) EMotionAffect.PositionAndScale)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Value", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _endValueDisplay.vector2Value = EditorGUILayout.Vector2Field("", _endValueDisplay.vector2Value, GUILayout.Height(18));
                    GUILayout.EndHorizontal();
                }

                _durationDisplay.floatValue = EditorGUILayout.FloatField("Duration", _durationDisplay.floatValue);
            }

            Uniform.DrawUppercaseSection("UIPOPUP_SETTING_HIDE", "HIDE", DrawHideSetting);

            void DrawHideSetting()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Type", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _motionAffectHide.enumValueIndex = EditorGUILayout.Popup(_motionAffectHide.enumValueIndex, PopupMotionType);
                EditorGUILayout.EndHorizontal();

                if (_motionAffectHide.enumValueIndex == (int) EMotionAffect.Position || _motionAffectHide.enumValueIndex == (int) EMotionAffect.PositionAndScale)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(DEFAULT_LABEL_WIDTH));

                    if (GUILayout.Button("Save To", GUILayout.Width(90)))
                    {
                        _positionToHide.vector2Value = ((Transform) _containerTransform.objectReferenceValue).localPosition;
                        ((Transform) _containerTransform.objectReferenceValue).localPosition = Vector3.zero;
                    }

                    if (GUILayout.Button("Clear", GUILayout.Width(90)))
                    {
                        _positionToHide.vector2Value = Vector2.zero;
                    }

                    EditorGUILayout.EndHorizontal();

                    GUI.enabled = false;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("   To", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _positionToHide.vector2Value = EditorGUILayout.Vector2Field("", _positionToHide.vector2Value, GUILayout.Height(18));
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Interpolator"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.PropertyField(_interpolatorHide, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                if (_motionAffectHide.enumValueIndex == (int) EMotionAffect.Scale || _motionAffectHide.enumValueIndex == (int) EMotionAffect.PositionAndScale)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Value", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _endValueHide.vector2Value = EditorGUILayout.Vector2Field("", _endValueHide.vector2Value, GUILayout.Height(18));
                    GUILayout.EndHorizontal();
                }

                _durationHide.floatValue = EditorGUILayout.FloatField("Duration", _durationHide.floatValue);
            }
            
            GUI.color = Color.white;
            Uniform.DrawUppercaseSection("UIPOPUP_SETTING_REF_ROOT", "ROOT", DrawReferenceRootSetting);

            void DrawReferenceRootSetting()
            {
                if (_canvas != null && (_canvas.objectReferenceValue == null || _canvas.objectReferenceValue != popup.GetComponent<Canvas>()))
                    _canvas.objectReferenceValue = popup.GetComponent<Canvas>();

                if (_canvas != null) GUI.color = _canvas.objectReferenceValue == null ? Uniform.InspectorNullError : Uniform.InspectorLock;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Canvas", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.ObjectField(_canvas, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                if (_graphicRaycaster != null && (_graphicRaycaster.objectReferenceValue == null ||
                                                  _graphicRaycaster.objectReferenceValue != popup.GetComponent<GraphicRaycaster>()))
                    _graphicRaycaster.objectReferenceValue = popup.GetComponent<GraphicRaycaster>();

                if (_graphicRaycaster != null) GUI.color = _graphicRaycaster.objectReferenceValue == null ? Uniform.InspectorNullError : Uniform.InspectorLock;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Graphic Raycaster", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.ObjectField(_graphicRaycaster, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
            }

            GUI.color = Color.white;
            Uniform.DrawUppercaseSection("UIPOPUP_SETTING_REF_BG", "BACKGROUND", DrawReferenceBackgroundSetting);

            void DrawReferenceBackgroundSetting()
            {
                if (_backgroundTransform != null)
                {
                    var bg = popup.transform.Find("Background");
                    if (_backgroundTransform.objectReferenceValue == null) _backgroundTransform.objectReferenceValue = bg != null ? bg : null;
                    else if (bg != null && _backgroundTransform.objectReferenceValue != bg) _backgroundTransform.objectReferenceValue = bg;
                    GUI.color = _backgroundTransform.objectReferenceValue == null ? Uniform.InspectorNullError : Uniform.InspectorLock;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Transform", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.ObjectField(_backgroundTransform, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                if (_backgroundCanvas != null)
                {
                    var bg = popup.transform.Find("Background")?.GetComponent<Canvas>();
                    if (_backgroundCanvas.objectReferenceValue == null) _backgroundCanvas.objectReferenceValue = bg != null ? bg : null;
                    else if (bg != null && _backgroundCanvas.objectReferenceValue != bg) _backgroundCanvas.objectReferenceValue = bg;
                    GUI.color = _backgroundCanvas.objectReferenceValue == null ? Uniform.InspectorNullError : Uniform.InspectorLock;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Canvas", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.ObjectField(_backgroundCanvas, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                if (_backgroundGraphicRaycaster != null)
                {
                    var bg = popup.transform.Find("Background")?.GetComponent<GraphicRaycaster>();
                    if (_backgroundGraphicRaycaster.objectReferenceValue == null)
                    {
                        _backgroundGraphicRaycaster.objectReferenceValue = bg != null ? bg : null;
                    }
                    else if (bg != null && _backgroundGraphicRaycaster.objectReferenceValue != bg)
                    {
                        _backgroundGraphicRaycaster.objectReferenceValue = bg;
                    }

                    GUI.color = _backgroundGraphicRaycaster.objectReferenceValue == null ? Uniform.InspectorNullError : Uniform.InspectorLock;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Graphic Raycaster", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.ObjectField(_backgroundGraphicRaycaster, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                if (_backgroundCanvasGroup != null)
                {
                    var bg = popup.transform.Find("Background")?.GetComponent<CanvasGroup>();
                    if (_backgroundCanvasGroup.objectReferenceValue == null)
                    {
                        _backgroundCanvasGroup.objectReferenceValue = bg != null ? bg : null;
                    }
                    else if (bg != null && _backgroundCanvasGroup.objectReferenceValue != bg)
                    {
                        _backgroundCanvasGroup.objectReferenceValue = bg;
                    }

                    GUI.color = _backgroundCanvasGroup.objectReferenceValue == null ? Uniform.InspectorNullError : Uniform.InspectorLock;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("CanvasGroup", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.ObjectField(_backgroundCanvasGroup, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
            }

            GUI.color = Color.white;
            Uniform.DrawUppercaseSection("UIPOPUP_SETTING_REF_CONTAINER", "CONTAINER", DrawReferenceContainerSetting);

            void DrawReferenceContainerSetting()
            {
                if (_containerTransform != null)
                {
                    var container = popup.transform.Find("Container");
                    if (_containerTransform.objectReferenceValue == null)
                    {
                        _containerTransform.objectReferenceValue = container != null ? container : null;
                    }
                    else if (container != null && _containerTransform.objectReferenceValue != container)
                    {
                        _containerTransform.objectReferenceValue = container;
                    }

                    GUI.color = _containerTransform.objectReferenceValue == null ? Uniform.InspectorNullError : Uniform.InspectorLock;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Transform", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.ObjectField(_containerTransform, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                if (_containerCanvas != null)
                {
                    var container = popup.transform.Find("Container")?.GetComponent<Canvas>();
                    if (_containerCanvas.objectReferenceValue == null)
                    {
                        _containerCanvas.objectReferenceValue = container != null ? container : null;
                    }
                    else if (container != null && _containerCanvas.objectReferenceValue != container)
                    {
                        _containerCanvas.objectReferenceValue = container;
                    }

                    GUI.color = _containerCanvas.objectReferenceValue == null ? Uniform.InspectorNullError : Uniform.InspectorLock;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Canvas", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.ObjectField(_containerCanvas, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                if (_containerGraphicRaycaster != null)
                {
                    var container = popup.transform.Find("Container")?.GetComponent<GraphicRaycaster>();
                    if (_containerGraphicRaycaster.objectReferenceValue == null)
                    {
                        _containerGraphicRaycaster.objectReferenceValue = container != null ? container : null;
                    }
                    else if (container != null && _containerGraphicRaycaster.objectReferenceValue != container)
                    {
                        _containerGraphicRaycaster.objectReferenceValue = container;
                    }

                    GUI.color = _containerGraphicRaycaster.objectReferenceValue == null ? Uniform.InspectorNullError : Uniform.InspectorLock;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Graphic Raycaster", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.ObjectField(_containerGraphicRaycaster, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                if (_containerCanvasGroup != null)
                {
                    var container = popup.transform.Find("Container")?.GetComponent<CanvasGroup>();
                    if (_containerCanvasGroup.objectReferenceValue == null)
                    {
                        _containerCanvasGroup.objectReferenceValue = container != null ? container : null;
                    }
                    else if (container != null && _containerCanvasGroup.objectReferenceValue != container)
                    {
                        _containerCanvasGroup.objectReferenceValue = container;
                    }

                    GUI.color = _containerCanvasGroup.objectReferenceValue == null ? Uniform.InspectorNullError : Uniform.InspectorLock;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Canvas Group", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.ObjectField(_containerCanvasGroup, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
            }
            
            
            
            EditorGUILayout.Space();
            Repaint();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        /// <summary>
        /// add blank button
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Button AddBlankButtonComponent(GameObject target)
        {
            var button = target.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            return button;
        }
    }
}