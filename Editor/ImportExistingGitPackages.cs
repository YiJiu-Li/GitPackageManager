using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GitPackageManager
{
    /// <summary>
    /// 导入现有Git包的编辑器工具
    /// </summary>
    public class ImportExistingGitPackages : EditorWindow
    {
        private GitPackageConfig config;
        private List<GitPackageInfo> detectedPackages = new List<GitPackageInfo>();
        private List<bool> packageSelections = new List<bool>();
        private Vector2 scrollPosition;

        [MenuItem("Window/Git Package/Import Existing Packages")]
        public static void ShowWindow()
        {
            GetWindow<ImportExistingGitPackages>("导入现有Git包");
        }

        private void OnEnable()
        {
            LoadConfig();
            DetectExistingPackages();
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
            }
        }

        private void DetectExistingPackages()
        {
            Debug.Log("开始检测manifest.json中的Git包...");
            detectedPackages = GitPackageUtils.ParseGitPackagesFromManifest();
            packageSelections = new List<bool>(detectedPackages.Count);

            Debug.Log($"检测到 {detectedPackages.Count} 个Git包");

            // 默认选中所有包
            for (int i = 0; i < detectedPackages.Count; i++)
            {
                // 如果配置中已经包含了这个包，则不选中
                bool alreadyExists = config.gitPackages.Any(p =>
                    p.name == detectedPackages[i].name
                );
                packageSelections.Add(!alreadyExists);

                // 输出检测到的包信息
                Debug.Log(
                    $"检测到Git包: {detectedPackages[i].name}, URL: {detectedPackages[i].gitUrl}"
                );
            }

            // 如果没有检测到任何包，添加一些手动示例
            if (detectedPackages.Count == 0)
            {
                Debug.LogWarning("尝试手动检查manifest.json文件...");
                string manifestPath = Path.Combine(
                    Application.dataPath,
                    "..",
                    "Packages",
                    "manifest.json"
                );

                if (File.Exists(manifestPath))
                {
                    string content = File.ReadAllText(manifestPath);
                    Debug.Log($"Manifest内容：\n{content}");

                    // 检查是否包含http或git关键字
                    if (content.Contains("http") || content.Contains("git"))
                    {
                        Debug.Log("Manifest中可能包含Git包，但解析失败。请报告此问题。");
                    }
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("导入现有Git包", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "这个工具可以导入manifest.json中已经存在的Git包到Git包管理器中。",
                MessageType.Info
            );
            EditorGUILayout.Space();

            if (detectedPackages.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "没有在manifest.json中检测到任何Git包。",
                    MessageType.Warning
                );
                return;
            }

            // 全选/全不选按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全选"))
            {
                for (int i = 0; i < packageSelections.Count; i++)
                {
                    bool alreadyExists = config.gitPackages.Any(p =>
                        p.name == detectedPackages[i].name
                    );
                    packageSelections[i] = !alreadyExists;
                }
            }
            if (GUILayout.Button("全不选"))
            {
                for (int i = 0; i < packageSelections.Count; i++)
                {
                    packageSelections[i] = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("检测到的Git包:", EditorStyles.boldLabel);
            for (int i = 0; i < detectedPackages.Count; i++)
            {
                GitPackageInfo package = detectedPackages[i];

                // 检查配置中是否已经包含了这个包
                bool alreadyExists = config.gitPackages.Any(p => p.name == package.name);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // 显示已存在的提示
                if (alreadyExists)
                {
                    EditorGUILayout.HelpBox(
                        $"此包'{package.name}'已存在于配置中。",
                        MessageType.Warning
                    );
                }

                EditorGUI.BeginDisabledGroup(alreadyExists);
                packageSelections[i] = EditorGUILayout.ToggleLeft(
                    package.name,
                    packageSelections[i]
                );
                EditorGUI.EndDisabledGroup();

                // 显示Git URL
                EditorGUILayout.LabelField($"Git URL: {package.gitUrl}");

                // 显示分支
                if (!string.IsNullOrEmpty(package.branch))
                {
                    EditorGUILayout.LabelField($"分支: {package.branch}");
                }

                // 显示路径
                if (!string.IsNullOrEmpty(package.path))
                {
                    EditorGUILayout.LabelField($"路径: {package.path}");
                }

                // 编辑包信息
                if (packageSelections[i] && !alreadyExists)
                {
                    // 添加额外信息
                    package.displayName = EditorGUILayout.TextField(
                        "显示名称",
                        package.displayName
                    );
                    package.description = EditorGUILayout.TextField("描述", package.description);
                    package.version = EditorGUILayout.TextField("版本", package.version);

                    // 分类选择
                    int categoryIndex = 0; // 默认为"Other"
                    string currentCategory = string.IsNullOrEmpty(package.category)
                        ? "Other"
                        : package.category;
                    if (config.categories.Contains(currentCategory))
                    {
                        categoryIndex = config.categories.IndexOf(currentCategory);
                    }
                    categoryIndex = EditorGUILayout.Popup(
                        "分类",
                        categoryIndex,
                        config.categories.ToArray()
                    );
                    package.category = config.categories[categoryIndex];
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // 导入按钮
            if (GUILayout.Button("导入选中的包"))
            {
                ImportSelectedPackages();
            }
        }

        private void ImportSelectedPackages()
        {
            int importCount = 0;

            for (int i = 0; i < detectedPackages.Count; i++)
            {
                if (packageSelections[i])
                {
                    GitPackageInfo package = detectedPackages[i];

                    // 检查是否已存在
                    bool exists = config.gitPackages.Any(p => p.name == package.name);
                    if (!exists)
                    {
                        // 将包添加到配置中
                        config.gitPackages.Add(package);
                        importCount++;
                    }
                }
            }

            if (importCount > 0)
            {
                // 保存配置
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();

                EditorUtility.DisplayDialog(
                    "导入成功",
                    $"成功导入了{importCount}个Git包。",
                    "确定"
                );
                Close();
            }
            else
            {
                EditorUtility.DisplayDialog("导入", "没有选中任何包进行导入。", "确定");
            }
        }
    }
}
