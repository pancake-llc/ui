#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace Pancake.Toolbar
{
    public class KillNormalMapFix : EditorViewModule
    {
        public override IEnumerable<Type> GetTargetTypes()
        {
            yield return typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.BumpMapSettingsFixingWindow");
        }

        public override void OnViewRefresh()
        {
            view.window.Close();
        }
    }
}
#endif