# Supported devices / Поддерживаемые устройства

**Tested** means checked on physical hardware in this project. **Experimental** means protocol-based implementation and automated tests only; hardware confirmation is still needed. / **Проверено** — испытано на реальном устройстве в этом проекте. **Экспериментально** — реализовано по протоколу и проверено тестами; нужна проверка владельцами устройств.

| Family / Семейство | USB VID:PID | Status / Статус |
|---|---|---|
| MCHOSE X9 receiver / приёмник | `3837:6045` | Tested / Проверено |
| MCHOSE V9 Turbo+ receiver / приёмник | `3837:600A` | Experimental / Экспериментально |
| MCHOSE GLW keyboards: G75 V2, G87 V2, K99 V3 family / клавиатуры | `3837:3033`, collection / коллекция `FF31:0074`, 65-byte reports | Experimental / Экспериментально |
| Xbox and compatible XInput gamepads / Xbox и совместимые геймпады XInput | Windows XInput API, up to 4 slots / до 4 слотов | Protocol-tested; hardware confirmation needed / Проверено тестами; нужна проверка на оборудовании |
| Paired Bluetooth LE devices exposing standard Battery Service / сопряжённые BLE-устройства со стандартным Battery Service | GATT service `180F`, characteristic `2A19` | Experimental / Экспериментально |
| Logitech HID++ 2.x mice and keyboards / мыши и клавиатуры | VID `046D`, 20-byte HID++ collection, battery features `1000` / `1004` | Experimental / Экспериментально |
| Compatible Razer wireless mice/keyboards / совместимые беспроводные мыши и клавиатуры | VID `1532`, 91-byte feature report, battery command `07:80` | Experimental / Экспериментально |
| SteelSeries Arctis 7+ variants / варианты | `1038:220E`, `2212`, `2216`, `2236` | Experimental; discrete levels / Экспериментально; уровни |
| SteelSeries Arctis Nova 3 Wireless / Nova 5 | `1038:2269`, `226D`, `2232`, `2253` | Experimental / Экспериментально |
| SteelSeries Arctis Nova 7 variants / варианты | `1038:2202`, `2206`, `220A`, `223A`, `227A`, `22A4`, `22A1`, `227E`, `2258`, `229E`, `22AD`, `22A9`, `22A5`, `22A7` | Experimental; format varies by PID / Экспериментально; формат зависит от PID |
| SteelSeries Arctis GameBuds / GameBuds X | `1038:230A`, `2317` | Experimental; lowest active earbud / Экспериментально; минимум среди активных наушников |
| MCHOSE AX5 Pro Max receiver / приёмник | `2023:F024` | Tested on Pro Max / Проверено на Pro Max |
| MCHOSE AX5 Pro receiver / приёмник | `2023:F024` (shared / общий) | Experimental / Экспериментально |
| MCHOSE A5 Pro / Pro Max receivers / приёмники | `2023:F013`, `2023:F015` | Experimental / Экспериментально |
| MCHOSE A5 Pro cable / кабель | `2023:F017` | Experimental / Экспериментально |
| MCHOSE A5 Pro Max cable / кабель | `2023:F019` | Experimental / Экспериментально |
| MCHOSE AX5 Pro cable / кабель | `2023:F026` | Experimental / Экспериментально |
| MCHOSE AX5 Pro Max cable / кабель | `2023:F022` | Experimental / Экспериментально |
| Classic L7 / M7 / A7 family receiver / приёмник | `5253:1020` | Experimental / Экспериментально |
| Classic Realtek MCHOSE receivers / приёмники | `3837:1001`, `3837:100A`, `3837:100B`, `3837:100C`, `3837:1012` | Experimental / Экспериментально |
| Modern MCHOSE receivers, including K7 V2 Ultra+ / новые приёмники, включая K7 V2 Ultra+ | `3837:1014`, `3837:1018`, `3837:1016`, `3837:102C`, `3837:102D` | Experimental / Экспериментально |
| K7 V2 Ultra+ cable / кабель | `3837:4028` | Experimental / Экспериментально |

Shared receiver IDs do not identify a precise model. Classic and modern modules use the USB product string when available. Only listed IDs and compatible vendor collections are queried; a family name does not imply every variant is supported. Bootloaders are excluded. / Общие ID приёмников не определяют точную модель. Классический и новый модули используют USB-название, если оно доступно. Опрос ограничен перечисленными ID и подходящими vendor-коллекциями; название семейства не означает поддержку всех вариантов. Загрузчики прошивки исключены.

## Protocol references / Источники протоколов

