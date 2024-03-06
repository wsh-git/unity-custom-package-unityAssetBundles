using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Wsh.Net.Https;

namespace Wsh.AssetBundles.Editor {
    public class AssetBundleUploader {
        
        public static async void Upload(string dirPath, string serverIp, string originalDir, PlatformType uploadPlatformType, string account, string passward) {
            string remoteDirUrl = serverIp + "/" + originalDir + "/" + uploadPlatformType.ToString();
            Http.IsExist(remoteDirUrl, response => {
                Log.Info(response.IsSuccess, response.StatusCode, response.Message);
                if(response.IsSuccess) {
                    StartUpload(dirPath, uploadPlatformType, remoteDirUrl, account, passward);
                }
                else {
                    Log.Error($"服务器不存在 {originalDir} 目录。");
                }
            });
        }

        private static void StartUpload(string dirPath, PlatformType uploadPlatformType, string remoteDirUrl, string account, string passward) {
            DirectoryInfo directory = new DirectoryInfo(Path.Combine(dirPath, uploadPlatformType.ToString()));
            FileInfo[] files = directory.GetFiles();
            List<FileInfo> uploadFileList = new List<FileInfo>();
            for(int i = 0; i < files.Length; i++) {
                string fileName = files[i].Name;
                if(!fileName.EndsWith("manifest") && !fileName.EndsWith("lt")) {
                    uploadFileList.Add(files[i]);
                }
            }
            int totalNumber = uploadFileList.Count;
            int successNumber = 0;
            for(int i = 0; i < uploadFileList.Count; i++) {
                FileInfo fileInfo = uploadFileList[i];
                Http.Upload(account, passward, remoteDirUrl, fileInfo.Name, fileInfo.FullName, response => {
                    if(response.IsSuccess) {
                        successNumber++;
                        Log.Info(fileInfo.Name, "上传成功", $"{successNumber}/{totalNumber}");
                    } else {
                        Log.Error(fileInfo.Name, "上传失败", response.StatusCode, response.Message);
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