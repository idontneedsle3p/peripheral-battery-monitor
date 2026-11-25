using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

public enum DeviceKind { Headset, Mouse, Keyboard, Controller, Other }
public enum BatteryLevel { Empty, Low, Medium, Full, Wired, Unknown }
public class Reading {
 public string Id,Name,State="Нет ответа",Transport="USB-приёмник · 2,4 ГГц";
 public DeviceKind Kind;public int? Percent;public bool Charging,IsStale,Experimental;public DateTime? Time;
 public BatteryLevel? Level;
 public int? Steps;public int MaxSteps;
 public bool HasBattery{get{return Percent.HasValue || Level.HasValue || Steps.HasValue;}}
 public bool LowBattery{get{return Percent.HasValue?Percent<=15:Steps.HasValue?Steps.Value*100<=MaxSteps*15:Level==BatteryLevel.Empty || Level==BatteryLevel.Low;}}
 public string ShortDisplay{get{return Percent.HasValue?Percent+"%":Steps.HasValue?Steps+"/"+MaxSteps:Level.HasValue?L.T(new[]{"Пусто","Низк.","Сред.","Полн.","USB","?"}[(int)Level.Value]):"—";}}
 public Reading(string name){Name=name;Kind=DeviceKind.Other;}
 public string Display {get{return (Steps.HasValue?L.T("Уровень ")+Steps+"/"+MaxSteps:Percent.HasValue?Percent+"%":Level.HasValue?L.T(new[]{"Разряжен","Низкий заряд","Средний заряд","Полный заряд","Питание по USB","Заряд неизвестен"}[(int)Level.Value]):"—")+(Charging?L.T(" · зарядка"):"");}}
}
public interface IBatteryDriver {
 string Id {get;}
 bool Matches(HidCollection collection);
 Reading Describe(HidCollection collection);
 Reading Read(HidCollection collection,Action<string> log);
}
public interface IBatteryPollingPolicy {TimeSpan MinimumPollInterval{get;}}
public interface IRequireBatteryReading {}
public sealed class BatteryMonitor {
 readonly IBatteryDriver[] drivers;
 readonly Dictionary<string,Reading> known=new Dictionary<string,Reading>(StringComparer.OrdinalIgnoreCase);
 readonly Dictionary<string,DateTime> lastPoll=new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase);
 public BatteryMonitor(params IBatteryDriver[] modules){drivers=modules;}
 public Reading[] Poll(List<HidCollection> collections,Action<string> log,bool force=false){
  var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  foreach(var c in collections){
   var driver=drivers.FirstOrDefault(d=>d.Matches(c));if(driver==null)continue;
   string id=driver.Id+":"+c.Path;if(!seen.Add(id))continue;
   var policy=driver as IBatteryPollingPolicy;Reading cached;DateTime attempted;
   if(!force && policy!=null && known.TryGetValue(id,out cached) && cached.State!="Отключено" && lastPoll.TryGetValue(id,out attempted) && DateTime.UtcNow-attempted<policy.MinimumPollInterval)continue;
   lastPoll[id]=DateTime.UtcNow;
   Reading current;
   try{current=driver.Read(c,log);if(current==null)throw new InvalidOperationException("Пустой ответ модуля");}
   catch(Exception e){log(driver.Id+": "+e.Message);current=driver.Describe(c);current.State="Ошибка чтения";}
   current.Id=id;var description=driver.Describe(c);current.Kind=description.Kind;current.Experimental=description.Experimental;Reading previous;
   if(current.Percent.HasValue && (current.Percent<0 || current.Percent>100)){current.Percent=null;current.State="Некорректный заряд";}
   if(current.Steps.HasValue && (current.MaxSteps<=0 || current.Steps<0 || current.Steps>current.MaxSteps)){current.Steps=null;current.State="Некорректный заряд";}
   if(driver is IRequireBatteryReading && !current.HasBattery && !known.ContainsKey(id))continue;
   if(!current.HasBattery && known.TryGetValue(id,out previous) && previous.HasBattery){current.Percent=previous.Percent;current.Level=previous.Level;current.Steps=previous.Steps;current.MaxSteps=previous.MaxSteps;current.Time=previous.Time;current.IsStale=true;current.Charging=false;current.State="Нет ответа · последнее значение";}
   known[id]=current;
  }
  foreach(var pair in known)if(!seen.Contains(pair.Key)){pair.Value.IsStale=true;pair.Value.Charging=false;pair.Value.State="Отключено";}
  return known.Values.OrderBy(r=>r.Kind).ThenBy(r=>r.Name).ThenBy(r=>r.Id,StringComparer.OrdinalIgnoreCase).ToArray();
 }
}
public static class Devices {
 static readonly IBatterySource[] sources={new XInputSource(),new BluetoothBatterySource(),new LogitechBatterySource()};
 static readonly BatteryMonitor monitor=new BatteryMonitor(new MchoseX9Driver(),new MchoseAx5Driver(),new MchoseClassicDriver(),new MchoseModernDriver(),new MchoseKeyboardDriver(),new RazerBatteryDriver(),new SteelSeriesBatteryDriver(),new PlayStationBatteryDriver(),new SteelSeriesProBatteryDriver());
 public static Reading[] Poll(List<HidCollection> colls,Action<string> log,bool force=false){var result=monitor.Poll(colls,log,force).ToList();foreach(var source in sources){try{result.AddRange(source.Poll(log));}catch(Exception e){log(source.GetType().Name+": "+e.Message);}}return result.ToArray();}
 public static int? ParseX9(byte[] r){if(r!=null && r.Length>=4 && r[0]==0x55 && r[1]==0x65 && r[3]==2 && r[2]<=100)return r[2];return null;}
 public static Reading ParseMouse(byte[] r){
  if(r==null || r.Length<9 || r[0]!=0 || r[1]!=0xA1 || r[3]!=2 || r[5]!=0 || r[6]!=0x83 || r[7]>1 || r[8]>100)return null;
  return new Reading("MCHOSE AX5 PRO MAX"){Percent=r[8],Charging=r[7]==1,State="Подключена",Time=DateTime.Now};
 }
}
