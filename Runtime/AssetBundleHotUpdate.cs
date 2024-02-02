using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Wsh.AssetBundles {
    
    public class AssetBundleHotUpdate : MonoBehaviour {

        private static AssetBundleHotUpdate m_instance;
        public static AssetBundleHotUpdate Instance {
            get {
                if(m_instance == null) {
                    GameObject go = new GameObject("__AssetBundleHotUpdate");
                    m_instance = go.AddComponent<AssetBundleHotUpdate>();
                }
                return m_instance;
            }
        }

        private string m_localABVersion;
        private string m_remoteABVersion;
        private Action m_onFinish;
        private Dictionary<string, AssetBundleInfo> m_localAbInfoDic = new Dictionary<string, AssetBundleInfo>();
        private Dictionary<string, AssetBundleInfo> m_remoteAbInfoDic = new Dictionary<string, AssetBundleInfo>();
        // 待下载的ab包列表文件，存储ab包的名字
        private List<string> m_waitDownloadList = new List<string>();
        private int m_downloadedCount;
        
        private void OnDeinit() {
            Destroy(this.gameObject);
            m_instance = null;
        }

        public void CheckHotUpdate(string resUrl, Action onFinish) {
            m_onFinish = () => {
                onFinish?.Invoke();
                Log.Info("AssetBundle update finish.");
                OnDeinit();
            };
            string platformResUrl = resUrl + "/" + PlatformUtils.Platform + "/";
            Log.Info("Start check local assetBundle version.");
            CompareVersion(platformResUrl, isUpdateAsset => {
                if(isUpdateAsset) {
                    // 开始获取更新资源
                    Log.Info("Start update assetbundles");
                    StartUpdateAssetBundle(platformResUrl, m_onFinish);
                } else {
                    Log.Info("The assetbundles is lastest.");
                    m_onFinish();
                }
            });
        }

        private void StartUpdateAssetBundle(string resUrl, Action onFinish) {
            GetLocalCompareFile(() => {
                // m_localAbInfoDic init end.
                Log.Info("Get local compare file init end.");
                Log.Info("Start request remote compare file.");
                ReqRemoteCompareFile(resUrl, () => {
                    // get remote compare file init end
                    Log.Info("Get remote compare file init end.");
                    Log.Info("TODO:");
                });
            });
        }

        private void ReqRemoteCompareFile(string resUrl, Action onFinish){
            StartCoroutine(ReqRemoteFile(resUrl + AssetBundleDefine.ASSET_BUNDLE_COMPARE_FILE_NAME, (isSuccess, message) => {
                if(isSuccess) {
                    string localTempCompareFilePath = Path.Combine(PlatformUtils.PersistentDataPathWithStream, AssetBundleDefine.ASSET_BUNDLE_COMPARE_TEMP_FILE_NAME);
                    Log.Info("Get remote version file success.", message);
                    File.WriteAllText(localTempCompareFilePath, message);
                    m_remoteAbInfoDic = GetCompareFileDic(message);
                    onFinish?.Invoke();
                } else {
                    Log.Error("Get remote compare file failed.", message);
                }
            }));
        }
        private void GetLocalCompareFile(Action onFinish) {
            string localCompareFilePath = Path.Combine(PlatformUtils.PersistentDataPathWithStream, AssetBundleDefine.ASSET_BUNDLE_COMPARE_FILE_NAME);
            m_localAbInfoDic.Clear();
            if(File.Exists(localCompareFilePath)) {
                StartCoroutine(GetLocalFile(localCompareFilePath, (isOk, msg) => {
                    if(isOk) {
                        m_localAbInfoDic = GetCompareFileDic(msg);
                        onFinish?.Invoke();
                    } else {
                        Log.Error("Get local compare file failed.", msg);
                    }
                }));
            } else {
                onFinish?.Invoke();
            }
        }
        
        private Dictionary<string, AssetBundleInfo> GetCompareFileDic(string txtInfo) {
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
        
        private void CompareVersion(string resUrl, Action<bool> onFinish) {
            string localVersionPath = Path.Combine(PlatformUtils.PersistentDataPathWithStream, AssetBundleDefine.ASSET_BUNDLE_VERSION_FILE_NAME);
            if(File.Exists(localVersionPath)) {
                StartCoroutine(GetLocalFile(localVersionPath, (isOk, msg) => {
                    if(isOk) {
                        Log.Info("Get local version:", msg);
                        m_localABVersion = msg;
                        StartCoroutine(ReqRemoteFile(resUrl + AssetBundleDefine.ASSET_BUNDLE_VERSION_FILE_NAME, (isSuccess, message) => {
                            if(isSuccess) {
                                Log.Info("Get remote version file success.", message);
                                m_remoteABVersion = message;
                                onFinish?.Invoke(m_remoteABVersion != m_localABVersion);
                            } else {
                                Log.Error("Get remote version file failed.", message);
                            }
                        }));
                    } else {
                        Log.Error("Get local version file failed.", msg);
                    }
                }));
            } else {
                onFinish?.Invoke(false);
            }
        }

        private IEnumerator ReqRemoteFile(string remotesFileUrl, Action<bool, string> onFinish) {
            UnityWebRequest request = UnityWebRequest.Get(remotesFileUrl);
            request.timeout = 15;
            Log.Info("request.timeout", request.timeout);
            yield return request.SendWebRequest();
            if (!request.isDone || request.isNetworkError || request.isHttpError || request.result != UnityWebRequest.Result.Success) {
                Log.Error($"req remote failed.", remotesFileUrl, request.result, request.error);
                onFinish?.Invoke(false, request.error);
            } else {
                onFinish?.Invoke(true, request.downloadHandler.text);
            }
        }
        
        private IEnumerator GetLocalFile(string filePath, Action<bool, string> onFinish) {
            string requestUrl = PlatformUtils.FILE_PREFIX + filePath;
            UnityWebRequest request = UnityWebRequest.Get(requestUrl);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success) {
                onFinish?.Invoke(true, request.downloadHandler.text);
            } else {
                onFinish?.Invoke(false, request.error);
            }
        }
        
    }
    
}