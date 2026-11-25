using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

public static class L {
 public static string Language {get;private set;}
 static L(){Language="ru";}
 static readonly Dictionary<string,string> English=new Dictionary<string,string>{
  {"Уровень ","Level "},
  {"Пусто","Empty"},{"Низк.","Low"},{"Сред.","Med"},{"Полн.","Full"},
  {"Разряжен","Empty"},{"Низкий заряд","Low battery"},{"Средний заряд","Medium battery"},{"Полный заряд","Full battery"},{"Питание по USB","USB powered"},{"Заряд неизвестен","Battery level unavailable"},
  {" · экспериментально"," · experimental"},
  {"Обновить","Refresh"},{"Подключение…","Connecting…"},{"Устройства","Devices"},
  {"Заряд на панели задач","Show battery on taskbar"},{"Запускать с Windows","Start with Windows"},
  {"Автозапуск","Startup"},{"Открыть журнал","Open log"},{"Выход","Exit"},{"Мониторы","Monitors"},
  {"Все мониторы","All monitors"},{"Только основной","Primary monitor only"},{"Монитор ","Monitor "},
  {" · основной"," · primary"},{"Выбранный монитор отключён","Selected monitor is disconnected"},
  {"Читаю заряд устройств…","Reading battery levels…"},{"Пора зарядить ","Time to charge "},
  {"Осталось ","Remaining: "},{"Обновлено ","Updated "},{" · каждые 30 с · уведомления при ≤ 15%"," · every 30 s · alerts at ≤ 15%"},
  {"Ошибка: ","Error: "},{"Peripheral Battery Monitor: нет устройств","Peripheral Battery Monitor: no devices"},
  {"Поддерживаемые устройства не найдены","No supported devices found"},{" · старое значение"," · last known value"},
  {"Последний замер: ","Last reading: "},{"Нет замеров","No readings yet"},{"Нет устройств","No devices"},
  {"Нет ответа","No response"},{"Подключена","Connected"},{"Подключено","Connected"},{"Подключено · данные Windows","Connected · Windows data"},
  {"Нет ответа · последнее значение","No response · last known value"},{"Отключено","Disconnected"},
  {"Приёмник занят или нет доступа","Receiver busy or access denied"},{"Ошибка запроса","Request failed"},
  {"Ошибка чтения","Read failed"},{"Некорректный заряд","Invalid battery level"},
  {"USB-приёмник · 2,4 ГГц","USB receiver · 2.4 GHz"},{"USB-кабель","USB cable"},{" · зарядка"," · charging"}
 };
 public static void Set(string language){if(language!="ru" && language!="en")throw new ArgumentException("Unsupported language");Language=language;}
 public static string T(string text){string translated;return Language=="en" && English.TryGetValue(text,out translated)?translated:text;}
 public static void Load(string path){
  string language=CultureInfo.InstalledUICulture.TwoLetterISOLanguageName=="ru"?"ru":"en";
  try{string saved=File.ReadAllText(path).Trim();if(saved=="ru" || saved=="en")language=saved;}catch(IOException){}catch(UnauthorizedAccessException){}
  Set(language);
 }
 public static void Save(string path){File.WriteAllText(path,Language);}
}
