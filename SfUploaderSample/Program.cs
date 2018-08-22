using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace SfUploaderSample
{
    class Program
    {
        static void Main(string[] args)
        {
            string hostName = "eliezeren.sf-api.com"; //replace subdomain to test different account
            string shareId = "rec227cfaf7e4abda";
            string filePath = "a file path here";

            var legacyUploader = new LegacyUploader();
            var sdkUploader = new SdkUploader();

            legacyUploader.UploadFile(hostName, shareId, filePath);
            sdkUploader.UploadFile(hostName, shareId, filePath);
        }
    }
}
