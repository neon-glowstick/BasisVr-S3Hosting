using System.IO;
using UnityEngine;

namespace org.BasisVr.Contrib.S3Hosting
{
    public record S3Config
    {
        public string AccessKey;
        public string SecretKey;
        public string ServiceUrl;
        public string AvatarBucket;

        private const string ConfigFile = "BasisVrS3UploadConfig.json";
        private static string ConfigDirectory => Application.persistentDataPath;

        public static S3Config Load()
        {
            var path = Path.Combine(ConfigDirectory, ConfigFile);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var config = JsonUtility.FromJson<S3Config>(json);
                return config;
            }

            return new S3Config();
        }

        public static void Save(S3Config config)
        {
            var directory = ConfigDirectory;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var path = Path.Combine(directory, ConfigFile);
            var json = JsonUtility.ToJson(config);
            File.WriteAllText(path, json);
        }

        public static void Delete()
        {
            var path = Path.Combine(ConfigDirectory, ConfigFile);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
