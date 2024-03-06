using System.Collections.Generic;
using System.IO;
using Wsh.Net.Ftps;
using Wsh.Net.Https;

namespace Wsh.AssetBundles.Editor {
    public class AssetBundleUploader {

        public class UploadFileInfo {
            
            public string Name;
            public string FullName;
            
        }

        private static bool IsHttpServer(string serverIp) {
            return serverIp.StartsWith("http");
        }
        
        public static void Upload(string dirPath, string serverIp, string originalDir, PlatformType uploadPlatformType, string account, string passward) {
            string remoteDirUrl = serverIp + "/" + originalDir + "/" + uploadPlatformType.ToString();
            if(IsHttpServer(serverIp)) {
                Http.IsExist(remoteDirUrl, response => {
                    Log.Info(response.IsSuccess, response.StatusCode, response.Message);
                    if(response.IsSuccess) {
                        StartHttpUpload(dirPath, uploadPlatformType, remoteDirUrl, account, passward);
                    } else {
                        Log.Error($"服务器不存在 {originalDir} 目录。");
                    }
                });
            } else {
                StartFtpUpload(dirPath, uploadPlatformType, remoteDirUrl, account, passward);
            }
        }

        private static List<UploadFileInfo> GetUploadFileInfoList(string dirPath, PlatformType uploadPlatformType) {
            List<UploadFileInfo> list = new List<UploadFileInfo>();
            DirectoryInfo directory = new DirectoryInfo(Path.Combine(dirPath, uploadPlatformType.ToString()));
            FileInfo[] files = directory.GetFiles();
            for(int i = 0; i < files.Length; i++) {
                string fileName = files[i].Name;
                if(!fileName.EndsWith("manifest") && !fileName.EndsWith("lt")) {
                    list.Add(new UploadFileInfo{Name = files[i].Name, FullName = files[i].FullName});
                }
            }
            return list;
        }

        private static void StartHttpUpload(string dirPath, PlatformType uploadPlatformType, string remoteDirUrl, string account, string passward) {
            List<UploadFileInfo> uploadFileList = GetUploadFileInfoList(dirPath, uploadPlatformType);
            int totalNumber = uploadFileList.Count;
            int successNumber = 0;
            for(int i = 0; i < uploadFileList.Count; i++) {
                var fileInfo = uploadFileList[i];
                Http.Upload(account, passward, remoteDirUrl, fileInfo.Name, fileInfo.FullName, response => {
                    if(response.IsSuccess) {
                        successNumber++;
                        Log.Info(fileInfo.Name, "上传成功", $"{successNumber}/{totalNumber}");
                        if(successNumber == totalNumber) {
                            Log.Info("All files upload success.");
                        }
                    } else {
                        Log.Error(fileInfo.Name, "上传失败", response.StatusCode, response.Message);
                    }
                });
            }
        }

        private static void StartFtpUpload(string dirPath, PlatformType uploadPlatformType, string remoteDirUrl, string account, string password) {
            List<UploadFileInfo> uploadFileList = GetUploadFileInfoList(dirPath, uploadPlatformType);
            int totalNumber = uploadFileList.Count;
            int successNumber = 0;
            for(int i = 0; i < uploadFileList.Count; i++) {
                var fileInfo = uploadFileList[i];
                FTP.Upload(account, password, remoteDirUrl + "/" + fileInfo.Name, fileInfo.FullName, res => {
                    if(res.IsSuccess) {
                        successNumber++;
                        Log.Info(fileInfo.Name, "上传成功", $"{successNumber}/{totalNumber}");
                        if(successNumber == totalNumber) {
                            Log.Info("All files upload success.");
                        }
                    } else {
                        Log.Error(fileInfo.Name, "上传失败", res.Message);
                    }
                });
            }
        }

        /*public static void TryCreateDir(string remoteDirUrl) {
            try {
                // 创建一个HttpWebRequest指向你想要创建文件夹的位置
                HttpWebRequest mkColRequest = (HttpWebRequest)WebRequest.Create(remoteDirUrl);
                mkColRequest.Method = "MKCOL"; // 使用MKCOL方法表示创建目录
                mkColRequest.Credentials = new NetworkCredential("wsh", "123123"); // 如果需要认证，请提供凭据
                using(var response = (HttpWebResponse)mkColRequest.GetResponse()) {
                    if(response.StatusCode == HttpStatusCode.Created ||
                       response.StatusCode == HttpStatusCode.NoContent) {
                        Log.Info("目录成功创建");
                    } else {
                        Log.Info($"未能创建目录，服务器返回状态码: {response.StatusCode}");
                    }
                }
            } catch(WebException ex) {
                Log.Info($"创建目录时发生错误: {ex.Message}");
            }
        }*/
    }
}