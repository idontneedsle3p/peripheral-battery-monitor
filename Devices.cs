using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

public enum DeviceKind { Headset, Mouse, Keyboard, Controller, Other }
public class Reading {
 public string Id,Name,State="Нет ответа",Transport="USB-приёмник · 2,4 ГГц";
 public DeviceKind Kind;public int? Percent;public bool Charging,IsStale;public DateTime? Time;
 public Reading(string name){Name=name;Kind=DeviceKind.Other;}
 public string Display {get{return Percent.HasValue?Percent+"%"+(Charging?L.T(" · зарядка"):""):"—";}}
}
public interface IBatteryDriver {
 string Id {get;}
 bool Matches(HidCollection collection);
 Reading Describe(HidCollection collection);
 Reading Read(HidCollection collection,Action<string> log);
}
public sealed class BatteryMonitor {
 readonly IBatteryDriver[] drivers;
 readonly Dictionary<string,Reading> known=new Dictionary<string,Reading>(StringComparer.OrdinalIgnoreCase);
 public BatteryMonitor(params IBatteryDriver[] modules){drivers=modules;}
 public Reading[] Poll(List<HidCollection> collections,Action<string> log){
  var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  foreach(var c in collections){
   var driver=drivers.FirstOrDefault(d=>d.Matches(c));if(driver==null)continue;
   string id=driver.Id+":"+c.Path;if(!seen.Add(id))continue;
   Reading current;
   try{current=driver.Read(c,log);if(current==null)throw new InvalidOperationException("Пустой ответ модуля");}
   catch(Exception e){log(driver.Id+": "+e.Message);current=driver.Describe(c);current.State="Ошибка чтения";}
   current.Id=id;current.Kind=driver.Describe(c).Kind;Reading previous;
   if(current.Percent.HasValue && (current.Percent<0 || current.Percent>100)){current.Percent=null;current.State="Некорректный заряд";}
   if(!current.Percent.HasValue && known.TryGetValue(id,out previous) && previous.Percent.HasValue){current.Percent=previous.Percent;current.Time=previous.Time;current.IsStale=true;current.Charging=false;current.State="Нет ответа · последнее значение";}
   known[id]=current;
  }
  foreach(var pair in known)if(!seen.Contains(pair.Key)){pair.Value.IsStale=true;pair.Value.Charging=false;pair.Value.State="Отключено";}
  return known.Values.OrderBy(r=>r.Kind).ThenBy(r=>r.Name).ThenBy(r=>r.Id,StringComparer.OrdinalIgnoreCase).ToArray();
 }
}
public static class Devices {
 static readonly BatteryMonitor monitor=new BatteryMonitor(new MchoseX9Driver(),new MchoseAx5Driver());
 public static Reading[] Poll(List<HidCollection> colls,Action<string> log){return monitor.Poll(colls,log);}
 public static int? ParseX9(byte[] r){if(r!=null && r.Length>=4 && r[0]==0x55 && r[1]==0x65 && r[3]==2 && r[2]<=100)return r[2];return null;}
 public static Reading ParseMouse(byte[] r){
  if(r==null || r.Length<9 || r[0]!=0 || r[1]!=0xA1 || r[3]!=2 || r[5]!=0 || r[6]!=0x83 || r[7]>1 || r[8]>100)return null;
  return new Reading("MCHOSE AX5 PRO MAX"){Percent=r[8],Charging=r[7]==1,State="Подключена",Time=DateTime.Now};
 }
}
