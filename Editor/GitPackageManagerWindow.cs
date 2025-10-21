using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitPackageManager;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace GitPackageManager
{
    public class GitPackageManagerWindow : EditorWindow
    {
        private GitPackageConfig config;
        private Vector2 scrollPosition;
        private string searchText = "";
        private int selectedCategoryIndex = 0;
        private bool showAddPackagePanel = false;
        private GitPackageInfo newPackage = new GitPackageInfo();
        private AddRequest addRequest;
        private RemoveRequest removeRequest;
        private ListRequest listRequest;
        private Dictionary<string, bool> installedPackages = new Dictionary<string, bool>();
        private bool isRefreshing = false;
        private bool showSettings = false;
        private string newCategory = "";

        [MenuItem("Window/Git Package/Package Manager", false, 1)]
        public static void ShowWindow()
        {
            GetWindow<GitPackageManagerWindow>("Git Package Manager");
        }

        private void OnEnable()
        {
            LoadOrCreateConfig();
            RefreshInstalledPackages();
            EditorApplication.update += PackageManagerUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= PackageManagerUpdate;
        }

        private void PackageManagerUpdate()
        {
            if (addRequest != null && addRequest.IsCompleted)
            {
                if (addRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"Package {addRequest.Result.displayName} installed successfully");
                    UpdatePackageInstalledStatus(addRequest.Result.name, true);
                }
                else if (addRequest.Status == StatusCode.Failure)
                {
                    string errorMessage =
                        addRequest.Error != null ? addRequest.Error.message : "未知错误";
                    Debug.LogError($"Failed to install package: {errorMessage}");
                }
                addRequest = null;
                RefreshInstalledPackages();
            }

            if (removeRequest != null && removeRequest.IsCompleted)
            {
                if (removeRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Package removed successfully");
                }
                else if (removeRequest.Status == StatusCode.Failure)
                {
                    string errorMessage =
                        removeRequest.Error != null ? removeRequest.Error.message : "未知错误";
                    Debug.LogError($"Failed to remove package: {errorMessage}");
                }
                removeRequest = null;
                RefreshInstalledPackages();
            }

            if (listRequest != null && listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                {
                    installedPackages.Clear();
                    foreach (var package in listRequest.Result)
                    {
                        installedPackages[package.name] = true;
                    }

                    foreach (var package in config.gitPackages)
                    {
                        package.isInstalled = installedPackages.ContainsKey(package.name);
                    }

                    isRefreshing = false;
                    SaveConfig();
                    Repaint();
                }
                else if (listRequest.Status == StatusCode.Failure)
                {
                    string errorMessage =
                        listRequest.Error != null ? listRequest.Error.message : "未知错误";
                    Debug.LogError($"Failed to list packages: {errorMessage}");
                    isRefreshing = false;
                }
                listRequest = null;
            }
        }

        private void LoadOrCreateConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:GitPackageConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                config = AssetDatabase.LoadAssetAtPath<GitPackageConfig>(path);
            }
            else
            {
                // 创建新的配置文件
                config = CreateInstance<GitPackageConfig>();
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

        private void SaveConfig()
        {
            if (config != null)
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }
        }

        private void RefreshInstalledPackages()
        {
            if (isRefreshing)
                return;

            isRefreshing = true;
            listRequest = Client.List();
        }

        private void UpdatePackageInstalledStatus(string packageName, bool installed)
        {
            var package = config.gitPackages.FirstOrDefault(p => p.name == packageName);
            if (package != null)
            {
                package.isInstalled = installed;
                SaveConfig();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Git Package Manager", EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("刷新", GUILayout.Width(60)))
            {
                RefreshInstalledPackages();
            }
            if (GUILayout.Button("设置", GUILayout.Width(60)))
            {
                showSettings = !showSettings;
            }
            if (GUILayout.Button("编辑Manifest", GUILayout.Width(100)))
            {
                // 打开Manifest编辑器窗口
                EditorApplication.ExecuteMenuItem("Window/Git Package/Edit Manifest.json");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 搜索框
            EditorGUILayout.BeginHorizontal();
            searchText = EditorGUILayout.TextField("搜索", searchText, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("清除", GUILayout.Width(60)))
            {
                searchText = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();

            // 分类选择
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("分类", GUILayout.Width(40));
            selectedCategoryIndex = EditorGUILayout.Popup(
                selectedCategoryIndex,
                config.categories.ToArray()
            );
            EditorGUILayout.EndHorizontal();

            DrawSettingsPanel();

            if (showAddPackagePanel)
            {
                DrawAddPackagePanel();
            }
            else
            {
                if (GUILayout.Button("添加Git包"))
                {
                    showAddPackagePanel = true;
                    newPackage = new GitPackageInfo { branch = "main", category = "Other" };
                }
            }

            EditorGUILayout.Space();

            // 显示包列表
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawPackageList();
            EditorGUILayout.EndScrollView();

            // 添加作者信息
            GUIStyle footerStyle = new GUIStyle(EditorStyles.miniLabel);
            footerStyle.alignment = TextAnchor.MiddleCenter;
            footerStyle.normal.textColor = new Color(0.5f, 0.5f, 0.8f);

            if (GUILayout.Button("作者:依旧 | GitHub: https://github.com/YiJiu-Li", footerStyle))
            {
                Application.OpenURL("https://github.com/YiJiu-Li");
            }
        }

        private void DrawSettingsPanel()
        {
            if (!showSettings)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("设置", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("分类管理", EditorStyles.boldLabel);

            // 显示现有分类
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            List<string> categoriesToRemove = new List<string>();

            for (int i = 1; i < config.categories.Count; i++) // 跳过 "All"
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(config.categories[i], GUILayout.ExpandWidth(true));
                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    categoriesToRemove.Add(config.categories[i]);
                }
                EditorGUILayout.EndHorizontal();
            }

            foreach (var category in categoriesToRemove)
            {
                config.categories.Remove(category);
                // 将使用此分类的包重新分类为"Other"
                foreach (var package in config.gitPackages.Where(p => p.category == category))
                {
                    package.category = "Other";
                }
                SaveConfig();
            }
            EditorGUILayout.EndVertical();

            // 添加新分类
            EditorGUILayout.BeginHorizontal();
            newCategory = EditorGUILayout.TextField("新分类", newCategory);
            GUI.enabled =
                !string.IsNullOrEmpty(newCategory) && !config.categories.Contains(newCategory);
            if (GUILayout.Button("添加分类", GUILayout.Width(80)))
            {
                if (!string.IsNullOrEmpty(newCategory) && !config.categories.Contains(newCategory))
                {
                    config.categories.Add(newCategory);
                    newCategory = "";
                    SaveConfig();
                }
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("关闭设置"))
            {
                showSettings = false;
            }

            EditorGUILayout.Space();
        }

        private void DrawAddPackagePanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("添加新的Git包", EditorStyles.boldLabel);

            newPackage.name = EditorGUILayout.TextField("包ID (com.xxx.yyy)", newPackage.name);
            newPackage.displayName = EditorGUILayout.TextField("显示名称", newPackage.displayName);
            newPackage.description = EditorGUILayout.TextField("描述", newPackage.description);
            newPackage.gitUrl = EditorGUILayout.TextField("Git URL", newPackage.gitUrl);
            newPackage.branch = EditorGUILayout.TextField("分支", newPackage.branch);
            newPackage.path = EditorGUILayout.TextField("子目录路径(可选)", newPackage.path);
            newPackage.version = EditorGUILayout.TextField("版本", newPackage.version);

            // 分类选择
            int categoryIndex = config.categories.IndexOf(newPackage.category);
            if (categoryIndex == -1)
                categoryIndex = 0;
            categoryIndex = EditorGUILayout.Popup(
                "分类",
                categoryIndex,
                config.categories.ToArray()
            );
            newPackage.category = config.categories[categoryIndex];

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("保存"))
            {
                if (
                    string.IsNullOrEmpty(newPackage.name) || string.IsNullOrEmpty(newPackage.gitUrl)
                )
                {
                    EditorUtility.DisplayDialog("错误", "包ID和Git URL不能为空", "确定");
                }
                else
                {
                    // 检查是否已存在
                    bool exists = config.gitPackages.Any(p => p.name == newPackage.name);
                    if (!exists)
                    {
                        config.gitPackages.Add(
                            new GitPackageInfo
                            {
                                name = newPackage.name,
                                displayName = newPackage.displayName,
                                description = newPackage.description,
                                gitUrl = newPackage.gitUrl,
                                branch = newPackage.branch,
                                path = newPackage.path,
                                version = newPackage.version,
                                category = newPackage.category,
                                isInstalled = false,
                            }
                        );
                        SaveConfig();
                        showAddPackagePanel = false;
                        RefreshInstalledPackages();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("错误", "已存在同名的包", "确定");
                    }
                }
            }
            if (GUILayout.Button("取消"))
            {
                showAddPackagePanel = false;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawPackageList()
        {
            string selectedCategory = config.categories[selectedCategoryIndex];

            var packagesToShow = config
                .gitPackages.Where(p =>
                    (selectedCategory == "All" || p.category == selectedCategory)
                    && (
                        string.IsNullOrEmpty(searchText)
                        || p.name.ToLower().Contains(searchText.ToLower())
                        || p.displayName.ToLower().Contains(searchText.ToLower())
                        || p.description.ToLower().Contains(searchText.ToLower())
                    )
                )
                .ToList();

            if (packagesToShow.Count == 0)
            {
                EditorGUILayout.HelpBox("没有找到匹配的包", MessageType.Info);
                return;
            }

            foreach (var package in packagesToShow)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();

                GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
                nameStyle.fontSize = 12;

                EditorGUILayout.LabelField(
                    string.IsNullOrEmpty(package.displayName) ? package.name : package.displayName,
                    nameStyle
                );
                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(isRefreshing);
                if (package.isInstalled)
                {
                    if (GUILayout.Button("移除", GUILayout.Width(60)))
                    {
                        removeRequest = Client.Remove(package.name);
                    }
                }
                else
                {
                    if (GUILayout.Button("安装", GUILayout.Width(60)))
                    {
                        addRequest = Client.Add(package.GetFullUrl());
                    }
                }

                if (GUILayout.Button("编辑", GUILayout.Width(60)))
                {
                    showAddPackagePanel = true;
                    newPackage = new GitPackageInfo
                    {
                        name = package.name,
                        displayName = package.displayName,
                        description = package.description,
                        gitUrl = package.gitUrl,
                        branch = package.branch,
                        path = package.path,
                        version = package.version,
                        category = package.category,
                        isInstalled = package.isInstalled,
                    };
                }

                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    if (
                        EditorUtility.DisplayDialog(
                            "确认删除",
                            $"确定要从列表中删除 {package.displayName} 吗?",
                            "确定",
                            "取消"
                        )
                    )
                    {
                        config.gitPackages.Remove(package);
                        SaveConfig();
                        GUIUtility.ExitGUI();
                        return;
                    }
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();

                // 显示包ID
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("ID:", GUILayout.Width(30));
                EditorGUILayout.LabelField(package.name, EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                // 显示版本
                if (!string.IsNullOrEmpty(package.version))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("版本:", GUILayout.Width(30));
                    EditorGUILayout.LabelField(package.version, EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }

                // 显示分类
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("分类:", GUILayout.Width(30));
                EditorGUILayout.LabelField(package.category, EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                // 显示状态
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("状态:", GUILayout.Width(30));
                EditorGUILayout.LabelField(
                    package.isInstalled ? "已安装" : "未安装",
                    EditorStyles.miniLabel
                );
                EditorGUILayout.EndHorizontal();

                // 显示描述
                if (!string.IsNullOrEmpty(package.description))
                {
                    EditorGUILayout.LabelField(
                        package.description,
                        EditorStyles.wordWrappedMiniLabel
                    );
                }

                // 显示Git URL
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Git URL:", GUILayout.Width(50));
                EditorGUILayout.TextField(package.gitUrl);
                EditorGUILayout.EndHorizontal();

                // 显示分支
                if (!string.IsNullOrEmpty(package.branch))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("分支:", GUILayout.Width(50));
                    EditorGUILayout.LabelField(package.branch, EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }

                // 显示路径
                if (!string.IsNullOrEmpty(package.path))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("路径:", GUILayout.Width(50));
                    EditorGUILayout.LabelField(package.path, EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }
    }
}
