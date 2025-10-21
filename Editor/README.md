# Git Package Manager for Unity

这是一个Unity编辑器工具，用于管理Unity插件包（UPM包），支持Git仓库包、本地文件夹包及已安装的包管理。

## 功能

- 管理和跟踪Unity插件包（包括GitHub和本地包）
- 轻松安装/移除Git包
- 分类管理包
- 搜索和筛选包
- 多种导入方式：
  - 从manifest.json自动导入
  - 通过GitHub URL直接导入
  - 从本地文件夹导入
- 自定义分类管理

## 使用方法

1. 打开Git包管理器: `Window > Git Package > Package Manager`
2. 从多种方式导入包:
   - 从manifest.json: `Window > Git Package > Import From Manifest`
   - 从GitHub URL: `Window > Git Package > Import From GitHub URL`
   - 从本地文件夹: `Window > Git Package > Import From Local Folder`

### 添加新的Git包

1. 在Git包管理器窗口中，点击"添加Git包"按钮，或使用导入功能
2. 填写包的信息：
   - 包ID：包的唯一标识符（如：com.example.package）
   - 显示名称：在管理器中显示的名称
   - 描述：包的简要描述
   - Git URL：GitHub仓库的URL
   - 分支：使用的Git分支（默认为main）
   - 子目录路径：如果包在仓库的子目录中，填写相对路径
   - 版本：包的版本号
   - 分类：选择一个分类

### 管理已有的包

- **安装**：点击包旁边的"安装"按钮
- **移除**：点击包旁边的"移除"按钮
- **编辑**：点击包旁边的"编辑"按钮修改包的信息
- **删除**：点击包旁边的"删除"按钮从列表中删除包

### 管理分类

1. 点击Git包管理器窗口中的"设置"按钮
2. 在分类管理部分，可以：
   - 查看现有分类
   - 删除不需要的分类
   - 添加新的分类

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

## 导入/导出配置

### 导出包配置

将包配置导出为JSON文件，方便团队共享：

1. 选择 `Window > Git Package > Export Packages Config`
2. 选择要导出的包
3. 点击"导出选中的包配置"按钮
4. 选择保存位置

### 导入包配置

从之前导出的JSON文件导入包配置：

1. 选择 `Window > Git Package > Import Packages Config`
2. 选择之前导出的JSON文件
3. 点击"导入包配置"按钮

## 直接编辑Manifest.json

如果自动检测Git包失败，您可以直接查看和编辑manifest.json文件：

1. 选择 `Window > Git Package > Edit Manifest.json`
2. 在编辑器中查看和编辑manifest.json内容
3. 点击"保存"按钮保存更改
4. 然后尝试重新导入Git包

## 注意事项

- 包ID必须是唯一的
- Git URL应该是有效的GitHub/GitLab仓库URL
- 如果包在仓库的子目录中，需要填写正确的路径
- 导入本地包时，需要指定包含package.json的文件夹