using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;
using AB = UnityEngine.AssetBundle;
using Object = UnityEngine.Object;

namespace Wsh.AssetBundles {
    
    public class AssetBundleManager : MonoBehaviour {
        
        private AB m_mainAB;
        private AssetBundleManifest m_manifest;
        private Dictionary<string, AB> m_abDic;

        private static AssetBundleManager m_instance;

        public static AssetBundleManager Instance {
            get {
                if(m_instance == null) {
                    GameObject go = new GameObject("__AssetBundleManager");
                    m_instance = go.AddComponent<AssetBundleManager>();
                    DontDestroyOnLoad(go);
                }
                return m_instance;
            }
        }

        public void Init(Action onFinish) {
            m_abDic = new Dictionary<string, AB>();
            LoadBundle(PlatformUtils.Platform, ab => {
                m_mainAB = ab;
                m_manifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                onFinish?.Invoke();
            });
        }

        private void LoadBundle(string bundleName, Action<AB> onFinish) {
            StartCoroutine(IELoadBundle(bundleName, onFinish));
        }
        
        private IEnumerator IELoadBundle(string bundleName, Action<AB> onFinish) {
            if(m_abDic.ContainsKey(bundleName)) {
                onFinish?.Invoke(m_abDic[bundleName]);
            } else {
                string abPath = Path.Combine(PlatformUtils.StreamingAssetsPathWithStream, bundleName);
                bool isWeb = false;
#if UNITY_EDITOR
                isWeb = false;
#elif UNITY_WEBGL
                isWeb = true;
#else
                isWeb = false;
#endif
                if(isWeb) {
                    UnityWebRequest webReq = UnityWebRequestAssetBundle.GetAssetBundle(abPath);
                    yield return webReq.SendWebRequest();
                    if(webReq.isDone) {
                        if(webReq.result != UnityWebRequest.Result.Success) {
                            Log.Error("Error downloading AssetBundle:", webReq.error);
                        } else {
                            AB ab = DownloadHandlerAssetBundle.GetContent(webReq);
                            m_abDic.Add(bundleName, ab);
                            onFinish?.Invoke(ab);
                        }
                    }
                } else {
                    int offset = AssetBundleUtils.GetBundleOffset(bundleName, AssetBundleDefine.ASSET_BUNDLE_OFFSET);
                    AssetBundleCreateRequest req = AB.LoadFromFileAsync(abPath, 0, (ulong)offset);
                    yield return req;
                    if(req.isDone) {
                        m_abDic.Add(bundleName, req.assetBundle);
                        onFinish?.Invoke(req.assetBundle);
                    }
                }
            }
        }

        public void UnloadBundle(string path) {
            string bundleName = MD5Calculater.GetTextMD5(AssetBundleDefine.RES_ROOT + path);
            if(m_abDic.ContainsKey(bundleName)) {
                m_abDic[bundleName].Unload(false);
                m_abDic.Remove(bundleName);
            }
        }

        public void UnloadAllBundle(bool isUnloadMainBundle = false) {
            if(isUnloadMainBundle) {
                m_mainAB.Unload(false);
                m_mainAB = null;
                m_manifest = null;
            }
            foreach(var key in m_abDic.Keys) {
                m_abDic[key].Unload(false);
            }
            m_abDic.Clear();
        }
        
        public string GetResName(string path) {
            int index = path.LastIndexOf('/');
            return path.Substring(index+1, path.Length - index - 1);
        }

        private void LoadResAsync<T>(string path, Action<T> onFinish) where T : Object {
            string resName = GetResName(path);
            string bundleName = MD5Calculater.GetTextMD5(AssetBundleDefine.RES_ROOT + path);
            string[] dependencies = m_manifest.GetAllDependencies(bundleName);
            LoadBundle(bundleName, bundle => {
                // m_abDic.Add(path, bundle);
                StartCoroutine(GetDependenciesBundles(dependencies, () => {
                    // Log.Info("load res success.", "res name", resName, "res type", typeof(T));
                    onFinish?.Invoke(bundle.LoadAsset<T>(resName));
                }));
            });
        }

        private IEnumerator GetDependenciesBundles(string[] dependencies, Action onFinish) {
            int count = dependencies.Length;
            for(int i = 0; i < dependencies.Length; i++) {
                StartCoroutine(IELoadBundle(dependencies[i], bundle => {
                    count--;
                }));
            }
            while(count != 0) {
                yield return null;
            }
            onFinish?.Invoke();
        }

        public void LoadPrefabAsync(string path, Action<GameObject> onFinish) {
            LoadResAsync<GameObject>(path, onFinish);
        }

        public void LoadTexture2DAsync(string path, Action<Texture2D> onFinish) {
            LoadResAsync<Texture2D>(path, onFinish);
        }
        
        public void LoadTextureAsync(string path, Action<Texture> onFinish) {
            LoadResAsync<Texture>(path, onFinish);
        }

        public void LoadFontAsync(string path, Action<Font> onFinish) {
            LoadResAsync<Font>(path, onFinish);
        }

        public void LoadAudioClipAsync(string path, Action<AudioClip> onFinish) {
            LoadResAsync<AudioClip>(path, onFinish);
        }

        public void LoadTextAssetAsync(string path, Action<TextAsset> onFinish) {
            LoadResAsync(path, onFinish);
        }

        public void LoadSpriteAtlasAsync(string path, Action<SpriteAtlas> onFinish) {
            LoadResAsync<SpriteAtlas>(path, onFinish);
        }
        
    }
    
}