#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Playroom
{
    public static class PlayroomWebGLTools
    {
        private const string TemplateName = "DiscordTemplate";
        private const string InstallMenuPath = "PlayroomKit/WebGL/Install Discord Template";
        private const string SettingsMenuPath =
            "PlayroomKit/WebGL/Apply WebGL Settings (Run In Background, Discord Template, Compression None)";

        [MenuItem(InstallMenuPath)]
        public static void InstallTemplate()
        {
            var sourcePath = GetPackageTemplatePath();
            if (!Directory.Exists(sourcePath))
            {
                EditorUtility.DisplayDialog(
                    "PlayroomKit",
                    "Could not find DiscordTemplate in the package.",
                    "OK");
                return;
            }

            var templatesRoot = Path.Combine("Assets", "WebGLTemplates");
            var destinationPath = Path.Combine(templatesRoot, TemplateName);

            if (Directory.Exists(destinationPath))
            {
                var overwrite = EditorUtility.DisplayDialog(
                    "PlayroomKit",
                    $"Template already exists at {destinationPath}. Overwrite?",
                    "Overwrite",
                    "Cancel");
                if (!overwrite)
                {
                    return;
                }

                FileUtil.DeleteFileOrDirectory(destinationPath);
                var metaPath = destinationPath + ".meta";
                if (File.Exists(metaPath))
                {
                    FileUtil.DeleteFileOrDirectory(metaPath);
                }
            }

            Directory.CreateDirectory(templatesRoot);
            FileUtil.CopyFileOrDirectory(sourcePath, destinationPath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "PlayroomKit",
                $"Installed WebGL template to {destinationPath}.",
                "OK");
        }

        [MenuItem(SettingsMenuPath)]
        public static void ApplyRecommendedSettings()
        {
            var destinationPath = Path.Combine("Assets", "WebGLTemplates", TemplateName);
            if (!Directory.Exists(destinationPath))
            {
                var install = EditorUtility.DisplayDialog(
                    "PlayroomKit",
                    "DiscordTemplate is not installed. Install it now?",
                    "Install",
                    "Cancel");
                if (!install)
                {
                    return;
                }

                InstallTemplate();
            }

            PlayerSettings.runInBackground = true;
            PlayerSettings.WebGL.template = $"PROJECT:{TemplateName}";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(
                "PlayroomKit",
                "Applied WebGL settings (run in background, Discord template, compression none).",
                "OK");
        }

        private static string GetPackageTemplatePath()
        {
            return Path.Combine(
                "Packages",
                "com.playroomkit.sdk",
                "Assets",
                "WebGLTemplates",
                TemplateName);
        }
    }
}
#endif
