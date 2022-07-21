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
        public static Popup Instance { get; private set; }
        public bool IsDoneFindAllPopupLightWeight { get; set; }
        private const string LABEL = "uipopup";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Instance = new GameObject("[Popup]").AddComponent<Popup>();
            Instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(Instance);
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
        
        public async void LazyFindAllPrefabLocation()
        {
            IsDoneFindAllPopupLightWeight = false;
            _container = new Dictionary<IResourceLocation, IPopup>();
            var allPopopupNames = await Addressables.LoadResourceLocationsAsync(LABEL);
            foreach (var className in allPopopupNames)
            {
                if (!_container.ContainsKey(className)) _container.Add(className, null);
            }

            IsDoneFindAllPopupLightWeight = true;
        }

        /// <summary>
        /// hide popup in top stack
        /// </summary>
        public void Close()
        {
            _stacks.Pop(); // remove the highest popup in stack
            var orderOfBoard = 0;
            if (_stacks.Count >= 1)
            {
                var top = _stacks.Peek();
                top.Rise();
                if (_stacks.Count > 1) orderOfBoard = top.Canvas.sortingOrder - 10;
            }

            _sortingOrder = orderOfBoard;
        }

        /// <summary>
        /// hide all popup in top stack
        /// </summary>
        public void CloseAll()
        {
            int count = _stacks.Count;
            for (var i = 0; i < count; i++)
            {
                _stacks.Pop().Close();
            }

            _sortingOrder = 0;
        }

        public async void Show<T>() where T : IPopup
        {
            if (_container == null)
            {
                LazyFindAllPrefabLocation();
                await UniTask.WaitUntil(() => IsDoneFindAllPopupLightWeight);
            }
            var className = typeof(T).Name;
            IResourceLocation location = null;
            bool exist = false;
            foreach (var key in _container.Keys)
            {
                if (key.PrimaryKey.Equals(className))
                {
                    exist = true;
                    location = key;
                    break;
                }
            }

            if (!exist)
            {
                Debug.LogError($"[Popup] Can not find popup in addressable with key '{className}'");
                return;
            }

            if (_container[location] == null)
            {
                var obj = await Addressables.InstantiateAsync(location);
                _container[location] = obj.GetComponent<IPopup>();
            }

            Show(_container[location]);
        }

        public void Release<T>() where T : IPopup
        {
            if (_container == null) return;
            var className = typeof(T).Name;
            IResourceLocation location = null;
            bool exist = false;
            foreach (var key in _container.Keys)
            {
                if (key.PrimaryKey.Equals(className))
                {
                    exist = true;
                    location = key;
                    break;
                }
            }
            
            if (!exist)
            {
                Debug.LogError($"[Popup] Can not find popup in addressable with key '{className}'");
                return;
            }

            if (_container[location] != null)
            {
                Addressables.ReleaseInstance(_container[location].GameObject);
                _container[location] = null;
            }
        }

        /// <summary>
        /// show popup
        /// </summary>
        /// <param name="popup">popup wanna show</param>
        private void Show(IPopup popup)
        {
            var lastOrder = 0;
            if (_stacks.Count > 0)
            {
                var top = _stacks.Peek();
                top.Collapse();
                lastOrder = top.Canvas.sortingOrder;
            }

            popup.UpdateSortingOrder(lastOrder + 10);
            _sortingOrder = lastOrder;
            _stacks.Push(popup);
            popup.Show(); // show
        }

        /// <summary>
        /// show popup and hide previous popup
        /// </summary>
        /// <param name="popup">popup wanna show</param>
        /// <param name="number">number previous popup wanna hide</param>
        private void Show(IPopup popup, int number)
        {
            if (number > _stacks.Count) number = _stacks.Count;

            for (int i = 0; i < number; i++)
            {
                var p = _stacks.Pop();
                p.Close();
            }

            Show(popup);
        }

        /// <summary>
        /// show popup and hide all previous popup
        /// </summary>
        /// <param name="popup">popup wanna show</param>
        private void ShowAndColapseAll(IPopup popup) { Show(popup, _stacks.Count); }

        /// <summary>
        /// check has exist <paramref name="popup"/> in active stack
        /// </summary>
        /// <param name="popup"></param>
        /// <returns></returns>
        private bool Contains(IPopup popup)
        {
            foreach (var handler in _stacks)
            {
                if (handler == popup) return true;
            }

            return false;
        }
    }
}