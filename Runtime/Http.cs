using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Wsh.Net {
    public class Http {
        public static async Task<bool> CheckOriginalDirAsync(string url) {
            using(HttpClient client = new HttpClient()) {
                HttpResponseMessage response = await client.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
        }

        public static async Task<bool> CreateServerDirectoryAsync(string url) {
            using(HttpClient client = new HttpClient()) {
                var request = new HttpRequestMessage(HttpMethod.Put, url);

                HttpResponseMessage response = await client.SendAsync(request);
                Log.Info(response.StatusCode);
                return response.IsSuccessStatusCode;
            }
        }

        public static async Task UploadFilesAsync(string url, string dir) {
            string[] files = Directory.GetFiles(dir);

            foreach(string filePath in files) {
                await UploadFileAsync(filePath, url);
            }
        }

        /*static async Task UploadFileAsync(string filePath, string serverUrl) {
            using(HttpClient client = new HttpClient()) {
                using(FileStream fileStream = File.OpenRead(filePath))
                using(StreamContent fileContent = new StreamContent(fileStream))
                using(MultipartFormDataContent formData = new MultipartFormDataContent()) {
                    formData.Add(fileContent, "file", Path.GetFileName(filePath));
                    HttpResponseMessage response = await client.PostAsync(serverUrl, formData);
                    if(response.IsSuccessStatusCode) {
                        Log.Info($"File {Path.GetFileName(filePath)} uploaded successfully.");
                    }
                    else {
                        Log.Info($"Failed to upload file {Path.GetFileName(filePath)}", response.StatusCode);
                    }
                }
            }
        }*/
        
        static async Task UploadFileAsync(string filePath, string serverUrl)
        {
            using (HttpClient client = new HttpClient())
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                var content = new StreamContent(fileStream);
                HttpResponseMessage response = await client.PutAsync(serverUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Log.Info($"File {Path.GetFileName(filePath)} uploaded successfully.");
                }
                else
                {
                    Log.Info($"Failed to upload file {Path.GetFileName(filePath)}", response.StatusCode);
                }
            }
        }

    }
}