# Supported devices / Поддерживаемые устройства

**Tested** means checked on physical hardware in this project. **Experimental** means protocol-based implementation and automated tests only; hardware confirmation is still needed. / **Проверено** — испытано на реальном устройстве в этом проекте. **Экспериментально** — реализовано по протоколу и проверено тестами; нужна проверка владельцами устройств.

| Family / Семейство | USB VID:PID | Status / Статус |
|---|---|---|
| MCHOSE X9 receiver / приёмник | `3837:6045` | Tested / Проверено |
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

Only battery/status queries are implemented. No configuration, lighting, pairing, DPI, or firmware write commands are sent. / Реализованы только запросы заряда/состояния. Команды изменения настроек, подсветки, сопряжения, DPI и прошивки не отправляются.
