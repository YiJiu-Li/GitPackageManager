using System;
using UnityEditor;
using UnityEngine;

namespace GitPackageManager
{
    /// <summary>
    /// 从GitHub URL导入Git包的编辑器窗口
    /// </summary>
    public class ImportFromUrlWindow : EditorWindow
    {
        private string gitUrl = "";
        private string packageName = "";
        private string displayName = "";
        private string description = "";
        private string path = "";
        private string branch = "main";
        private string version = "";
        private string category = "Other";
        private GitPackageConfig config;
        private int selectedCategoryIndex = 0;

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

                if (!System.IO.Directory.Exists("Assets/Editor/GitPackageManager"))
                {
                    System.IO.Directory.CreateDirectory("Assets/Editor/GitPackageManager");
                }

                string assetPath = "Assets/Editor/GitPackageManager/GitPackagesConfig.asset";
                AssetDatabase.CreateAsset(config, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("从GitHub URL导入Git包", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // GitHub URL输入
            EditorGUILayout.LabelField("填写Git包信息", EditorStyles.boldLabel);
            gitUrl = EditorGUILayout.TextField("GitHub URL", gitUrl);

            // 检测输入的URL是否是GitHub地址
            bool isValidGitHubUrl =
                !string.IsNullOrEmpty(gitUrl)
                && (gitUrl.Contains("github.com") || gitUrl.Contains("gitlab.com"));

            if (!string.IsNullOrEmpty(gitUrl) && !isValidGitHubUrl)
            {
                EditorGUILayout.HelpBox("请输入有效的GitHub或GitLab仓库URL", MessageType.Warning);
            }

            // 其他包信息
            packageName = EditorGUILayout.TextField("包ID (com.xxx.yyy)", packageName);
            displayName = EditorGUILayout.TextField("显示名称", displayName);
            description = EditorGUILayout.TextField("描述", description);
            branch = EditorGUILayout.TextField("分支", branch);
            path = EditorGUILayout.TextField("子目录路径 (可选)", path);
            version = EditorGUILayout.TextField("版本", version);

            // 分类选择
            selectedCategoryIndex = EditorGUILayout.Popup(
                "分类",
                selectedCategoryIndex,
                config.categories.ToArray()
            );
            category = config.categories[selectedCategoryIndex];

            // 辅助信息
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "URL示例: https://github.com/username/repository.git",
                MessageType.Info
            );
            EditorGUILayout.HelpBox("如果包在仓库的子目录中，请填写相对路径", MessageType.Info);

            EditorGUILayout.Space();

            // 导入按钮
            GUI.enabled = isValidGitHubUrl && !string.IsNullOrEmpty(packageName);
            if (GUILayout.Button("导入到Git包管理器"))
            {
                ImportPackage();
            }
            GUI.enabled = true;

            // 自动生成包ID按钮
            if (!string.IsNullOrEmpty(gitUrl) && string.IsNullOrEmpty(packageName))
            {
                if (GUILayout.Button("从URL生成包ID"))
                {
                    GeneratePackageNameFromUrl();
                }
            }
        }

        // 从URL自动生成包ID
        private void GeneratePackageNameFromUrl()
        {
            try
            {
                // 解析URL获取仓库名
                Uri uri = new Uri(gitUrl);
                string path = uri.AbsolutePath;
                string[] segments = path.Split(
                    new[] { '/' },
                    StringSplitOptions.RemoveEmptyEntries
                );

                if (segments.Length >= 2)
                {
                    string username = segments[0];
                    string repo = segments[1];
                    if (repo.EndsWith(".git"))
                    {
                        repo = repo.Substring(0, repo.Length - 4);
                    }

                    // 生成格式如: com.username.reponame
                    packageName = $"com.{username.ToLower()}.{repo.ToLower()}";

                    // 同时设置显示名称
                    if (string.IsNullOrEmpty(displayName))
                    {
                        displayName = repo.Replace('-', ' ').Replace('.', ' ');
                        // 将每个单词首字母大写
                        string[] words = displayName.Split(' ');
                        for (int i = 0; i < words.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(words[i]))
                            {
                                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
                            }
                        }
                        displayName = string.Join(" ", words);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"解析URL失败: {e.Message}");
            }
        }

        private void ImportPackage()
        {
            // 检查是否已存在
            bool exists = false;
            foreach (var package in config.gitPackages)
            {
                if (package.name == packageName)
                {
                    exists = true;
                    break;
                }
            }

            if (exists)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "包已存在",
                    $"包 {packageName} 已存在于配置中。是否覆盖?",
                    "覆盖",
                    "取消"
                );

                if (!overwrite)
                    return;

                // 移除现有包
                config.gitPackages.RemoveAll(p => p.name == packageName);
            }

            // 创建新的包信息
            GitPackageInfo newPackage = new GitPackageInfo
            {
                name = packageName,
                displayName = displayName,
                description = description,
                gitUrl = gitUrl,
                branch = branch,
                path = path,
                version = version,
                category = category,
                isInstalled = false,
            };

            // 添加到配置
            config.gitPackages.Add(newPackage);

            // 保存配置
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "导入成功",
                $"成功导入了包 {displayName ?? packageName}",
                "确定"
            );

            // 询问是否立即安装
            bool installNow = EditorUtility.DisplayDialog(
                "安装包",
                "是否立即安装此包?",
                "是",
                "否"
            );

            if (installNow)
            {
                string fullUrl = newPackage.GetFullUrl();
                UnityEditor.PackageManager.Client.Add(fullUrl);
                Debug.Log($"正在安装包: {fullUrl}");
            }

            // 关闭窗口
            Close();
        }
    }
}
