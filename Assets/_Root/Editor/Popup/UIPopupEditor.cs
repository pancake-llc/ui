using System;
using Pancake.Editor;
using Pancake.UIQuery;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditorInternal;
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
        private ReorderableList _closeButtonList;

        public UIPopup popup;
        public UICache uiCache;

        protected virtual void OnEnable()
        {
            popup = target as UIPopup;
            if (uiCache == null && popup != null) uiCache = popup.GetComponent<UICache>();
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
            _interpolatorDisplay = serializedObject.FindProperty("interpolatorDisplay");
            _interpolatorHide = serializedObject.FindProperty("interpolatorHide");
            _endValueHide = serializedObject.FindProperty("endValueHide");
            _endValueDisplay = serializedObject.FindProperty("endValueDisplay");
            _durationHide = serializedObject.FindProperty("durationHide");
            _durationDisplay = serializedObject.FindProperty("durationDisplay");
            _closeButtonList = new ReorderableList(serializedObject,
                _closeButtons,
                true,
                true,
                true,
                true);
            _closeButtonList.drawElementCallback = DrawListButtonItem;
            _closeButtonList.drawHeaderCallback = DrawHeader;
        }

        private void DrawHeader(Rect rect) { EditorGUI.LabelField(rect, "Close Button"); }

        private void DrawListButtonItem(Rect rect, int index, bool isactive, bool isfocused)
        {
            SerializedProperty element = _closeButtonList.serializedProperty.GetArrayElementAtIndex(index); //The element in the list
            EditorGUI.PropertyField(rect, element, new GUIContent(element.displayName), element.isExpanded);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUIUtility.labelWidth = 110;

            bool updateCacheUI = false;
            try
            {
                var _ = uiCache.Get<RectTransform>("Background");
            }
            catch (Exception)
            {
                updateCacheUI = true;
            }

            if (updateCacheUI)
            {
                Uniform.HelpBox("Please update UICache first to use!", MessageType.Warning);
            }
            else
            {
                if (PrefabUtility.IsPartOfAnyPrefab(popup.gameObject))
                {
                    if (!IsAddressable())
                    {
                        Uniform.HelpBox("Click the toogle below to mark the popup as can be loaded by addressable", MessageType.Warning);
                        if (GUILayout.Button("Mark Popup")) MarkPopup();
                    }
                    else
                    {
                        Uniform.HelpBox("Marked as popup", MessageType.Info);
                    }
                }
                Uniform.SpaceOneLine();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Ignore Time Scale", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _ignoreTimeScale.boolValue = GUILayout.Toggle(_ignoreTimeScale.boolValue, "");
                EditorGUILayout.EndHorizontal();
                Uniform.SpaceOneLine();
                Uniform.DrawUppercaseSection("UIPOPUP_CLOSE", "CLOSE BY", DrawCloseSetting);
                Uniform.SpaceOneLine();
                Uniform.DrawUppercaseSection("UIPOPUP_SETTING_DISPLAY", "DISPLAY", DrawDisplaySetting);
                Uniform.SpaceOneLine();
                Uniform.DrawUppercaseSection("UIPOPUP_SETTING_HIDE", "HIDE", DrawHideSetting);
            }

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
                var backgroundTransform = uiCache.Get<RectTransform>("Background");
                if (backgroundTransform != null)
                {
                    backgroundTransform.TryGetComponent<Button>(out var btn);
                    if (_closeByClickBackground.boolValue)
                    {
                        if (btn == null) btn = AddBlankButtonComponent(backgroundTransform.gameObject);
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

                var containerTransform = uiCache.Get<RectTransform>("Container");
                if (containerTransform != null)
                {
                    containerTransform.TryGetComponent<Button>(out var btn);
                    if (_closeByClickContainer.boolValue)
                    {
                        if (btn == null) btn = AddBlankButtonComponent(containerTransform.gameObject);
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
#pragma warning restore 612

                _closeButtonList.DoLayoutList();
            }

            void DrawDisplaySetting()
            {
                var containerTransform = uiCache.Get<RectTransform>("Container");
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
                        _positionFromDisplay.vector2Value = containerTransform.localPosition;
                        containerTransform.localPosition = Vector3.zero;
                    }

                    if (GUILayout.Button("Save To", GUILayout.Width(90)))
                    {
                        _positionToDisplay.vector2Value = containerTransform.localPosition;
                        containerTransform.localPosition = Vector3.zero;
                    }

                    if (GUILayout.Button("Clear", GUILayout.Width(90)))
                    {
                        _positionFromDisplay.vector2Value = Vector2.zero;
                        _positionToDisplay.vector2Value = Vector2.zero;
                        containerTransform.localPosition = Vector3.zero;
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

            void DrawHideSetting()
            {
                var containerTransform = uiCache.Get<RectTransform>("Container");
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
                        _positionToHide.vector2Value = containerTransform.localPosition;
                        containerTransform.localPosition = Vector3.zero;
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

            void MarkPopup()
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (!settings.GetLabels().Contains(PopupHelper.POPUP_LABEL)) settings.AddLabel(PopupHelper.POPUP_LABEL);
                AddressableAssetGroup group = settings.FindGroup("Default Local Group");
                var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(popup.gameObject);
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid)) return;
                var entry = settings.CreateOrMoveEntry(guid, group);
                
                if (!entry.labels.Contains(PopupHelper.POPUP_LABEL))  entry.labels.Add(PopupHelper.POPUP_LABEL);
                entry.address = popup.gameObject.name;
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            bool IsAddressable()
            {
                bool flag = false;
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings.GetLabels().Contains(PopupHelper.POPUP_LABEL)) settings.AddLabel(PopupHelper.POPUP_LABEL);
                AddressableAssetGroup group = settings.FindGroup("Default Local Group");
                var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(popup.gameObject);
                var guid = AssetDatabase.AssetPathToGUID(path);

                AddressableAssetEntry entry = null;
                foreach (var addressableAssetEntry in group.entries)
                {
                    if (addressableAssetEntry.guid == guid)
                    {
                        flag = true;
                        entry = addressableAssetEntry;
                        break;
                    }
                }

                if (flag)  flag = entry.labels.Contains(PopupHelper.POPUP_LABEL);

                return flag;
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
        private static Button AddBlankButtonComponent(GameObject target)
        {
            var button = target.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            return button;
        }
    }
}