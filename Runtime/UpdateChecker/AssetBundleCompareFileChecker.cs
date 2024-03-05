using System.Collections;
using System.Collections.Generic;
using System.IO;
using Wsh.Net.UnityWebRequests;

namespace Wsh.AssetBundles {
    
    public class AssetBundleCompareFileChecker {

        public static IEnumerator Handle(string platformResUrl, AssetBundleUpdateInfo updateInfo) {
            if(updateInfo.Status == AssetBundleUpdateStatus.Skip) {
                yield break;
            } else if(updateInfo.Status == AssetBundleUpdateStatus.Next) {
                Log.Info("0 Start check compare file.");
                string compareFileUrl = platformResUrl + AssetBundleDefine.ASSET_BUNDLE_COMPARE_FILE_NAME;
                bool isContinue = false;
                Dictionary<string, AssetBundleInfo> newCompareDic = new Dictionary<string, AssetBundleInfo>();
                Dictionary<string, AssetBundleInfo> oldCompareDic = new Dictionary<string, AssetBundleInfo>();
                Log.Info("1 (1/2) Start request compare file.");
                UnityWebRequestManager.Instance.RequestText(compareFileUrl, null, res => {
                    if(res.IsSuccess) {
                        newCompareDic = GetCompareFileDic(res.Text);
                        updateInfo.SetNewCompareText(res.Text);
                        isContinue = true;
                    } else {
                        Log.Error("request compare file failed. error:", res.Result, res.Message);
                    }
                });

                while(!isContinue) {
                    yield return null;
                }

                isContinue = false;
                Log.Info("2 (1/2) Start request local compare file.");
                string localCompareFilePath = Path.Combine(PlatformUtils.PersistentDataPathWithStream, AssetBundleDefine.ASSET_BUNDLE_COMPARE_FILE_NAME);
                if(File.Exists(localCompareFilePath)) {
                    string localCompareFileReqPath = Path.Combine(PlatformUtils.PersistentDataPath, AssetBundleDefine.ASSET_BUNDLE_COMPARE_FILE_NAME);
                    UnityWebRequestManager.Instance.RequestText(localCompareFileReqPath, null, res => {
                        if(res.IsSuccess) {
                            Log.Info("2 (2/2) request local compare file success.");
                            oldCompareDic = GetCompareFileDic(res.Text);
                            isContinue = true;
                        } else {
                            Log.Error("request local compare file failed. error:", res.Result, res.Message);
                        }
                    });
                } else {
                    Log.Info("2 (2/2) local compare file not exist.");
                    isContinue = true;
                }

                while(!isContinue) {
                    yield return null;
                }

                isContinue = false;
                
                Log.Info("3 Start compare two files.");
                List<string> waitDownloadList = new List<string>();
                ulong totalSize = 0;
                foreach(var name in newCompareDic.Keys) {
                    Log.Info(name, newCompareDic[name].Name, newCompareDic[name].Md5, newCompareDic[name].Size);
                    if(oldCompareDic.ContainsKey(name)) {
                        if(newCompareDic[name] != oldCompareDic[name]) {
                            totalSize += newCompareDic[name].Size;
                            waitDownloadList.Add(name);
                        }
                    } else {
                        totalSize += newCompareDic[name].Size;
                        waitDownloadList.Add(name);
                    }
                }
                updateInfo.SetWaitDownloadList(totalSize, waitDownloadList);
                Finish(updateInfo);
            }
        }
        
        private static Dictionary<string, AssetBundleInfo> GetCompareFileDic(string txtInfo) {
            Dictionary<string, AssetBundleInfo> dic = new Dictionary<string, AssetBundleInfo>();
            // 获取已下载的对比文件
            string[] abs = txtInfo.Split(AssetBundleDefine.ASSET_BUNDLE_COMPARE_FILE_SLIP_CHAR);
            string[] abInfo = null;
            for (int i = 0; i < abs.Length; i++) {
                abInfo = abs[i].Split(AssetBundleDefine.ASSET_BUNDLE_COMPARE_INFO_SLIP_CHAR);
                AssetBundleInfo ab = new AssetBundleInfo(abInfo[0], abInfo[1], abInfo[2]);
                // 记录每一个远端ab包的信息之后，好做对比信息；
                dic.Add(ab.Name, ab);
            }
            return dic;
        }
        
        private static void Finish(AssetBundleUpdateInfo updateInfo) {
            Log.Info("Finish check assetBundle compare file.", updateInfo.Status, updateInfo.Version);
        }
        
    }
    
}