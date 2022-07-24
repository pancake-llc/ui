﻿using Pancake.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pancake.UI.Editor
{
    public static class MenuItemCreator
    {
        [MenuItem("GameObject/Pancake/UIButton", false, 1000)]
        public static void CreateUniButtonEmpty()
        {
            var button = CreateObject<UIButton>("Button");
            Undo.RegisterCreatedObjectUndo(button.gameObject, "Create UIButton");
            SetupUIButton(button);
            Selection.activeTransform = button;
        }

        private static void SetupUIButton(RectTransform button)
        {
            var uiButton = button.GetComponent<UIButton>();
            button.sizeDelta = new Vector2(160, 60);
            uiButton.IsMotion = true;
        }

        [MenuItem("GameObject/Pancake/UIButton (TMP)", false, 1000)]
        private static void AddUniButtonTMP()
        {
            var button = CreateObject<UIButtonTMP>("Button");
            Undo.RegisterCreatedObjectUndo(button.gameObject, "Create UIButton TMP");
            SetupUIButton(button);
            Selection.activeTransform = button;
        }

        [MenuItem("GameObject/Pancake/UIPopup", false, 1100)]
        public static void CreateUIPopupEmpty()
        {
            var popup = CreateUIPopupObject();
            Undo.RegisterCreatedObjectUndo(popup.gameObject, "Create UniPopup");
            Selection.activeTransform = popup;
        }
        
        private static RectTransform CreateUIPopupObject()
        {
            // find canvas in scene
            var allCanvases = (Canvas[]) Object.FindObjectsOfType(typeof(Canvas));
            if (allCanvases.Length > 0)
            {
                if (Selection.activeTransform == null) return CreateUniPopup(allCanvases[0].transform);

                for (int i = 0; i < allCanvases.Length; i++)
                {
                    if (!Selection.activeTransform.IsChildOf(allCanvases[i].transform)) continue;
                    return CreateUniPopup(Selection.activeTransform);
                }

                return CreateUniPopup(allCanvases[0].transform);
            }

            var canvas = CreateCanvas();
            return CreateUniPopup(canvas.transform);
        }
        
        private static RectTransform CreateUniPopup(Transform parent)
        {
#if UNITY_2020_2_OR_NEWER
            static RectTransform Create(Transform parentLocalVar, string name, bool overSorting = true)
#else
            RectTransform Create(Transform parentLocalVar, string name, bool overSorting = true)
#endif
            {
                var r = CreateEmptyRectTransformObject(parentLocalVar, name);
                r.gameObject.AddComponent<Canvas>().overrideSorting = overSorting;
                r.gameObject.AddComponent<GraphicRaycaster>();
                return r;
            }

            var popup = Create(parent, "Popup");
            popup.FullScreen();

            var background = Create(popup.transform, "Background", false);
            background.FullScreen();
            background.gameObject.AddComponent<CanvasGroup>();
            var img = background.gameObject.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.78f);

            var container = Create(popup.transform, "Container", false);
            container.gameObject.AddComponent<CanvasGroup>();
            container.gameObject.AddComponent<Image>().GetComponent<RectTransform>().sizeDelta = new Vector2(800, 500);

            var button = CreateObjectWithComponent<UIButton>(container.transform, "BtnClose");
            button.gameObject.layer = LayerMask.NameToLayer("UI");
            button.sizeDelta = new Vector2(80, 80);
            button.anchorMax = Vector2.one;
            button.anchorMin = Vector2.one;
            button.anchoredPosition = Vector2.zero;
            return popup;
        }
        
        private static RectTransform CreateEmptyRectTransformObject(Transform parent, string name)
        {
            var obj = new GameObject(name);
            obj.gameObject.layer = LayerMask.NameToLayer("UI");
            var rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        private static Canvas CreateCanvas()
        {
            var canvas = new GameObject("Canvas").AddComponent<Canvas>();
            canvas.gameObject.layer = LayerMask.NameToLayer("UI");
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvas.gameObject.AddComponent<GraphicRaycaster>();

            var eventSystem = (EventSystem) Object.FindObjectOfType(typeof(EventSystem));
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }

            return canvas;
        }

        private static RectTransform CreateObjectWithComponent<T>(Transform parent, string name) where T : Component
        {
            var r = CreateEmptyRectTransformObject(parent, name);
            r.gameObject.AddComponent<T>();
            return r;
        }

        private static RectTransform CreateObject<T>(string name) where T : Component
        {
            // find canvas in scene
            var allCanvases = (Canvas[]) Object.FindObjectsOfType(typeof(Canvas));
            if (allCanvases.Length > 0)
            {
                if (Selection.activeTransform == null) return CreateObjectWithComponent<T>(allCanvases[0].transform, name);

                for (int i = 0; i < allCanvases.Length; i++)
                {
                    if (!Selection.activeTransform.IsChildOf(allCanvases[i].transform)) continue;
                    return CreateObjectWithComponent<T>(Selection.activeTransform, name);
                }

                return CreateObjectWithComponent<T>(allCanvases[0].transform, name);
            }

            var canvas = CreateCanvas();
            return CreateObjectWithComponent<T>(canvas.transform, name);
        }
    }
}