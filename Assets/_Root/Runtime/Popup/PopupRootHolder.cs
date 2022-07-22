using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pancake.UI
{
    [DisallowMultipleComponent]
    public class PopupRootHolder : MonoBehaviour
    {
        public static PopupRootHolder instance;
        public AssetLabelReference label;

        private void Awake() { instance = this; }
    }
}