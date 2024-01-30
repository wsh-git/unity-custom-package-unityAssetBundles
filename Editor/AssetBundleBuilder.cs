using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Wsh.AssetBundles.Editor {

    public class AssetBundleBuilder {

        private static Dictionary<string, string> m_dicAssetsBundles;

        private static void InitDic() {
            if(m_dicAssetsBundles == null) {
                m_dicAssetsBundles = new Dictionary<string, string>();
            }
            m_dicAssetsBundles.Clear();
        }
        
        public static void BuildAssetBundles(string resRootDir, string outputDir, BuildTargetType buildTarget,
            bool isClearOutputDir, bool isCopyAssetStreaming, CompressOptionsType compressOption, string version) {
            if(!Directory.Exists(resRootDir)) {
                Log.Error("res root directory not exists.", resRootDir);
                return;
            }
            if(!Directory.Exists(outputDir)) {
                Log.Error("output directory not exists.", outputDir);
                return;
            }

            InitDic();
            string bundleOutputPath = Path.Combine(outputDir, buildTarget.ToString());
            if(isClearOutputDir) {
                if(!Directory.Exists(bundleOutputPath)) {
                    Directory.CreateDirectory(bundleOutputPath);
                }
                ClearDirectory(bundleOutputPath);
                string streamingAssetsPath = Application.streamingAssetsPath;
                if(!Directory.Exists(streamingAssetsPath)) {
                    Directory.CreateDirectory(streamingAssetsPath);
                }
                ClearDirectory(streamingAssetsPath); 
            }

            string[] filePaths = Directory.GetFiles(resRootDir, "*.*", SearchOption.AllDirectories);
            
            string dataPath = Application.dataPath;
            int subStartIndex = dataPath.Length + 1;
            for(int i = 0; i < filePaths.Length; i++) {
                if(!filePaths[i].EndsWith(".meta") && !Directory.Exists(filePaths[i])) {
                    string fullPath = filePaths[i].Replace('\\', '/');
                    string fileInAsstsPath = GetPathInAssets(fullPath, subStartIndex);
                    string fileName = Path.GetFileNameWithoutExtension(fileInAsstsPath);
                    string fileInAsstsPathWithoutExtension = fileInAsstsPath.Substring(0, fileInAsstsPath.LastIndexOf('.'));
                    int index = fileInAsstsPathWithoutExtension.IndexOf('/');
                    string filePathInRes = fileInAsstsPathWithoutExtension.Substring(index + 1,
                        fileInAsstsPathWithoutExtension.Length - index - 1);
                    string assetBundleName = MD5Calculater.GetTextMD5(filePathInRes);
                    if(m_dicAssetsBundles.ContainsKey(assetBundleName)) {
                        continue;
                    }
                    
                    Log.Info(filePaths[i], fileInAsstsPath, fileInAsstsPathWithoutExtension, filePathInRes, assetBundleName);
                    // 获取资源及其依赖的资源
                    string[] dependencies = GetDependencies(fileInAsstsPath);
                    // Log.Info("-", dependencies.Length);
                    for(int j = 0; j < dependencies.Length; j++) {
                        string fileInAsstsPathWithoutExt = dependencies[j].Substring(0, dependencies[j].LastIndexOf('.'));
                        string dependencyAssetBundleName = MD5Calculater.GetTextMD5(fileInAsstsPathWithoutExt);
                        Log.Info("--", dependencies[j], dependencyAssetBundleName);
                        if(!m_dicAssetsBundles.ContainsKey(dependencyAssetBundleName)) {
                            AssetImporter.GetAtPath(dependencies[j]).SetAssetBundleNameAndVariant(dependencyAssetBundleName, "");
                            m_dicAssetsBundles.Add(dependencyAssetBundleName, dependencies[j]);
                        }
                    }
                    AssetImporter.GetAtPath("Assets/" + fileInAsstsPath).SetAssetBundleNameAndVariant(assetBundleName, "");
                    m_dicAssetsBundles.Add(assetBundleName, "Assets/" + fileInAsstsPath);
                }
            }
            if(!Directory.Exists(bundleOutputPath)) {
                Directory.CreateDirectory(bundleOutputPath);
            }
            CreateVersionFile(bundleOutputPath, version);
            BuildPipeline.BuildAssetBundles(bundleOutputPath, AssetBundleEditorHelper.GetAssetBundleOptions(compressOption), AssetBundleEditorHelper.GetBuildTarget(buildTarget));
            RevertAllFilesBundleName();
            CreateOutputResNameList(m_dicAssetsBundles, Path.Combine(bundleOutputPath, "ResNameList.lt"));
            if(isCopyAssetStreaming) {
                //string targetPath = Path.Combine(streamingAssetsPath, buildTarget.ToString());
                CopyDirectory(bundleOutputPath, Application.streamingAssetsPath, new string[]{".manifest", ".lt"});
                AssetDatabase.Refresh();
            }
        }

        private static void CreateVersionFile(string bundleOutputPath, string version) {
            string filePath = Path.Combine(bundleOutputPath, "Version.txt");
            File.WriteAllText(filePath, version);
        }
        
        private static void RevertAllFilesBundleName() {
            foreach(var key in m_dicAssetsBundles.Keys) {
                AssetImporter.GetAtPath(m_dicAssetsBundles[key]).SetAssetBundleNameAndVariant("", "");
            }
        }
        
        private static string[] GetDependencies(string fileInAsstsPath) {
            List<string> list = new List<string>();
            string fileInAssets = "Assets/" + fileInAsstsPath;
            string[] dependencies = AssetDatabase.GetDependencies(fileInAssets, true);
            for(int i = 0; i < dependencies.Length; i++) {
                if(!dependencies[i].EndsWith(".meta") && !Directory.Exists(dependencies[i]) && dependencies[i] != fileInAssets) {
                    list.Add(dependencies[i]);
                }
            }
            return list.ToArray();
        }
        
        private static string GetPathInAssets(string filePath, int subStartIndex) {
            string path = filePath.Substring(subStartIndex);
            return path;
        }

        private static void ClearDirectory(string directoryPath) {
            try {
                string[] files = Directory.GetFiles(directoryPath);
                foreach(string file in files) {
                    File.Delete(file);
                }
                string[] subDirs = Directory.GetDirectories(directoryPath);
                foreach(string subDir in subDirs) {
                    ClearDirectory(subDir);
                }
                // Directory.Delete(directoryPath);
            } catch(IOException e) {
                Log.Error("Clear Directory error.", e.Message);
            }
        }

        private static bool IsFit(string file, string[] ignoreList) {
            if(ignoreList != null && ignoreList.Length > 0) {
                for(int i = 0; i < ignoreList.Length; i++) {
                    if(file.EndsWith(ignoreList[i])) {
                        return true;
                    }
                }
            }
            return false;
        }
        
        private static void CopyDirectory(string bundleOutputPath, string targetPath, string[] ignoreList) {
            try {
                if(!Directory.Exists(targetPath)) {
                    Directory.CreateDirectory(targetPath);
                }
                string[] files = Directory.GetFiles(bundleOutputPath);
                // 复制所有文件到目标目录
                foreach(string file in files) {
                    string fileName = Path.GetFileName(file);
                    if(!IsFit(file, ignoreList)) {
                        string destinationPath = Path.Combine(targetPath, fileName);
                        File.Copy(file, destinationPath);
                    }
                }
                
                // 获取源目录下的所有子目录
                string[] subdirectories = Directory.GetDirectories(bundleOutputPath);
                // 递归复制所有子目录
                foreach (string subdirectory in subdirectories) {
                    string subdirectoryName = Path.GetFileName(subdirectory);
                    string destinationSubdirectory = Path.Combine(targetPath, subdirectoryName);
                    CopyDirectory(subdirectory, destinationSubdirectory, ignoreList);
                }
            } catch(IOException e) {
                Log.Error("Copy AssetBundles To AssetStreaming error", e.Message);
            }

        }
        
        private static void CreateOutputResNameList(Dictionary<string, string> dicAssetsBundles, string fileName) {
            StringBuilder sb = new StringBuilder();
            int space = 50;
            Dictionary<string, string> dicTemp = new Dictionary<string, string>();
            foreach(var key in dicAssetsBundles.Keys) {
                if(dicTemp.ContainsKey(dicAssetsBundles[key])) {
                    Log.Error("Has same name res", dicAssetsBundles[key]);
                } else {
                    dicTemp.Add(dicAssetsBundles[key], key);
                }
            }
            List<string> resNameList = dicTemp.Keys.ToList();
            resNameList.Sort();
            for(int i = 0; i < resNameList.Count; i++) {
                sb.Append(resNameList[i]);
                if(resNameList[i].Length < space) {
                    for(int j = 0; j < (space - resNameList[i].Length); j++) {
                        sb.Append(" ");
                    }
                } else {
                    sb.Append("        ");
                }
                sb.Append(dicTemp[resNameList[i]]);
                sb.Append('\n');
            }
            File.WriteAllText(fileName, sb.ToString());
        }
        
    }
    
}