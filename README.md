# Peripheral Battery Monitor

[English](#english) · [Русский](#русский)

## English

Wireless mouse, keyboard and headset battery levels on the Windows taskbar. An extensible monitor for Windows 10/11 x64; see the support list for currently implemented devices.

The first stage of generalization is implemented: a driver registry, device discovery, a dynamic device list in the tray menu, and a taskbar widget. **MCHOSE X9** and **MCHOSE AX5 Pro Max** have been tested on hardware. An extensible architecture does not mean every device is supported. Standard Bluetooth LE Battery Service is now supported; vendor-specific Bluetooth protocols are not covered.

The taskbar shows minimalist outline icons and battery percentages on a transparent background. Text and icons are antialiased and scaled to the taskbar DPI.

### Features

- Reads actual battery levels through Windows HID, Bluetooth GATT and XInput APIs.
- Refreshes automatically every 30 seconds, with manual refresh from the menu.
- Shows the widget on all monitors, the primary monitor, or a selected screen.
- Positions the widget next to the system tray; uses UI Automation to locate the clock on secondary taskbars.
- Uses one tray icon with device battery levels in its tooltip.
- Sends low-battery notifications at 15% or below.
- Offers optional startup with Windows and remembers the selected monitor.
- Runs entirely from the tray menu, without a main window.
- Supports Russian and English: **Language / Язык → Русский / English**. Changes apply immediately and persist across restarts. On first launch, Russian is selected for Russian Windows installations, and English otherwise.
- Lists devices, battery levels, connection states, and last reading times under **Devices**; identical models are distinguished by their HID paths.
- Adjusts the taskbar widget to the device list. With more than six devices, the last slot shows `+N`; the full list remains available in the **Devices** menu.
- Isolates individual driver read failures. Disconnected devices retain an explicitly marked last known reading; readings recover after reconnection.

The application runs locally and makes no network requests.

### Supported devices

Native Sony **DualShock 4 / DualSense / DualSense Edge** and SteelSeries **Nova Pro Wireless / Omni / Elite** modules are included experimentally. Coarse battery reports are displayed as steps (`n/10`, `n/8`). See the [support matrix](SUPPORTED_DEVICES.md) for USB IDs and transport restrictions.

New broad-protocol batch: **standard Bluetooth LE Battery Service**, **Logitech HID++ 2.x**, **compatible Razer mice/keyboards** and **24 SteelSeries Arctis/Nova/GameBuds USB IDs**. These additions are experimental; exact protocol boundaries and hardware status are documented in [SUPPORTED_DEVICES.md](SUPPORTED_DEVICES.md).

Standard **XInput controller support** is now available across compatible brands, with discrete battery levels rather than invented percentages. Experimental GLW keyboard and V9 Turbo+ support is also included. See [compatibility and limitations](SUPPORTED_DEVICES.md).

Expanded MCHOSE support includes A5/AX5 variants, classic L7/M7/A7-family receivers, and newer K7 V2-family receivers. New additions are **experimental**, not hardware-verified in this project. See the complete [device and USB ID matrix](SUPPORTED_DEVICES.md).

| Device | Connection | VID:PID | Status |
|---|---|---|---|
| MCHOSE X9 | 2.4 GHz USB receiver | `3837:6045` | Tested on hardware |
| MCHOSE AX5 Pro Max | 2.4 GHz USB receiver | `2023:F024` | Tested on hardware |
| MCHOSE AX5 Pro Max | USB cable | `2023:F022` | Implemented, not tested on hardware |

Windows must show taskbars on secondary screens for the widget to appear there. Horizontal taskbars are supported. Battery history and vendor-specific classic Bluetooth protocols are not implemented yet. The X9 protocol does not report charging state. When a device stops responding, its last reading is explicitly marked as stale.

### Build and run

Requires Windows x64 and .NET Framework 4.8 with the `csc.exe` compiler. Rust, Visual Studio, and third-party libraries are not required.

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
.\PeripheralBatteryMonitor.exe
```

The build produces a portable `PeripheralBatteryMonitor.exe`. The existing `--tray` argument is also accepted:

```powershell
.\PeripheralBatteryMonitor.exe --tray
```

The application always starts in the tray, without a main window. Right-click its tray icon to access **Devices**, **Refresh**, **Monitors**, **Language / Язык**, **Show battery on taskbar**, **Start with Windows**, **Open log**, and **Exit**. Clicking the taskbar widget also opens the menu. After moving the EXE, re-enable startup with Windows to update its path.

### Tests and diagnostics

```powershell
powershell -ExecutionPolicy Bypass -File .\test.ps1
.\PeripheralBatteryMonitor.exe --diagnose report.txt
```

Tests cover real protocol replies, request echoes, charging state, invalid commands, truncated packets, out-of-range values, the driver registry, and localization. They do not require connected hardware. Diagnostics read HID device identifiers and battery responses; review the report before publishing it.

Settings and the latest log are stored in `%LOCALAPPDATA%\MyMchose`. The selected monitor is remembered while disconnected. Widgets are recreated after Explorer restarts.

### Source layout

| File | Purpose |
|---|---|
| `App.cs` | Background ApplicationContext, menus, single tray icon, polling, and notifications |
| `Devices.cs` | Data model, driver registry, device state, and response parsing |
| `MchoseDrivers.cs` | X9 and A5/AX5 battery drivers |
| `MchoseMouseDrivers.cs` | Experimental classic and modern MCHOSE mouse protocols |
| `MchoseKeyboardDriver.cs` | Experimental GLW keyboard protocol |
| `XInputSource.cs` | Standard Windows controller battery source |
| `BluetoothBatterySource.cs` | Paired BLE devices with standard Battery Service |
| `LogitechBatterySource.cs` | HID++ 2.x direct devices and receiver slots |
| `RazerBatteryDriver.cs` | Razer battery and charging feature reports |
| `SteelSeriesBatteryDriver.cs` | Whitelisted Arctis/Nova/GameBuds status reports |
| `Localization.cs` | Translation dictionary, language selection, and persistence |
| `Hid.cs` | HID enumeration and Win32 communication |
| `Monitors.cs` | Monitor selection and widget instance management |
| `TaskbarStrip.cs` | Rendering, transparency, and taskbar positioning |
| `Tests.cs` | Protocol, registry, and localization tests |

`Devices.cs` contains the `Reading` model, `IBatteryDriver` interface, and `BatteryMonitor` registry and cache. Adding a protocol does not require changing the menu or renderer. See [CONTRIBUTING.md](CONTRIBUTING.md) for instructions (currently in Russian).

Disconnected device state is kept in memory for the current session. Cable and receiver connections are treated as separate HID instances; merging them requires a reliable device identifier from the protocol.

### Protocol references

The application is written in C# / WinForms using the Windows HID API. Battery queries were checked against these open implementations:

- [BatteryHub — MCHOSE X9](https://github.com/ferloom/Wireless-Device-Battery-Monitor/blob/main/src/drivers/mchose_x9.rs).
- [HeadsetControl](https://github.com/Sapd/HeadsetControl).
- [mchose-linux — A5 / AX5 protocol](https://github.com/Wangpinglan/mchose-linux/blob/main/PROTOCOL.md).

The taskbar indicator was inspired by [BatteryHub](https://github.com/ferloom/Wireless-Device-Battery-Monitor). No third-party executable components are bundled.

---

## Русский

Заряд беспроводных мышей, клавиатур и гарнитур на панели задач Windows. Расширяемый монитор для Windows 10/11 x64; список реализованных устройств приведён ниже.

Реализована первая стадия универсализации: реестр модулей, обнаружение устройств, произвольный список в меню трея и динамический виджет. Сейчас проверены **MCHOSE X9** и **MCHOSE AX5 Pro Max**; наличие расширяемой основы не означает поддержку всех моделей. Стандартный Bluetooth LE Battery Service теперь поддерживается; фирменные Bluetooth-протоколы пока не реализованы.

На панели задач отображаются минималистичные контурные значки гарнитуры и мыши с процентами заряда. Фон прозрачный; подписи и значки сглажены и учитывают DPI панели задач.

### Возможности

- Чтение заряда через Windows HID, Bluetooth GATT и XInput.
- Автоматическое обновление каждые 30 секунд и ручное обновление из меню.
- Показ на всех мониторах, только на основном или на выбранном экране.
- Автоматическое размещение у системного трея; на дополнительном мониторе положение часов определяется через UI Automation.
- Один общий значок в трее с показаниями двух устройств в подсказке.
- Уведомления при заряде 15% и меньше.
- Необязательный автозапуск с Windows и сохранение выбранного монитора.
- Управление через меню по правому клику на значке трея; главное окно отсутствует.
- Русский и английский интерфейс: **Language / Язык → Русский / English**. Переключение применяется сразу и сохраняется между запусками. При первом запуске язык выбирается по языку Windows: русский для русской Windows, английский для остальных.
- Полный список устройств с зарядом, состоянием и временем последнего замера в подменю «Устройства»; одинаковые модели различаются по HID-пути.
- Виджет автоматически подстраивается под список; при количестве больше шести последний слот показывает `+N`. Полный список доступен в меню «Устройства».
- Ошибка чтения одного модуля не мешает остальным. Последний заряд отключённого устройства помечается как устаревший; после переподключения показания восстанавливаются.

Программа работает локально и не обращается к сети.

### Устройства

Экспериментально добавлены штатные протоколы Sony **DualShock 4 / DualSense / DualSense Edge** и SteelSeries **Nova Pro Wireless / Omni / Elite**. Ступенчатый заряд отображается как `n/10` или `n/8`. USB ID и ограничения подключения — в [таблице совместимости](SUPPORTED_DEVICES.md).

Новая пачка: **стандартный Bluetooth LE Battery Service**, **Logitech HID++ 2.x**, **совместимые мыши и клавиатуры Razer**, **24 USB ID гарнитур SteelSeries Arctis/Nova/GameBuds**. Дополнения экспериментальные; границы протоколов и аппаратные проверки перечислены в [SUPPORTED_DEVICES.md](SUPPORTED_DEVICES.md).

Добавлен стандартный источник **XInput** для совместимых геймпадов разных брендов: уровни заряда вместо выдуманных процентов. Также включены экспериментальные модули клавиатур GLW и V9 Turbo+. Подробнее: [совместимость и ограничения](SUPPORTED_DEVICES.md).

Добавлена экспериментальная поддержка вариантов A5/AX5, классических приёмников семейств L7/M7/A7 и новых приёмников K7 V2. Новые модели ещё не проверены в этом проекте на оборудовании. Полная [таблица моделей и USB ID](SUPPORTED_DEVICES.md).

| Устройство | Подключение | VID:PID | Проверка |
|---|---|---|---|
| MCHOSE X9 | USB-приёмник 2,4 ГГц | `3837:6045` | Проверено на реальном устройстве |
| MCHOSE AX5 Pro Max | USB-приёмник 2,4 ГГц | `2023:F024` | Проверено на реальном устройстве |
| MCHOSE AX5 Pro Max | USB-кабель | `2023:F022` | Реализовано, на устройстве не проверено |

Для дополнительных экранов Windows должна показывать панели задач на этих экранах. Поддерживаются горизонтальные панели задач. История заряда и фирменные протоколы классического Bluetooth пока не реализованы. Протокол X9 не сообщает состояние зарядки. Если устройство не отвечает, последнее значение явно помечается как старое.

### Сборка и запуск

Нужны Windows x64 и .NET Framework 4.8 с компилятором `csc.exe`. Rust, Visual Studio и сторонние библиотеки не требуются.

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
.\PeripheralBatteryMonitor.exe
```

Сборка создаёт переносимый `PeripheralBatteryMonitor.exe`. Для запуска сразу в трее:

```powershell
.\PeripheralBatteryMonitor.exe --tray
```

Программа сразу работает в трее, без главного окна. Правый щелчок по значку трея открывает все настройки: **Устройства**, **Обновить**, **Мониторы**, **Заряд на панели задач**, **Запускать с Windows**, **Открыть журнал** и **Выход**. Щелчок по виджету также открывает меню. При перемещении EXE автозапуск нужно включить заново.

### Проверка и диагностика

```powershell
powershell -ExecutionPolicy Bypass -File .\test.ps1
.\PeripheralBatteryMonitor.exe --diagnose report.txt
```

Тесты проверяют разбор реальных ответов, эхо запроса, зарядку, неверные команды, обрезанные пакеты и значения вне диапазона. Они не требуют подключённых устройств. Диагностика считывает идентификаторы HID-устройств и ответы батареи; перед публикацией отчёта проверь его содержимое.

Настройки и последний журнал находятся в `%LOCALAPPDATA%\MyMchose`. Выбранный монитор сохраняется; если он отключён, настройка остаётся до его возвращения. После перезапуска Explorer виджеты создаются заново.

### Исходники

| Файл | Назначение |
|---|---|
| `App.cs` | Фоновый ApplicationContext, меню, единый значок трея, опрос и уведомления |
| `Devices.cs` | Модель данных, реестр модулей, состояние устройств и разбор ответов |
| `MchoseDrivers.cs` | Модули X9 и A5/AX5 |
| `MchoseMouseDrivers.cs` | Экспериментальные классический и новый протоколы MCHOSE |
| `MchoseKeyboardDriver.cs` | Экспериментальный протокол клавиатур GLW |
| `XInputSource.cs` | Стандартный источник заряда геймпадов Windows |
| `BluetoothBatterySource.cs` | Сопряжённые BLE-устройства с Battery Service |
| `LogitechBatterySource.cs` | HID++ 2.x и слоты приёмников |
| `RazerBatteryDriver.cs` | Feature-отчёты батареи и зарядки Razer |
| `SteelSeriesBatteryDriver.cs` | Разрешённые отчёты Arctis/Nova/GameBuds |
| `Localization.cs` | Словарь переводов, выбор и сохранение языка |
| `Hid.cs` | Перечисление HID и обмен через Win32 |
| `Monitors.cs` | Выбор экранов и управление копиями виджета |
| `TaskbarStrip.cs` | Отрисовка, прозрачность и привязка к панели задач |
| `Tests.cs` | Тесты разбора протоколов |

`Devices.cs` содержит модель `Reading`, интерфейс `IBatteryDriver`, реестр и кэш `BatteryMonitor`. Добавление нового протокола не требует изменений меню или отрисовки. Инструкция: [CONTRIBUTING.md](CONTRIBUTING.md).

Состояние отключённых устройств хранится в памяти текущего сеанса. Подключение по кабелю и через приёмник считается разными HID-экземплярами; объединение таких подключений требует надёжного идентификатора от конкретного протокола.

### Источники сведений о протоколах

Приложение написано на C# / WinForms с использованием Windows HID API. Сведения о запросах заряда сверялись с открытыми реализациями:

- [BatteryHub — MCHOSE X9](https://github.com/ferloom/Wireless-Device-Battery-Monitor/blob/main/src/drivers/mchose_x9.rs).
- [HeadsetControl](https://github.com/Sapd/HeadsetControl).
- [mchose-linux — протокол A5 / AX5](https://github.com/Wangpinglan/mchose-linux/blob/main/PROTOCOL.md).

Идея индикатора на панели задач вдохновлена [BatteryHub](https://github.com/ferloom/Wireless-Device-Battery-Monitor). Сторонние исполняемые компоненты в сборку не включены.
