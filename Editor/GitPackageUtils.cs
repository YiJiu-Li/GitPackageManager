using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace GitPackageManager
{
    /// <summary>
    /// Git包管理器的工具类
    /// </summary>
    public static class GitPackageUtils
    {
        /// <summary>
        /// 从manifest.json中解析Git包的信息
        /// </summary>
        /// <returns>Git包列表</returns>
        public static List<GitPackageInfo> ParseGitPackagesFromManifest()
        {
            List<GitPackageInfo> result = new List<GitPackageInfo>();
            string manifestPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "manifest.json"
            );

            if (File.Exists(manifestPath))
            {
                string manifestContent = File.ReadAllText(manifestPath);
                Debug.Log("Manifest Path: " + manifestPath);

                // 使用正则表达式直接解析JSON内容，因为JsonUtility不支持直接解析Dictionary
                Dictionary<string, string> dependencies = new Dictionary<string, string>();

                try
                {
                    // 匹配dependencies部分
                    Match dependenciesMatch = Regex.Match(
                        manifestContent,
                        @"""dependencies"":\s*{([^}]*)}",
                        RegexOptions.Singleline
                    );

                    if (dependenciesMatch.Success)
                    {
                        string dependenciesContent = dependenciesMatch.Groups[1].Value;

                        // 匹配每一个键值对
                        MatchCollection entryMatches = Regex.Matches(
                            dependenciesContent,
                            @"""([^""]+)"":\s*""([^""]+)"""
                        );

                        foreach (Match entryMatch in entryMatches)
                        {
                            string key = entryMatch.Groups[1].Value;
                            string value = entryMatch.Groups[2].Value;
                            dependencies[key] = value;

                            Debug.Log($"Found dependency: {key} = {value}");
                        }

                        // 处理Git和HTTP包
                        foreach (var entry in dependencies)
                        {
                            if (entry.Value.StartsWith("git") || entry.Value.StartsWith("http"))
                            {
                                GitPackageInfo package = new GitPackageInfo();
                                package.name = entry.Key;
                                package.gitUrl = ExtractGitUrl(entry.Value);
                                package.path = ExtractPath(entry.Value);
                                package.branch = ExtractBranch(entry.Value);
                                package.isInstalled = true;
                                package.displayName = entry.Key.Split('.')[
                                    entry.Key.Split('.').Length - 1
                                ]; // 简单地从包ID提取名称

                                Debug.Log(
                                    $"Found Git package: {package.name}, URL: {package.gitUrl}"
                                );
                                result.Add(package);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("无法在manifest.json中找到dependencies部分");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"解析manifest.json时出错: {e.Message}\n{e.StackTrace}");
                }
            }
            else
            {
                Debug.LogError($"找不到manifest.json文件：{manifestPath}");
            }

            Debug.Log($"从manifest.json解析到{result.Count}个Git包");

            // 如果没有找到Git包，尝试添加一些常见的包作为示例
            if (result.Count == 0)
            {
                Debug.LogWarning("未检测到Git包，您可以手动添加Git包或尝试以下常见的Git包示例。");
            }

            return result;
        }

        /// <summary>
        /// 从URL中提取Git仓库地址
        /// </summary>
        private static string ExtractGitUrl(string packageUrl)
        {
            int questionMarkIndex = packageUrl.IndexOf('?');
            if (questionMarkIndex >= 0)
            {
                return packageUrl.Substring(0, questionMarkIndex);
            }

            int hashIndex = packageUrl.IndexOf('#');
            if (hashIndex >= 0)
            {
                return packageUrl.Substring(0, hashIndex);
            }

            return packageUrl;
        }

        /// <summary>
        /// 从URL中提取路径
        /// </summary>
        private static string ExtractPath(string packageUrl)
        {
            Match match = Regex.Match(packageUrl, @"path=([^&]*)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// 从URL中提取分支名
        /// </summary>
        private static string ExtractBranch(string packageUrl)
        {
            // 如果URL中包含#，后面的部分通常是分支或提交SHA
            int hashIndex = packageUrl.IndexOf('#');
            if (hashIndex >= 0)
            {
                string branchOrSha = packageUrl.Substring(hashIndex + 1);

                // 如果参数中还有其他部分，需要进一步分割
                int paramSeparator = branchOrSha.IndexOf('&');
                if (paramSeparator >= 0)
                {
                    return branchOrSha.Substring(0, paramSeparator);
                }

                return branchOrSha;
            }

            return "main"; // 默认分支
        }
    }
}
