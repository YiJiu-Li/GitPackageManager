````markdown
# Unity Git Package Manager

Unity Git Package Manager是一个强大的Unity编辑器扩展工具，用于管理Unity项目中的UPM包，特别是Git仓库包。它提供了直观的用户界面，支持包的安装、移除、分类和搜索功能。

## 功能特点

- 管理和跟踪Unity插件包（包括GitHub和本地包）
- 轻松安装/移除Git包
- 分类管理包
- 搜索和筛选包
- 多种导入方式：
  - 从manifest.json自动导入
  - 通过GitHub URL直接导入
  - 从本地文件夹导入
- 自定义分类管理
- 导入/导出包配置

## 安装方法

### 通过Unity Package Manager安装

1. 打开Unity项目
2. 打开Window > Package Manager
3. 点击左上角的"+"按钮
4. 选择"Add package from git URL..."
5. 输入以下URL:
   ```
   https://github.com/YiJiu-Li/GitPackageManager.git
   ```
6. 点击"Add"按钮完成安装

或者，您也可以在项目的`manifest.json`文件中添加以下依赖:

```json
{
  "dependencies": {
    "com.yijiu.gitpackagemanager": "https://github.com/YiJiu-Li/GitPackageManager.git",
    ...
  }
}
```

## 使用方法

1. 打开Git包管理器: `Window > Git Package > Package Manager`
2. 从多种方式导入包:
   - 从manifest.json: `Window > Git Package > Import From Manifest`
   - 从GitHub URL: `Window > Git Package > Import From GitHub URL`
   - 从本地文件夹: `Window > Git Package > Import From Local Folder`
3. 管理已有的包：安装、移除、编辑或删除
4. 使用分类和搜索功能快速查找包

查看[详细使用文档](Documentation~/usage.md)获取更多信息。

## 界面预览

![工具界面预览](Documentation~/screenshot.png)

## 贡献

欢迎提交Issue和Pull Request来帮助改进这个工具。

## 许可

本项目遵循MIT许可协议。详情请参阅[LICENSE](./LICENSE)文件。

## 作者

**依旧 (YiJiu-Li)**

- GitHub: [https://github.com/YiJiu-Li](https://github.com/YiJiu-Li)
