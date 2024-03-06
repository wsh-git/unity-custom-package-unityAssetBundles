using System;
using System.Collections;
using System.IO;
using System.Net;
using Wsh.Net.Https;
using Wsh.Net.UnityWebRequests;

namespace Wsh.AssetBundles {
    public class AssetBundleVersionChecker {
        public static IEnumerator Handle(string platformResUrl, AssetBundleUpdateInfo updateInfo) {
            if(updateInfo.Status == AssetBundleUpdateStatus.Skip) {
                yield break;
            }
            else if(updateInfo.Status == AssetBundleUpdateStatus.Next) {
                Log.Info("0 Start check assetBundle version.");
                string versionUrl = platformResUrl + AssetBundleDefine.ASSET_BUNDLE_VERSION_FILE_NAME;
                bool isContinue = false;

                // Http.IsExist() 在Android手机上不适用
                /*Log.Info("1 (1/2) Start check exist version file.");
                Http.IsExist(versionUrl, res => {
                    Log.Info(res.IsSuccess, res.StatusCode, res.Message);
                    if(res.IsSuccess) {
                        isContinue = true;
                        Log.Info("1 (2/2) remote version file exist.");
                    } else if(res.StatusCode == HttpStatusCode.NotFound) {
                        updateInfo.SetStatus(AssetBundleUpdateStatus.Skip);
                        isContinue = true;
                        Log.Info("1 (2/2) remote version file not exist.");
                    } else {
                        Log.Info("Check exist version file. error:", versionUrl, res.StatusCode, res.Message);
                        updateInfo.SetStatus(AssetBundleUpdateStatus.Skip);
                        isContinue = true;
                    }
                });
                
                while(!isContinue) {
                    yield return null;
                }
                
                isContinue = false;*/

                if(updateInfo.Status == AssetBundleUpdateStatus.Next) {
                    Log.Info("1 (1/2) Start request remote version file.");
                    UnityWebRequestManager.Instance.RequestText(versionUrl, null, res => {
                        if(res.IsSuccess) {
                            updateInfo.SetVersion(res.Text);
                            isContinue = true;
                            Log.Info("1 (2/2) remote version file.", updateInfo.Version);
                        }
                        else {
                            updateInfo.SetStatus(AssetBundleUpdateStatus.Skip);
                            isContinue = true;
                            Log.Error("Request version.txt. error:", res.Result, res.Message);
                        }
                    });

                    while(!isContinue) {
                        yield return null;
                    }

                    isContinue = false;
                    if(updateInfo.Status == AssetBundleUpdateStatus.Next) {
                        Log.Info("2 (1/2) Start request local version file.");
                        string localVersionFilePath = Path.Combine(PlatformUtils.PersistentDataPathWithStream,
                            AssetBundleDefine.ASSET_BUNDLE_VERSION_FILE_NAME);
                        if(File.Exists(localVersionFilePath)) {
                            Log.Info(PlatformUtils.PersistentDataPathWithStream);
                            string localVersionFileReqPath = Path.Combine(PlatformUtils.PersistentDataPath,
                                AssetBundleDefine.ASSET_BUNDLE_VERSION_FILE_NAME);
                            UnityWebRequestManager.Instance.RequestText(localVersionFileReqPath, null, res => {
                                if(res.IsSuccess) {
                                    Log.Info("2 (2/2) request local version file success.", res.Text);
                                    int localVersion = 0;
                                    try {
                                        localVersion = int.Parse(res.Text);
                                    }
                                    catch(Exception e) {
                                        Log.Error("int parse error. text", res.Text, e.Message);
                                    }

                                    if(updateInfo.Version > localVersion) {
                                        updateInfo.SetStatus(AssetBundleUpdateStatus.Next);
                                    }
                                    else {
                                        updateInfo.SetVersion(0);
                                        updateInfo.SetStatus(AssetBundleUpdateStatus.Skip);
                                    }

                                    isContinue = true;
                                    Finish(updateInfo);
                                }
                                else {
                                    Log.Error("Request local version file. error:", res.Result, res.Message);
                                }
                            });
                            while(!isContinue) {
                                yield return null;
                            }
                        }
                        else {
                            Log.Info("2 (2/2) local version file not exist.");
                            Finish(updateInfo);
                        }
                    } else if(updateInfo.Status == AssetBundleUpdateStatus.Skip){
                        Finish(updateInfo);
                    }
                }
                else {
                    Finish(updateInfo);
                }
            }
        }

        private static void Finish(AssetBundleUpdateInfo updateInfo) {
            Log.Info("Finish check assetBundle version.", updateInfo.Status, updateInfo.Version);
        }
    }
}