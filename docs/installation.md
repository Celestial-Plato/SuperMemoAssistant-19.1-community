# SuperMemoAssistant 19.1 社区完整包 r11.2

日期：2026-10-06。包含完整 SMA 主程序、运行依赖和修复后的 PDF 插件，不需要预装 SMA。程序集版本保持 2.1.0.0，以兼容原组件。r11.2 整合 HTML 源文件路径修复，并保留恢复及覆盖升级流程。

本社区版本基于 [supermemo/SuperMemoAssistant](https://github.com/supermemo/SuperMemoAssistant) 的遗留源码进行 SuperMemo 19.1 适配；PDF 插件基于 [supermemo/SuperMemoAssistant.Plugins.PDF](https://github.com/supermemo/SuperMemoAssistant.Plugins.PDF)。感谢原作者及贡献者，保留原 MIT 版权声明。这是独立社区发布，不是原项目官方版本。

## 安装条件

- Windows，.NET Framework 4.7.2 或更高版本。
- 全新安装时，安装盘和数据盘各留至少 800 MB 空间；首次下载的临时文件也需要空间。恢复程序时，安装盘至少留 200 MB。
- 自备 SuperMemo 19.1；`sm19.exe` 的 CRC32 必须是 **0B6A9DA4**。其他同版本文件尚未验证，安装脚本会拒绝不匹配的文件。
- 安装器直接核对已验证 EXE 的 SHA-256：`CAC24B8209C2E25D6D1A940C204B95AA3327E4F298039B3088FE8FCB3253D751`，不再生成临时 C# 源文件或调用编译器。
- 首次安装需要联网，从 NuGet 官方地址获取 **Pdfium.Net.SDK 4.53.2704**。也可指定自己从官方取得的同版本 `.nupkg`，安装时校验 SHA-256。
- 没有旧数据时，全新安装。已有兼容的配置和 PDF 插件时，自动恢复或覆盖升级程序，先备份旧程序及需要更新的 PDF DLL。无需用户自行删除。

## 双击安装

1. 保存并关闭 SM 和 SMA。解压整个 ZIP，不要在压缩软件里直接运行。
2. 双击 `Install.cmd`，选择自己的 `sm19.exe`。
3. 全新安装需要阅读 PDF SDK 许可；符合其许可条件并同意时输入 `ACCEPT`，脚本会从官方 NuGet 下载固定版本。恢复程序时，脚本核对已有依赖并直接复用，不下载 SDK、不重新接受许可。
4. 安装成功后，打开桌面的 `SMA 19.1 Community r11.2` 快捷方式。全新安装在首次设置中阅读 SMA 许可并选择自己的集合；恢复程序继续使用原来的设置和集合列表。

通常用普通 Windows 账户即可。程序必须位于 SMA 认可的安装目录，所以不要从解压目录直接打开 EXE。

```text
程序：%LOCALAPPDATA%\SuperMemoAssistant\app-19.1-community-r11.1-test
配置、插件、日志：%USERPROFILE%\SuperMemoAssistant
```

程序目录沿用 `app-19.1-community-r11.1-test`，便于从 r11.1 就地覆盖升级；发布包和快捷方式标注当前 r11.2 版本。

如果账户已有 `supermemoassistant.json` 自定义数据位置，安装器遵循其中 `AppDataDirPath`，不会改写该文件。全新安装的 Core 和插件自动更新均关闭。恢复程序保留已有更新设置；程序目录的 `-test` 后缀阻止 Core 自动更新覆盖适配文件，手动或自动升级 PDF 插件后仍须核对适配情况。

## 保留旧数据恢复或覆盖升级

直接运行同一个 `Install.cmd` 即可。恢复模式会核对原配置中的 SM 路径、PDF 插件版本、依赖及 SDK 校验值。配置不完整或组件不匹配时会给出具体文件，不重置旧数据。

通过检查后，安装器先暂存并校验程序，备份 `Configs` 和 `Plugins/plugins.json`，必要时备份并更新 PDF DLL。已有程序目录移动到备份中，新程序再替换到原路径。失败时自动恢复旧程序与已替换的 PDF DLL。备份位置会打印在窗口中：

```text
%LOCALAPPDATA%\SuperMemoAssistant\RecoveryBackups\r11.2-日期-随机编号
```

恢复或升级不改写配置、集合列表、插件注册、日志或授权设置。只在校验值不同时更新 PDF 插件 DLL，其他插件保持原状。旧程序位于备份的 `PreviousProgram`；旧 PDF DLL（如有替换）位于 `PDFPlugin`。不需要删除旧数据，不需要另建 Windows 账户。

原版 SMA 已有主程序（Squirrel）及插件（NuGet）升级机制。当前社区 PDF DLL 沿用 `2.1.0-beta.14`，原版界面按包版本判断更新，无法识别 r10/r11 等文件修复批次。当前请通过本完整包升级；以后要接入插件界面更新，需要独立社区版本号和发布源，不能把未发布的社区包当作官方源中的新版。

也可用 PowerShell：

```powershell
# 只核对文件、兼容性和安装条件，不安装、不下载
powershell -NoProfile -ExecutionPolicy Bypass -File .\Install-Full.ps1 -SuperMemoExe "C:\SuperMemo\sm19.exe" -WhatIf

# 使用自己从官方取得的 SDK 包
powershell -NoProfile -ExecutionPolicy Bypass -File .\Install-Full.ps1 -SuperMemoExe "C:\SuperMemo\sm19.exe" -SdkPackage "D:\Downloads\pdfium.net.sdk.4.53.2704.nupkg"
```

## 本次改动

- r11.2：HTML 源文件保存在集合的 `sma/ImportedHtml`，每次使用唯一文件名并保留，不再复用 Windows TEMP 中的同名文件；导入前检查文件存在，调用前后记录文件状态。用户已确认中文和日文文件名 PDF 重新导入成功。
- r11.1：新增保留旧数据恢复及覆盖升级、旧程序和 PDF DLL 备份、依赖校验、暂存校验及失败回滚流程。
- r11.1 安装器修正：移除 EXE 校验中的 `Add-Type -TypeDefinition`，解决临时 `.cs` 文件丢失导致安装中断的问题。程序、Core、Hook 和 PDF 导入逻辑沿用原适配结果。
- 基于 19.1 IDR 反汇编核对方法特征和全局指针；35 个方法特征唯一匹配。
- 修正 FileSpace 偏移、学习模式字段读取及子元素数量限制指针。
- PDF 导入检查实际元素创建结果，失败时显示提示和日志，并始终释放导入锁。
- 中文标题通过 SuperMemo 的 Unicode 标题接口写入，避免变为问号。
- HTML 改为无 BOM 的 UTF-8，保留实体转义，解决开头乱码和残缺标签。
- 正文只显示一次文件名；Base64 元数据存放在不可见的 HTML 注释里，保持原有识别和保存格式。
- 通知注册权限异常不再阻止启动。
- 更新程序内及 PDF 更新日志，增加完整运行文件、安装入口和 SHA-256 校验清单。

更完整的版本记录见 `CHANGELOG.txt` 和 `PDF-CHANGELOG.txt`。旧的问号标题或损坏 HTML 不会自动批量修复。

新生成的 HTML 源文件会随集合保留，不要删除仍被元素引用的文件。无集合时使用 SMA 数据目录的 `Data/ImportedHtml`。路径修复不会解除 PDF SDK 的试用限制。

## PDF SDK 许可与包内范围

SMA 和 PDF 插件的许可文件在 `licenses`，依赖包内的许可与 NuGet 元数据保留。PDF 插件包为本次重新打包的未签名社区修改版，已替换 DLL 并移除原包中的 SDK 原生文件，不是原作者发布的原始 NuGet 包。

PDF 渲染依赖 Patagames 的商业 SDK。包中不重新分发这份 SDK，安装器从官方 NuGet 获取固定版本；下载本身不授予长期使用或再分发授权。试用限制及适用条件须遵循随 SDK 提供的许可和官方 EULA：

https://pdfium.patagames.com/faq/eula/

安装器不会填入授权密钥，也不会消除注册提示。需要公开分发已包含 SDK 的离线版时，应先确认相应的再分发授权。

本包包含完整 SMA 和 PDF 插件。其他可选插件没有预装，可按自己的需要另外安装；它们未完成本次 19.1 回归测试。包内不包含 SuperMemo 程序、集合、个人配置、日志、API 密钥或私人授权。

## 验证范围与反馈

本机此前已观察到启动/注入、PDF 元素创建/识别、中文标题更新成功。r11.2 安装后，用户确认两个 PDF 导入恢复正常。HTML 单次标题显示、元数据隐藏、识别及保存往返通过 MSHTML 检查；唯一源文件、无 TEMP 依赖、备份和恢复也通过检查。安装脚本与完整包进行了校验和独立目录安装检查；这不等同于另一台电脑上的首次启动或所有 PDF 提取功能测试。

接收者请用集合副本检查：启动、导入中文文件名 PDF、关闭后重开 PDF 元素、创建提取并保存。热键以自己的配置为准，可在插件设置里核对。遇到问题提供对应日志及 `sm19.exe` CRC32，勿直接分享整个个人配置目录。

## 分享与移除

分享整个 `SMA-19.1-community-full-r11.2.zip` 和同目录 `.zip.sha256` 文件，附上本说明。可以用 `Get-FileHash -Algorithm SHA256` 核对 ZIP。校验值用于检测传输损坏，不替代发布者签名。

安装器不会修改 SuperMemo 程序及集合。需要停用时，保存并退出 SM/SMA，停止使用社区快捷方式；移除新安装的程序目录和桌面快捷方式即可。数据目录可保留备份。集合中已导入的 PDF 元素属于你的集合，移除 SMA 不会撤销它们。
