using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wsh.AssetBundles.Editor {
    
    public class AssetBundleMainWindow : EditorWindow {

        private const int FIRST_SPACE = 15;
        private const string EMPTY_STRING = "<None>";

        private readonly static Dictionary<CompressOptionsType, GUIContent> COMPRESSIONOPTIONS_CONTENT = new Dictionary<CompressOptionsType, GUIContent>() {
            {CompressOptionsType.Uncompressed, new GUIContent("No Compression")},
            {CompressOptionsType.StandardCompression, new GUIContent("LZMA")},
            {CompressOptionsType.ChunkBasedCompression, new GUIContent("LZ4")},
        };

        private readonly static string[] LOCK_TYPE_DES = { "锁定", "解锁"};
        
        public string ResRootDir { get { return m_resRootDir; } }
        public string OutputDir { get { return m_outputPath; } }
        public string Version { get { return m_version; } }
        public PlatformType BuildTarget { get { return m_buildTargetType; } }
        public bool IsClearOutputDir { get { return m_isClearOutputDir; } }
        public bool IsCopyAssetStreaming { get { return m_isCopyAssetStreaming; } }
        public CompressOptionsType CompressOptionsType { get { return m_compressOptionsType; }}

        // config data
        private string m_resRootDir;
        private PlatformType m_buildTargetType;
        private string m_outputPath;
        private string m_version;
        private bool m_isClearOutputDir;
        private bool m_isCopyAssetStreaming;
        private CompressOptionsType m_compressOptionsType;
        private bool m_isInited;
        private ABScriptableObjectLoader m_scriptableObjLoader;
        
        // gui
        private int m_selectLockTypeIndex;
        private GUIContent m_targetContent;
        private GUIContent m_isClearOutputDirContent;
        private GUIContent m_isCopyStreamingContent;

        private static AssetBundleMainWindow m_instance = null;

        public static AssetBundleMainWindow Instance {
            get {
                if(m_instance == null) {
                    m_instance = GetWindow<AssetBundleMainWindow>();
                }
                return m_instance;
            }
        }
        
        [MenuItem("AssetBundleTool/Builder #%t")]
        private static void ShowWindow() {
            m_instance = null;
            Instance.titleContent = new GUIContent("AssetBundles");
            Instance.Show();
        }

        private void InitMainWindowData() {
            var data = m_scriptableObjLoader.LoadABMainScriptableObject();
            if(data != null) {
                m_resRootDir = data.ResRootDir;
                m_buildTargetType = data.BuildTarget;
                m_outputPath = data.ABOutputDir;
                m_isClearOutputDir = data.IsClearOutputDir;
                m_isCopyAssetStreaming = data.IsCopyAssetStreaming;
                m_compressOptionsType = data.CompressOptionsType;
                m_version = data.Version;
                m_isInited = true;
            }
        }
        
        private void OnEnable() {
            m_scriptableObjLoader = new ABScriptableObjectLoader();
            m_scriptableObjLoader.CheckScriptableObject();
            m_targetContent = new GUIContent("Build Target", "Choose target platform to build for.");
            m_isClearOutputDirContent = new GUIContent("Is Clear Output Dir", "clear output dir or not.");
            m_isCopyStreamingContent = new GUIContent("Is Copy To AssetStreaming", "");
            InitMainWindowData();
        }

        private void OnDisable() {
            m_instance = null;
        }

        private void OnGUI() {
            if(!m_isInited) { return; }

            GUILayout.Space(10);
            m_selectLockTypeIndex = GUI.Toolbar(new Rect(Screen.width - 155, 20, 150, 25), m_selectLockTypeIndex, LOCK_TYPE_DES);
            GUILayout.Space(35);
            GUI.enabled = m_selectLockTypeIndex != 0;
            GUILayout.BeginVertical();
            
            GUILayout.Space(10);
            
            #region ResRootDir
            GUILayout.BeginHorizontal();
            GUILayout.Space(FIRST_SPACE);
            GUILayout.Label("ResRootDir:");
            if(string.IsNullOrEmpty(m_resRootDir)) {
                GUILayout.Label(EMPTY_STRING);
            } else {
                GUILayout.Label(m_resRootDir);
            }
            if(GUILayout.Button("浏览")) {
                string path = EditorUtility.OpenFolderPanel("选择项目资源路径", Application.dataPath, "");
                if(path != null) {
                    m_resRootDir = path;
                }
            }
            GUILayout.EndHorizontal();
            #endregion
            
            GUILayout.Space(10);
            
            #region OutputDir
            GUILayout.BeginHorizontal();
            GUILayout.Space(FIRST_SPACE);
            GUILayout.Label("OutputDir:");
            if(string.IsNullOrEmpty(m_outputPath)) {
                GUILayout.Label(EMPTY_STRING);
            } else {
                GUILayout.Label(m_outputPath);
            }
            if(GUILayout.Button("浏览")) {
                string path = EditorUtility.OpenFolderPanel("选择AssetBundle导出路径", Application.dataPath, "");
                if(path != null) {
                    m_outputPath = path;
                }
            }
            GUILayout.EndHorizontal();
            #endregion
            
            GUILayout.Space(10);
            
            #region Version
            GUILayout.BeginHorizontal();
            GUILayout.Space(FIRST_SPACE);
            GUILayout.Label("资源版本号:");
            m_version = EditorGUILayout.TextField(m_version);
            GUILayout.EndHorizontal();
            #endregion
            
            GUILayout.Space(10);
            
            #region BuildTarget
            GUILayout.BeginHorizontal();
            GUILayout.Space(FIRST_SPACE);
            m_buildTargetType = (PlatformType)EditorGUILayout.EnumPopup(m_targetContent, m_buildTargetType);
            GUILayout.EndHorizontal();
            #endregion
            
            GUILayout.Space(10);
            
            #region IS Clear Output Dir
            GUILayout.BeginHorizontal();
            GUILayout.Space(FIRST_SPACE);
            m_isClearOutputDir = EditorGUILayout.ToggleLeft(m_isClearOutputDirContent, m_isClearOutputDir);
            GUILayout.EndHorizontal();
            #endregion
            
            GUILayout.Space(10);
            
            #region IS Copy AssetStreaming
            GUILayout.BeginHorizontal();
            GUILayout.Space(FIRST_SPACE);
            m_isCopyAssetStreaming = EditorGUILayout.ToggleLeft(m_isCopyStreamingContent, m_isCopyAssetStreaming);
            GUILayout.EndHorizontal();
            #endregion
            
            GUILayout.Space(10);
            
            #region CompressOptions Type
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(FIRST_SPACE);
            GUILayout.Label("压缩类型");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(FIRST_SPACE * 2);
            m_compressOptionsType = (CompressOptionsType)EditorGUILayout.EnumPopup(COMPRESSIONOPTIONS_CONTENT[m_compressOptionsType], m_compressOptionsType);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion
            
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Save Config", GUILayout.Height(30))) {
                m_scriptableObjLoader.SaveScriptableObject(this);
            }
            GUILayout.EndHorizontal();
            
            GUI.enabled = true;
            
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Build AssetBundle", GUILayout.Height(30))) {
                AssetBundleBuilder.BuildAssetBundles(ResRootDir, OutputDir, BuildTarget, IsClearOutputDir, IsCopyAssetStreaming, CompressOptionsType, Version);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        
    }
    
}