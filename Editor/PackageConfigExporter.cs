using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GitPackageManager
{
    /// <summary>
    /// 导出和导入Git包配置的工具
    /// </summary>
    public class PackageConfigExporter : EditorWindow
    {
        private GitPackageConfig config;
        private List<bool> packageSelections = new List<bool>();
        private Vector2 scrollPosition;
        private string exportPath = "";
        private string importPath = "";

        [MenuItem("Window/Git Package/Export Packages Config", false, 30)]
        public static void ShowExportWindow()
        {
            var window = GetWindow<PackageConfigExporter>("导出包配置");
            window.minSize = new Vector2(500, 400);
            window.titleContent = new GUIContent("导出包配置");
        }

        [MenuItem("Window/Git Package/Import Packages Config", false, 31)]
        public static void ShowImportWindow()
        {
            var window = GetWindow<PackageConfigExporter>("导入包配置");
            window.minSize = new Vector2(500, 300);
            window.titleContent = new GUIContent("导入包配置");
        }

        private void OnEnable()
        {
            LoadConfig();
            InitPackageSelections();
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
                config = ScriptableObject.CreateInstance<GitPackageConfig>();
            }
        }

        private void InitPackageSelections()
        {
            packageSelections = new List<bool>(config.gitPackages.Count);
            for (int i = 0; i < config.gitPackages.Count; i++)
            {
                packageSelections.Add(true);
            }
        }

        private void OnGUI()
        {
            if (titleContent.text == "导出包配置")
            {
                DrawExportUI();
            }
            else if (titleContent.text == "导入包配置")
            {
                DrawImportUI();
            }
        }

        private void DrawExportUI()
        {
            EditorGUILayout.LabelField("导出Git包配置", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "选择要导出的包，然后点击导出按钮将配置保存到JSON文件。",
                MessageType.Info
            );
            EditorGUILayout.Space();

            if (config.gitPackages.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "没有找到任何Git包配置。请先在Git包管理器中添加包。",
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
                    packageSelections[i] = true;
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

            // 显示包列表
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < config.gitPackages.Count; i++)
            {
                var package = config.gitPackages[i];

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                packageSelections[i] = EditorGUILayout.ToggleLeft(
                    string.IsNullOrEmpty(package.displayName) ? package.name : package.displayName,
                    packageSelections[i]
                );

                if (packageSelections[i])
                {
                    // 显示包详细信息
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"ID: {package.name}");
                    EditorGUILayout.LabelField(
                        $"类型: {(package.gitUrl.StartsWith("file:") ? "本地包" : "Git包")}"
                    );
                    if (!string.IsNullOrEmpty(package.version))
                    {
                        EditorGUILayout.LabelField($"版本: {package.version}");
                    }
                    EditorGUILayout.LabelField($"分类: {package.category}");
                    EditorGUILayout.LabelField(
                        $"状态: {(package.isInstalled ? "已安装" : "未安装")}"
                    );
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // 导出按钮
            if (GUILayout.Button("导出选中的包配置"))
            {
                ExportSelectedPackages();
            }
        }

        private void DrawImportUI()
        {
            EditorGUILayout.LabelField("导入Git包配置", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "选择一个之前导出的JSON配置文件来导入包配置。",
                MessageType.Info
            );
            EditorGUILayout.Space();

            // 选择文件按钮
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("配置文件");
            if (GUILayout.Button("浏览...", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFilePanel("选择配置文件", "", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    importPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            importPath = EditorGUILayout.TextField("文件路径", importPath);

            EditorGUILayout.Space();

            // 导入按钮
            GUI.enabled = !string.IsNullOrEmpty(importPath) && File.Exists(importPath);
            if (GUILayout.Button("导入包配置"))
            {
                ImportPackages();
            }
            GUI.enabled = true;

            if (!string.IsNullOrEmpty(importPath) && !File.Exists(importPath))
            {
                EditorGUILayout.HelpBox("指定的文件不存在", MessageType.Error);
            }
        }

        private void ExportSelectedPackages()
        {
            // 收集选中的包
            List<GitPackageInfo> selectedPackages = new List<GitPackageInfo>();
            for (int i = 0; i < config.gitPackages.Count; i++)
            {
                if (packageSelections[i])
                {
                    selectedPackages.Add(config.gitPackages[i]);
                }
            }

            if (selectedPackages.Count == 0)
            {
                EditorUtility.DisplayDialog("导出", "没有选中任何包", "确定");
                return;
            }

            // 选择保存路径
            string path = EditorUtility.SaveFilePanel("导出包配置", "", "GitPackages", "json");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // 创建导出数据
            ExportData exportData = new ExportData
            {
                packages = selectedPackages.ToArray(),
                categories = config.categories.ToArray(),
                exportDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                exportVersion = "1.0",
            };

            // 序列化为JSON
            string json = JsonUtility.ToJson(exportData, true);

            // 保存到文件
            try
            {
                File.WriteAllText(path, json);
                EditorUtility.DisplayDialog(
                    "导出成功",
                    $"成功导出了{selectedPackages.Count}个包的配置。",
                    "确定"
                );
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("导出失败", $"保存文件时出错: {e.Message}", "确定");
            }
        }

        private void ImportPackages()
        {
            if (string.IsNullOrEmpty(importPath) || !File.Exists(importPath))
            {
                EditorUtility.DisplayDialog("导入", "请先选择有效的配置文件", "确定");
                return;
            }

            try
            {
                // 读取JSON文件
                string json = File.ReadAllText(importPath);
                ExportData importData = JsonUtility.FromJson<ExportData>(json);

                if (
                    importData == null
                    || importData.packages == null
                    || importData.packages.Length == 0
                )
                {
                    EditorUtility.DisplayDialog("导入失败", "无效的配置文件或配置中没有包", "确定");
                    return;
                }

                // 导入分类
                if (importData.categories != null && importData.categories.Length > 0)
                {
                    foreach (string category in importData.categories)
                    {
                        if (!config.categories.Contains(category))
                        {
                            config.categories.Add(category);
                        }
                    }
                }

                // 导入包
                int importCount = 0;
                int updateCount = 0;

                foreach (var packageToImport in importData.packages)
                {
                    // 检查是否已存在
                    bool exists = false;
                    for (int i = 0; i < config.gitPackages.Count; i++)
                    {
                        if (config.gitPackages[i].name == packageToImport.name)
                        {
                            exists = true;

                            // 询问是否更新
                            bool update = EditorUtility.DisplayDialog(
                                "包已存在",
                                $"包 {packageToImport.name} 已存在，是否更新?",
                                "更新",
                                "跳过"
                            );

                            if (update)
                            {
                                config.gitPackages[i] = packageToImport;
                                updateCount++;
                            }

                            break;
                        }
                    }

                    if (!exists)
                    {
                        // 添加新包
                        config.gitPackages.Add(packageToImport);
                        importCount++;
                    }
                }

                // 保存配置
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();

                EditorUtility.DisplayDialog(
                    "导入成功",
                    $"成功导入了{importCount}个新包，更新了{updateCount}个现有包。",
                    "确定"
                );
                Close();
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog(
                    "导入失败",
                    $"读取或处理配置文件时出错: {e.Message}",
                    "确定"
                );
            }
        }

        // 导出数据结构
        [System.Serializable]
        private class ExportData
        {
            public GitPackageInfo[] packages;
            public string[] categories;
            public string exportDate;
            public string exportVersion;
        }
    }
}
