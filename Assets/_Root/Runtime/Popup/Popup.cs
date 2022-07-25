using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Pancake.UI
{
    /// <summary>
    /// prefab popup must be mark label is uipopup
    /// Marking the label has the effect of loading all available prefab popups in the address location form and performing a search by type.
    /// </summary>
    [AddComponentMenu("")]
    public class Popup : MonoBehaviour
    {
        private static Popup instance;
        public bool IsDoneFindAllPopupLightWeight { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            instance = new GameObject("[Popup]").AddComponent<Popup>();
            instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(instance);
        }

        /// <summary>
        /// stack contains all popup (LIFO)
        /// </summary>
        private readonly Stack<IPopup> _stacks = new Stack<IPopup>();

        private Dictionary<IResourceLocation, IPopup> _container;

        /// <summary>
        /// control sorting order of root canvas popup
        /// </summary>
        private int _sortingOrder;

        public static async void LazyFindAllPrefabLocation()
        {
            instance.IsDoneFindAllPopupLightWeight = false;
            instance._container = new Dictionary<IResourceLocation, IPopup>();
            var allPopopupNames = await Addressables.LoadResourceLocationsAsync(PopupHelper.POPUP_LABEL);
            foreach (var className in allPopopupNames)
            {
                if (!instance._container.ContainsKey(className)) instance._container.Add(className, null);
            }

            instance.IsDoneFindAllPopupLightWeight = true;
        }

        /// <summary>
        /// Close popup in top of stack
        /// popup will still alive but it deactive
        /// </summary>
        public static void Close()
        {
            if (instance._stacks.Count == 0)
            {
                Debug.LogError("[Popup] stack holder popup is empty, you can not close");
                return;
            }

            instance._stacks.Pop().Close(); // remove the highest popup in stack
            var orderOfBoard = 0;
            if (instance._stacks.Count >= 1)
            {
                var top = instance._stacks.Peek();
                top.Rise();
                if (instance._stacks.Count > 1) orderOfBoard = top.Canvas.sortingOrder - 10;
            }

            instance._sortingOrder = orderOfBoard;
        }

        /// <summary>
        /// Close all popup in top stack
        /// popup will still alive but it deactive
        /// </summary>
        public static void CloseAll()
        {
            int count = instance._stacks.Count;
            for (var i = 0; i < count; i++)
            {
                instance._stacks.Pop().Close();
            }

            instance._sortingOrder = 0;
        }

        public static async void Show<T>() where T : IPopup
        {
            if (instance._container == null)
            {
                LazyFindAllPrefabLocation();
                await UniTask.WaitUntil(() => instance.IsDoneFindAllPopupLightWeight);
            }

            if (instance._container == null)
            {
                Debug.Log("[Popup] container null. Please check again!");
                return;
            }

            var location = Get<T>();

            if (location == null)
            {
                Debug.LogError($"[Popup] Can not find popup in addressable with key '{typeof(T).Name}'");
                return;
            }

            if (instance._container[location] == null)
            {
                var obj = await Addressables.InstantiateAsync(location, parent: PopupRootHolder.instance.transform);
                instance._container[location] = obj.GetComponent<IPopup>();
            }

            Show(instance._container[location]);
        }

        public static void Release<T>() where T : IPopup
        {
            if (instance._container == null) return;
            var location = Get<T>();

            if (location == null)
            {
                Debug.LogError($"[Popup] Can not find popup in addressable with key '{typeof(T).Name}'");
                return;
            }

            if (instance._container[location] != null)
            {
                Addressables.ReleaseInstance(instance._container[location].GameObject);
                instance._container[location] = null;
            }
        }

        private static IResourceLocation Get<T>()
        {
            string typeName = typeof(T).Name; // reflection
            IResourceLocation result = null;
            foreach (var key in instance._container.Keys)
            {
                if (key.PrimaryKey.Equals(typeName)) result = key;
            }

            return result;
        }

        /// <summary>
        /// show popup
        /// </summary>
        /// <param name="popup">popup wanna show</param>
        private static void Show(IPopup popup)
        {
            var lastOrder = 0;
            if (instance._stacks.Count > 0)
            {
                var top = instance._stacks.Peek();
                if (top == popup)
                {
                    Debug.Log("[Popup] you trying show popup is already displayed!");
                    return;
                }
                top.Collapse();
                lastOrder = top.Canvas.sortingOrder;
            }

            popup.UpdateSortingOrder(lastOrder + 10);
            instance._sortingOrder = lastOrder;
            instance._stacks.Push(popup);
            popup.Show();
        }

        /// <summary>
        /// show popup and hide previous popup
        /// </summary>
        /// <param name="popup">popup wanna show</param>
        /// <param name="number">number previous popup wanna hide</param>
        private static void Show(IPopup popup, int number)
        {
            if (number > instance._stacks.Count) number = instance._stacks.Count;

            for (int i = 0; i < number; i++)
            {
                var p = instance._stacks.Pop();
                p.Close();
            }

            Show(popup);
        }

        /// <summary>
        /// show popup and hide all previous popup
        /// </summary>
        /// <param name="popup">popup wanna show</param>
        private static void ShowAndColapseAll(IPopup popup) { Show(popup, instance._stacks.Count); }

        /// <summary>
        /// check has exist <paramref name="popup"/> in active stack
        /// </summary>
        /// <param name="popup"></param>
        /// <returns></returns>
        private static bool Contains(IPopup popup)
        {
            foreach (var handler in instance._stacks)
            {
                if (handler == popup) return true;
            }

            return false;
        }
    }
}