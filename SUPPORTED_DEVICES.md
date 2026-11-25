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
| Sony DualShock 4 | `054C:05C4`, `09CC` | Experimental; USB/Bluetooth, reported steps / Экспериментально; ступени заряда |
| Sony DualSense / DualSense Edge | `054C:0CE6`, `0DF2` | Experimental; USB/Bluetooth, reported steps / Экспериментально; ступени заряда |
| SteelSeries Arctis Nova Pro Wireless variants / варианты | `1038:12E0`, `12E5`, `225D`, vendor collection on interface / vendor-коллекция интерфейса `04` | Experimental; 0–8 steps / Экспериментально; 0–8 ступеней |
| SteelSeries Arctis Nova Pro Omni / Nova Elite | `1038:2290`, `2244`, `2270`, collection / коллекция `FFC0:0001` | Experimental / Экспериментально |
| Corsair Void / Void Pro / Void Elite / compatible HS70 family | VID `1B1C`; PIDs `1B1C`, `1B27`, `0A14`, `0A16`, `0A17`, `0A1D`, `0A1A`, `1B2A`, `1B23`, `1B29`, `0A55`, `0A51`, `0A52`, `0A38`, `0A4F`, `0A2B`, `0A75`, `0A56`; `FFC5:0001` | Experimental / Экспериментально |
| HyperX Cloud III S Wireless / Cloud Stinger 3 Wireless | `03F0:06BE`, `02CC`, `03C6`; `01C0:0001` | Experimental / Экспериментально |
| HyperX Cloud II Core Wireless / Cloud III Wireless | `03F0:0995`, `0795`, `05B7`, `0C9D`; `FF13:0001`, 62-byte reports | Experimental / Экспериментально |
| HyperX Cloud Alpha Wireless | `03F0:098D`; `FF43:0202` | Experimental / Экспериментально |
| SteelSeries Aerox 3 / 5 / 9 and Prime Wireless variants | VID `1038`; PIDs `1838`, `183A`, `1878`, `187A`, `1890`, `1892`, `1852`, `1854`, `185C`, `185E`, `1858`, `185A`, `1840`, `1842`, `1860`; interface `03` | Experimental / Экспериментально |
| Jabra headsets behind Link 390 / гарнитуры за приёмником Link 390 | `0B0E:2E50`, `2E51`, `2E56`, `2E57`; `FF00:0005`, 64-byte reports | Experimental / Экспериментально |
| Standard HID Battery Strength / стандартный HID | Usage `0006:0020`, scalar absolute 0–100 field in feature or input reports | Experimental; descriptor-driven / Экспериментально; по дескриптору |
| Logitech HID++ 1.x battery registers / регистры батареи | VID `046D`, `FF00:0001`, 7-byte input/output, registers `0D` or `07` | Experimental / Экспериментально |
| Logitech G PRO X 2 LIGHTSPEED | `046D:0AF7`, `FFA0:0001`, 64-byte reports | Experimental; fixed legacy query / Экспериментально; фиксированный запрос |
| SteelSeries Arctis 1 / 7X family | `1038:12B3`, `12B6`, `12D7`, `FF43:0202` | Experimental / Экспериментально |
| SteelSeries Arctis 7 / 2019 / Pro | `1038:1260`, `12AD`, `1252`, `FF43:0202` | Experimental / Экспериментально |
| SteelSeries Arctis 9 / Pro Wireless | `1038:12C2`, `1290`, vendor collection on interface `00` | Experimental; raw steps / Экспериментально; ступени |
| HyperX Cloud II Wireless (Kingston) | `0951:1718`, vendor page, 62-byte output | Experimental / Экспериментально |
| CompX: Pulsar / Kysona M600 / VXE R1 / LAMZU / Zaopin / ATK | `3554:F508`, `F509`, `F507`, `F57C`, `F57D`, `F5D5`, `F58F`, `F58A`, `F58C`, `F510`, `F50F`, `F527`; `373B:1085`, `10C9`; `3710:5406`, `3415`; `FF02:0002`, 17-byte input/output | Experimental / Экспериментально |
| Keychron Link 1K mouse receiver / приёмник мыши | `3434:D030`, `D031`; page `008C`, feature report `51` | Experimental / Экспериментально |
| AULA / BY wireless keyboards / клавиатуры | `3554:FA09`, `FB40`; `41E4:2001`; `FF02:0002` or `FF04:0002`, 20-byte input/output | Experimental / Экспериментально |
| Logitech G321 LIGHTSPEED | `046D:0B39`; interface `03`, `FF00:0212`, feature `24`, input `27` | Experimental; percentage only / Экспериментально; только процент |
| Logitech G735 | `046D:0AD8`; `FF03:0001`, 271-byte input/output | Experimental / Экспериментально |
| Razer Nari | `1532:051C`; `FF00:0001`, feature `FF` | Experimental; reported coarse percentage / Экспериментально; грубый процент устройства |
| Razer BlackShark V2 Pro (2023) | `1532:0555`; `FF00`, 64-byte input/output | Experimental; 2023 model only / Экспериментально; только модель 2023 |
| AJAZZ AJ139-family / семейство AJ139; VGN Dragonfly 3 Pro | `A8A5:2255` receiver / приёмник, `A8A4:2255` cable / кабель, `391D:1A03` VGN receiver; `FF01:0010`, 65-byte Windows reports; `A8A5` also 64-byte `F0` protocol | Experimental; AJAZZ ID is shared / Экспериментально; общий ID AJAZZ |
| AJAZZ AJ159P / AJ179-family | `249A:5C2F`, interface `02`, `0001:0000`, 33-byte input/output | Experimental; shared receiver / Экспериментально; общий приёмник |
| Darmoshark M3 / M3S | `248A:FF30` / `FF31`, interface `01`, `008C:0001`, feature `51` | Experimental / Экспериментально |
| Attack Shark X11 / X3 Max / V3 Pro / V6 / X6 / R1; IO Nova SE | `1D57:FA60` receiver; model marker in report `03`, interface `02`, page `000A`, 5-byte input | Experimental; passive reads / Экспериментально; пассивное чтение |
| Delux M900 Pro / Inphic IN10R MAX / IN10 MAX | `1D57:FA65` KP 8K receiver; model marker, same passive collection | Experimental / Экспериментально |
| IO Nova SE cable / кабель | `1D57:2015`, same passive collection / та же пассивная коллекция | Experimental / Экспериментально |
| Redragon / SinoWealth compatible mice (M602-KS / M690 Pro / M910-KS / M811 Pro family) | `258A:002F`, interface `01`, `FF00:0001`, feature 8 bytes, no input/output | Experimental; shared ID is not a model guarantee / Экспериментально; общий ID не гарантирует конкретную модель |
| WLMOUSE Beast X / Mini / Mini Pro / Max | `36A7:A887` / `A885` / `A868` / `A880`, page `FFFF`, feature 65 bytes | Experimental / Экспериментально |
| NZXT Lift Elite | `1E71:2101` receiver / приёмник, `2129` cable / кабель; `FFCA:0001`, 64/65-byte reports | Experimental / Экспериментально |
| Endgame Gear OP1w 4k / OP1w 4k v2 / XM2w 4k / XM2w 4k v2 | `3367:1970` receiver / приёмник; `FF01:0002`, feature 64 bytes; inner PIDs `1972`, `1984`, `1968`, `1982` | Experimental / Экспериментально |
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
- [Linux PlayStation input report layouts](https://github.com/torvalds/linux/blob/master/drivers/hid/hid-playstation.c).
- [Linux Corsair Void status report](https://github.com/torvalds/linux/blob/master/drivers/hid/hid-corsair-void.c).
- [HyperHeadset](https://github.com/LennardKittner/HyperHeadset).
- [HyperX Battery Tray](https://github.com/GuacamoleGH/HyperX-Battery-Tray).
- [rivalcfg SteelSeries protocol research](https://github.com/flozz/rivalcfg).
- [HeadsetControl Jabra Link 390](https://github.com/Sapd/HeadsetControl/blob/master/lib/devices/jabra_link_390.hpp).
- [Microsoft HIDP_VALUE_CAPS](https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/hidpi/ns-hidpi-_hidp_value_caps).
- [Microsoft HidP_GetUsageValue](https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/hidpi/nf-hidpi-hidp_getusagevalue).

## Standard HID and legacy devices / Стандартный HID и старые устройства

Standard HID support is descriptor-driven, not a brand whitelist. Only a non-aliased scalar absolute Battery Strength value with logical range exactly 0–100 is accepted. The Windows HID parser extracts the declared field; no report offsets or vendor commands are guessed. Feature reports are tried first, then input snapshots. Value arrays, usage ranges, voltage-only fields and other scales are excluded. Charging is not inferred. The driver must allow the corresponding HID read. Only one matching battery field per report type/collection is used.

HID++ 1.x uses the short register reads `0D` (percentage) and `07` (qualitative level), and does not invent percentages for coarse levels. Short and long HID++ collections are grouped per receiver, with a fresh HID++ 2.x reading preferred. Some legacy receiver slots have only generic names. PRO X 2 support is limited to firmware accepting the fixed legacy battery query; G522 and generic Centurion discovery are not included. Arctis 9/Pro Wireless report native `n/54` and `n/4` levels. The Arctis 7 zero response is treated as unavailable rather than a confirmed empty battery.

Стандартный HID определяется по дескриптору, а не списку брендов. Принимается только одиночное абсолютное поле Battery Strength без alias, с логическим диапазоном ровно 0–100. Поле извлекает парсер HID Windows; смещения и фирменные команды не угадываются. Сначала читается feature-отчёт, затем снимок input. Массивы, диапазоны usage, напряжение и другие шкалы исключены. Зарядка не выводится из косвенных признаков. Драйвер должен разрешать соответствующее чтение. Используется первое подходящее поле каждого типа в коллекции.

HID++ 1.x читает короткие регистры `0D` (проценты) и `07` (уровни), без выдуманных процентов для грубых значений. Короткие и длинные коллекции объединяются по приёмнику; свежий результат HID++ 2.x имеет приоритет. Некоторые старые слоты имеют только общее имя. PRO X 2 ограничен прошивками с фиксированным legacy-запросом батареи; G522 и общий Centurion пока не включены. Arctis 9/Pro Wireless отображают `n/54` и `n/4`. Нулевой ответ Arctis 7 считается отсутствием данных, а не подтверждённым разрядом.

## Corsair, HyperX, Aerox and Jabra

Corsair capacity is shown as reported with the microphone bit removed; it can read high while charging. Initializing/disconnected and malformed states are ignored. HyperX III S/Stinger 3 battery reads do not query charging state; unsolicited charger events are not tracked in this implementation. Core/III and Alpha have separate charging queries. The older Kingston Cloud II uses a separate module. Voltage-only Flight/Stinger Core protocols are not included.

Aerox/Prime packets with 21 battery codes are displayed as `n/20` steps. Aerox 3 Gen 2 uses its actual percentage field. Transport-specific commands are selected by PID. Other Rival protocols are not included. Jabra coverage is limited to GNP battery queries through the listed Link 390 IDs, not all Jabra products or Link 380; the displayed name identifies the receiver rather than the paired headset model.

Corsair показывает переданный заряд без бита положения микрофона; во время зарядки он может быть завышен. Состояния инициализации, отключения и некорректные ответы исключаются. У HyperX III S/Stinger 3 зарядка отдельным запросом не определяется, события подключения кабеля пока не отслеживаются. Core/III и Alpha используют отдельные запросы зарядки. Старый Kingston Cloud II использует отдельный модуль. Протоколы Flight/Stinger Core с напряжением пока не включены.

Для Aerox/Prime с 21 кодом заряда отображается `n/20`; Aerox 3 Gen 2 использует поле процентов. Команда зависит от PID и подключения. Протоколы остальных Rival пока не включены. Jabra ограничена запросом батареи GNP через перечисленные Link 390: это не поддержка всех Jabra и не Link 380. Название в меню обозначает приёмник, а не точную модель гарнитуры.

## Native Sony and Pro headsets / Sony и гарнитуры Pro

The Sony module reads native gamepad input reports. DualShock 4, DualSense and DualSense Edge expose coarse battery buckets, shown as `n/10` rather than an invented exact percentage. USB requires a full 64-byte report; Bluetooth requires the full 78-byte report and a valid input CRC. A read of calibration feature `05` is used on Bluetooth to request full reports; no calibration data is written. Reduced reports and battery error states are ignored. The Sony `0BA0` USB adapter and third-party clones are not included. HidHide, exclusive controller tools and emulation can hide the native device or produce a separate XInput entry.

Nova Pro Wireless uses `n/8` levels, and Omni/Elite uses the reported percentage. The base station's spare battery is not shown. Hardware confirmation is still needed for all new models and connection variants.

Модуль Sony читает штатные входные отчёты геймпада. DualShock 4, DualSense и DualSense Edge сообщают грубые ступени: они отображаются как `n/10`, без выдуманного точного процента. Для USB нужен полный 64-байтовый отчёт; для Bluetooth — полный 78-байтовый отчёт с корректной CRC. Чтение feature `05` запрашивает полный формат Bluetooth; калибровка не записывается. Сокращённые отчёты и ошибки батареи игнорируются. Адаптер Sony `0BA0` и сторонние клоны пока не включены. HidHide, эксклюзивный доступ и эмуляторы могут скрыть исходный контроллер или создать отдельную запись XInput.

Nova Pro Wireless показывает уровень `n/8`, Omni/Elite — переданный устройством процент. Запасная батарея в базовой станции не отображается. Новым моделям и вариантам подключения нужна проверка на оборудовании.

## Broad protocol support / Поддержка семейств протоколов

**Bluetooth:** only already paired, connected BLE devices with a readable standard Battery Level characteristic are queried. No pairing, scanning for unpaired devices, or vendor-specific battery guessing is performed. Not all Bluetooth peripherals expose this service. Classic Bluetooth and manufacturer-specific earbud advertisements are outside this module. Multiple BAS instances are shown separately.

**Logitech:** discovers battery features and names through HID++ 2.x, including direct index `FF` and receiver slots 1–6. Requires a 20-byte input/output collection (`FF00:0002` or compatible `FF43`). Short HID++ 1.x and the fixed PRO X 2 query use separate paths described above; generic Centurion, voltage-only batteries and ADC-based G headsets are not implemented. MX/LIGHTSPEED/Unifying/Bolt branding alone is not proof of compatibility; the required features must be present.

**Razer:** uses only battery and charging status commands, verifies transaction/command/CRC, and shows a new device only after a valid battery reply. Compatible wireless variants of Viper, DeathAdder, Basilisk, Orochi, Pro Click, BlackWidow and DeathStalker are candidates, not individually hardware-verified models. Some models use other report formats or firmware.

**SteelSeries:** only the listed PIDs and vendor collection `FFC0:0001` are queried. Percentage replies remain percentages; step-based reports remain qualitative levels. GameBuds display the lower battery among the active buds. Nova Pro/Omni/Elite use a separate module listed above. Original Arctis 1/7/9/Pro Wireless and Aerox/Prime mice now have separate modules. Rival mouse protocols are not included.

**Bluetooth:** опрашиваются только уже сопряжённые и подключённые BLE-устройства с читаемой стандартной характеристикой уровня батареи. Сопряжение, поиск несопряжённых устройств и угадывание фирменных протоколов не выполняются. Не все Bluetooth-устройства предоставляют Battery Service; классический Bluetooth и фирменные объявления наушников не входят в этот модуль. Несколько батарей BAS показываются отдельно.

**Logitech:** обнаружение имени и батареи через HID++ 2.x, прямой индекс `FF` и слоты приёмника 1–6. Нужна 20-байтовая input/output-коллекция `FF00:0002` либо совместимая `FF43`. Короткие HID++ 1.x и фиксированный запрос PRO X 2 имеют отдельные пути выше; общий Centurion, батареи только с напряжением и ADC-гарнитуры пока не поддерживаются. Название MX/LIGHTSPEED/Unifying/Bolt само по себе совместимость не гарантирует.

**Razer:** запросы только батареи и зарядки, проверка транзакции, команды и CRC; устройство появляется после первого корректного ответа батареи. Беспроводные варианты Viper, DeathAdder, Basilisk, Orochi, Pro Click, BlackWidow и DeathStalker — кандидаты на совместимость, а не модели с подтверждённым аппаратным тестированием. У отдельных моделей другие прошивки и форматы отчётов.

**SteelSeries:** опрашиваются только перечисленные PID и коллекция `FFC0:0001`. Проценты остаются процентами, ступенчатые значения показываются уровнями. Для GameBuds отображается меньший заряд среди активных наушников. Nova Pro/Omni/Elite используют отдельный модуль из таблицы выше. Старые Arctis 1/7/9/Pro Wireless и мыши Aerox/Prime имеют отдельные модули. Мыши Rival пока не включены.

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

## CompX, Keychron and AULA / CompX, Keychron и AULA

CompX support uses the listed receiver IDs only. A confirmed wireless link is required before querying battery; checksums, command and range are validated. Shared receiver IDs use family names rather than guessing a precise variant. Keychron support is limited to the 1K Link receivers and feature `51:06` mouse status; stock keyboard firmware and 4K/8K protocols are not included. The inner mouse ID selects the displayed M1/M2/M3/M6/M7 name. AULA/BY checks the 20-byte frame checksum (or the documented BY single-frame exception) and rejects request echoes. A zero level is treated as unavailable. Background AULA polling is limited to once per minute. All these additions still require physical hardware confirmation.

CompX ограничен перечисленными ID: перед запросом батареи требуется подтверждённая связь с мышью; проверяются контрольная сумма, команда и диапазон. Общие ID отображаются названием семейства. Keychron ограничен приёмниками Link 1K и запросом мыши `51:06`; штатные клавиатуры и протоколы 4K/8K не включены. Название M1/M2/M3/M6/M7 выбирается по ID мыши из ответа. AULA/BY проверяет контрольную сумму 20-байтового кадра или документированное исключение BY и отбрасывает эхо запроса. Нулевой уровень считается отсутствием данных. Фоновый опрос AULA выполняется не чаще раза в минуту. Всем этим дополнениям нужна проверка на реальных устройствах.

Alternate HID collections for a supported physical receiver are tried until one returns a valid battery reading, then merged into one entry. This does not merge a receiver with a separate cable connection. / Альтернативные HID-коллекции одного поддерживаемого приёмника перебираются до достоверного ответа и объединяются в одну запись. Это не объединяет приёмник с отдельным кабельным подключением.

Protocol references / Источники: [CompX and related drivers](https://github.com/ferloom/Wireless-Device-Battery-Monitor/tree/main/src/drivers), [VXE R1 battery research](https://github.com/S1berla/vxe-r1-pro-max-battery), [AULA F75 receiver research](https://github.com/ushineko/sanshoku/issues/36).
## Additional Logitech and Razer headsets / Дополнительные гарнитуры Logitech и Razer

G321 uses feature request `24:2B` and accepts only interrupt replies `27:2B:00:0B`; no charging flag is inferred. G735 discovers battery feature `0104` inside its `F2` wrapper and validates feature index and software ID before accepting a result. These are separate from the generic HID++ path.

Nari accepts only fresh feature `FF` status, with a plausible nonzero voltage and known power state. The level is the headset's own coarse percentage; charging hides that byte because it no longer represents battery capacity. A confirmed charging state is displayed with an unknown level. Stale dongle copies, powered-off and malformed reports are discarded. Nari Ultimate/Essential and other PIDs are not included in this module.

BlackShark V2 Pro support here is specifically the 2023 `0555` receiver. It wraps battery/charging reads in temporary remote-mode routing, closes that routing even after a failed query, rejects unrelated status frames and limits background queries to once every two minutes. The 2020 and V3 families use different protocols and are not included. A missing response retains an explicitly stale last reading. All four modules are experimental pending physical tests.

G321 читает feature-запрос `24:2B` и принимает только input-ответ `27:2B:00:0B`; зарядка не угадывается. G735 обнаруживает батарейный feature `0104` в оболочке `F2` и проверяет индекс и software ID. Это отдельные протоколы, не общий HID++.

Nari принимает только свежий feature-статус `FF` с допустимым ненулевым напряжением и известным состоянием питания. Отображается переданный гарнитурой грубый процент; при зарядке этот байт скрывается, поскольку перестаёт означать ёмкость батареи. Подтверждённая зарядка показывается с неизвестным уровнем. Старая копия приёмника, выключенная гарнитура и неверные ответы исключаются. Nari Ultimate/Essential и другие PID этот модуль не включает.

BlackShark V2 Pro здесь означает только приёмник `0555` модели 2023. Чтение батареи и зарядки временно включает маршрутизацию remote mode и завершает её даже после ошибки. Посторонние статусы отбрасываются, фоновый запрос ограничен одним разом в две минуты. Модели 2020 и V3 используют другие протоколы и пока не включены. При отсутствии ответа последнее показание явно помечается устаревшим. Все четыре модуля экспериментальные до проверки на оборудовании.

References / Источники: [G321 protocol](https://github.com/gleyssonsc/g321-studio/blob/main/LightspeedConnection.cs), [G735 protocol](https://github.com/symNistaken/TaskbarVitals/blob/main/src/drivers/logitech_g735.cpp), [BlackShark 2023 protocol](https://github.com/matej-basic/blackshark-battery), [Nari capture](https://github.com/Modzeleczek/RazerNariBatteryLevel).
## Additional mouse protocols / Дополнительные протоколы мышей

AJ139-family requests use only the documented battery read. Windows unnumbered reports include report ID zero (65 bytes); the alternate 64-byte `F0` path is restricted to `A8A5:2255`. Replies must have the expected command/header and a valid battery range. Receiver zero levels mean unavailable, while a cable can report zero. Shared AJAZZ IDs keep a generic family name. VGN is limited to the listed receiver ID. The AJ179-family module validates its checksum and link flag, rejecting the cached battery of an absent mouse. A successful reply with the mouse linked is required before a new entry appears.

Darmoshark first reads the bonded mouse identity/link, then reads its battery, checking the opcode, inner VID/PID and percentage. No-link acknowledgements stop the query. It does not report charging, and cable variants are not implemented here.

Attack Shark / KP 8K / IO Nova support is passive: no output reports or configuration requests are sent. The exact listed status collection is read for up to 5.5 seconds. Consecutive identical frames confirm a reading; a single reconnect frame claiming 100% on battery is rejected. Model markers choose the name. X6/R1 levels are shown as `n/10`, preserving their native resolution. `FA60` external power does not imply charging; `FA65` state 2 means fully charged. Battery values can be approximate or frozen while on power. All models in this section need hardware confirmation in this project.

AJ139 использует только документированное чтение батареи. Для ненумерованных отчётов Windows передаёт нулевой report ID (65 байт); альтернативный 64-байтовый запрос `F0` ограничен `A8A5:2255`. Проверяются команда, заголовок и диапазон заряда. Ноль от приёмника означает отсутствие данных, по кабелю нулевой заряд допустим. Общий ID AJAZZ сохраняет название семейства; VGN ограничен указанным ID. Модуль AJ179 проверяет контрольную сумму и флаг связи, отбрасывая последнее значение отсутствующей мыши. Новая запись появляется только после ответа с подтверждённой связью.

Darmoshark сначала читает привязку и связь с мышью, затем заряд. Проверяются команда, внутренние VID/PID и проценты. Ответ об отсутствии связи прекращает запрос. Зарядка не определяется; кабельные варианты этот модуль не включает.

Attack Shark / KP 8K / IO Nova читаются пассивно: запросы и команды настройки не отправляются. Читается только указанная коллекция статуса, ожидание ограничено 5,5 секунды. Два одинаковых последовательных кадра подтверждают показание; одиночный кадр переподключения со значением 100% от батареи отбрасывается. Название выбирается по маркеру модели. X6/R1 показывает штатные ступени `n/10`. Для `FA60` внешнее питание не означает зарядку; состояние 2 у `FA65` означает полный заряд. При внешнем питании значение может быть приблизительным или временно не меняться. Всем моделям этого раздела нужна аппаратная проверка в этом проекте.

References / Источники: [AJ139 protocol](https://github.com/werquess/ajazz-aj139-macos/blob/main/PROTOCOL.md), [AJ179 capture research](https://github.com/HeyOkay/HaloBattery/blob/main/providers/ajazz.py), [Darmoshark M3 protocol](https://github.com/Barba2k2/darmoshark-m3-configurator/blob/main/PROTOCOL.md), [Attack Shark passive protocol](https://github.com/daeoxide/attackshark-battery-windows), [Delux M900 research](https://github.com/AeonArcHQ/omarchy-delux-m900/blob/main/docs/re/findings.md).
## SinoWealth, WLMOUSE, NZXT and Endgame Gear

SinoWealth support is restricted to the exact feature-only receiver collection above. It reads link status `05:80` before battery `05:90`, because the receiver can return cached battery with the mouse off. A linked mouse, battery marker `11` and level 0–100 are required. The configuration report `08` is never used. Since `258A:002F` is shared across products, the product string is used when meaningful; a generic receiver stays named “Wireless mouse”. No charging flag is inferred.

WLMOUSE support is restricted to the four listed receiver PIDs. The feature reply must contain the status command `02:00:83` with `A1`/`A2` at the documented offset, with or without a leading zero report ID. Request echoes and invalid levels are rejected. The explicit charging byte is used.

NZXT sends only the telemetry request `4E:02:81:00:B0`. The output size chooses whether report ID zero is required; a `4E:E5` acknowledgement is not a measurement. The percentage is a 16-bit little-endian field, and charging uses bit 0 of the telemetry flags. Cable and receiver remain separate instances; this module does not guess whether two endpoints belong to the same physical mouse.

Endgame Gear queries battery `B4`, handles busy/asleep states and reads identity `0E` for a known model name. Recovery probes `0F` are sent only after an invalid or missing battery response, never after a successful or explicitly asleep response. No charging flag is known for these receivers, so charging is not inferred. The shared 4K receiver is not support for all Endgame Gear products.

SinoWealth ограничен указанной feature-only коллекцией приёмника. Перед батареей `05:90` читается связь `05:80`: приёмник может хранить старый заряд выключенной мыши. Требуются подтверждённая связь, маркер батареи `11` и уровень 0–100. Отчёт настройки `08` не используется. ID `258A:002F` общий, поэтому используется содержательное USB-название; общий приёмник остаётся «Wireless mouse». Зарядка не угадывается.

WLMOUSE ограничен четырьмя перечисленными PID. Feature-ответ должен содержать `02:00:83` и `A1`/`A2` на документированном смещении, с ведущим нулевым report ID или без него. Эхо запроса и неверные уровни исключаются. Используется явный байт зарядки.

NZXT отправляет только запрос телеметрии `4E:02:81:00:B0`. Размер output определяет необходимость ведущего нулевого report ID; подтверждение `4E:E5` не считается замером. Процент читается как 16-битное поле little-endian, зарядка — по биту 0 флагов. Кабель и приёмник остаются отдельными экземплярами: принадлежность одной физической мыши не угадывается.

Endgame Gear читает батарею `B4`, обрабатывает занятость/сон и запрашивает идентификацию `0E` для названия модели. Восстановительные запросы `0F` выполняются только после неверного ответа или ошибки чтения, а не после успешного ответа или явного сна. Флаг зарядки для этих приёмников неизвестен и не выводится косвенно. Общий приёмник 4K не означает поддержку всех устройств Endgame Gear.

All additions here are experimental and need hardware confirmation. / Все дополнения раздела экспериментальные и требуют аппаратной проверки.

References / Источники: [SinoWealth / Redragon capture](https://github.com/OpenMouse-Project/mouse-protocol/blob/main/docs/redragon-m690-pro-testing.md), [WLMOUSE status protocol](https://github.com/HeyOkay/HaloBattery/blob/main/providers/wlmouse.py), [NZXT telemetry](https://github.com/HeyOkay/HaloBattery/blob/main/providers/nzxt.py), [Endgame Gear protocol research](https://github.com/johanneszab/vole/blob/main/re/PROTOCOL.md).