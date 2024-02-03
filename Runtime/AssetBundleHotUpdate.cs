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
        private IAssetBundleHotUpdateListener m_listener;
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

        public void CheckHotUpdate(string resUrl, IAssetBundleHotUpdateListener listener) {
            m_listener = listener;
            m_onFinish = () => {
                m_listener.OnFinish();
                OnDeinit();
            };
            string platformResUrl = resUrl + "/" + PlatformUtils.Platform + "/";
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
            StartCoroutine(AssetBundleUtils.ReqRemoteFile(resUrl + AssetBundleDefine.ASSET_BUNDLE_COMPARE_FILE_NAME, (isSuccess, message) => {
                if(isSuccess) {
                    string localTempCompareFilePath = Path.Combine(PlatformUtils.PersistentDataPathWithStream, AssetBundleDefine.ASSET_BUNDLE_COMPARE_TEMP_FILE_NAME);
                    Log.Info("Get remote version file success.", message);
                    File.WriteAllText(localTempCompareFilePath, message);
                    m_remoteAbInfoDic = AssetBundleUtils.GetCompareFileDic(message);
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
                StartCoroutine(AssetBundleUtils.GetLocalFile(localCompareFilePath, (isOk, msg) => {
                    if(isOk) {
                        m_localAbInfoDic = AssetBundleUtils.GetCompareFileDic(msg);
                        onFinish?.Invoke();
                    } else {
                        Log.Error("Get local compare file failed.", msg);
                    }
                }));
            } else {
                onFinish?.Invoke();
            }
        }
        
        private void CompareVersion(string resUrl, Action<bool> onFinish) {
            string localVersionPath = Path.Combine(PlatformUtils.PersistentDataPathWithStream, AssetBundleDefine.ASSET_BUNDLE_VERSION_FILE_NAME);
            m_listener.OnBeforeCompareVersion();
            if(File.Exists(localVersionPath)) {
                StartCoroutine(AssetBundleUtils.GetLocalFile(localVersionPath, (isOk, msg) => {
                    if(isOk) {
                        Log.Info("Get local version:", msg);
                        m_localABVersion = msg;
                        StartCoroutine(AssetBundleUtils.ReqRemoteFile(resUrl + AssetBundleDefine.ASSET_BUNDLE_VERSION_FILE_NAME, (isSuccess, message) => {
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
                //TODO: 从AssetStreamingPath目录下检测
                onFinish?.Invoke(false);
            }
        }
        
    }
    
}