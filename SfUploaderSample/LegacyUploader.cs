using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace SfUploaderSample
{
    public class LegacyUploader
    {
        /// <summary>
        /// Uploads a File using the Standard upload method with a multipart/form mime encoded POST.
        /// </summary>
        /// <param name="hostname">the accounthost name</param>
        /// <param name="parentId">where to upload the file</param>
        /// <param name="localPath">the full path of the file to upload, like "c:\\path\\to\\file.name"</param>
        public void UploadFile(string hostname, string parentId, string localPath)
        {
            String uri = string.Format("https://{0}/sf/v3/Shares({1})/Upload2", hostname, parentId);
            Console.WriteLine("Starting upload with legacy code");

            string jsonParameters;

            using (var file = File.Open(localPath, FileMode.OpenOrCreate))
            {
                var jsonObject = new
                {
                    FileName = file.Name,
                    FileLength = file.Length
                };

                jsonParameters = JsonConvert.SerializeObject(jsonObject);
            }

            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.Method = "POST";
            request.ContentType = "application/json";

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(jsonParameters);
                writer.Flush();
                writer.Close();
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                String body = reader.ReadToEnd();

                JObject uploadConfig = JObject.Parse(body);
                string chunkUri = (string)uploadConfig["ChunkUri"];
                if (chunkUri != null)
                {
                    UploadMultiPartFile("File1", new FileInfo(localPath), chunkUri);
                }
            }
        }

        /// <summary>
        /// Does a multipart form post upload of a file to a url.
        /// </summary>
        /// <param name="parameterName">multipart parameter name. File1 for a standard upload.</param>
        /// <param name="file">the FileInfo to upload</param>
        /// <param name="uploadUrl">the url of the server to upload to</param>
        private void UploadMultiPartFile(string parameterName, FileInfo file, string uploadUrl)
        {
            string boundaryGuid = "upload-" + Guid.NewGuid().ToString("n");
            string contentType = "multipart/form-data; boundary=" + boundaryGuid;

            MemoryStream ms = new MemoryStream();
            byte[] boundaryBytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundaryGuid + "\r\n");

            // Write MIME header
            ms.Write(boundaryBytes, 2, boundaryBytes.Length - 2);
            string header = String.Format(@"Content-Disposition: form-data; name=""{0}""; filename=""{1}""" +
                "\r\nContent-Type: application/octet-stream\r\n\r\n", parameterName, file.Name);
            byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
            ms.Write(headerBytes, 0, headerBytes.Length);

            // Load the file into the byte array
            using (FileStream source = file.OpenRead())
            {
                byte[] buffer = new byte[1024 * 1024];
                int bytesRead;

                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }
            }

            // Write MIME footer
            boundaryBytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundaryGuid + "--\r\n");
            ms.Write(boundaryBytes, 0, boundaryBytes.Length);

            byte[] postBytes = ms.ToArray();
            ms.Close();

            HttpWebRequest request = WebRequest.CreateHttp(uploadUrl);
            request.Timeout = 1000 * 60; // 60 seconds
            request.Method = "POST";
            request.ContentType = contentType;
            request.ContentLength = postBytes.Length;
            request.Credentials = CredentialCache.DefaultCredentials;

            using (Stream postStream = request.GetRequestStream())
            {
                int chunkSize = 48 * 1024;
                int remaining = postBytes.Length;
                int offset = 0;

                do
                {
                    if (chunkSize > remaining) { chunkSize = remaining; }
                    postStream.Write(postBytes, offset, chunkSize);

                    remaining -= chunkSize;
                    offset += chunkSize;

                } while (remaining > 0);

                postStream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Console.WriteLine("Legacy Upload completed");
            response.Close();
        }
    }
}

