# YourBattery

[English](#english) · [Русский](#русский)

## English

An extensible wireless device battery monitor for Windows 10/11 x64.

The first stage of generalization is implemented: a driver registry, device discovery, a dynamic device list in the tray menu, and a taskbar widget. **MCHOSE X9** and **MCHOSE AX5 Pro Max** have been tested on hardware. An extensible architecture does not mean every device is supported. Bluetooth Battery Service is not implemented yet.

The taskbar shows minimalist outline icons and battery percentages on a transparent background. Text and icons are antialiased and scaled to the taskbar DPI.

### Features

- Reads actual battery levels through the standard Windows HID API.
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

| Device | Connection | VID:PID | Status |
|---|---|---|---|
| MCHOSE X9 | 2.4 GHz USB receiver | `3837:6045` | Tested on hardware |
| MCHOSE AX5 Pro Max | 2.4 GHz USB receiver | `2023:F024` | Tested on hardware |
| MCHOSE AX5 Pro Max | USB cable | `2023:F022` | Implemented, not tested on hardware |

Windows must show taskbars on secondary screens for the widget to appear there. Horizontal taskbars are supported. Bluetooth and battery history are not implemented yet. The X9 protocol does not report charging state. When a device stops responding, its last reading is explicitly marked as stale.

### Build and run

Requires Windows x64 and .NET Framework 4.x with the `csc.exe` compiler. Rust, Visual Studio, and third-party libraries are not required.

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
.\MyMchose.exe
```

The build produces a portable `MyMchose.exe`. The existing `--tray` argument is also accepted:

```powershell
.\MyMchose.exe --tray
```

The application always starts in the tray, without a main window. Right-click its tray icon to access **Devices**, **Refresh**, **Monitors**, **Language / Язык**, **Show battery on taskbar**, **Start with Windows**, **Open log**, and **Exit**. Clicking the taskbar widget also opens the menu. After moving the EXE, re-enable startup with Windows to update its path.

### Tests and diagnostics

```powershell
powershell -ExecutionPolicy Bypass -File .\test.ps1
.\MyMchose.exe --diagnose report.txt
```

Tests cover real protocol replies, request echoes, charging state, invalid commands, truncated packets, out-of-range values, the driver registry, and localization. They do not require connected hardware. Diagnostics read HID device identifiers and battery responses; review the report before publishing it.

Settings and the latest log are stored in `%LOCALAPPDATA%\MyMchose`. The selected monitor is remembered while disconnected. Widgets are recreated after Explorer restarts.

### Source layout

| File | Purpose |
|---|---|
| `App.cs` | Background ApplicationContext, menus, single tray icon, polling, and notifications |
| `Devices.cs` | Data model, driver registry, device state, and response parsing |
| `MchoseDrivers.cs` | Separate X9 and AX5 battery drivers |
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

Расширяемый монитор заряда беспроводных устройств для Windows 10/11 x64.

Реализована первая стадия универсализации: реестр модулей, обнаружение устройств, произвольный список в меню трея и динамический виджет. Сейчас проверены **MCHOSE X9** и **MCHOSE AX5 Pro Max**; наличие расширяемой основы не означает поддержку всех моделей. Bluetooth Battery Service пока не реализован.

На панели задач отображаются минималистичные контурные значки гарнитуры и мыши с процентами заряда. Фон прозрачный; подписи и значки сглажены и учитывают DPI панели задач.

### Возможности

- Чтение реального заряда через стандартный Windows HID API.
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

| Устройство | Подключение | VID:PID | Проверка |
|---|---|---|---|
| MCHOSE X9 | USB-приёмник 2,4 ГГц | `3837:6045` | Проверено на реальном устройстве |
| MCHOSE AX5 Pro Max | USB-приёмник 2,4 ГГц | `2023:F024` | Проверено на реальном устройстве |
| MCHOSE AX5 Pro Max | USB-кабель | `2023:F022` | Реализовано, на устройстве не проверено |

Для дополнительных экранов Windows должна показывать панели задач на этих экранах. Поддерживаются горизонтальные панели задач. Bluetooth и история заряда пока не реализованы. Протокол X9 не сообщает состояние зарядки. Если устройство не отвечает, последнее значение явно помечается как старое.

### Сборка и запуск

Нужны Windows x64 и .NET Framework 4.x с компилятором `csc.exe`. Rust, Visual Studio и сторонние библиотеки не требуются.

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
.\MyMchose.exe
```

Сборка создаёт переносимый `MyMchose.exe`. Для запуска сразу в трее:

```powershell
.\MyMchose.exe --tray
```

Программа сразу работает в трее, без главного окна. Правый щелчок по значку трея открывает все настройки: **Устройства**, **Обновить**, **Мониторы**, **Заряд на панели задач**, **Запускать с Windows**, **Открыть журнал** и **Выход**. Щелчок по виджету также открывает меню. При перемещении EXE автозапуск нужно включить заново.

### Проверка и диагностика

```powershell
powershell -ExecutionPolicy Bypass -File .\test.ps1
.\MyMchose.exe --diagnose report.txt
```

Тесты проверяют разбор реальных ответов, эхо запроса, зарядку, неверные команды, обрезанные пакеты и значения вне диапазона. Они не требуют подключённых устройств. Диагностика считывает идентификаторы HID-устройств и ответы батареи; перед публикацией отчёта проверь его содержимое.

Настройки и последний журнал находятся в `%LOCALAPPDATA%\MyMchose`. Выбранный монитор сохраняется; если он отключён, настройка остаётся до его возвращения. После перезапуска Explorer виджеты создаются заново.

### Исходники

| Файл | Назначение |
|---|---|
| `App.cs` | Фоновый ApplicationContext, меню, единый значок трея, опрос и уведомления |
| `Devices.cs` | Модель данных, реестр модулей, состояние устройств и разбор ответов |
| `MchoseDrivers.cs` | Отдельные модули чтения X9 и AX5 |
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
