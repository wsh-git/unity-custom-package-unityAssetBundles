using UnityEditor;

namespace Wsh.AssetBundles.Editor {
    
    public class AssetBundleEditorHelper {
        
        public static BuildAssetBundleOptions GetAssetBundleOptions(CompressOptionsType compressOptionsType) {
            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            switch(compressOptionsType) {
                case CompressOptionsType.Uncompressed:
                    opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
                    break;
                case CompressOptionsType.ChunkBasedCompression:
                    opt |= BuildAssetBundleOptions.ChunkBasedCompression;
                    break;
            }
            return opt;
        }

        public static BuildTarget GetBuildTarget(BuildTargetType buildTargetType) {
            if(buildTargetType == BuildTargetType.Android) {
                return BuildTarget.Android;
            } else if(buildTargetType == BuildTargetType.iOS) {
                return BuildTarget.iOS;
            } else if(buildTargetType == BuildTargetType.Webgl) {
                return BuildTarget.WebGL;
            }
            return BuildTarget.StandaloneWindows64;
        }
        
    }
}