using System;
using System.Collections.Generic;
using UnityEngine;

namespace GitPackageManager
{
    /// <summary>
    /// 表示Git包的信息
    /// </summary>
    [Serializable]
    public class GitPackageInfo
    {
        public string name; // 包名称
        public string displayName; // 显示名称
        public string description; // 描述
        public string gitUrl; // Git URL
        public string branch = "main"; // 分支，默认为main
        public string path; // 包在仓库中的路径（如果是子目录）
        public bool isInstalled; // 是否已安装
        public string version; // 版本号
        public string category; // 分类

        public string GetFullUrl()
        {
            string url = gitUrl;
            if (!string.IsNullOrEmpty(path))
            {
                url += $"?path={path}";
            }
            return url;
        }
    }

    /// <summary>
    /// 保存Git包的配置
    /// </summary>
    [CreateAssetMenu(
        fileName = "GitPackagesConfig",
        menuName = "Git Package Manager/Packages Config"
    )]
    public class GitPackageConfig : ScriptableObject
    {
        public List<GitPackageInfo> gitPackages = new List<GitPackageInfo>();

        // 保存所有包的分类
        public List<string> categories = new List<string>
        {
            "All",
            "Tools",
            "UI",
            "VFX",
            "Utilities",
            "Other",
        };
    }
}
