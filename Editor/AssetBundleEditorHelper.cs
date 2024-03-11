using UnityEditor;
using UnityEngine;

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

        public static void GetDependencies(GameObject obj) {
            if(obj != null) {
                string objPath = AssetDatabase.GetAssetPath(obj);
                string[] dependencies = AssetDatabase.GetDependencies(objPath);
                int index = 0;
                for(int i = 0; i < dependencies.Length; i++) {
                    if(!dependencies[i].EndsWith(".cs") && dependencies[i] != objPath) {
                        index++;
                        Log.Info(index, dependencies[i]);
                    }
                }
            }
        }
        
    }
}