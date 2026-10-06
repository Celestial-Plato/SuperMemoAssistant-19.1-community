# SuperMemoAssistant 19.1 Community

[简体中文](README.md) | **English**

This project adapts the recovered legacy source of [supermemo/SuperMemoAssistant](https://github.com/supermemo/SuperMemoAssistant) for **SuperMemo 19.1**, with compatibility fixes and repairs to PDF importing. The PDF plugin is based on [supermemo/SuperMemoAssistant.Plugins.PDF](https://github.com/supermemo/SuperMemoAssistant.Plugins.PDF). Credit belongs to the original authors and contributors, and their MIT copyright notices are retained. This is an independent community release, not an official upstream release.

## Download and installation

Download **SMA-19.1-community-full-r11.2.zip** and its `.zip.sha256` file from [GitHub Releases](https://github.com/Celestial-Plato/SuperMemoAssistant-19.1-community/releases/latest). The full package includes the SMA application, runtime dependencies, and PDF plugin. An existing SMA installation is not required.

1. Save your work and close SuperMemo and SMA. Extract the entire ZIP.
2. Double-click `Install.cmd` and select your `sm19.exe`.
3. For a fresh installation, review and accept the applicable PDF SDK license when prompted. The installer obtains a fixed SDK version from official NuGet. Compatible existing installations reuse their SDK.
4. Open the `SMA 19.1 Community r11.2` desktop shortcut and select your collection as needed.

Use the same installer to upgrade or repair a compatible existing installation. It preserves settings and the collection list, backs up replaced files, and rolls back changes if installation fails. You do not need to delete your existing data first. Do not launch the EXE directly from the extracted package.

See the [installation guide](docs/installation.md) (in Chinese) for detailed instructions, offline SDK options, and backup locations. The repository's `distribution` scripts require the Release ZIP's `payload` directory and `manifest.json`; the scripts alone are not a complete installer. GitHub's automatically generated **Source code** archives are not installation packages.

## Verified compatibility

- Windows with .NET Framework 4.7.2 or later. SMA runs as an x86 application.
- SuperMemo **19.1**, with `sm19.exe` CRC32 **`0B6A9DA4`**.
- Verified EXE SHA-256: `CAC24B8209C2E25D6D1A940C204B95AA3327E4F298039B3088FE8FCB3253D751`. The installer rejects files with a different checksum. Other builds of SuperMemo 19.1 have not been verified.
- Startup, injection, and importing PDFs with Chinese and Japanese filenames were verified on the development machine. HTML title and metadata save/load round trips, installation, upgrades, backups, and recovery passed checks in isolated directories. Testing on other computers and regression testing of all optional plugins remain incomplete.

## Changes in this adaptation

- Checked method signatures and global pointers against the SuperMemo 19.1 disassembly and updated `NativeDataCfg.json`. All 35 method signatures matched uniquely in the verified executable.
- Corrected relevant memory offsets, mode field reads, and the child-element limit pointer.
- Used the Unicode title interface for Chinese titles and BOM-free UTF-8 with HTML entity escaping, fixing question marks in titles, garbled prefixes, and damaged markup.
- Displayed the PDF filename once in the element body and stored metadata in an invisible HTML comment.
- Added specific failure messages and logging for PDF imports, and ensured the import lock is released.
- **r11.2:** Stored HTML source files persistently under the collection's `sma/ImportedHtml` directory with unique filenames. Checked source files before native calls, fixing import failures caused by a missing `C:\WINDOWS\TEMP\sm_element_0.htm`.
- Added installation upgrades and recovery that preserve settings, removed temporary C# compilation from EXE validation, and handled notification registration permission errors.

See the [community changelog](CHANGELOG.md), [SMA historical and adaptation logs](ChangeLogs), and [PDF plugin logs](src/Plugins/SuperMemoAssistant.Plugins.PDF/ChangeLogs) for the full history. Community documentation is currently in Chinese; historical logs also contain English entries. Previously damaged titles and HTML are not automatically repaired in bulk. Keep the collection's `sma/ImportedHtml` files with the collection.

## Source and development

This repository contains recovered source snapshots of SMA, the PDF plugin, and related dependencies. Former submodules are retained as source directories; see [project provenance and licenses](docs/provenance.md) (in Chinese). The adapted [NativeDataCfg.json](src/Core/SuperMemoAssistant.Core/NativeDataCfg.json) is in Core.

The legacy projects depend on .NET Framework, MSBuild, VC++, a custom SMA SDK, and historical NuGet versions. Machine-local caches, precompiled binaries, and personal paths were removed from the public snapshot. Building the entire legacy solution on a clean development machine using only public NuGet sources has not been verified. See the [source and build notes](docs/build.md) (in Chinese). For normal use, download the full package from Releases.

## Licensing and PDF SDK

SMA and its open-source plugins retain their respective licenses. The root [MIT License](LICENSE) is from the original project. Third-party dependencies remain subject to their own licenses and should not all be treated as MIT-licensed.

PDF rendering uses **Patagames Pdfium.Net.SDK 4.53.2704**, a commercial SDK subject to its trial and licensing terms. The full package does not bundle this SDK; the installer obtains and verifies it separately from official NuGet. It does not supply a license key or remove trial restrictions. See the [SDK EULA](https://pdfium.patagames.com/faq/eula/).

Published files do not include the SuperMemo application, collections, personal settings, logs, PDF documents, disassembly files, or private licenses.
