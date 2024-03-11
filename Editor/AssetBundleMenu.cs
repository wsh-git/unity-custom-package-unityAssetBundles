using UnityEditor;

namespace Wsh.AssetBundles.Editor {
    
    public class AssetBundleMenu {
        
        [MenuItem("AssetBundleTool/Builder #%t", priority = 1)]
        public static void ShowMainWindow() {
            AssetBundleMainWindow.ShowWindow();
        }
        
        [MenuItem("AssetBundleTool/GetDependencies", priority = 3)]
        public static void GetDependencies() {
            AssetBundleEditorHelper.GetDependencies(Selection.activeGameObject);
        }
        
    }
}