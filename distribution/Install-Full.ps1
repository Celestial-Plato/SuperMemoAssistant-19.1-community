[CmdletBinding(SupportsShouldProcess=$true)]
param(
  [string]$SuperMemoExe,
  [string]$SdkPackage,
  [switch]$AcceptPdfSdkLicense,
  [switch]$NoShortcut
)
$ErrorActionPreference = 'Stop'
function Get-SMAFileHash {
  param([Parameter(Mandatory=$true)][string]$LiteralPath, [string]$Algorithm = 'SHA256')
  if ($Algorithm -ne 'SHA256') { throw 'Only SHA256 is supported.' }
  $hashStream = [IO.File]::OpenRead($LiteralPath)
  $hashProvider = [Security.Cryptography.SHA256]::Create()
  try { [pscustomobject]@{Hash=[BitConverter]::ToString($hashProvider.ComputeHash($hashStream)).Replace('-','')} }
  finally { $hashProvider.Dispose(); $hashStream.Dispose() }
}
$root = $PSScriptRoot
$installDir = Join-Path $env:LOCALAPPDATA 'SuperMemoAssistant/app-19.1-community-r11.1-test'
$dataParent = $env:USERPROFILE
$preInit = Join-Path $env:USERPROFILE 'supermemoassistant.json'
if (Test-Path -LiteralPath $preInit) {
  $cfg = Get-Content -LiteralPath $preInit -Raw | ConvertFrom-Json
  if (![string]::IsNullOrWhiteSpace($cfg.AppDataDirPath)) { $dataParent = [IO.Path]::GetFullPath($cfg.AppDataDirPath) }
}
$dataDir = Join-Path $dataParent 'SuperMemoAssistant'
$existingData = (Test-Path -LiteralPath (Join-Path $dataDir 'Configs')) -or (Test-Path -LiteralPath (Join-Path $dataDir 'Plugins'))
$driveName = [IO.Path]::GetPathRoot([IO.Path]::GetFullPath($installDir))
$targetDrive = New-Object IO.DriveInfo($driveName)
if ($targetDrive.AvailableFreeSpace -lt $(if ($existingData) { 200MB } else { 800MB })) { throw 'Insufficient free space on the installation drive (200 MB for recovery; 800 MB for fresh installation).' }
$dataDrive = New-Object IO.DriveInfo([IO.Path]::GetPathRoot([IO.Path]::GetFullPath($dataDir)))
if (!$existingData -and $dataDrive.AvailableFreeSpace -lt 800MB) { throw 'At least 800 MB of free space is required on the data drive.' }
if (Get-Process | Where-Object { $_.ProcessName -match '^(SuperMemoAssistant|sm19|PluginHost)$' }) { throw 'Save and close SM and SMA first.' }
if ((Test-Path -LiteralPath $installDir) -and !$existingData) {
  throw "The application directory exists but configuration is missing. Installation stopped to preserve it: $installDir"
}
$framework = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' -ErrorAction SilentlyContinue
if (!$framework -or $framework.Release -lt 461808) { throw 'Install .NET Framework 4.7.2 or newer first: https://dotnet.microsoft.com/download/dotnet-framework' }
if (!$SuperMemoExe -and $existingData) {
  $savedCorePath = Join-Path $dataDir 'Configs/Core/CoreCfg.json'
  if (Test-Path -LiteralPath $savedCorePath) {
    $savedCore = Get-Content -LiteralPath $savedCorePath -Raw | ConvertFrom-Json
    if ($savedCore.SuperMemo.SMBinPath -and (Test-Path -LiteralPath $savedCore.SuperMemo.SMBinPath)) { $SuperMemoExe = $savedCore.SuperMemo.SMBinPath }
  }
}
if (!$SuperMemoExe) {
  Add-Type -AssemblyName System.Windows.Forms
  $dialog = New-Object Windows.Forms.OpenFileDialog
  $dialog.Title = 'Select your SuperMemo 19.1 executable (sm19.exe)'
  $dialog.Filter = 'SuperMemo executable|sm19.exe'
  if ($dialog.ShowDialog() -ne 'OK') { throw 'No SuperMemo executable selected.' }
  $SuperMemoExe = $dialog.FileName
  $dialog.Dispose()
}
$smExe = (Resolve-Path -LiteralPath $SuperMemoExe).Path
# Pin the verified 19.1 executable without invoking CodeDom's temporary C# compiler.
# This SHA-256 belongs to the supported executable with CRC32 0B6A9DA4.
$supportedSmHash = 'CAC24B8209C2E25D6D1A940C204B95AA3327E4F298039B3088FE8FCB3253D751'
if ((Get-SMAFileHash -LiteralPath $smExe).Hash -ne $supportedSmHash) { throw 'Unsupported SuperMemo executable. Required CRC32: 0B6A9DA4; the verified executable SHA-256 must also match.' }
$manifest = Get-Content -LiteralPath (Join-Path $root 'manifest.json') -Raw | ConvertFrom-Json
foreach ($entry in $manifest.Files) {
  $source = [IO.Path]::GetFullPath((Join-Path $root $entry.Path))
  if (!$source.StartsWith($root.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid manifest path.' }
  if ((Get-SMAFileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $entry.SHA256) { throw "Checksum mismatch: $($entry.Path)" }
}
Write-Output "Verified SMA r11.2 and SuperMemo 19.1 / 0B6A9DA4. App: $installDir. Data: $dataDir"
if ($existingData) {
  . (Join-Path $root 'Recovery.ps1')
  Restore-SMAProgram -InstallerRoot $root -InstallDirectory $installDir -DataDirectory $dataDir -SuperMemoExe $smExe -NoShortcut:$NoShortcut -WhatIf:$WhatIfPreference
  return
}
if (!$PSCmdlet.ShouldProcess($installDir, 'Install full SMA and PDF plugin with fresh settings')) { return }
if (!$AcceptPdfSdkLicense) {
  Write-Output 'PDF rendering uses the commercial Pdfium.Net.SDK 4.53.2704. Downloading does not grant a production or redistribution license.'
  Write-Output 'Read licenses/Pdfium.Net.SDK-license.md and https://pdfium.patagames.com/faq/eula/ . Trial limitations apply without the appropriate license.'
  if ((Read-Host 'Type ACCEPT to install the SDK under its license; anything else cancels') -cne 'ACCEPT') { throw 'SDK license was not accepted. No application files were installed.' }
}
$workDir = Join-Path ([IO.Path]::GetTempPath()) ('SMA-r11.2-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $workDir | Out-Null
$sdkZip = Join-Path $workDir 'sdk.zip'
if ($SdkPackage) {
  Copy-Item -LiteralPath (Resolve-Path -LiteralPath $SdkPackage).Path -Destination $sdkZip
} else {
  [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
  Invoke-WebRequest -UseBasicParsing -Uri $manifest.PdfSdk.Url -OutFile $sdkZip
}
if ((Get-SMAFileHash -LiteralPath $sdkZip -Algorithm SHA256).Hash -ne $manifest.PdfSdk.SHA256) { throw 'PDF SDK checksum mismatch. No application files were installed.' }
$sdkDir = Join-Path $workDir 'sdk'
Expand-Archive -LiteralPath $sdkZip -DestinationPath $sdkDir
foreach ($name in @('pdfium.net.sdk.nuspec','build/icudt.dll','build/x86/pdfium.dll','lib/net472/Patagames.Pdf.dll')) {
  if (!(Test-Path -LiteralPath (Join-Path $sdkDir $name))) { throw "Incomplete PDF SDK: $name" }
}
if (Get-Process | Where-Object { $_.ProcessName -match '^(SuperMemoAssistant|sm19|PluginHost)$' }) { throw 'SM/SMA started during validation. Close it and retry.' }
if ((Test-Path -LiteralPath $installDir) -or (Test-Path -LiteralPath (Join-Path $dataDir 'Configs')) -or (Test-Path -LiteralPath (Join-Path $dataDir 'Plugins'))) { throw 'Destination changed during validation. No files were installed.' }
New-Item -ItemType Directory -Path $installDir -Force | Out-Null
Copy-Item -Path (Join-Path $root 'payload/app/*') -Destination $installDir -Recurse
New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
Copy-Item -Path (Join-Path $root 'payload/data/*') -Destination $dataDir -Recurse
$packagesDir = Join-Path $dataDir 'Plugins/Packages'
$installedSdk = Join-Path $packagesDir 'Pdfium.Net.SDK.4.53.2704'
Copy-Item -LiteralPath $sdkDir -Destination $installedSdk -Recurse
Copy-Item -LiteralPath $sdkZip -Destination (Join-Path $installedSdk 'Pdfium.Net.SDK.4.53.2704.nupkg')
$pdfHome = Join-Path $dataDir 'Plugins/Home/SuperMemoAssistant.Plugins.PDF'
New-Item -ItemType Directory -Path (Join-Path $pdfHome 'x86') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $installedSdk 'build/icudt.dll') -Destination $pdfHome
Copy-Item -LiteralPath (Join-Path $installedSdk 'build/x86/pdfium.dll') -Destination (Join-Path $pdfHome 'x86')
$plugins = Get-Content -LiteralPath (Join-Path $dataDir 'Plugins/plugins.json') -Raw | ConvertFrom-Json
$plugins.Plugins[0].HomeDir = $pdfHome.Replace('\','/')
$plugins | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath (Join-Path $dataDir 'Plugins/plugins.json') -Encoding UTF8
$corePath = Join-Path $dataDir 'Configs/Core/CoreCfg.json'
$coreCfg = Get-Content -LiteralPath $corePath -Raw | ConvertFrom-Json
$coreCfg.SuperMemo.SMBinPath = $smExe
$coreCfg | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath $corePath -Encoding UTF8
foreach ($entry in $manifest.Files | Where-Object { $_.Path.StartsWith('payload/app/') }) {
  if ((Get-SMAFileHash -LiteralPath (Join-Path $installDir $entry.Path.Substring(12))).Hash -ne $entry.SHA256) { throw "Installed file checksum mismatch: $($entry.Path)" }
}
$exe = Join-Path $installDir 'SuperMemoAssistant.exe'
if (!$NoShortcut) {
  $shell = New-Object -ComObject WScript.Shell
  $shortcut = $shell.CreateShortcut((Join-Path ([Environment]::GetFolderPath('Desktop')) 'SMA 19.1 Community r11.2.lnk'))
  $shortcut.TargetPath = $exe
  $shortcut.WorkingDirectory = $installDir
  $shortcut.Save()
}
Write-Output "Installed. Start: $exe"
Write-Output 'Open SMA, review its license, and select your own collection. Core/plugin automatic updates are disabled for this community build.'
Write-Output "Temporary SDK download retained at $workDir. You may remove that folder after installation."
