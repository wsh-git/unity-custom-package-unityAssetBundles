using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Wsh.Net {
    
    public class Ftp {
        
        public static bool DownloadFile(string downloadUrl, string localFileName, string ftpServerAccount, string ftpServerPassword) {
            try {
                // 远端服务器如果没用只当的目录结构会出错的，这里最好通过命令检测同时直接创建需要的目录
                Uri uri = new Uri(downloadUrl);
                Log.Info("Start download url:", uri.ToString());
                FtpWebRequest req = FtpWebRequest.Create(uri) as FtpWebRequest;
                // 创建Ftp服务器账号通信凭证，正式项目中不能存在匿名账号
                NetworkCredential networkCredential = new NetworkCredential(ftpServerAccount, ftpServerPassword);
                req.Credentials = networkCredential;
                // 设置代理为null;
                req.Proxy = null;
                // 请求完毕后，是否关闭控制连接
                req.KeepAlive = false;
                // 设置操作命令 - 下载
                req.Method = WebRequestMethods.Ftp.DownloadFile;
                // 指定使用二进制传送
                req.UseBinary = true;
                // 获取下载的数据流
                FtpWebResponse res = req.GetResponse() as FtpWebResponse;
                Stream downloadStream = res.GetResponseStream();
                using (FileStream file = File.Create(localFileName)) {
                    // 一点一点的下载内容，2k
                    byte[] bytes = new byte[2048];
                    // 返回值表示读取了多少内容
                    int contentLength = downloadStream.Read(bytes, 0, bytes.Length);
                    while (contentLength != 0) {
                        file.Write(bytes, 0, contentLength);
                        contentLength = downloadStream.Read(bytes, 0, bytes.Length);
                    }
                    file.Close();
                    downloadStream.Close();
                    Log.Info("Download file success. fileName", localFileName);
                    return true;
                }
            } catch (Exception e) {
                Log.Error("Start download file error.", e.Message, "fileName", localFileName);
                return false;
            }
        }

        /*public static async Task CheckOriginalDir(string uploadUrl, string originalDir, string ftpServerAccount,
            string ftpServerPassword, Action onFinish) {
            Log.Info("start Check original dir");
            bool directoryExists = await DirectoryExistsAsync(uploadUrl, originalDir, ftpServerAccount, ftpServerPassword, onFinish);
            Log.Info("start Check original dir ---");
            if (directoryExists) {
                Log.Info("Remote directory exists.");
            } else {
                Log.Info("Remote directory does not exist.");
            }
            Log.Info("onCheck original dir");
        }

        public static async Task<bool> DirectoryExistsAsync(string uploadUrl, string originalDir, string ftpServerAccount, string ftpServerPassword, Action onFinish) {
            try {
                Log.Info("start DirectoryExistsAsync");
                // FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{uploadUrl}{originalDir}");
                FtpWebRequest request = FtpWebRequest.Create(uploadUrl) as FtpWebRequest;
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(ftpServerAccount, ftpServerPassword);
                Log.Info("start DirectoryExistsAsync 1");
                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync()) {
                    // If the directory exists, the request will succeed.
                    Log.Info("start DirectoryExistsAsync 3");
                    return true;
                }
            } catch(WebException ex) {
                Log.Info(ex.Message);
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                
                Log.Error(response.ToString(), response.StatusCode);
                // Check if the error is because the directory does not exist.
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable) {
                    return false; // Directory does not exist.
                }

                return false;
                throw; // Other exception, rethrow.
            }
        }*/
        
        public static void UploadFile(string uploadUrl, string localFilePath, string ftpServerAccount, string ftpServerPassword) {
            try {
                // 远端服务器如果没用只当的目录结构会出错的，这里最好通过命令检测同时直接创建需要的目录,这个url一定要带上对用的文件名；
                Uri uri = new Uri(uploadUrl + "/PC");
                Log.Info("Start upload url:", uri.ToString());
                FtpWebRequest req = FtpWebRequest.Create(uri) as FtpWebRequest;

                //FtpWebRequest req = (FtpWebRequest)WebRequest.Create(uploadUrl + "/PC.txt");

                req.UsePassive = false;
                // 创建Ftp服务器账号通信凭证
                NetworkCredential networkCredential = new NetworkCredential(ftpServerAccount, ftpServerPassword);
                req.Credentials = networkCredential;
                // 设置代理为null;
                req.Proxy = null;
                // 请求完毕后，是否关闭控制连接
                req.KeepAlive = false;
                // 设置操作命令 - 上传
                req.Method = WebRequestMethods.Ftp.UploadFile;
                // 指定使用二进制传送
                req.UseBinary = true;
                // 获取上传的数据流
                Stream uploadStream = req.GetRequestStream();
                using(FileStream file = File.OpenRead(localFilePath)) {
                    // 一点一点的上传内容，2k
                    byte[] bytes = new byte[2048];
                    // 返回值表示读取了多少内容
                    int contentLength = file.Read(bytes, 0, bytes.Length);
                    while(contentLength != 0) {
                        uploadStream.Write(bytes, 0, contentLength);
                        contentLength = file.Read(bytes, 0, bytes.Length);
                    }

                    file.Close();
                    uploadStream.Close();
                }
            } catch(Exception e) {
                Log.Error(e.Message);
            }
            
        }
        
    }    
}

