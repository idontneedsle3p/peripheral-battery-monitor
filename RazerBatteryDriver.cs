using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32.SafeHandles;

public sealed class RazerBatteryDriver:IBatteryDriver,IRequireBatteryReading {
 public string Id{get{return "razer-battery";}}
 public bool Matches(HidCollection c){return c.Vid==0x1532 && c.Feature==91;}
 public Reading Describe(HidCollection c){string name=string.IsNullOrWhiteSpace(c.Product)?"Razer device":c.Product.Trim();string n=name.ToLowerInvariant();var kind=n.Contains("keyboard") || n.Contains("blackwidow") || n.Contains("deathstalker") || n.Contains("huntsman")?DeviceKind.Keyboard:n.Contains("viper") || n.Contains("basilisk") || n.Contains("deathadder") || n.Contains("orochi") || n.Contains("mouse") || n.Contains("pro click")?DeviceKind.Mouse:DeviceKind.Other;return new Reading(name){Kind=kind,Experimental=true,Transport="Razer USB / receiver"};}
 public static byte[] Request(byte transaction,byte command){var r=new byte[91];r[2]=transaction;r[6]=2;r[7]=7;r[8]=command;Checksum(r);return r;}
 public static void Checksum(byte[] r){byte sum=0;for(int i=3;i<89;i++)sum^=r[i];r[89]=sum;}
 public static byte? Parse(byte[] r,byte transaction,byte command){
  if(r==null || r.Length!=91 || r[0]!=0 || r[1]!=2 || r[2]!=transaction || r[3]!=0 || r[4]!=0 || r[5]!=0 || r[6]!=2 || r[7]!=7 || r[8]!=command)return null;
  byte sum=0;for(int i=3;i<89;i++)sum^=r[i];if(sum!=r[89])return null;return r[10];
 }
 byte? Query(SafeFileHandle h,byte transaction,byte command){
  var request=Request(transaction,command);if(!Hid.HidD_SetFeature(h,request,91))return null;
  var time=Stopwatch.StartNew();while(time.ElapsedMilliseconds<180){Thread.Sleep(20);var r=new byte[91];if(!Hid.HidD_GetFeature(h,r,91))return null;var value=Parse(r,transaction,command);if(value.HasValue)return value;}
  return null;
 }
 public Reading Read(HidCollection c,Action<string> log){
  var reading=Describe(c);
  using(var h=Hid.CreateFile(c.Path,0,3,IntPtr.Zero,3,0,IntPtr.Zero)){
   if(h.IsInvalid)return reading;
   foreach(byte tid in new byte[]{0x1F,0x3F,0xFF,0x08,0x9F}){
    var value=Query(h,tid,0x80);if(!value.HasValue)continue;
    reading.Percent=(value.Value*100+127)/255;reading.Time=DateTime.Now;reading.State="Подключено";
    var charge=Query(h,tid,0x84);reading.Charging=charge==1;log("Razer battery "+reading.Percent);break;
   }
  }return reading;
 }
}
