using System;
using System.Collections;
using Wsh.Singleton;

namespace Wsh.AssetBundles {
    
    public class AssetBundleUpdateManager : Singleton<AssetBundleUpdateManager>, ISingleton {
        

        public void OnInit() {
            
        }
        
        public void OnDeinit() {
            
        }

        public void CheckHotUpdate(string resUrl, Action<float, float> onLoad, Action onFinish) {
            string platformResUrl = resUrl + "/" + PlatformUtils.Platform + "/";
            StartCoroutine(StartCheckHotUpdate(platformResUrl, onLoad, onFinish));
        }

        private IEnumerator StartCheckHotUpdate(string platformResUrl, Action<float, float> onLoad, Action onFinish) {
            AssetBundleUpdateInfo updateInfo = new AssetBundleUpdateInfo();
            yield return AssetBundleVersionChecker.Handle(platformResUrl, updateInfo);
            yield return AssetBundleCompareFileChecker.Handle(platformResUrl, updateInfo);
            yield return AssetBundleUpdateDownloader.Handle(platformResUrl, updateInfo, onLoad);
            bool isFinish = false;
            Log.Info("Start save version and compare files.");
            updateInfo.SaveLocalFile(() => { isFinish = true;});
            while(!isFinish) {
                yield return null;
            }
            Log.Info("Finish save version and compare files.");
            onFinish?.Invoke();
            Log.Info("AssetBundle update finish.");
            Destroy();
        }
        
    }
    
}