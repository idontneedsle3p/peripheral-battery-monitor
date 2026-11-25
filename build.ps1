$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$wpf = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\WPF'
$winrtReferences = & (Join-Path $PSScriptRoot 'winrt-references.ps1')
& $compiler $winrtReferences /nologo /target:winexe /platform:x64 /optimize+ /win32manifest:app.manifest /out:PeripheralBatteryMonitor.exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "/reference:$wpf\UIAutomationClient.dll" "/reference:$wpf\UIAutomationTypes.dll" "/reference:$wpf\WindowsBase.dll" BluetoothBatterySource.cs Localization.cs Hid.cs Devices.cs MchoseDrivers.cs MchoseMouseDrivers.cs MchoseKeyboardDriver.cs XInputSource.cs RazerBatteryDriver.cs LogitechBatterySource.cs SteelSeriesBatteryDriver.cs PlayStationBatteryDriver.cs SteelSeriesProBatteryDriver.cs HeadsetBatteryDrivers.cs BusinessHeadsetBatteryDrivers.cs LogitechProX2BatteryDriver.cs LegacyHeadsetBatteryDrivers.cs StandardHidBatteryDriver.cs SteelSeriesMouseBatteryDriver.cs App.cs TaskbarStrip.cs Monitors.cs
if ($LASTEXITCODE) { throw 'Build failed' }
