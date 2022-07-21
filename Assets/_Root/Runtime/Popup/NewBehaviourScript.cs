using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pancake.UI
{
    public class NewBehaviourScript : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Addressables.InitializeAsync();
            //Popup.Instance.Show<PopupSetting>();
        }

        [ContextMenu("A")]
        public void A()
        {
            Popup.Instance.Release<PopupSetting>();
        }
        
        
    }
}
