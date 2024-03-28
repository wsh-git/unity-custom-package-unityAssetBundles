using UnityEngine;

namespace Wsh.AssetBundles.Editor {
    
    public class ABMainScriptableObject : ScriptableObject {
        public string ResRootDir = "";
        public string ABOutputDir = "";
        public int Version = 0;
        public string BlackDirList = "";
        public PlatformType BuildTarget = PlatformType.PC;
        public AssetBundleLoadType LoadType;
        public bool IsCopyResources = false;
        public bool IsClearOutputDir = true;
        public bool IsCopyAssetStreaming = true;
        public CompressOptionsType CompressOptionsType = CompressOptionsType.Uncompressed;
        public string UploadDir = "";
        public string ServerIp = "";
        public string OriginalDir = "";
        public PlatformType UploadTargetType = PlatformType.PC;
        public string Account = "";
        public string Password = "";
    }
    
}