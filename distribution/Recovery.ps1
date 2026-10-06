# Invoked after Install-Full.ps1 has verified the package manifest and SuperMemo CRC.
function Restore-SMAProgram {
  [CmdletBinding(SupportsShouldProcess=$true)]
  param([string]$InstallerRoot, [string]$InstallDirectory, [string]$DataDirectory, [string]$SuperMemoExe, [switch]$NoShortcut)
  $ErrorActionPreference = 'Stop'
  $manifest = Get-Content -LiteralPath (Join-Path $InstallerRoot 'manifest.json') -Raw | ConvertFrom-Json
  $corePath = Join-Path $DataDirectory 'Configs/Core/CoreCfg.json'
  $pluginPath = Join-Path $DataDirectory 'Plugins/plugins.json'
  foreach ($path in @($corePath, $pluginPath)) {
    if (!(Test-Path -LiteralPath $path -PathType Leaf)) { throw "Existing data is incomplete: $path. Recovery stopped; existing data was preserved." }
  }
  $coreCfg = Get-Content -LiteralPath $corePath -Raw | ConvertFrom-Json
  if (!$coreCfg.SuperMemo.SMBinPath -or [IO.Path]::GetFullPath($coreCfg.SuperMemo.SMBinPath) -ne [IO.Path]::GetFullPath($SuperMemoExe)) {
    throw 'The saved SuperMemo path differs from the selected executable. Recovery will not rewrite your settings. Select the saved executable or correct its path in your configuration.'
  }
  $plugins = Get-Content -LiteralPath $pluginPath -Raw | ConvertFrom-Json
  $pdfPlugins = @($plugins.Plugins | Where-Object { $_.Id -eq 'SuperMemoAssistant.Plugins.PDF' })
  if ($pdfPlugins.Count -ne 1 -or $pdfPlugins[0].Version -ne '2.1.0-beta.14') { throw 'Recovery requires the existing PDF 2.1.0-beta.14 plugin. Your plugin registry was preserved.' }
  $template = Get-Content -LiteralPath (Join-Path $InstallerRoot 'payload/data/Plugins/plugins.json') -Raw | ConvertFrom-Json
  foreach ($dependency in $template.Plugins[0].Dependencies) {
    if (!@($pdfPlugins[0].Dependencies | Where-Object { $_.Id -eq $dependency.Id -and $_.Version -eq $dependency.Version }).Count) {
      throw "Existing PDF dependency differs: $($dependency.Id). Recovery stopped without changing existing data."
    }
  }
  $pdfUpdates = @()
  foreach ($entry in $manifest.Files | Where-Object { $_.Path.StartsWith('payload/data/Plugins/Packages/') -and $_.Path -match '\.(dll|exe)$' }) {
    $destination = Join-Path $DataDirectory $entry.Path.Substring('payload/data/'.Length)
    if ($entry.Path -eq 'payload/data/Plugins/Packages/SuperMemoAssistant.Plugins.PDF.2.1.0-beta.14/lib/net472/SuperMemoAssistant.Plugins.PDF.dll') {
      if (!(Test-Path -LiteralPath $destination -PathType Leaf) -or (Get-SMAFileHash -LiteralPath $destination).Hash -ne $entry.SHA256) {
        $pdfUpdates += [pscustomobject]@{Source=(Join-Path $InstallerRoot $entry.Path); Destination=$destination; SHA256=$entry.SHA256; Existed=(Test-Path -LiteralPath $destination -PathType Leaf)}
      }
      continue
    }
    if (!(Test-Path -LiteralPath $destination -PathType Leaf)) { throw "Existing PDF dependency is missing: $destination. Recovery stopped without changing existing data." }
    if ((Get-SMAFileHash -LiteralPath $destination -Algorithm SHA256).Hash -ne $entry.SHA256) {
      throw "Existing PDF dependency differs from this community package: $destination. Recovery stopped without changing existing data."
    }
  }
  $sdkDir = Join-Path $DataDirectory 'Plugins/Packages/Pdfium.Net.SDK.4.53.2704'
  foreach ($entry in $manifest.PdfSdk.RuntimeFiles) {
    $path = Join-Path $sdkDir $entry.Path
    if (!(Test-Path -LiteralPath $path -PathType Leaf) -or (Get-SMAFileHash -LiteralPath $path).Hash -ne $entry.SHA256) {
      throw "Existing PDF SDK is missing or differs: $path. Recovery stopped; no SDK download or license acceptance was performed."
    }
  }
  if ([string]::IsNullOrWhiteSpace($pdfPlugins[0].HomeDir)) { throw 'Existing PDF home directory is missing from the registry.' }
  $pdfHome = [IO.Path]::GetFullPath($pdfPlugins[0].HomeDir)
  foreach ($entry in $manifest.PdfSdk.RuntimeFiles | Where-Object { $_.Path.StartsWith('build/') }) {
    $path = Join-Path $pdfHome $entry.Path.Substring('build/'.Length)
    if (!(Test-Path -LiteralPath $path -PathType Leaf) -or (Get-SMAFileHash -LiteralPath $path).Hash -ne $entry.SHA256) { throw "Existing PDF native dependency is missing or differs: $path. Existing data was preserved." }
  }
  $hadProgram = Test-Path -LiteralPath $InstallDirectory -PathType Container
  Write-Output "Mode: $(if ($hadProgram) { 'Upgrade/repair' } else { 'Restore' }). Existing settings, collections and SDK are retained. PDF DLL updates required: $($pdfUpdates.Count)."
  if (!$PSCmdlet.ShouldProcess($InstallDirectory, 'Back up and replace the SMA program; update the PDF DLL when necessary; preserve personal settings')) { return }
  if ((Get-Process | Where-Object { $_.ProcessName -match '^(SuperMemoAssistant|sm19|PluginHost)$' }) -or (Test-Path -LiteralPath $InstallDirectory -PathType Container) -ne $hadProgram) { throw 'The destination or running processes changed during validation. No files were restored.' }
  $appRoot = [IO.Path]::GetFullPath((Split-Path $InstallDirectory -Parent)).TrimEnd('\')
  $installPath = [IO.Path]::GetFullPath($InstallDirectory)
  if (!$installPath.StartsWith($appRoot + '\', [StringComparison]::OrdinalIgnoreCase) -or !(Split-Path $installPath -Leaf).EndsWith('-test')) { throw 'Invalid recovery application directory.' }
  $tag = (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N')
  $stageDir = Join-Path $appRoot ('recovery-staging-' + $tag)
  $backupDir = Join-Path $appRoot ('RecoveryBackups/r11.2-' + $tag)
  $previousProgram = [IO.Path]::GetFullPath((Join-Path $backupDir 'PreviousProgram'))
  if (!$previousProgram.StartsWith([IO.Path]::GetFullPath($backupDir).TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid program backup path.' }
  New-Item -ItemType Directory -Path $stageDir -Force | Out-Null
  $movedOldProgram = $false
  $writtenPdf = @()
  try {
    Copy-Item -Path (Join-Path $InstallerRoot 'payload/app/*') -Destination $stageDir -Recurse
    foreach ($entry in $manifest.Files | Where-Object { $_.Path.StartsWith('payload/app/') }) {
      if ((Get-SMAFileHash -LiteralPath (Join-Path $stageDir $entry.Path.Substring('payload/app/'.Length))).Hash -ne $entry.SHA256) { throw "Staged program checksum mismatch: $($entry.Path)" }
    }
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $DataDirectory 'Configs') -Destination (Join-Path $backupDir 'Configs') -Recurse
    Copy-Item -LiteralPath $pluginPath -Destination (Join-Path $backupDir 'plugins.json')
    $snapshot = @(Get-ChildItem -LiteralPath $backupDir -Recurse -File | ForEach-Object {
      [pscustomobject]@{Path=$_.FullName.Substring($backupDir.Length + 1); SHA256=(Get-SMAFileHash -LiteralPath $_.FullName).Hash}
    })
    $pdfBackups = @()
    foreach ($update in $pdfUpdates) {
      $backupFile = Join-Path $backupDir 'PDFPlugin/SuperMemoAssistant.Plugins.PDF.dll'
      if ($update.Existed) {
        New-Item -ItemType Directory -Path (Split-Path $backupFile -Parent) -Force | Out-Null
        Copy-Item -LiteralPath $update.Destination -Destination $backupFile
      }
      $pdfBackups += [pscustomobject]@{Destination=$update.Destination; Existed=$update.Existed; BackupPath=$backupFile; SHA256=$(if($update.Existed){(Get-SMAFileHash -LiteralPath $backupFile).Hash}else{$null})}
    }
    @{DataDirectory=$DataDirectory; InstallDirectory=$installPath; PreservedPersonalSettings=$true; HadProgram=$hadProgram; PreviousProgram=$previousProgram; Files=$snapshot; PdfFiles=$pdfBackups} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $backupDir 'recovery.json') -Encoding UTF8
    foreach ($entry in $snapshot) {
      $original = if ($entry.Path -eq 'plugins.json') { $pluginPath } else { Join-Path $DataDirectory $entry.Path }
      if ((Get-SMAFileHash -LiteralPath $original).Hash -ne $entry.SHA256) { throw 'Settings changed while backing up. Recovery stopped before publishing the program.' }
    }
    if ((Test-Path -LiteralPath $installPath -PathType Container) -ne $hadProgram -or (Get-Process | Where-Object { $_.ProcessName -match '^(SuperMemoAssistant|sm19|PluginHost)$' })) { throw 'SM/SMA or another installer started while staging. Recovery stopped before publishing the program.' }
    foreach ($update in $pdfUpdates) {
      $writtenPdf += $update
      Copy-Item -LiteralPath $update.Source -Destination $update.Destination -Force
      if ((Get-SMAFileHash -LiteralPath $update.Destination).Hash -ne $update.SHA256) { throw 'PDF DLL checksum mismatch after updating.' }
    }
    if ($hadProgram) {
      [IO.Directory]::Move($installPath, $previousProgram)
      $movedOldProgram = $true
    }
    [IO.Directory]::Move($stageDir, $installPath)
  } catch {
    foreach ($update in $writtenPdf) {
      $saved = $pdfBackups | Where-Object { $_.Destination -eq $update.Destination }
      if ($saved.Existed) { Copy-Item -LiteralPath $saved.BackupPath -Destination $saved.Destination -Force }
      elseif (Test-Path -LiteralPath $saved.Destination) {
        $createdPdf = [IO.Path]::GetFullPath($saved.Destination)
        if (!$createdPdf.StartsWith([IO.Path]::GetFullPath($DataDirectory).TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid rollback PDF path.' }
        Remove-Item -LiteralPath $createdPdf -Force
      }
    }
    if ($movedOldProgram -and !(Test-Path -LiteralPath $installPath)) { [IO.Directory]::Move($previousProgram, $installPath) }
    Write-Warning "Installation stopped; changes rolled back. Personal settings were not modified. Staging/backup: $stageDir ; $backupDir"
    throw
  }
  $exe = Join-Path $installPath 'SuperMemoAssistant.exe'
  if (!$NoShortcut) {
    try {
      $shell = New-Object -ComObject WScript.Shell
      $shortcut = $shell.CreateShortcut((Join-Path ([Environment]::GetFolderPath('Desktop')) 'SMA 19.1 Community r11.2.lnk'))
      $shortcut.TargetPath = $exe; $shortcut.WorkingDirectory = $installPath; $shortcut.Save()
    } catch { Write-Warning "Program restored; desktop shortcut could not be created. Start $exe" }
  }
  Write-Output "Installed/restored. Backup: $backupDir"
  Write-Output "Start: $exe"
  Write-Output 'Existing configurations, collections, other plugins, SDK and license settings were retained. The PDF DLL was updated only when its checksum differed.'
}
