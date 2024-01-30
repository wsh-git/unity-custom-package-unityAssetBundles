using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Wsh.AssetBundles {
    
    public class MD5Calculater {
        public static string GetTextMD5(string text) {
            using(MD5 md5 = MD5.Create()) {
                byte[] inputBytes = Encoding.UTF8.GetBytes(text);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder stringBuilder = new StringBuilder();
                for(int i = 0; i < hashBytes.Length; i++) {
                    stringBuilder.Append(hashBytes[i].ToString("x2")); // 转换为十六进制，并保留两位
                }
                return stringBuilder.ToString();
            }
        }

        public static string GetFileMD5(string filePath) {
            using(FileStream fileStream = new FileStream(filePath, FileMode.Open)) {
                MD5 md5 = new MD5CryptoServiceProvider();
                // 得到 md5 16个字节的数组
                byte[] byteCode = md5.ComputeHash(fileStream);
                fileStream.Close();
                StringBuilder sb = new StringBuilder();
                // 把16个字节数组转成16进制字符串，减少长度
                for(int i = 0; i < byteCode.Length; i++) {
                    sb.Append(byteCode[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}