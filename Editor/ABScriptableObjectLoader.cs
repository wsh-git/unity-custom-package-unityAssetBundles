using System;
using UnityEditor;
using UnityEngine;

namespace Wsh.AssetBundles.Editor {
    
    public class ABScriptableObjectLoader {

        private string SCRIPTABLEOBJECT_PATH = "Assets/ABMainWindowScriptableObject.asset";

        public ABMainScriptableObject LoadABMainScriptableObject() {
            ABMainScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ABMainScriptableObject>(SCRIPTABLEOBJECT_PATH);
            return scriptableObject;
        }
        
        public void SaveScriptableObject(AssetBundleMainWindow window) {
            ABMainScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ABMainScriptableObject>(SCRIPTABLEOBJECT_PATH);
            if(scriptableObject == null) {
                scriptableObject = ScriptableObject.CreateInstance<ABMainScriptableObject>();
                AssetDatabase.CreateAsset(scriptableObject, SCRIPTABLEOBJECT_PATH);
            }
            scriptableObject.ResRootDir = window.ResRootDir;
            scriptableObject.ABOutputDir = window.OutputDir;
            scriptableObject.BuildTarget = window.BuildTarget;
            scriptableObject.IsClearOutputDir = window.IsClearOutputDir;
            scriptableObject.IsCopyAssetStreaming = window.IsCopyAssetStreaming;
            scriptableObject.CompressOptionsType = window.CompressOptionsType;
            scriptableObject.Version = window.Version;
            scriptableObject.UploadDir = window.UploadDir;
            scriptableObject.ServerIp = window.ServerIp;
            scriptableObject.OriginalDir = window.OriginalDir;
            scriptableObject.UploadTargetType = window.UploadTargetType;
            scriptableObject.Account = window.Account;
            scriptableObject.Password = window.Password;
            EditorUtility.SetDirty(scriptableObject);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private void TryCreateScriptableObject() {
            ABMainScriptableObject scriptableObject = ScriptableObject.CreateInstance<ABMainScriptableObject>();
            AssetDatabase.CreateAsset(scriptableObject, SCRIPTABLEOBJECT_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        public void CheckScriptableObject() {
            Type scriptableObj = typeof(ABMainScriptableObject);
            string[] assetPaths = AssetDatabase.FindAssets("t:" + scriptableObj);
            if(assetPaths == null || assetPaths.Length == 0) {
                TryCreateScriptableObject();
            }
        }
        
    }
    
}