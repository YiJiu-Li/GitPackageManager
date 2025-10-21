using System.IO;
using UnityEditor;
using UnityEngine;

namespace GitPackageManager
{
    /// <summary>
    /// 从本地文件系统导入包的编辑器窗口
    /// </summary>
    public class ImportFromLocalWindow : EditorWindow
    {
        private string packagePath = "";
        private string displayName = "";
        private string description = "";
        private string version = "";
        private string category = "Other";
        private GitPackageConfig config;
        private int selectedCategoryIndex = 0;
        private bool autoFillInfo = true;

        [MenuItem("Window/Git Package/Import From Local Folder", false, 12)]
        public static void ShowWindow()
        {
            var window = GetWindow<ImportFromLocalWindow>("从本地文件夹导入");
            window.minSize = new Vector2(500, 200);
        }

        private void OnEnable()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:GitPackageConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                config = AssetDatabase.LoadAssetAtPath<GitPackageConfig>(path);
            }
            else
            {
                // 如果没有找到配置文件，创建一个新的
                config = ScriptableObject.CreateInstance<GitPackageConfig>();

                if (!Directory.Exists("Assets/Editor/GitPackageManager"))
                {
                    Directory.CreateDirectory("Assets/Editor/GitPackageManager");
                }

                string assetPath = "Assets/Editor/GitPackageManager/GitPackagesConfig.asset";
                AssetDatabase.CreateAsset(config, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("从本地文件夹导入包", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("包文件夹路径");
            if (GUILayout.Button("浏览...", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("选择包文件夹", "", "");
                if (!string.IsNullOrEmpty(path))
                {
                    packagePath = path;
                    if (autoFillInfo)
                    {
                        TryLoadPackageInfo();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            packagePath = EditorGUILayout.TextField("路径", packagePath);

            autoFillInfo = EditorGUILayout.Toggle("自动填充信息", autoFillInfo);

            EditorGUILayout.Space();

            // 检查路径是否有效
            bool isValidPath = !string.IsNullOrEmpty(packagePath) && Directory.Exists(packagePath);
            bool hasPackageJson =
                isValidPath && File.Exists(Path.Combine(packagePath, "package.json"));

            if (!string.IsNullOrEmpty(packagePath) && !isValidPath)
            {
                EditorGUILayout.HelpBox("指定的路径不存在", MessageType.Error);
            }
            else if (isValidPath && !hasPackageJson)
            {
                EditorGUILayout.HelpBox(
                    "指定的文件夹中不包含package.json文件",
                    MessageType.Warning
                );
            }

            // 包信息输入
            EditorGUILayout.LabelField("包信息", EditorStyles.boldLabel);

            // 如果找到了package.json，显示自动填充的信息
            displayName = EditorGUILayout.TextField("显示名称", displayName);
            description = EditorGUILayout.TextField("描述", description);
            version = EditorGUILayout.TextField("版本", version);

            // 分类选择
            selectedCategoryIndex = EditorGUILayout.Popup(
                "分类",
                selectedCategoryIndex,
                config.categories.ToArray()
            );
            category = config.categories[selectedCategoryIndex];

            EditorGUILayout.Space();

            // 导入按钮
            GUI.enabled = isValidPath && hasPackageJson;
            if (GUILayout.Button("导入到Git包管理器"))
            {
                ImportPackage();
            }
            GUI.enabled = true;

            if (isValidPath && !hasPackageJson)
            {
                if (GUILayout.Button("重新加载包信息"))
                {
                    TryLoadPackageInfo();
                }
            }
        }

        private void TryLoadPackageInfo()
        {
            string packageJsonPath = Path.Combine(packagePath, "package.json");
            if (File.Exists(packageJsonPath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(packageJsonPath);
                    PackageJson packageJson = JsonUtility.FromJson<PackageJson>(jsonContent);

                    if (packageJson != null)
                    {
                        displayName = packageJson.displayName ?? packageJson.name;
                        description = packageJson.description ?? "";
                        version = packageJson.version ?? "";
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"读取package.json失败: {e.Message}");
                }
            }
        }

        private void ImportPackage()
        {
            string packageJsonPath = Path.Combine(packagePath, "package.json");
            if (!File.Exists(packageJsonPath))
            {
                EditorUtility.DisplayDialog("错误", "无法找到package.json文件", "确定");
                return;
            }

            // 读取package.json获取包ID
            string jsonContent = File.ReadAllText(packageJsonPath);
            PackageJson packageJson = JsonUtility.FromJson<PackageJson>(jsonContent);

            if (packageJson == null || string.IsNullOrEmpty(packageJson.name))
            {
                EditorUtility.DisplayDialog("错误", "package.json中没有找到有效的包ID", "确定");
                return;
            }

            // 检查是否已存在
            bool exists = false;
            foreach (var package in config.gitPackages)
            {
                if (package.name == packageJson.name)
                {
                    exists = true;
                    break;
                }
            }

            if (exists)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "包已存在",
                    $"包 {packageJson.name} 已存在于配置中。是否覆盖?",
                    "覆盖",
                    "取消"
                );

                if (!overwrite)
                    return;

                // 移除现有包
                config.gitPackages.RemoveAll(p => p.name == packageJson.name);
            }

            // 创建新的包信息
            GitPackageInfo newPackage = new GitPackageInfo
            {
                name = packageJson.name,
                displayName = displayName,
                description = description,
                gitUrl = "file:" + packagePath.Replace("\\", "/"),
                version = version,
                category = category,
                isInstalled = false,
            };

            // 添加到配置
            config.gitPackages.Add(newPackage);

            // 保存配置
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("导入成功", $"成功导入了本地包: {newPackage.name}", "确定");

            // 询问是否立即安装
            bool installNow = EditorUtility.DisplayDialog(
                "安装包",
                "是否立即安装此包?",
                "是",
                "否"
            );

            if (installNow)
            {
                UnityEditor.PackageManager.Client.Add("file:" + packagePath.Replace("\\", "/"));
                Debug.Log($"正在安装包: {newPackage.name} 从 {packagePath}");
            }

            // 关闭窗口
            Close();
        }

        [System.Serializable]
        private class PackageJson
        {
            public string name;
            public string displayName;
            public string version;
            public string description;
        }
    }
}
