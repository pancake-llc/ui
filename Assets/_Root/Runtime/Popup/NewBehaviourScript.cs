using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Pancake.UI
{
    public class NewBehaviourScript : MonoBehaviour
    {
        private void Awake()
        {
            //DontDestroyOnLoad(gameObject);
        }

        [ContextMenu("Show")]
        public void ShowPopupSetting()
        {
            Popup.Show<PopupSetting>();
        }

        [ContextMenu("Release")]
        public void A()
        {
            Popup.Release<PopupSetting>();
        }     
        
        [ContextMenu("Close")]
        public void Close()
        {
            Popup.Close();
        }       
        
        [ContextMenu("menu")]
        public void Menu()
        {
            SceneManager.LoadScene("menu");
        }
        
        
    }
}
