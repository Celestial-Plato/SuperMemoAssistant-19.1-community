# SuperMemoAssistant 19.1 Community

基于 [supermemo/SuperMemoAssistant](https://github.com/supermemo/SuperMemoAssistant) 的遗留源码，针对 **SuperMemo 19.1** 进行兼容性适配与 PDF 导入修复。PDF 插件基于 [supermemo/SuperMemoAssistant.Plugins.PDF](https://github.com/supermemo/SuperMemoAssistant.Plugins.PDF)。感谢原作者和贡献者，保留原 MIT 版权声明。本仓库为独立社区发布，不是原项目官方版本。

## 下载和安装

在 [GitHub Releases](https://github.com/Celestial-Plato/SuperMemoAssistant-19.1-community/releases/latest) 下载 **SMA-19.1-community-full-r11.2.zip** 和对应 `.zip.sha256`。这是包含 SMA 主程序、运行依赖和 PDF 插件的完整包，无需预装 SMA。

1. 保存并关闭 SuperMemo 和 SMA，完整解压 ZIP。
2. 双击 `Install.cmd`，选择自己的 `sm19.exe`。
3. 全新安装按提示阅读并接受适用的 PDF SDK 许可；安装器从官方 NuGet 获取固定版本。已有兼容安装会复用 SDK。
4. 打开桌面的 `SMA 19.1 Community r11.2` 快捷方式，按需选择自己的集合。

已有兼容安装也运行同一个安装器，支持保留配置和集合列表的覆盖升级、程序恢复、备份与失败回滚，无需先删除旧数据。不要从解压目录直接启动 EXE。

详细步骤、离线 SDK 参数及备份位置见 [安装说明](docs/installation.md)。仓库中的 `distribution` 脚本需要 Release ZIP 的 `payload` 和 `manifest.json`，不能单独作为完整安装包运行。

## 已验证的兼容范围

- Windows，.NET Framework 4.7.2 或更高版本；SMA 为 x86 程序。
- SuperMemo **19.1**，`sm19.exe` CRC32：`0B6A9DA4`。
- 已验证 EXE 的 SHA-256：`CAC24B8209C2E25D6D1A940C204B95AA3327E4F298039B3088FE8FCB3253D751`。安装器会拒绝校验值不匹配的文件；其他 19.1 构建尚未验证。
- 本机已验证启动、注入及中文和日文文件名 PDF 的导入。HTML 标题与元数据保存往返、安装、升级、备份及恢复通过独立目录检查。其他机器和全部可选插件尚未完成回归测试。

## 本次适配

- 根据 19.1 反汇编核对方法特征和全局指针，更新 `NativeDataCfg.json`，35 个方法特征唯一匹配。
- 修正相关内存偏移、模式字段读取及子元素数量限制指针。
- 中文标题使用 Unicode 接口；HTML 使用无 BOM 的 UTF-8 和实体转义，修复问号标题、开头乱码及残缺标签。
- PDF 正文只显示一次文件名，元数据保存在不可见的 HTML 注释中。
- PDF 导入失败显示具体结果并记录日志，释放导入锁。
- **r11.2**：HTML 源文件保存在集合的 `sma/ImportedHtml`，使用唯一文件名并保留；在原生调用前检查源文件，修复 `C:\WINDOWS\TEMP\sm_element_0.htm` 缺失引起的导入失败。
- 安装器支持保留配置的升级和恢复，移除临时 C# 编译校验，处理通知注册权限异常。

完整记录见 [社区更新日志](CHANGELOG.md)、[SMA 原有及适配日志](ChangeLogs) 与 [PDF 插件日志](src/Plugins/SuperMemoAssistant.Plugins.PDF/ChangeLogs)。旧的损坏标题和 HTML 不会自动批量修复。集合中的 `sma/ImportedHtml` 文件需随集合保留。

## 源码与开发

本仓库包含恢复的 SMA、PDF 插件及相关依赖源码快照。原来的子模块以源码目录保留，来源见 [项目来源与许可](docs/provenance.md)。`NativeDataCfg.json` 位于 [Core](src/Core/SuperMemoAssistant.Core/NativeDataCfg.json)。

旧工程依赖 .NET Framework、MSBuild、VC++、自定义 SMA SDK 和历史 NuGet 版本。公开快照移除了本机缓存、预编译二进制及个人路径；尚未验证在全新开发机器上仅靠公共 NuGet 即可构建完整旧解决方案。开发说明见 [源码与构建](docs/build.md)。使用者请下载 Release 的完整包。

## 许可与 PDF SDK

SMA 与开源插件保留各自许可，根目录为原项目 [MIT License](LICENSE)。第三方依赖遵循各自许可，不能统一视为 MIT。

PDF 渲染使用 **Patagames Pdfium.Net.SDK 4.53.2704**，属于商业 SDK，适用其试用及授权条件。完整包不包含这份 SDK；安装器从官方 NuGet 单独获取并校验。不会填入授权密钥，也不会解除试用限制，详见 [SDK EULA](https://pdfium.patagames.com/faq/eula/)。

发布内容不含 SuperMemo 程序、集合、个人配置、日志、PDF 文档、反汇编文件或私人授权。
