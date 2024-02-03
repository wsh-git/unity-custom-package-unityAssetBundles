using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Wsh.AssetBundles {
    
    public class AssetBundleUtils {
        
        public static int GetBundleOffset(string md5, int defaultOffset = 0) {
            if(string.IsNullOrEmpty(md5) || md5.Length == 0) {
                return defaultOffset;
            }
            try {
                int number = int.Parse(md5[0].ToString());
                return number;
            } catch( FormatException) {
                return defaultOffset;
            }
        }

        public static Dictionary<string, AssetBundleInfo> GetCompareFileDic(string txtInfo) {
            Dictionary<string, AssetBundleInfo> dic = new Dictionary<string, AssetBundleInfo>();
            // 获取已下载的对比文件
            string[] abs = txtInfo.Split(AssetBundleDefine.ASSET_BUNDLE_COMPARE_FILE_SLIP_CHAR);
            string[] abInfo = null;
            for(int i = 0; i < abs.Length; i++) {
                abInfo = abs[i].Split(AssetBundleDefine.ASSET_BUNDLE_COMPARE_INFO_SLIP_CHAR);
                AssetBundleInfo ab = new AssetBundleInfo(abInfo[0], abInfo[1], abInfo[2]);
                // 记录每一个远端ab包的信息之后，好做对比信息；
                dic.Add(ab.Name, ab);
            }
            return dic;
        }

        public static IEnumerator ReqRemoteFile(string remotesFileUrl, Action<bool, string> onFinish) {
            UnityWebRequest request = UnityWebRequest.Get(remotesFileUrl);
            yield return request.SendWebRequest();
            if(!request.isDone || request.result != UnityWebRequest.Result.Success) {
                Log.Error("req remote file failed.", remotesFileUrl, request.result, request.error);
                onFinish?.Invoke(false, request.error);
            } else {
                onFinish?.Invoke(true, request.downloadHandler.text);
            }
        }

        public static IEnumerator GetLocalFile(string filePath, Action<bool, string> onFinish) {
            string requestUrl = PlatformUtils.FILE_PREFIX + filePath;
            UnityWebRequest request = UnityWebRequest.Get(requestUrl);
            yield return request.SendWebRequest();
            if(request.result == UnityWebRequest.Result.Success) {
                onFinish?.Invoke(true, request.downloadHandler.text);
            } else {
                Log.Error("get local file failed.", requestUrl, request.result, request.error);
                onFinish?.Invoke(false, request.error);
            }
        }
    }
    
}