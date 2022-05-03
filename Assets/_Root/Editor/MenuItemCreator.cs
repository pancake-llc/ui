using Pancake.Tween;
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
            var tweenDown = button.GetComponent<TweenPlayer>();
            tweenDown.timeMode = TimeMode.Normal;
            tweenDown.arrivedAction = ArrivedAction.AlwaysStopOnArrived;
            tweenDown.sampleOnAwake = false;
            var scaleDown = tweenDown.AddAnimation<TweenTransformScale>();
            scaleDown.toggle.x = true;
            scaleDown.toggle.y = true;
            var uiButton = button.GetComponent<UIButton>();
            uiButton.TweenDown = tweenDown;

            var tweenUp = button.gameObject.AddComponent<TweenPlayer>();
            tweenUp.timeMode = TimeMode.Normal;
            tweenUp.arrivedAction = ArrivedAction.AlwaysStopOnArrived;
            tweenUp.sampleOnAwake = false;
            var scaleUp = tweenUp.AddAnimation<TweenTransformScale>();
            scaleUp.toggle.x = true;
            scaleUp.toggle.y = true;
            uiButton.TweenUp = tweenUp;

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