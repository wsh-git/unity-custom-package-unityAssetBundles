namespace Wsh.AssetBundles {

    public interface IAssetBundleHotUpdateListener {

        void OnBeforeCompareVersion();
        void OnFinishCompareVersion();
        void OnFinish();

    }

}