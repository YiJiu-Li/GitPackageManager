using System.IO;
using UnityEditor;
using UnityEngine;

namespace GitPackageManager
{
    /// <summary>
    /// 直接查看和编辑manifest.json的工具
    /// </summary>
    public class ManifestEditor : EditorWindow
    {
        private string manifestContent = "";
        private Vector2 scrollPosition;
        private string manifestPath = "";
        private bool hasChanges = false;

        [MenuItem("Window/Git Package/Edit Manifest.json", false, 50)]
        public static void ShowWindow()
        {
            var window = GetWindow<ManifestEditor>("Manifest.json 编辑器");
            window.minSize = new Vector2(600, 400);
        }

        private void OnEnable()
        {
            LoadManifestContent();
        }

        private void LoadManifestContent()
        {
            manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");

            if (File.Exists(manifestPath))
            {
                manifestContent = File.ReadAllText(manifestPath);
                hasChanges = false;
            }
            else
            {
                manifestContent = "找不到 manifest.json 文件!";
                EditorUtility.DisplayDialog("错误", "无法找到 manifest.json 文件", "确定");
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Manifest.json 编辑器", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新", GUILayout.Width(80)))
            {
                if (hasChanges)
                {
                    bool refresh = EditorUtility.DisplayDialog(
                        "有未保存的更改",
                        "您有未保存的更改，确定要刷新吗？未保存的更改将丢失。",
                        "刷新",
                        "取消"
                    );

                    if (refresh)
                    {
                        LoadManifestContent();
                    }
                }
                else
                {
                    LoadManifestContent();
                }
            }

            if (GUILayout.Button("保存", GUILayout.Width(80)))
            {
                SaveManifestContent();
            }

            if (GUILayout.Button("在文件系统中显示", GUILayout.Width(120)))
            {
                EditorUtility.RevealInFinder(manifestPath);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 显示路径
            EditorGUILayout.TextField("文件路径", manifestPath, EditorStyles.textField);

            EditorGUILayout.Space();

            // 编辑区域
            EditorGUILayout.LabelField("编辑 Manifest.json (警告: 直接编辑可能导致格式错误!):");

            EditorGUI.BeginChangeCheck();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 使用文本区域编辑内容
            GUI.SetNextControlName("ManifestEditor");
            string newContent = EditorGUILayout.TextArea(
                manifestContent,
                GUILayout.ExpandHeight(true)
            );

            if (EditorGUI.EndChangeCheck())
            {
                if (manifestContent != newContent)
                {
                    manifestContent = newContent;
                    hasChanges = true;
                }
            }

            EditorGUILayout.EndScrollView();

            // 如果有未保存的更改，显示提示
            if (hasChanges)
            {
                EditorGUILayout.HelpBox("有未保存的更改!", MessageType.Warning);
            }

            EditorGUILayout.Space();

            // 帮助信息
            EditorGUILayout.HelpBox(
                "提示: Git包的URL格式应为 'https://github.com/username/repo.git'，可以添加 '?path=subfolder' 指定子目录。",
                MessageType.Info
            );
            EditorGUILayout.HelpBox(
                "警告: 直接编辑manifest.json可能导致Unity包管理器出错。确保JSON格式正确。",
                MessageType.Warning
            );

            // 使文本区域获得焦点
            if (GUI.GetNameOfFocusedControl() == "")
            {
                GUI.FocusControl("ManifestEditor");
            }
        }

        private void SaveManifestContent()
        {
            try
            {
                File.WriteAllText(manifestPath, manifestContent);
                AssetDatabase.Refresh();
                hasChanges = false;
                EditorUtility.DisplayDialog("保存成功", "Manifest.json 已保存", "确定");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog(
                    "保存失败",
                    $"保存 manifest.json 时出错: {e.Message}",
                    "确定"
                );
            }
        }

        private void OnLostFocus()
        {
            if (hasChanges)
            {
                Debug.Log("Manifest.json 编辑器有未保存的更改");
            }
        }
    }
}
