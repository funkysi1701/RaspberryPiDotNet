using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Camera;

namespace RaspberryPiDotNet
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var config = Config.GetIConfiguration(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            var settings = new CameraStillSettings
            {
                CaptureWidth = 640,
                CaptureHeight = 480,
                CaptureJpegQuality = 90,
                CaptureTimeoutMilliseconds = 300,
                HorizontalFlip = true,
                VerticalFlip = true
            };
            string filename = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + ".jpg";
            var pictureBytes = Pi.Camera.CaptureImage(settings);
            var targetPath = config.GetSection("LocalPath").Value + filename;
            if (File.Exists(targetPath))
                File.Delete(targetPath);

            File.WriteAllBytes(targetPath, pictureBytes);
            CloudSave(filename, string.Empty, config.GetSection("LocalPath").Value, config.GetSection("Storage").Value, "Dev").GetAwaiter().GetResult();
        }

        public static async Task CloudSave(string FileName, string cloudPath, string sourcepath, string Storage, string Env)
        {
            var cloudBlobContainer = await GetCloudContainer(Storage, Env);
            string imageName = Path.Combine(cloudPath, FileName);

            BlobClient cloudBlockBlob = cloudBlobContainer.GetBlobClient(imageName);

            await cloudBlockBlob.UploadAsync(Path.Combine(sourcepath, FileName), true);
        }

        private static async Task<BlobContainerClient> GetCloudContainer(string Storage, string Env)
        {
            BlobContainerClient cloudBlobContainer = new(Storage, Env.ToLower());
            if (!cloudBlobContainer.Exists())
            {
                await cloudBlobContainer.SetAccessPolicyAsync(accessType: PublicAccessType.None);
            }

            return cloudBlobContainer;
        }
    }
}
