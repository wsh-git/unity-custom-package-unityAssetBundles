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
        public int Version { get { return m_version; } }
        public string BlackDirList { get {return m_blackDirList;} }
        public PlatformType BuildTarget { get { return m_buildTargetType; } }
        public bool IsClearOutputDir { get { return m_isClearOutputDir; } }
        public bool IsCopyAssetStreaming { get { return m_isCopyAssetStreaming; } }
        public bool IsCopyResources { get { return m_isCopyResources;} }
        public AssetBundleLoadType LoadType { get { return m_loadType;} }
        public CompressOptionsType CompressOptionsType { get { return m_compressOptionsType; }}
        public string UploadDir { get { return m_uploadDir; } }
        public string ServerIp { get { return m_serverIp; } }
        // 远程目录文件夹
        public string OriginalDir { get { return m_originalDir; } }
        public PlatformType UploadTargetType { get { return m_uploadTargetType; } }
        public string Account { get { return m_account; } }
        public string Password { get { return m_password; } }

        // config data
        private string m_resRootDir;
        private PlatformType m_buildTargetType;
        private string m_outputPath;
        private int m_version;
        private string m_blackDirList;
        private bool m_isClearOutputDir;
        private bool m_isCopyAssetStreaming;
        private CompressOptionsType m_compressOptionsType;
        private string m_uploadDir;
        private string m_serverIp;
        private string m_originalDir;
        private PlatformType m_uploadTargetType;
        private string m_account;
        private string m_password;
        private bool m_isCopyResources;
        private AssetBundleLoadType m_loadType;
        private bool m_isInited;
        private ABScriptableObjectLoader m_scriptableObjLoader;
        
        // gui
        private bool m_isUploadFoldout;
        private int m_selectLockTypeIndex;
        private GUIContent m_targetContent;
        private GUIContent m_isClearOutputDirContent;
        private GUIContent m_isCopyStreamingContent;
        private GUIContent m_uploadTargetContent;
        private GUIContent m_loadTypeContent;
        private GUIContent m_isCopyResourcesContent;

        private static AssetBundleMainWindow m_instance = null;

        public static AssetBundleMainWindow Instance {
            get {
                if(m_instance == null) {
                    m_instance = GetWindow<AssetBundleMainWindow>();
                }
                return m_instance;
            }
        }
        
        public static void ShowWindow() {
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
                m_isCopyResources = data.IsCopyResources;
                m_loadType = data.LoadType;
                m_compressOptionsType = data.CompressOptionsType;
                m_uploadDir = data.UploadDir;
                m_serverIp = data.ServerIp;
                m_originalDir = data.OriginalDir;
                m_uploadTargetType = data.UploadTargetType;
                m_account = data.Account;
                m_password = data.Password;
                m_version = data.Version;
                m_blackDirList = data.BlackDirList;
                m_isInited = true;
            }
        }

        private string[] GetBlackDirList() {
            List<string> list = new List<string>();
            var l = m_blackDirList.Split(';');
            for(int i = 0; i < l.Length; i++) {
                if(!string.IsNullOrEmpty(l[i])) {
                    list.Add(l[i]);
                }
            }
            return list.ToArray();
        }
        
        private void OnEnable() {
            m_scriptableObjLoader = new ABScriptableObjectLoader();
            m_scriptableObjLoader.CheckScriptableObject();
            m_targetContent = new GUIContent("Build Target", "Choose target platform to build for.");
            m_loadTypeContent = new GUIContent("Load Type", "StreamingAssets, Resources");
            m_uploadTargetContent = new GUIContent("Target Platform", "");
            m_isClearOutputDirContent = new GUIContent("Is Clear Output Dir", "clear output dir or not.");
            m_isCopyStreamingContent = new GUIContent("Is Copy To AssetStreaming", "");
            m_isCopyResourcesContent = new GUIContent("Is Copy To Resources", "");
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
            GUILayout.Label("ResRootDir:", GUILayout.Width(100));
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
            GUILayout.Label("OutputDir:", GUILayout.Width(100));
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
            GUILayout.Label("资源版本号:", GUILayout.Width(100));
            m_version = EditorGUILayout.IntField(m_version);
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

            #region LoadType
            GUILayout.BeginHorizontal();
            GUILayout.Space(FIRST_SPACE);
            m_loadType = (AssetBundleLoadType)EditorGUILayout.EnumPopup(m_loadTypeContent, m_loadType);
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
            
            #region IS Copy Resources
            GUILayout.BeginHorizontal();
            GUILayout.Space(FIRST_SPACE);
            m_isCopyResources = EditorGUILayout.ToggleLeft(m_isCopyResourcesContent, m_isCopyResources);
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

            #region Server IP
                GUILayout.BeginHorizontal();
                GUILayout.Space(FIRST_SPACE);
                GUILayout.Label("忽略的目录(多个目录用‘;’分隔开)", GUILayout.Width(200));
                m_blackDirList = GUILayout.TextField(m_blackDirList, GUILayout.Width(500));
                GUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region Save Config
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Save Config", GUILayout.Height(30))) {
                m_scriptableObjLoader.SaveScriptableObject(this);
            }
            GUILayout.EndHorizontal();
            #endregion

            GUI.enabled = true;
            
            GUILayout.Space(20);

            #region Build AssetBundle
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Build AssetBundle", GUILayout.Height(30))) {
                AssetBundleBuilder.BuildAssetBundles(ResRootDir, OutputDir, BuildTarget, LoadType, IsClearOutputDir, IsCopyAssetStreaming, IsCopyResources, CompressOptionsType, Version, GetBlackDirList());
            }
            GUILayout.EndHorizontal();
            #endregion
            
            GUILayout.Space(20);
            
            m_isUploadFoldout = EditorGUILayout.Foldout(m_isUploadFoldout, "AssetBundles Uploader");
            if(m_isUploadFoldout) {
                // GUILayout.PasswordField("");
                GUI.enabled = m_selectLockTypeIndex != 0;
                GUILayout.Space(10);
            
                #region Upload Dir
                GUILayout.BeginHorizontal();
                GUILayout.Space(FIRST_SPACE);
                GUILayout.Label("Upload Dir:", GUILayout.Width(100));
                if(string.IsNullOrEmpty(m_uploadDir)) {
                    GUILayout.Label(EMPTY_STRING);
                } else {
                    GUILayout.Label(m_uploadDir);
                }
                if(GUILayout.Button("浏览")) {
                    string path = EditorUtility.OpenFolderPanel("选择上传资源的本地路径", Application.dataPath, "");
                    if(path != null) {
                        m_uploadDir = path;
                    }
                }
                GUILayout.EndHorizontal();
                #endregion
            
                GUILayout.Space(10);

                #region Server IP
                GUILayout.BeginHorizontal();
                GUILayout.Space(FIRST_SPACE);
                GUILayout.Label("ServerIP", GUILayout.Width(100));
                m_serverIp = GUILayout.TextField(m_serverIp, GUILayout.Width(200));
                GUILayout.Label("OriginalDir", GUILayout.Width(100));
                m_originalDir = GUILayout.TextField(m_originalDir, GUILayout.Width(200));
                GUILayout.EndHorizontal();
                #endregion
                
                GUILayout.Space(10);
                
                #region Upload Target Type
                GUILayout.BeginHorizontal();
                GUILayout.Space(FIRST_SPACE);
                m_uploadTargetType = (PlatformType)EditorGUILayout.EnumPopup(m_uploadTargetContent, m_uploadTargetType);
                GUILayout.EndHorizontal();
                #endregion
                
                GUILayout.Space(10);
                
                #region Account && Password
                GUILayout.BeginHorizontal();
                GUILayout.Space(FIRST_SPACE);
                GUILayout.Label("Account", GUILayout.Width(100));
                m_account = GUILayout.TextField(m_account, GUILayout.Width(200));
                GUILayout.Label("Password", GUILayout.Width(100));
                m_password = GUILayout.TextField(m_password, GUILayout.Width(200));
                GUILayout.EndHorizontal();
                #endregion

                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Save Config", GUILayout.Height(30))) {
                    m_scriptableObjLoader.SaveScriptableObject(this);
                }
                GUILayout.EndHorizontal();
                GUI.enabled = true;
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Upload", GUILayout.Height(30))) {
                    AssetBundleUploader.Upload(m_uploadDir, m_serverIp, m_originalDir, m_uploadTargetType, m_account, m_password);
                }
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndVertical();
        }
        
    }
    
}