using System.Collections.Generic;
using UnityEngine;

namespace Pancake.UI
{
    public class Popup : MonoBehaviour
    {
        public static Popup Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Instance = new GameObject("[Popup]").AddComponent<Popup>();

            DontDestroyOnLoad(Instance);
        }

        /// <summary>
        /// stack contains all popup (LIFO)
        /// </summary>
        private readonly Stack<IPopup> _stacks = new Stack<IPopup>();
        private Dictionary<string, IPopup> _container = new Dictionary<string, IPopup>();

        /// <summary>
        /// control sorting order of root canvas popup
        /// </summary>
        private int _sortingOrder;

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

        public void Show<T>() where T : IPopup
        {
            
        }
        
        /// <summary>
        /// show popup
        /// </summary>
        /// <param name="popup">popup wanna show</param>
        public void Show(IPopup popup)
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
        public void Show(IPopup popup, int number)
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
        public void ShowAndColapseAll(IPopup popup) { Show(popup, _stacks.Count); }

        /// <summary>
        /// check has exist <paramref name="popup"/> in active stack
        /// </summary>
        /// <param name="popup"></param>
        /// <returns></returns>
        public bool Contains(IPopup popup)
        {
            foreach (var handler in _stacks)
            {
                if (handler == popup) return true;
            }

            return false;
        }
    }
}