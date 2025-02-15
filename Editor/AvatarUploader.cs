using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using UnityEditor;
using UnityEngine;

namespace org.BasisVr.Contrib.S3Hosting
{
    public static class AvatarUploader
    {
        public static async void Upload(S3Config config)
        {
            try
            {
                var assetBundleDirectory = Path.Combine(Environment.CurrentDirectory, "AssetBundles");
                if (!TryGetFilesToUpload(assetBundleDirectory, out var assetBundlePath, out var metaFilePath))
                {
                    Debug.LogError($"Could not find avatar to upload in :{assetBundleDirectory}");
                    return;
                }

                var credentials = new BasicAWSCredentials(config.AccessKey, config.SecretKey);
                using var client = new AmazonS3Client(credentials, new AmazonS3Config
                {
                    ServiceURL = config.ServiceUrl,
                    RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                    ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
                });

                await UploadFile(client, config.AvatarBucket, assetBundlePath, Application.exitCancellationToken);
                await UploadFile(client, config.AvatarBucket, metaFilePath, Application.exitCancellationToken);
            }
            catch (Exception e)
            {
                Debug.LogError($"Upload failed: {e.Message}");
            }
        }

        private static async Task UploadFile(IAmazonS3 s3Client, string bucketName, string filePath, CancellationToken cancellationToken)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var request = new PutObjectRequest
            {
                FilePath = filePath,
                BucketName = bucketName,
                DisablePayloadSigning = true
            };

            var title = "Upload";
            var infoText = $"Uploading to bucket {bucketName}\n{filePath}";
            try
            {
                request.StreamTransferProgress += (sender, args) =>
                {
                    var cancelled =
                        EditorUtility.DisplayCancelableProgressBar(title, infoText, args.PercentDone / 100f);
                    if (cancelled)
                    {
                        cts.Cancel();
                    }
                };

                var response = await s3Client.PutObjectAsync(request, cts.Token);
                if (response.HttpStatusCode is >= HttpStatusCode.OK and < HttpStatusCode.Ambiguous)
                {
                    Debug.Log($"Upload completed with status code {response.HttpStatusCode} for {filePath}");
                }
                else
                {
                    Debug.LogError($"Upload failed with status code {response.HttpStatusCode} for {filePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Upload failed {e.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static bool TryGetFilesToUpload(string directory, out string assetBundlePath, out string metaFilePath)
        {
            assetBundlePath = string.Empty;
            metaFilePath = string.Empty;

            // Make some assumptions, at the time of writing:
            // The files are placed in the AssetBundles folder at the root of the Unity project
            // There can only be 1 avatar built at a time. Building a new avatar replaces the files in AssetBundles
            if (!Directory.Exists(directory))
                return false;

            var files = Directory.GetFiles(directory);
            assetBundlePath = files.FirstOrDefault(f => f.EndsWith("BasisEncyptedBundle"));
            metaFilePath = files.FirstOrDefault(f => f.EndsWith("BasisEncyptedMeta"));

            var foundBoth = !string.IsNullOrEmpty(assetBundlePath) && !string.IsNullOrEmpty(metaFilePath);
            return foundBoth;
        }
    }
}
