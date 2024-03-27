using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wsh.AssetBundles.Editor {
    
    public class ABScriptableObjectLoader {

        private const string SCRIPTABLEOBJECT_FOLDER = "Assets/WshConfig/";

        private const string SCRIPTABLEOBJECT_PATH = SCRIPTABLEOBJECT_FOLDER + "ABMainWindowScriptableObject.asset";

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
            scriptableObject.BlackDirList = window.BlackDirList;
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
            TryCreateFolder();
            AssetDatabase.CreateAsset(scriptableObject, SCRIPTABLEOBJECT_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private void TryCreateFolder() {
            if(!Directory.Exists(SCRIPTABLEOBJECT_FOLDER)) {
                Directory.CreateDirectory(SCRIPTABLEOBJECT_FOLDER);
            }
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