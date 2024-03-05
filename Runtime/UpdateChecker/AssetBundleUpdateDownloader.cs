using System;
using System.Collections;
using System.IO;
using Wsh.Net.UnityWebRequests;

namespace Wsh.AssetBundles {
    
    public class AssetBundleUpdateDownloader {

        public static IEnumerator Handle(string platformResUrl, AssetBundleUpdateInfo updateInfo, Action<float, float> onLoad) {
            if(updateInfo.Status == AssetBundleUpdateStatus.Skip) {
                yield break;
            } else if(updateInfo.Status == AssetBundleUpdateStatus.Next) {
                Log.Info("0 Start download assetBundles.");
                int downloadNumber = 0;
                bool isContinue = false;
                ulong currentSize = 0;
                
                onLoad?.Invoke(currentSize, updateInfo.TotalSize);
                for(int i = 0; i < updateInfo.WaitDownloadList.Count; i++) {
                    string fileName = updateInfo.WaitDownloadList[i];
                    Log.Info("Start load file", fileName);
                    ulong currentDownloadSize = 0;
                    UnityWebRequestManager.Instance.DownloadFile(platformResUrl + fileName,  Path.Combine(PlatformUtils.PersistentDataPathWithStream, fileName), (arg1, arg2) => {
                        currentDownloadSize = arg2;
                    }, res => {
                        if(res.IsSuccess) {
                            currentSize += currentDownloadSize;
                            onLoad(currentSize, updateInfo.TotalSize);
                            isContinue = true;
                        } else {
                            Log.Error("download file error.", fileName, res.Result, res.Message);
                        }
                    });
                    while(!isContinue) {
                        yield return null;
                    }
                    isContinue = false;
                }
                
            }

        }
        
    }
    
}