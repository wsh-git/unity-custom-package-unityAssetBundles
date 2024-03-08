﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;
using AB = UnityEngine.AssetBundle;
using Object = UnityEngine.Object;
using Wsh.Singleton;

namespace Wsh.AssetBundles {
    
    public class AssetBundleManager : Singleton<AssetBundleManager>, ISingleton {
        
        private AB m_mainAB;
        private AssetBundleManifest m_manifest;
        private Dictionary<string, AB> m_abDic;
        private Dictionary<string, int> m_abDependCountDic;

        public void OnInit() {
            m_abDic = new Dictionary<string, AB>();
            m_abDependCountDic = new Dictionary<string, int>();
        }
        
        public void OnDeinit() {
            UnloadAllBundle(true);
        }
        
        public void InitAsync(Action onFinish) {
            LoadBundle(PlatformUtils.Platform, ab => {
                m_mainAB = ab;
                m_manifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                onFinish?.Invoke();
            });
        }
        
        private string GetAssetBundlePath(string bundleName, bool isWeb) {
            if(isWeb) {
                return Path.Combine(PlatformUtils.StreamingAssetsPathWithStream, bundleName);
            }
            string localPersistentDataPath = Path.Combine(PlatformUtils.PersistentDataPathWithStream, bundleName);
            if(File.Exists(localPersistentDataPath)) {
                return Path.Combine(PlatformUtils.PersistentDataPathWithStream, bundleName);
            } else {
                return Path.Combine(PlatformUtils.StreamingAssetsPathWithStream, bundleName);
            }
        }
        
        private string GetAssetBundleName(string path){
            return MD5Calculater.GetTextMD5(AssetBundleDefine.RES_ROOT + path);
        }
        
        private void LoadBundle(string bundleName, Action<AB> onFinish) {
            StartCoroutine(IELoadBundle(bundleName, onFinish));
        }
        
        private IEnumerator IELoadBundle(string bundleName, Action<AB> onFinish) {
            if(m_abDic.ContainsKey(bundleName)) {
                TryAddAssetBundleDependCount(bundleName);
                onFinish?.Invoke(m_abDic[bundleName]);
            } else {
                bool isWeb = false;
#if UNITY_EDITOR
                isWeb = false;
#elif UNITY_WEBGL
                isWeb = true;
#else
                isWeb = false;
#endif
                string abPath = GetAssetBundlePath(bundleName, isWeb);
                if(isWeb) {
                    UnityWebRequest webReq = UnityWebRequestAssetBundle.GetAssetBundle(abPath);
                    yield return webReq.SendWebRequest();
                    if(webReq.isDone) {
                        if(webReq.result != UnityWebRequest.Result.Success) {
                            Log.Error("Error downloading AssetBundle:", webReq.error);
                        } else {
                            AB ab = DownloadHandlerAssetBundle.GetContent(webReq);
                            AddBundle(bundleName, ab);
                            onFinish?.Invoke(ab);
                        }
                    }
                } else {
                    int offset = AssetBundleUtils.GetBundleOffset(bundleName, AssetBundleDefine.ASSET_BUNDLE_OFFSET);
                    AssetBundleCreateRequest req = AB.LoadFromFileAsync(abPath, 0, (ulong)offset);
                    yield return req;
                    if(req.isDone) {
                        AddBundle(bundleName, req.assetBundle);
                        onFinish?.Invoke(req.assetBundle);
                    }
                }
            }
        }

        private void AddBundle(string bundleName, AB assetBundle) {
            m_abDic.Add(bundleName, assetBundle);
            TryAddAssetBundleDependCount(bundleName);
        }

        private void TryAddAssetBundleDependCount(string bundleName) {
            if(!m_abDependCountDic.ContainsKey(bundleName)) {
                m_abDependCountDic.Add(bundleName, 0);
            }
            m_abDependCountDic[bundleName]++;
        }

        private void TryRemoveAssetBundleDependCount(string bundleName) {
            if(m_abDependCountDic.ContainsKey(bundleName) && m_abDependCountDic[bundleName] > 0) {
                m_abDependCountDic[bundleName]--;
            }
        }

        private int GetAssetBundleDependCount(string bundleName) {
            if(m_abDependCountDic.ContainsKey(bundleName)) {
                return m_abDependCountDic[bundleName];
            }
            return 0;
        }
        
        public void UnloadBundle(string path) {
            string bundleName = GetAssetBundleName(path);
            TryUnloadBundle(bundleName);
            string[] dependencies = m_manifest.GetAllDependencies(bundleName);
            for(int i = 0; i < dependencies.Length; i++) {
                TryUnloadBundle(dependencies[i]);
            }
        }

        private void TryUnloadBundle(string bundleName) {
            TryRemoveAssetBundleDependCount(bundleName);
            if(GetAssetBundleDependCount(bundleName) == 0) {
                if(m_abDic.ContainsKey(bundleName)) {
                    m_abDic[bundleName].Unload(false);
                    m_abDic.Remove(bundleName);
                }
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
            m_abDependCountDic.Clear();
        }
        
        public string GetResName(string path) {
            int index = path.LastIndexOf('/');
            return path.Substring(index+1, path.Length - index - 1);
        }

        private void LoadResAsync<T>(string path, Action<T> onFinish) where T : Object {
            string resName = GetResName(path);
            string bundleName = GetAssetBundleName(path);
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