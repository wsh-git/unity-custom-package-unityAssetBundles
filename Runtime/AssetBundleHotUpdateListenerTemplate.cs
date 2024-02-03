
namespace Wsh.AssetBundles {


    public class AssetBundleHotUpdateListenerTemplate : IAssetBundleHotUpdateListener {

        public void OnBeforeCompareVersion() {
            Log.Info("onBeforeCompareVersion");
        }

        public void OnFinishCompareVersion() {

        }

        public void OnFinish() {
            Log.Info("AssetBundle hot udate completed.");
        }

    }
}