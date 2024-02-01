using Wsh.Net;

namespace Wsh.AssetBundles.Editor {
    
    public class AssetBundleUploader {

        public static async void Upload(string dirPath, string serverIp, string originalDir, PlatformType uploadPlatformType, string account, string passward) {
            /*Ftp.CheckOriginalDir(serverIp, originalDir, account, passward, () => {
                Log.Info("onfinish");
            });*/
            //Log.Info(dirPath);
            // Ftp.UploadFile(serverIp, "D:/Projects/yiyiyaya/UnityProject/AssetBundleEditor/__AssetBundlesTemp/PC/PC",account, passward);
        }
        
    }
}