- [A5 / AX5 protocol](https://github.com/Wangpinglan/mchose-linux/blob/main/PROTOCOL.md).
- [Classic MCHOSE protocol](https://github.com/alexfrih/mchose-linux/blob/main/PROTOCOL.md).
- [Classic and modern protocol research](https://github.com/HifumiDaisuki01/MchoseBatteryTray/blob/main/PROTOCOL.md).
- [BatteryHub receiver IDs](https://github.com/ferloom/Wireless-Device-Battery-Monitor/tree/main/src/drivers).
- [XInputGetBatteryInformation](https://learn.microsoft.com/en-us/windows/win32/api/xinput/nf-xinput-xinputgetbatteryinformation).
- [XINPUT_BATTERY_INFORMATION](https://learn.microsoft.com/en-us/windows/win32/api/xinput/ns-xinput-xinput_battery_information).
- [Bluetooth Battery Service specification](https://www.bluetooth.com/specifications/specs/battery-service/).
- [Solaar HID++ reference implementation](https://github.com/pwr-Solaar/Solaar/blob/master/lib/logitech_receiver/hidpp20.py).
- [OpenRazer report format](https://github.com/openrazer/openrazer/blob/master/driver/razercommon.c).
- [HeadsetControl](https://github.com/Sapd/HeadsetControl).

## Broad protocol support / Поддержка семейств протоколов

**Bluetooth:** only already paired, connected BLE devices with a readable standard Battery Level characteristic are queried. No pairing, scanning for unpaired devices, or vendor-specific battery guessing is performed. Not all Bluetooth peripherals expose this service. Classic Bluetooth and manufacturer-specific earbud advertisements are outside this module. Multiple BAS instances are shown separately.

**Logitech:** discovers battery features and names through HID++ 2.x, including direct index `FF` and receiver slots 1–6. Requires a 20-byte input/output collection (`FF00:0002` or compatible `FF43`). HID++ 1.x, short-only interfaces, Centurion, voltage-only batteries and ADC-based G headsets are not implemented. MX/LIGHTSPEED/Unifying/Bolt branding alone is not proof of compatibility; the required features must be present.

**Razer:** uses only battery and charging status commands, verifies transaction/command/CRC, and shows a new device only after a valid battery reply. Compatible wireless variants of Viper, DeathAdder, Basilisk, Orochi, Pro Click, BlackWidow and DeathStalker are candidates, not individually hardware-verified models. Some models use other report formats or firmware.

**SteelSeries:** only the listed PIDs and vendor collection `FFC0:0001` are queried. Percentage replies remain percentages; step-based reports remain qualitative levels. GameBuds display the lower battery among the active buds. Original Arctis 1/7/9, Nova Pro, Elite and Aerox/Rival mouse protocols are not part of this module.

**Bluetooth:** опрашиваются только уже сопряжённые и подключённые BLE-устройства с читаемой стандартной характеристикой уровня батареи. Сопряжение, поиск несопряжённых устройств и угадывание фирменных протоколов не выполняются. Не все Bluetooth-устройства предоставляют Battery Service; классический Bluetooth и фирменные объявления наушников не входят в этот модуль. Несколько батарей BAS показываются отдельно.

**Logitech:** обнаружение имени и батареи через HID++ 2.x, прямой индекс `FF` и слоты приёмника 1–6. Нужна 20-байтовая input/output-коллекция `FF00:0002` либо совместимая `FF43`. HID++ 1.x, только короткие отчёты, Centurion, батареи только с напряжением и ADC-гарнитуры пока не поддерживаются. Название MX/LIGHTSPEED/Unifying/Bolt само по себе совместимость не гарантирует.

**Razer:** запросы только батареи и зарядки, проверка транзакции, команды и CRC; устройство появляется после первого корректного ответа батареи. Беспроводные варианты Viper, DeathAdder, Basilisk, Orochi, Pro Click, BlackWidow и DeathStalker — кандидаты на совместимость, а не модели с подтверждённым аппаратным тестированием. У отдельных моделей другие прошивки и форматы отчётов.

**SteelSeries:** опрашиваются только перечисленные PID и коллекция `FFC0:0001`. Проценты остаются процентами, ступенчатые значения показываются уровнями. Для GameBuds отображается меньший заряд среди активных наушников. Старые Arctis 1/7/9, Nova Pro, Elite и мыши Aerox/Rival требуют других модулей.

Cross-protocol deduplication is not implemented: one physical device exposing multiple independent battery APIs may appear more than once. / Объединение записей из разных протоколов пока не реализовано: устройство с несколькими независимыми API батареи может отображаться несколько раз.

## Standard controller support / Стандартная поддержка геймпадов

XInput support is based on the Windows driver, not a list of brands. Xbox 360, Xbox One, Xbox Series and third-party controllers can work when exposed through XInput and their driver reports battery data. Compatibility depends on the device, driver and connection. A model name alone is not confirmation. Sony controllers in native DirectInput mode are not covered by this source.

XInput reports **empty / low / medium / full**, not percentages. The widget uses short labels, and the menu shows the full state. Wired controllers show **USB powered**; unknown battery types show **Battery level unavailable**. No percentage is inferred. Low/empty states trigger the existing low-battery notification. Slot numbers identify XInput sessions, not permanent hardware identities.

Поддержка XInput зависит от драйвера Windows, а не от бренда. Могут работать Xbox 360, Xbox One, Xbox Series и сторонние геймпады, если они доступны через XInput и драйвер передаёт состояние батареи. Конкретная модель, драйвер и подключение требуют проверки. Sony в обычном режиме DirectInput этим источником не поддерживаются.

XInput сообщает уровни **разряжен / низкий / средний / полный**, а не проценты. На панели используются короткие подписи, в меню — полные. Для проводных устройств отображается **питание по USB**, для неизвестного типа батареи — **заряд неизвестен**. Низкий уровень вызывает уведомление. Номера слотов относятся к текущему сеансу XInput, а не к постоянному идентификатору устройства.

## Keyboard polling / Опрос клавиатур

GLW keyboards are automatically queried every 10 minutes to avoid repeatedly waking sleeping keyboards. Manual **Refresh** bypasses this interval. K87S is excluded because it uses a different protocol. Only the specified FF31 collection is implemented; the alternative 0001:0000 route is not claimed.

Клавиатуры GLW автоматически опрашиваются раз в 10 минут, чтобы не будить их слишком часто. Ручное **Обновить** выполняет запрос сразу. K87S исключена: у неё другой протокол. Реализована только коллекция FF31; альтернативный путь 0001:0000 пока не заявлен.

Only battery/status queries are implemented. No configuration, lighting, pairing, DPI, or firmware write commands are sent. / Реализованы только запросы заряда/состояния. Команды изменения настроек, подсветки, сопряжения, DPI и прошивки не отправляются.
