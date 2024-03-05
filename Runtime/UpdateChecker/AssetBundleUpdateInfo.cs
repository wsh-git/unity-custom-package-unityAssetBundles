using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Wsh.AssetBundles {
    
    public class AssetBundleUpdateInfo {

        public int Version => m_version;
        public AssetBundleUpdateStatus Status => m_status;
        public ulong TotalSize => m_totalSize;
        public List<string> WaitDownloadList => m_waitDownloadList;

        private int m_version;
        private string m_persistentDataPath;
        private AssetBundleUpdateStatus m_status;
        private string m_compareText;
        private ulong m_totalSize;
        // 待下载的ab包列表文件，存储ab包的名字
        private List<string> m_waitDownloadList = new List<string>();
        
        public AssetBundleUpdateInfo() {
            m_status = AssetBundleUpdateStatus.Next;
            m_version = 0;
            m_persistentDataPath = PlatformUtils.PersistentDataPathWithStream;
        }

        public void SetVersion(string text) {
            try {
                m_version = int.Parse(text);
            } catch(Exception e) {
                Log.Error("int parse error. text", text, e.Message);
            }
        }

        public void SetVersion(int version) {
            m_version = version;
        }
        
        public void SetStatus(AssetBundleUpdateStatus status) {
            m_status = status;
        }

        public void SetNewCompareText(string text) {
            m_compareText = text;
        }

        public void SetWaitDownloadList(ulong totalSize, List<string> list) {
            m_totalSize = totalSize;
            m_waitDownloadList = list;
        }

        public async void SaveLocalFile(Action onFinish) {
            await Task.Run(() => {
                if(Version != 0) {
                    string localVersionFilePath = Path.Combine(m_persistentDataPath, AssetBundleDefine.ASSET_BUNDLE_VERSION_FILE_NAME); 
                    string localCompareFilePath = Path.Combine(m_persistentDataPath, AssetBundleDefine.ASSET_BUNDLE_COMPARE_FILE_NAME);
                    File.WriteAllText(localVersionFilePath, m_version.ToString());
                    File.WriteAllText(localCompareFilePath, m_compareText);
                }
                onFinish?.Invoke();
            });
        }
        
    }
    
}