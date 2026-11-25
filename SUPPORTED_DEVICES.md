# Supported devices / Поддерживаемые устройства

**Tested** means checked on physical hardware in this project. **Experimental** means protocol-based implementation and automated tests only; hardware confirmation is still needed. / **Проверено** — испытано на реальном устройстве в этом проекте. **Экспериментально** — реализовано по протоколу и проверено тестами; нужна проверка владельцами устройств.

| Family / Семейство | USB VID:PID | Status / Статус |
|---|---|---|
| MCHOSE X9 receiver / приёмник | `3837:6045` | Tested / Проверено |
| MCHOSE V9 Turbo+ receiver / приёмник | `3837:600A` | Experimental / Экспериментально |
| MCHOSE GLW keyboards: G75 V2, G87 V2, K99 V3 family / клавиатуры | `3837:3033`, collection / коллекция `FF31:0074`, 65-byte reports | Experimental / Экспериментально |
| Xbox and compatible XInput gamepads / Xbox и совместимые геймпады XInput | Windows XInput API, up to 4 slots / до 4 слотов | Protocol-tested; hardware confirmation needed / Проверено тестами; нужна проверка на оборудовании |
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

## Standard controller support / Стандартная поддержка геймпадов

XInput support is based on the Windows driver, not a list of brands. Xbox 360, Xbox One, Xbox Series and third-party controllers can work when exposed through XInput and their driver reports battery data. Compatibility depends on the device, driver and connection. A model name alone is not confirmation. Sony controllers in native DirectInput mode are not covered by this source.

XInput reports **empty / low / medium / full**, not percentages. The widget uses short labels, and the menu shows the full state. Wired controllers show **USB powered**; unknown battery types show **Battery level unavailable**. No percentage is inferred. Low/empty states trigger the existing low-battery notification. Slot numbers identify XInput sessions, not permanent hardware identities.

Поддержка XInput зависит от драйвера Windows, а не от бренда. Могут работать Xbox 360, Xbox One, Xbox Series и сторонние геймпады, если они доступны через XInput и драйвер передаёт состояние батареи. Конкретная модель, драйвер и подключение требуют проверки. Sony в обычном режиме DirectInput этим источником не поддерживаются.

XInput сообщает уровни **разряжен / низкий / средний / полный**, а не проценты. На панели используются короткие подписи, в меню — полные. Для проводных устройств отображается **питание по USB**, для неизвестного типа батареи — **заряд неизвестен**. Низкий уровень вызывает уведомление. Номера слотов относятся к текущему сеансу XInput, а не к постоянному идентификатору устройства.

## Keyboard polling / Опрос клавиатур

GLW keyboards are automatically queried every 10 minutes to avoid repeatedly waking sleeping keyboards. Manual **Refresh** bypasses this interval. K87S is excluded because it uses a different protocol. Only the specified FF31 collection is implemented; the alternative 0001:0000 route is not claimed.

Клавиатуры GLW автоматически опрашиваются раз в 10 минут, чтобы не будить их слишком часто. Ручное **Обновить** выполняет запрос сразу. K87S исключена: у неё другой протокол. Реализована только коллекция FF31; альтернативный путь 0001:0000 пока не заявлен.

Only battery/status queries are implemented. No configuration, lighting, pairing, DPI, or firmware write commands are sent. / Реализованы только запросы заряда/состояния. Команды изменения настроек, подсветки, сопряжения, DPI и прошивки не отправляются.
