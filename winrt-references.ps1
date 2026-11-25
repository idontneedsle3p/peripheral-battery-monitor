$ErrorActionPreference = 'Stop'
$metadata = Join-Path $env:WINDIR 'System32\WinMetadata'
Get-ChildItem -LiteralPath $metadata -Filter '*.winmd' | ForEach-Object { '/reference:' + $_.FullName }
foreach ($assembly in 'System.Runtime','System.Threading.Tasks','System.Collections','System.Runtime.InteropServices.WindowsRuntime') {
    $root = Join-Path $env:WINDIR ('Microsoft.NET\assembly\GAC_MSIL\' + $assembly)
    Get-ChildItem -LiteralPath $root -Filter '*.dll' -Recurse | Select-Object -First 1 | ForEach-Object { '/reference:' + $_.FullName }
}
