# Peripheral Battery Monitor

[English](#english) · [Русский](#русский)

## English

Wireless mouse, keyboard and headset battery levels on the Windows taskbar. An extensible monitor for Windows 10/11 x64; see the support list for currently implemented devices.

The application uses a driver registry, device discovery, a dynamic device list in the tray menu, and a taskbar widget. **MCHOSE X9** and **MCHOSE AX5 Pro Max** have been tested on hardware. An extensible architecture does not mean every device is supported. Standard Bluetooth LE Battery Service is now supported; vendor-specific Bluetooth protocols are not covered.

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

Hardware-tested in this project: **MCHOSE X9** and **MCHOSE AX5 Pro Max**. Other additions are experimental and require confirmation on physical devices. The list below describes protocol coverage, not a guarantee for every product carrying a brand name.

| Group | Current coverage |
|---|---|
| Standard APIs | Bluetooth LE Battery Service, descriptor-declared HID Battery Strength, XInput controllers |
| Logitech | HID++ 1.x / 2.x battery devices, selected G PRO X 2 firmware, G321 and G735 |
| Razer | Compatible 91-byte battery-report mice/keyboards, Nari and BlackShark V2 Pro (2023) |
| SteelSeries | Arctis/Nova/GameBuds headsets, selected Aerox/Prime mice |
| Corsair | Compatible Void / Void Pro / Void Elite / HS70-family receivers |
| HyperX | Selected Cloud II/II Core/III/III S/Stinger 3/Alpha receivers |
| Sony | Native DualShock 4, DualSense and DualSense Edge input reports |
| Jabra | Headset battery queries through listed Link 390 receivers |
| CompX mice | Selected Pulsar, Kysona, VXE, LAMZU, Zaopin and ATK receivers |
| Keychron | M1/M2/M3/M6/M7 families through the listed 1K Link receivers |
| AULA / BY | Selected wireless keyboard receivers using the 20-byte battery protocol |
| MCHOSE | X9, V9 Turbo+, A5/AX5, selected classic/modern mice and GLW keyboards |

See [SUPPORTED_DEVICES.md](SUPPORTED_DEVICES.md) for exact USB IDs, report formats, connection restrictions and test status. Some devices expose only coarse levels; these remain levels or `n/max` steps, rather than invented precise percentages. The X9 read does not provide charging state. Battery history and manufacturer-specific classic Bluetooth protocols are not implemented. Windows must show a horizontal taskbar on each selected monitor.

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

For a graceful shutdown from a script, use `PeripheralBatteryMonitor.exe --quit`. Automatic polling respects longer protocol-specific intervals; manual refresh bypasses them.

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
| `*BatteryDriver.cs` / `*BatteryDrivers.cs` | Model-specific status-only protocol modules |
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

Реализованы реестр модулей, обнаружение устройств, произвольный список в меню трея и динамический виджет. Сейчас проверены **MCHOSE X9** и **MCHOSE AX5 Pro Max**; наличие расширяемой основы не означает поддержку всех моделей. Стандартный Bluetooth LE Battery Service теперь поддерживается; фирменные Bluetooth-протоколы пока не реализованы.

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

На оборудовании в этом проекте проверены **MCHOSE X9** и **MCHOSE AX5 Pro Max**. Остальные дополнения экспериментальные и требуют проверки владельцами устройств. Ниже перечислены протоколы, а не гарантия поддержки каждой модели указанного бренда.

| Группа | Текущая поддержка |
|---|---|
| Стандартные API | Bluetooth LE Battery Service, HID Battery Strength из дескриптора, геймпады XInput |
| Logitech | Батареи HID++ 1.x / 2.x, выбранные прошивки G PRO X 2, G321 и G735 |
| Razer | Совместимые мыши/клавиатуры с 91-байтовыми отчётами, Nari и BlackShark V2 Pro (2023) |
| SteelSeries | Гарнитуры Arctis/Nova/GameBuds, выбранные мыши Aerox/Prime |
| Corsair | Совместимые приёмники семейств Void / Void Pro / Void Elite / HS70 |
| HyperX | Выбранные приёмники Cloud II/II Core/III/III S/Stinger 3/Alpha |
| Sony | Штатные input-отчёты DualShock 4, DualSense и DualSense Edge |
| Jabra | Заряд гарнитуры через перечисленные приёмники Link 390 |
| Мыши CompX | Выбранные приёмники Pulsar, Kysona, VXE, LAMZU, Zaopin и ATK |
| Keychron | Семейства M1/M2/M3/M6/M7 через перечисленные приёмники Link 1K |
| AULA / BY | Выбранные приёмники клавиатур с 20-байтовым протоколом батареи |
| MCHOSE | X9, V9 Turbo+, A5/AX5, выбранные классические/новые мыши и клавиатуры GLW |

Точные USB ID, форматы отчётов, подключения и проверки — в [SUPPORTED_DEVICES.md](SUPPORTED_DEVICES.md). Грубые значения остаются уровнями или ступенями `n/max`, без выдуманной точности процентов. Запрос X9 не сообщает состояние зарядки. История заряда и фирменные протоколы классического Bluetooth пока не реализованы. Windows должна показывать горизонтальную панель задач на каждом выбранном мониторе.

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

Для корректного завершения из скрипта используйте `PeripheralBatteryMonitor.exe --quit`. Фоновый опрос учитывает увеличенные интервалы конкретных протоколов; ручное обновление обходит их.

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
