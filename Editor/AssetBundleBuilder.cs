﻿using System.Collections.Generic;
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

        private static string GetMd5Name(string fileName) {
            return MD5Calculater.GetTextMD5(fileName);
        }

        private static string ResourceAssetBundlePath {
            get {
                return Path.Combine(Application.dataPath, "Resources", AssetBundleDefine.RESOURCES_PATH);
            }
        }

        private static void AddExtensionForAssetBundles() {
            string resPath = ResourceAssetBundlePath;
            DirectoryInfo directoryInfo = new DirectoryInfo(resPath);
            var files = directoryInfo.GetFiles();
            for(int i = 0; i < files.Length; i++) {
                //Log.Info(files[i].FullName, files[i].Name, files[i].Extension);
                if(string.IsNullOrEmpty(files[i].Extension)) {
                    string oldFileName = files[i].FullName;
                    string newFileName = Path.Combine(resPath, files[i].Name + AssetBundleDefine.RESOURCES_EXTENSION);
                    //Log.Info(oldFileName, newFileName);
                    File.Move(oldFileName, newFileName);
                }
            }
        }

        public static void BuildAssetBundles(string resRootDir, string outputDir, PlatformType buildTarget, AssetBundleLoadType loadType,
            bool isClearOutputDir, bool isCopyAssetStreaming, bool isCopyResources, CompressOptionsType compressOption, int version, string[] blackDirList) {
            if(!Directory.Exists(resRootDir)) {
                Log.Error("res root directory not exists.", resRootDir);
                return;
            }
            if(!Directory.Exists(outputDir)) {
                Log.Error("output directory not exists.", outputDir);
                return;
            }
            bool isEncrypt = buildTarget != PlatformType.Webgl && loadType != AssetBundleLoadType.Resources;
            List<ABDependenciesInfo> dependenciesInfos = new List<ABDependenciesInfo>();
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
                string resPath = ResourceAssetBundlePath;
                if(!Directory.Exists(resPath)) {
                    Directory.CreateDirectory(resPath);
                }
                ClearDirectory(resPath);
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
                    string assetBundleName = GetMd5Name(fileInAsstsPathWithoutExtension);
                    // Log.Info(filePaths[i], fileInAsstsPath, fileInAsstsPathWithoutExtension, filePathInRes, assetBundleName);
                    ABDependenciesInfo dependenciesInfo = new ABDependenciesInfo("Assets/" + fileInAsstsPath);
                    // 获取资源及其依赖的资源
                    string[] dependencies = GetDependencies(fileInAsstsPath, blackDirList);
                    // Log.Info("-", dependencies.Length);
                    for(int j = 0; j < dependencies.Length; j++) {
                        string fileInAsstsPathWithoutExt = dependencies[j].Substring(0, dependencies[j].LastIndexOf('.'));
                        int depIndex = fileInAsstsPathWithoutExt.IndexOf('/');
                        fileInAsstsPathWithoutExt = fileInAsstsPathWithoutExt.Substring(depIndex + 1,
                            fileInAsstsPathWithoutExt.Length - depIndex - 1);

                        string dependencyAssetBundleName = GetMd5Name(fileInAsstsPathWithoutExt);
                        // Log.Info("--", dependencies[j], fileInAsstsPathWithoutExt, dependencyAssetBundleName);
                        dependenciesInfo.Add(dependencies[j]);
                        if(!m_dicAssetsBundles.ContainsKey(dependencyAssetBundleName)) {
                            AssetImporter.GetAtPath(dependencies[j]).SetAssetBundleNameAndVariant(dependencyAssetBundleName, "");
                            m_dicAssetsBundles.Add(dependencyAssetBundleName, dependencies[j]);
                        }
                    }
                    if(!m_dicAssetsBundles.ContainsKey(assetBundleName)) {
                        AssetImporter.GetAtPath("Assets/" + fileInAsstsPath).SetAssetBundleNameAndVariant(assetBundleName, "");
                        m_dicAssetsBundles.Add(assetBundleName, "Assets/" + fileInAsstsPath);
                    }
                    dependenciesInfo.Sort();
                    dependenciesInfos.Add(dependenciesInfo);
                }
            }
            if(!Directory.Exists(bundleOutputPath)) {
                Directory.CreateDirectory(bundleOutputPath);
            }
            CreateVersionFile(bundleOutputPath, version);
            BuildPipeline.BuildAssetBundles(bundleOutputPath, AssetBundleEditorHelper.GetAssetBundleOptions(compressOption), AssetBundleEditorHelper.GetBuildTarget(buildTarget));
            RevertAllFilesBundleName();
            if(isEncrypt) {
                EncryptAssetBundles(bundleOutputPath);
            }
            Dictionary<string, long> assetBundleSizeDic = new Dictionary<string, long>();
            CreateAssetBundleCompareFile(bundleOutputPath, ref assetBundleSizeDic);
            CreateOutputResNameList(m_dicAssetsBundles, dependenciesInfos, assetBundleSizeDic, Path.Combine(bundleOutputPath, "ResNameList.lt"));
            if(isCopyAssetStreaming) {
                //string targetPath = Path.Combine(streamingAssetsPath, buildTarget.ToString());
                CopyDirectory(bundleOutputPath, Application.streamingAssetsPath, new string[] { ".manifest", ".lt" });
            }
            if(isCopyResources) {
                CopyDirectory(bundleOutputPath, ResourceAssetBundlePath, new string[] { ".manifest", ".lt" });
                AddExtensionForAssetBundles();
            }
            AssetDatabase.Refresh();
            Log.Info("Build AssetBundles Success.");
        }

        private static void CreateVersionFile(string bundleOutputPath, int version) {
            string filePath = Path.Combine(bundleOutputPath, "Version.txt");
            File.WriteAllText(filePath, version.ToString());
        }

        private static void RevertAllFilesBundleName() {
            foreach(var key in m_dicAssetsBundles.Keys) {
                AssetImporter.GetAtPath(m_dicAssetsBundles[key]).SetAssetBundleNameAndVariant("", "");
            }
        }

        private static string[] GetDependencies(string fileInAsstsPath, string[] blackDirList) {
            List<string> list = new List<string>();
            string fileInAssets = "Assets/" + fileInAsstsPath;
            string[] dependencies = AssetDatabase.GetDependencies(fileInAssets, true);
            for(int i = 0; i < dependencies.Length; i++) {
                if(!dependencies[i].EndsWith(".meta") && !dependencies[i].EndsWith(".cs") && !Directory.Exists(dependencies[i]) && dependencies[i] != fileInAssets
                    && !IsBlackDir(dependencies[i], blackDirList)) {
                    list.Add(dependencies[i]);
                }
            }
            return list.ToArray();
        }

        private static bool IsBlackDir(string filePath, string[] blackDirList) {
            for(int i = 0; i < blackDirList.Length; i++) {
                if(filePath.Contains(blackDirList[i])) {
                    return true;
                }
            }
            return false;
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

        private static bool IsAssetBundle(FileInfo fileInfo) {
            return fileInfo.Extension == "";
        }

        private static string GetAssetName(string fileFullName) {
            int startIndex = fileFullName.LastIndexOf('\\') + 1;
            return fileFullName.Substring(startIndex, fileFullName.Length - startIndex);
        }

        private static void EncryptAB(string filePath, string fileName) {
            byte[] fileData = File.ReadAllBytes(filePath);
            int offset = AssetBundleUtils.GetBundleOffset(fileName, AssetBundleDefine.ASSET_BUNDLE_OFFSET);
            int fileLen = (offset + fileData.Length);
            byte[] buffer = new byte[fileLen];
            for(int slen = 0; slen < offset; slen++) {
                buffer[slen] = 1;
            }
            for(int slen = 0; slen < fileData.Length; slen++) {
                buffer[slen + offset] = fileData[slen];
            }
            FileStream fs = File.OpenWrite(filePath);
            fs.Write(buffer, 0, fileLen);
            fs.Close();
        }

        private static void EncryptAssetBundles(string bundleOutputPath) {
            DirectoryInfo directoryInfo = Directory.CreateDirectory(bundleOutputPath);
            FileInfo[] files = directoryInfo.GetFiles();
            for(int i = 0; i < files.Length; i++) {
                if(IsAssetBundle(files[i])) {
                    EncryptAB(files[i].FullName, files[i].Name);
                }
            }
        }

        private static void CreateAssetBundleCompareFile(string bundleOutputPath, ref Dictionary<string, long> dic) {
            DirectoryInfo directoryInfo = Directory.CreateDirectory(bundleOutputPath);
            FileInfo[] files = directoryInfo.GetFiles();
            StringBuilder sb = new StringBuilder();
            bool isError = false;
            for(int i = 0; i < files.Length; i++) {
                if(files[i].FullName.Contains(AssetBundleDefine.ASSET_BUNDLE_COMPARE_INFO_SLIP_CHAR)) {
                    Log.Error(files[i].FullName, "contain blank space");
                    isError = true;
                    break;
                }
                if(IsAssetBundle(files[i])) {
                    string assetName = GetAssetName(files[i].FullName);
                    sb.Append(assetName);
                    sb.Append(AssetBundleDefine.ASSET_BUNDLE_COMPARE_INFO_SLIP_CHAR);
                    sb.Append(files[i].Length);
                    dic.Add(assetName, files[i].Length);
                    sb.Append(AssetBundleDefine.ASSET_BUNDLE_COMPARE_INFO_SLIP_CHAR);
                    sb.Append(MD5Calculater.GetFileMD5(files[i].FullName));
                    sb.Append(AssetBundleDefine.ASSET_BUNDLE_COMPARE_FILE_SLIP_CHAR);
                    // Log.Info("Name", files[i].Name, "FullName", files[i].FullName, "Extension", files[i].Extension, "Length", files[i].Length);
                }
            }
            if(isError) {
                return;
            }
            string str = sb.ToString();
            if(str.EndsWith(AssetBundleDefine.ASSET_BUNDLE_COMPARE_FILE_SLIP_CHAR)) {
                str = str.Substring(0, str.Length - AssetBundleDefine.ASSET_BUNDLE_COMPARE_FILE_SLIP_CHAR.Length);
            }
            string filePath = Path.Combine(bundleOutputPath, AssetBundleDefine.ASSET_BUNDLE_COMPARE_FILE_NAME);
            File.WriteAllText(filePath, str);
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
                foreach(string subdirectory in subdirectories) {
                    string subdirectoryName = Path.GetFileName(subdirectory);
                    string destinationSubdirectory = Path.Combine(targetPath, subdirectoryName);
                    CopyDirectory(subdirectory, destinationSubdirectory, ignoreList);
                }
            } catch(IOException e) {
                Log.Error("Copy AssetBundles To AssetStreaming error", e.Message);
            }

        }

        private static void CreateOutputResNameList(Dictionary<string, string> dicAssetsBundles, List<ABDependenciesInfo> dependencies, Dictionary<string, long> assetBundleSizeDic, string fileName) {
            StringBuilder sb = new StringBuilder();
            int space = 16;
            dependencies.Sort((v1, v2) => {
                return v1.Name.CompareTo(v2.Name);
            });

            sb.Append("Dependencies:\n");
            for(int i = 0; i < dependencies.Count; i++) {
                sb.Append("    " + i.ToString() + ". " + dependencies[i].ToString());
            }
            sb.Append('\n');
            Dictionary<string, string> dicTemp = new Dictionary<string, string>();
            foreach(var key in dicAssetsBundles.Keys) {
                if(dicTemp.ContainsKey(dicAssetsBundles[key])) {
                    Log.Error("Has same name res", dicAssetsBundles[key]);
                } else {
                    dicTemp.Add(dicAssetsBundles[key], key);
                }
            }
            sb.Append("Res md5 list:\n");
            List<string> resNameList = dicTemp.Keys.ToList();
            resNameList.Sort();
            for(int i = 0; i < resNameList.Count; i++) {
                string resName = resNameList[i];
                string md5Name = dicTemp[resName];
                sb.Append("    " + md5Name);
                sb.Append("    ");
                long size = assetBundleSizeDic[md5Name];
                string sizeStr = (size/1024f).ToString("F2") + "KB";
                sb.Append(sizeStr);
                if(sizeStr.Length < space) {
                    for(int j = 0; j < (space - sizeStr.Length); j++) {
                        sb.Append(" ");
                    }
                } else {
                    sb.Append("        ");
                }
                sb.Append("    ");
                sb.Append(resName);
                sb.Append('\n');
            }
            File.WriteAllText(fileName, sb.ToString());
        }

    }

}