# 项目来源与许可

本次发布以用户保留的 SuperMemoAssistant 遗留源码及构建资料为基础，针对 SuperMemo 19.1 重新适配。恢复资料没有完整的顶层 Git 提交历史，因此本仓库是源码快照，不声称与上游某一提交完全对应。

- SMA 主项目：https://github.com/supermemo/SuperMemoAssistant （保留根目录 MIT 许可）。
- PDF 插件：https://github.com/supermemo/SuperMemoAssistant.Plugins.PDF （保留插件目录的 MIT 许可）。
- Interop：https://github.com/supermemo/SuperMemoAssistant.Interop
- SMA SDK：https://github.com/supermemo/SuperMemoAssistant.Sdk
- SDK Visual Studio 模板：https://github.com/supermemo/SuperMemoAssistant.Sdk.VisualStudio
- Process.NET：https://github.com/supermemo/Process.NET
- pngcs：https://github.com/supermemo/pngcs
- PluginManager：https://github.com/alexis-/PluginManager.Net （legacy-net-framework）
- MSBuild.Tools：https://github.com/alexis-/MSBuild.Tools
- Extensions.System.IO：https://github.com/alexis-/Extensions.System.IO
- Squirrel.Windows：https://github.com/supermemo/Squirrel.Windows
- VisualStudio.GitHooks：https://github.com/alexis-/VisualStudio.GitHooks

其余可选插件与 Services 的原项目链接保留在各自 README 和工程文件中。各目录保留原许可和版权声明；其是否兼容 19.1 不由源码存在这一事实保证。

公开源码不含二进制依赖、原子模块 Git 元数据、Squirrel 的旧二进制测试夹具与 vendor 工具、NuGet 缓存、签名密钥及本机配置。完整安装包在 Release 中提供，包内 `licenses` 保留依赖许可及 NuGet 元数据。

Pdfium.Net.SDK 为第三方商业 SDK，未收录其二进制。PDF WPF 开源包装的许可保留于原目录，不代表 SDK 本体也为 MIT。安装器单独获取固定的官方 SDK 包并校验，不授予额外授权。
