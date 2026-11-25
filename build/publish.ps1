<#
.SYNOPSIS
  Builds QuickVB as a single file per platform: copy the file anywhere and run it (xcopy deployment).

.DESCRIPTION
  Each platform gets one executable in artifacts/<rid>/ (quickvb.exe on Windows, quickvb elsewhere) and nothing else.
  The executable needs the .NET 10 runtime on the machine (QuickVB runs the programs you write with it), so the
  runtime is not bundled: the file is about 20 MB, which is almost all Roslyn (the compiler) and the reference assemblies.
  Self-contained and trimmed builds were measured and are not smaller in any way that matters: see the README.

.PARAMETER Runtime
  The runtime identifiers to build for. Default: win-x64, win-arm64, linux-x64, linux-arm64.

.PARAMETER Configuration
  Release (default) or Debug.

.PARAMETER Zip
  Also make artifacts/quickvb-<version>-<rid>.zip for each platform and print the SHA256 of each.

.EXAMPLE
  ./build/publish.ps1 -Runtime win-x64 -Zip
#>
[CmdletBinding()]
param(
  [string[]] $Runtime = @('win-x64', 'win-arm64', 'linux-x64', 'linux-arm64'),
  [string] $Configuration = 'Release',
  [switch] $Zip
)

$ErrorActionPreference = 'Stop'

# passing -Runtime a,b through "powershell -File" arrives as one string
$Runtime = @($Runtime | ForEach-Object { $_ -split ',' } | Where-Object { $_ })

# "-File publish.ps1 -Runtime a,b" arrives as one string
$Runtime = @($Runtime | ForEach-Object { $_ -split ',' } | Where-Object { $_ })

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'QuickVB/QuickVB.vbproj'
$artifacts = Join-Path $root 'artifacts'

[xml] $xml = Get-Content $project
$version = ($xml.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1).Version
if (-not $version) { $version = '0.0.0' }

foreach ($rid in $Runtime) {

  $out = Join-Path $artifacts $rid
  if (Test-Path $out) { Remove-Item $out -Recurse -Force }

  Write-Host "== $rid" -ForegroundColor Cyan

  dotnet publish $project -c $Configuration -r $rid `
    -p:PublishSingleFile=true -p:SelfContained=false `
    -p:DebugType=none -p:DebugSymbols=false -p:GenerateDocumentationFile=false `
    -p:SatelliteResourceLanguages=en `
    -o $out
  if ($LASTEXITCODE -ne 0) { throw "publish failed for $rid" }

  # the other projects (ConsoleGUI, QuickVB.Runtime) leave documentation and symbol files behind; the one file is all there is
  $name = if ($rid -like 'win-*') { 'quickvb.exe' } else { 'quickvb' }
  $built = Get-ChildItem $out -File | Where-Object { $_.BaseName -eq 'QuickVB' -and ($_.Extension -in '.exe', '') }
  Get-ChildItem $out -File | Where-Object { $_.FullName -ne $built.FullName } | Remove-Item -Force
  Rename-Item $built.FullName $name

  $file = Join-Path $out $name
  Write-Host ("   {0}  {1:N1} MB" -f $file, ((Get-Item $file).Length / 1MB))

  if ($Zip) {
    $archive = Join-Path $artifacts "quickvb-$version-$rid.zip"
    if (Test-Path $archive) { Remove-Item $archive -Force }
    Compress-Archive -Path $file -DestinationPath $archive
    $hash = (Get-FileHash $archive -Algorithm SHA256).Hash
    Write-Host "   $archive"
    Write-Host "   SHA256 $hash"
  }
}
