using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32.SafeHandles;

// HID++ 2.x feature discovery, without receiver configuration or notification writes.
public sealed class LogitechBatterySource:IBatterySource {
 readonly Func<List<HidCollection>> enumerate;readonly Dictionary<string,Reading> known=new Dictionary<string,Reading>();
 public LogitechBatterySource():this(Hid.Enumerate){}
 public LogitechBatterySource(Func<List<HidCollection>> discovery){enumerate=discovery;}
 public static bool Matches(HidCollection c){return c.Vid==0x046D && (c.Input==7 && c.Output==7 && c.Page==0xFF00 && c.Usage==1 || c.Input==20 && c.Output==20 && (c.Page==0xFF00 && c.Usage==2 || c.Page==0xFF43 && (c.Usage==0x202 || c.Pid==0xC54F && c.Usage==0x302)));}
 public static Reading ParseLegacy(byte register,byte[] reply){
  if(reply==null || reply.Length<2 || reply[0]==0)return null;var r=new Reading("Logitech wireless device"){Experimental=true,State="Подключено",Time=DateTime.Now,Transport="Logitech HID++ 1.x"};
  if(register==0x0D){if(reply.Length<3 || reply[0]>100)return null;r.Percent=reply[0];r.Charging=reply[2]>=0x21 && reply[2]<=0x26;}
  else if(register==7){if(reply[0]>7)return null;r.Level=reply[0]==7?BatteryLevel.Full:reply[0]>=5?BatteryLevel.Medium:BatteryLevel.Low;r.Charging=reply[1]>=0x21 && reply[1]<=0x26;}else return null;return r;
 }
 static byte[] LegacyCall(SafeFileHandle h,byte index,byte register){
  byte[] request={0x10,index,0x81,register,0,0,0};if(Hid.Transfer(h,request,true,150)==null)return null;var clock=Stopwatch.StartNew();while(clock.ElapsedMilliseconds<200){var r=Hid.Transfer(h,new byte[7],false,Math.Max(1,200-(int)clock.ElapsedMilliseconds));if(r==null)break;if(r.Length==7 && r[0]==0x10 && r[1]==index && r[2]==0x81 && r[3]==register)return r.Skip(4).ToArray();if(r.Length>=6 && r[1]==index && r[2]==0x8F && r[3]==0x81 && r[4]==register)return null;}return null;
 }
 void ReadLegacy(HidCollection c,SafeFileHandle h,string key,Action<string> log){
  foreach(byte index in new byte[]{255,1,2,3,4,5,6}){string id="logitech:"+key+":"+index;Reading previous;if(known.TryGetValue(id,out previous) && !previous.IsStale)continue;
   Reading r=null;foreach(byte register in new byte[]{0x0D,7}){r=ParseLegacy(register,LegacyCall(h,index,register));if(r!=null)break;}if(r==null)continue;
   r.Id=id;r.Name=previous!=null?previous.Name:index==255 && !string.IsNullOrWhiteSpace(c.Product)?c.Product:"Logitech wireless device #"+index;if(previous!=null)r.Kind=previous.Kind;known[id]=r;log(r.Name+": "+r.Display);
  }
 }
 public static string PhysicalKey(string path){var p=(path??"").ToLowerInvariant().Split('#');if(p.Length<4)return path;return Regex.Replace(p[1],"&col[0-9a-f]{2}","")+"#"+Regex.Replace(p[2],"&[0-9a-f]{4}$","");}
 public static byte[] Request(byte index,byte feature,byte function,byte[] parameters){var r=new byte[20];r[0]=0x11;r[1]=index;r[2]=feature;r[3]=(byte)((function<<4)|0x0D);Array.Copy(parameters,0,r,4,Math.Min(parameters.Length,16));return r;}
 public static byte[] Response(byte[] r,byte index,byte feature,byte function){if(r==null || r.Length<7 || r[0]!=0x11 && r[0]!=0x10 || r[1]!=index || r[2]!=feature || r[3]!=(byte)((function<<4)|0x0D))return null;return r.Skip(4).ToArray();}
 static byte[] Call(SafeFileHandle h,byte index,byte feature,byte function,params byte[] parameters){
  var request=Request(index,feature,function,parameters);if(Hid.Transfer(h,request,true,150)==null)return null;
  var clock=Stopwatch.StartNew();while(clock.ElapsedMilliseconds<250){var r=Hid.Transfer(h,new byte[20],false,Math.Max(1,250-(int)clock.ElapsedMilliseconds));if(r==null)break;var p=Response(r,index,feature,function);if(p!=null)return p;if(r.Length>=6 && r[1]==index && (r[2]==0xFF || r[2]==0x8F) && r[3]==feature && r[4]==request[3])return null;}return null;
 }
 static byte Feature(SafeFileHandle h,byte index,ushort id){var r=Call(h,index,0,0,(byte)(id>>8),(byte)id);return r!=null && r.Length>=3?r[0]:(byte)0;}
 public static Reading Parse(ushort feature,byte[] p){
  if(p==null || p.Length<3 || p[0]>100 || p[2]>4 || feature!=0x1000 && feature!=0x1004)return null;
  var r=new Reading("Logitech device"){Experimental=true,State="Подключено",Time=DateTime.Now,Transport="Logitech HID++"};
  if(feature==0x1004 && p[0]==0){if((p[1]&0xF0)!=0)return null;r.Level=p[1]==0?(p[2]==3?BatteryLevel.Full:BatteryLevel.Unknown):(p[1]&8)!=0?BatteryLevel.Full:(p[1]&4)!=0?BatteryLevel.Medium:BatteryLevel.Low;}
  else r.Percent=p[0];r.Charging=p[2]>=1 && p[2]<=3;return r;
 }
 void ReadReceiver(HidCollection c,Action<string> log){
  string key=PhysicalKey(c.Path);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){
   if(h.IsInvalid)return;Hid.HidD_FlushQueue(h);if(c.Output==7){ReadLegacy(c,h,key,log);return;}var watch=Stopwatch.StartNew();var direct=Call(h,255,0,1,0,0,0x5A);
   byte[] indices=direct!=null && direct.Length>=3 && direct[0]>=2 && direct[2]==0x5A?new byte[]{255}:c.Pid==0xC54F?new byte[]{1}:new byte[]{1,2,3,4,5,6};
   foreach(byte index in indices){if(watch.ElapsedMilliseconds>8000)break;if(index!=255){var ping=Call(h,index,0,1,0,0,0x5A);if(ping==null || ping.Length<3 || ping[0]<2 || ping[2]!=0x5A)continue;}
    Reading r=null;foreach(ushort battery in new ushort[]{0x1004,0x1000}){byte f=Feature(h,index,battery);if(f==0)continue;r=Parse(battery,Call(h,index,f,(byte)(battery==0x1004?1:0)));if(r!=null)break;}
    if(r==null)continue;r.Id="logitech:"+key+":"+index;r.Name=index==255 && !string.IsNullOrWhiteSpace(c.Product)?c.Product:"Logitech device #"+index;
    byte naming=Feature(h,index,5);if(naming!=0){var size=Call(h,index,naming,0);if(size!=null && size.Length>0 && size[0]>0 && size[0]<=128){var bytes=new List<byte>();while(bytes.Count<size[0]){var part=Call(h,index,naming,1,(byte)bytes.Count);if(part==null || part.Length==0)break;bytes.AddRange(part.Take(Math.Min(part.Length,size[0]-bytes.Count)));}if(bytes.Count==size[0])r.Name=Encoding.UTF8.GetString(bytes.ToArray()).TrimEnd('\0');}
     var type=Call(h,index,naming,2);if(type!=null && type.Length>0)r.Kind=type[0]<=2?DeviceKind.Keyboard:type[0]>=3 && type[0]<=5?DeviceKind.Mouse:type[0]==8?DeviceKind.Headset:DeviceKind.Other;}
    known[r.Id]=r;log(r.Name+": "+r.Display);
   }
  }
 }
 public Reading[] Poll(Action<string> log){
  foreach(var r in known.Values){r.IsStale=true;r.State="Нет ответа · последнее значение";}
  try{foreach(var group in enumerate().Where(Matches).GroupBy(c=>PhysicalKey(c.Path))){foreach(var c in group.OrderByDescending(c=>c.Output)){try{ReadReceiver(c,log);}catch(Exception e){log("Logitech: "+e.Message);}}}}catch(Exception e){log("Logitech discovery: "+e.Message);}
  return known.Values.ToArray();
 }
}
