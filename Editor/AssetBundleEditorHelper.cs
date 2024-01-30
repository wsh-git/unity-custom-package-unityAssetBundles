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

        public static BuildTarget GetBuildTarget(PlatformType buildTargetType) {
            if(buildTargetType == PlatformType.Android) {
                return BuildTarget.Android;
            } else if(buildTargetType == PlatformType.iOS) {
                return BuildTarget.iOS;
            } else if(buildTargetType == PlatformType.Webgl) {
                return BuildTarget.WebGL;
            }
            return BuildTarget.StandaloneWindows64;
        }
        
    }
}