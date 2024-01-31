using Wsh.Net;

namespace Wsh.AssetBundles.Editor {
    
    public class AssetBundleUploader {

        public static async void Upload(string dirPath, string serverIp, string originalDir, PlatformType uploadPlatformType, string account, string passward) {
            // Ftp.CheckOriginalDir(serverIp, originalDir, account, passward, () => {
            //     Log.Info("onfinish");
            // });
            
            /*Log.Info("1");
            bool isSuccess = await Http.CheckOriginalDirAsync(serverIp);
            Log.Info("3", isSuccess);

             if(!isSuccess) {
                 Log.Info("开始创建目录");
                 bool isCreateSuccess = await Http.CreateServerDirectoryAsync(serverIp);
                 if(isCreateSuccess) {
                     Log.Info("创建目录成功");
                 } else {
                     Log.Info("创建目录失败");
                     return;
                 }
             }
             
            // 上传本地目录下的所有文件至HTTP服务器对应的目录
            
            await Http.UploadFilesAsync(serverIp, dirPath);
            Log.Info("File upload completed.");*/
        }
        
    }
}