# Unity Git Package Manager 使用文档

Unity Git Package Manager是一个功能强大的Unity编辑器扩展工具，用于管理Unity项目中的UPM包，特别是Git仓库包。本文档提供了详细的使用说明和配置选项。

## 打开工具

打开工具窗口有以下几种方式：

1. 通过菜单：Window > Git Package > Package Manager
2. 通过代码：`EditorWindow.GetWindow<GitPackageManager.GitPackageManagerWindow>("Git Package Manager");`

## 基本功能

### 1. 浏览和管理包

在主界面上，您可以：

- 查看所有已添加的包
- 通过分类筛选包
- 使用搜索功能查找特定包
- 安装、移除、编辑或删除包

### 2. 添加新的Git包

1. 在Git包管理器窗口中，点击"添加Git包"按钮
2. 填写包的信息：
   - 包ID：包的唯一标识符（如：com.example.package）
   - 显示名称：在管理器中显示的名称
   - 描述：包的简要描述
   - Git URL：GitHub仓库的URL
   - 分支：使用的Git分支（默认为main）
   - 子目录路径：如果包在仓库的子目录中，填写相对路径
   - 版本：包的版本号
   - 分类：选择一个分类
3. 点击"保存"按钮添加包

## 导入包的多种方式

### 从Manifest导入

如果您的项目已经使用了一些Git包，可以自动检测并导入它们：

1. 选择 `Window > Git Package > Import From Manifest`
2. 选中要导入的包
3. 补充包的信息（显示名称、描述等）
4. 点击"导入选中的包"按钮

### 从GitHub URL导入

直接从GitHub仓库URL导入：

1. 选择 `Window > Git Package > Import From GitHub URL`
2. 输入GitHub仓库URL
3. 填写包信息
4. 点击"导入到Git包管理器"按钮

### 从本地文件夹导入

从本地文件系统导入包：

1. 选择 `Window > Git Package > Import From Local Folder`
2. 浏览选择包含package.json的文件夹
3. 确认包信息（会自动从package.json读取）
4. 点击"导入到Git包管理器"按钮

## 包分类管理

### 查看和编辑分类

1. 在Git包管理器窗口中，点击"设置"按钮
2. 在分类管理部分，可以：
   - 查看现有分类
   - 删除不需要的分类
   - 添加新的分类

### 为包分配分类

1. 添加或编辑包时，从分类下拉菜单中选择分类
2. 保存更改后，包将显示在所选分类中

## 疑难解答

如果遇到以下问题，请尝试对应的解决方案：

1. **包安装失败**：
   - 检查Git URL是否有效
   - 确保分支名称正确
   - 验证子目录路径（如有）是否正确

2. **包未显示在管理器中**：
   - 点击"刷新"按钮更新安装状态
   - 检查搜索和分类筛选条件
   - 确保正确导入了包配置

3. **导入manifest.json中的包失败**：
   - 尝试直接编辑manifest.json
   - 确保JSON格式正确无误
   - 手动添加包信息

## 更多信息

有关更多信息和最新更新，请访问GitHub仓库：[https://github.com/YiJiu-Li/GitPackageManager](https://github.com/YiJiu-Li/GitPackageManager)
