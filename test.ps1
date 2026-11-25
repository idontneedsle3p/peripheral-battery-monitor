$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    $winrtReferences = & (Join-Path $PSScriptRoot 'winrt-references.ps1')
& $compiler $winrtReferences /nologo /out:Tests.exe BluetoothBatterySource.cs Localization.cs Hid.cs Devices.cs MchoseDrivers.cs MchoseMouseDrivers.cs MchoseKeyboardDriver.cs XInputSource.cs RazerBatteryDriver.cs LogitechBatterySource.cs SteelSeriesBatteryDriver.cs PlayStationBatteryDriver.cs SteelSeriesProBatteryDriver.cs HeadsetBatteryDrivers.cs BusinessHeadsetBatteryDrivers.cs LogitechProX2BatteryDriver.cs LegacyHeadsetBatteryDrivers.cs StandardHidBatteryDriver.cs CompxMouseBatteryDriver.cs KeychronBatteryDriver.cs AulaKeyboardBatteryDriver.cs AdditionalHeadsetBatteryDrivers.cs AdditionalMouseBatteryDrivers.cs SteelSeriesMouseBatteryDriver.cs Tests.cs
    if ($LASTEXITCODE) { throw 'Test compilation failed' }
    & .\Tests.exe
    if ($LASTEXITCODE) { throw 'Protocol tests failed' }
} finally {
    Pop-Location
}
