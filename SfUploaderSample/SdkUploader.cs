using ShareFile.Api.Client;
using ShareFile.Api.Client.Transfers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SfUploaderSample
{
    public class SdkUploader
    {
        /// <summary>
        /// Upload File using the Sharefile C# SDK
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="parentId"></param>
        /// <param name="localPath"></param>
        public void UploadFile(string hostname, string parentId, string localPath)
        {
            Console.WriteLine("Starting Upload with SDK");
            var baseUri = string.Format("https://{0}/sf/v3", hostname);
            var sfClient = new ShareFileClient(baseUri);

            var shareUri = sfClient.Shares.GetEntityUriFromId(parentId);

            using (var file = File.Open(localPath, FileMode.OpenOrCreate))
            {
                var uploadRequest = new UploadSpecificationRequest
                {
                    FileName = Path.GetFileName(localPath),
                    FileSize = file.Length,
                    Parent = shareUri
                };

                var uploader = sfClient.GetFileUploader(uploadRequest, file);
                uploader.Upload();
            }
            Console.WriteLine("SDK Upload completed");
        }
    }
}
