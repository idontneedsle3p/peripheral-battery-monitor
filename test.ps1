$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    & $compiler /nologo /out:Tests.exe Hid.cs Devices.cs MchoseDrivers.cs Tests.cs
    if ($LASTEXITCODE) { throw 'Test compilation failed' }
    & .\Tests.exe
    if ($LASTEXITCODE) { throw 'Protocol tests failed' }
} finally {
    Pop-Location
}
