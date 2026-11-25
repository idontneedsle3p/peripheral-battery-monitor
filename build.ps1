$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$wpf = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\WPF'
& $compiler /nologo /target:winexe /platform:x64 /optimize+ /win32manifest:app.manifest /out:PeripheralBatteryMonitor.exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "/reference:$wpf\UIAutomationClient.dll" "/reference:$wpf\UIAutomationTypes.dll" "/reference:$wpf\WindowsBase.dll" Localization.cs Hid.cs Devices.cs MchoseDrivers.cs MchoseMouseDrivers.cs MchoseKeyboardDriver.cs XInputSource.cs App.cs TaskbarStrip.cs Monitors.cs
if ($LASTEXITCODE) { throw 'Build failed' }
