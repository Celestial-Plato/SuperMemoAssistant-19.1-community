# 源码与构建说明

这是遗留工程的公开源码快照和已验证二进制的发布仓库。完整旧解决方案在一台全新开发机器上的构建尚未验证。正常安装请使用 GitHub Release ZIP，而不是 GitHub 自动生成的 Source code ZIP。

原工程使用 Windows、.NET Framework 4.7.2、WPF/x86、MSBuild、VC++ 原生 Hook、自定义 `SuperMemoAssistant.Sdk.WindowsDesktop` 及历史 NuGet 包。发布时保留源码，排除本机预编译依赖与 NuGet 缓存；需要的第三方工具及历史包应从原项目和官方包源获取，并遵循其许可。

`nuget.config` 使用官方 NuGet，去除了本机用户缓存地址。两个工程去除了 `C:\LocalNuGet`，主工程的 Windows SDK `HintPath` 使用 `$(NuGetPackageRoot)`。这些是公开工程文件的路径整理，没有改变本次 Release 中已验证的程序集。

关键适配文件：

- `src/Core/SuperMemoAssistant.Core/NativeDataCfg.json`：19.1 特征、地址与结构配置。
- `src/Core/SuperMemoAssistant.Core/SuperMemo/Common/Content/Layout/XamlControls/XamlControlHtml.xaml.cs`：HTML 编码和持久源文件。
- `src/Core/SuperMemoAssistant.Core/SuperMemo/Common/Elements/ElementRegistryBase.cs`：元素导入及原生调用前文件验证。
- `src/Plugins/SuperMemoAssistant.Plugins.PDF/src/SuperMemoAssistant.Plugins.PDF/`：PDF 导入、识别、标题与元数据格式。
- `distribution/`：完整安装包的安装与恢复流程；需配合 Release 的 `manifest.json` 和 `payload` 使用。

本次发布保留已验证的 Hook 和依赖二进制，只重新编译必要的托管程序集。程序集身份保持 `2.1.0.0`；社区批次号由更新日志和发布清单标识。Release 的 `manifest.json` 列出全部包内文件的 SHA-256，外部 `.zip.sha256` 校验压缩包。

不要将插件工程直接生成的 SDK 文件当作本项目完整分享包发布；本次完整包单独排除了商业 SDK，本项目安装器按固定版本从官方 NuGet 下载。
