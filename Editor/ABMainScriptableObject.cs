using UnityEngine;

namespace Wsh.AssetBundles.Editor {
    
    public class ABMainScriptableObject : ScriptableObject {
        public string ResRootDir = "";
        public string ABOutputDir = "";
        public string Version = "1.0.0";
        public PlatformType BuildTarget = PlatformType.PC;
        public bool IsClearOutputDir = true;
        public bool IsCopyAssetStreaming = true;
        public CompressOptionsType CompressOptionsType = CompressOptionsType.Uncompressed;
    }
    
}