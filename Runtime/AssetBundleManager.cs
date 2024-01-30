using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AB = UnityEngine.AssetBundle;

namespace Wsh.AssetBundles {
    
    public class AssetBundleManager : MonoBehaviour {

        private const string RES_ROOT = "Res/";
        
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
                AssetBundleCreateRequest req = AB.LoadFromFileAsync(abPath);
                yield return req;
                if(req.isDone) {
                    m_abDic.Add(bundleName, req.assetBundle);
                    onFinish?.Invoke(req.assetBundle);
                }
            }
        }
        
        public void GetPrefab(string path, Action<GameObject> onFinish) {
            int index = path.LastIndexOf('/');
            string prefabName = path.Substring(index+1, path.Length - index - 1);
            Log.Info("GetPrefab", prefabName);
            string bundleName = MD5Calculater.GetTextMD5(RES_ROOT + path);
            string[] dependencies = m_manifest.GetAllDependencies(bundleName);
            LoadBundle(bundleName, bundle => {
                m_abDic.Add(path, bundle);
                StartCoroutine(GetDependenciesBundles(dependencies, () => {
                    onFinish?.Invoke(bundle.LoadAsset<GameObject>(prefabName));
                }));
            });
        }

        private IEnumerator GetDependenciesBundles(string[] dependencies, Action onFinish) {
            int count = dependencies.Length;
            for(int i = 0; i < dependencies.Length; i++) {
                string name = dependencies[i];
                StartCoroutine(IELoadBundle(dependencies[i], bundle => {
                    count--;
                }));
            }
            while(count != 0) {
                yield return null;
            }
            onFinish?.Invoke();
        }
        
    }
    
}