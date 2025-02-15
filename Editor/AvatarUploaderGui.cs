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

        private static S3Config _config = new();

        private static Button _uploadButton;
        private static TextField _accessKeyField;
        private static TextField _secretKeyField;
        private static TextField _serviceUrlField;
        private static TextField _bucketNameField;

        private static void OnInspectorGuiCreated(BasisAvatarSDKInspector inspector)
        {
            inspector.rootElement.Add(BuildGui(_config));
        }

        private static VisualElement BuildGui(S3Config config)
        {
            var container = new Foldout
            {
                text = "S3 Uploader"
            };

            container.Add(AddConfigFoldout(config));

            _uploadButton = new Button(() =>
            {
                _uploadButton.SetEnabled(false);
                AvatarUploader.Upload(_config);
                _uploadButton.SetEnabled(true);
            })
            {
                text = "Upload to bucket"
            };
            container.Add(_uploadButton);

            return container;
        }

        private static VisualElement AddConfigFoldout(S3Config config)
        {
            var container = new Foldout
            {
                text = "Config"
            };

            var crudButtons = ConfigCrudButtons();
            container.Add(crudButtons);

            _accessKeyField = PasswordField("Access key", config.AccessKey);
            _accessKeyField.RegisterValueChangedCallback(value => config.AccessKey = value.newValue);
            container.Add(_accessKeyField);

            _secretKeyField = PasswordField("Secret key", config.SecretKey);
            _secretKeyField.RegisterValueChangedCallback(value => config.SecretKey = value.newValue);
            container.Add(_secretKeyField);

            _serviceUrlField = Textfield("ServiceUrl", config.ServiceUrl);
            _serviceUrlField.RegisterValueChangedCallback(value => config.ServiceUrl = value.newValue);
            container.Add(_serviceUrlField);

            _bucketNameField = Textfield("Bucket name", config.AvatarBucket);
            _bucketNameField.RegisterValueChangedCallback(value => config.AvatarBucket = value.newValue);
            container.Add(_bucketNameField);

            return container;
        }

        private static VisualElement ConfigCrudButtons()
        {
            var crudButtons = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };

            var loadButton = new Button(() =>
            {
                _config = S3Config.Load();
                _secretKeyField.SetValueWithoutNotify(_config.SecretKey);
                _accessKeyField.SetValueWithoutNotify(_config.AccessKey);
                _serviceUrlField.SetValueWithoutNotify(_config.ServiceUrl);
                _bucketNameField.SetValueWithoutNotify(_config.AvatarBucket);
            })
            {
                text = "Load",
                style = { flexGrow = -1f }
            };
            crudButtons.Add(loadButton);
            var saveButton = new Button(() => S3Config.Save(_config))
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
            return crudButtons;
        }

        private static TextField PasswordField(string label, string value)
        {
            return new TextField(128, false, true, '•')
            {
                label = label,
                value = value
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
