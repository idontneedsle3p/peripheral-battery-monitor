$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    & $compiler /nologo /out:Tests.exe Localization.cs Hid.cs Devices.cs MchoseDrivers.cs MchoseMouseDrivers.cs MchoseKeyboardDriver.cs XInputSource.cs Tests.cs
    if ($LASTEXITCODE) { throw 'Test compilation failed' }
    & .\Tests.exe
    if ($LASTEXITCODE) { throw 'Protocol tests failed' }
} finally {
    Pop-Location
}
