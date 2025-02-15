using UnityEditor;
using UnityEngine.UIElements;

namespace org.BasisVr.Contrib.S3Hosting
{
    [InitializeOnLoad]
    public static class AvatarUploaderGui
    {
        static AvatarUploaderGui()
        {
            // We're extending the custom editor of the avatar inspector
            BasisAvatarSDKInspector.InspectorGuiCreated += OnInspectorGuiCreated;
        }

        private static void OnInspectorGuiCreated(BasisAvatarSDKInspector inspector)
        {
            inspector.rootElement.Add(BuildGui());
        }

        private static VisualElement BuildGui()
        {
            var container = new Foldout
            {
                text = "S3 Uploader"
            };
            var configFoldout = new Foldout
            {
                text = "Config"
            };
            container.Add(configFoldout);
            var crudButtons = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };
            configFoldout.Add(crudButtons);

            var config = S3Config.Load();
            var accessKeyField = PasswordField("Access key", config.AccessKey);
            accessKeyField.RegisterValueChangedCallback(value => config.AccessKey = value.newValue);
            configFoldout.Add(accessKeyField);

            var secretKeyField = PasswordField("Secret key", config.SecretKey);
            secretKeyField.RegisterValueChangedCallback(value => config.SecretKey = value.newValue);
            configFoldout.Add(secretKeyField);

            var serviceUrlField = Textfield("ServiceUrl", config.ServiceUrl);
            serviceUrlField.RegisterValueChangedCallback(value => config.ServiceUrl = value.newValue);
            configFoldout.Add(serviceUrlField);

            var bucketNameField = Textfield("Bucket name", config.AvatarBucket);
            bucketNameField.RegisterValueChangedCallback(value => config.AvatarBucket = value.newValue);
            configFoldout.Add(bucketNameField);

            var uploadButton = new Button
            {
                text = "Upload to bucket"
            };
            uploadButton.clicked += () =>
            {
                uploadButton.SetEnabled(false);
                AvatarUploader.Upload(config);
                uploadButton.SetEnabled(true);
            };
            container.Add(uploadButton);

            var saveButton = new Button(() => S3Config.Save(config))
            {
                text = "Save",
                style = { flexGrow = -1f }
            };
            crudButtons.Add(saveButton);

            var deleteButton = new Button(S3Config.Delete)
            {
                text = "Delete",
                style = { flexGrow = -1f }
            };
            crudButtons.Add(deleteButton);

            var loadButton = new Button(() =>
            {
                config = S3Config.Load();
                secretKeyField.SetValueWithoutNotify(config.SecretKey);
                accessKeyField.SetValueWithoutNotify(config.AccessKey);
                serviceUrlField.SetValueWithoutNotify(config.ServiceUrl);
                bucketNameField.SetValueWithoutNotify(config.AvatarBucket);
            })
            {
                text = "Load",
                style = { flexGrow = -1f }
            };
            crudButtons.Add(loadButton);

            return container;
        }

        private static TextField PasswordField(string label, string value)
        {
            return new TextField(128, false, true, '•')
            {
                label = label,
                value = value,
            };
        }

        private static TextField Textfield(string label, string value)
        {
            return new TextField
            {
                multiline = false,
                label = label,
                value = value
            };
        }
    }
}
