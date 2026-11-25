using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32.SafeHandles;

// Shared transport only: each protocol still validates its own response.
public static class BatteryQuery {
 public static Reading Ask(SafeFileHandle handle,HidCollection c,byte[] prefix,Func<byte[],Reading> parse,int timeout=800){
  if(prefix.Length>c.Output)return null;var request=new byte[c.Output];Array.Copy(prefix,request,prefix.Length);Hid.HidD_FlushQueue(handle);
  if(Hid.Transfer(handle,request,true,250)==null)return null;
  var watch=Stopwatch.StartNew();while(watch.ElapsedMilliseconds<timeout){var reply=Hid.Transfer(handle,new byte[c.Input],false,Math.Max(1,timeout-(int)watch.ElapsedMilliseconds));if(reply==null)break;var reading=parse(reply);if(reading!=null)return reading;}return null;
 }
 public static Reading Headset(string name,int percent,bool charging=false){return new Reading(name){Kind=DeviceKind.Headset,Experimental=true,Percent=percent,Charging=charging,State="Подключено",Time=DateTime.Now};}
}
public sealed class CorsairVoidBatteryDriver:IBatteryDriver,IRequireBatteryReading {
 static readonly ushort[] Pids={0x1B1C,0x1B27,0x0A14,0x0A16,0x0A17,0x0A1D,0x0A1A,0x1B2A,0x1B23,0x1B29,0x0A55,0x0A51,0x0A52,0x0A38,0x0A4F,0x0A2B,0x0A75,0x0A56};
 public string Id{get{return "corsair-void";}}
 public bool Matches(HidCollection c){return c.Vid==0x1B1C && Pids.Contains(c.Pid) && c.Page==0xFFC5 && c.Usage==1 && c.Input>=5 && c.Output>=2;}
 public Reading Describe(HidCollection c){return new Reading(string.IsNullOrWhiteSpace(c.Product)?"Corsair Void / HS70":c.Product.Trim()){Kind=DeviceKind.Headset,Experimental=true};}
 public static Reading Parse(byte[] r){if(r==null || r.Length<5 || r[0]!=0x64 || r[3]!=177 && r[3]!=16 || r[4]<1 || r[4]>5 || (r[2]&127)>100)return null;return BatteryQuery.Headset("Corsair headset",r[2]&127,r[4]==5);}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return original;var r=BatteryQuery.Ask(h,c,new byte[]{0xC9,0x64},Parse);if(r!=null){r.Name=original.Name;return r;}}return original;}
}
public sealed class HyperXBatteryDriver:IBatteryDriver,IRequireBatteryReading {
 static readonly ushort[] Modern={0x06BE,0x02CC,0x03C6},Core={0x0995,0x0795,0x05B7,0x0C9D};
 public string Id{get{return "hyperx-wireless";}}
 public bool Matches(HidCollection c){return c.Vid==0x03F0 && c.Usage==1 && (Modern.Contains(c.Pid)?c.Page==0x01C0 && c.Input>=9 && c.Output==64:Core.Contains(c.Pid) && c.Page==0xFF13 && c.Input>=62 && c.Output==62);}
 public Reading Describe(HidCollection c){string n=c.Pid==0x03C6?"Cloud Stinger 3 Wireless":Modern.Contains(c.Pid)?"Cloud III S Wireless":c.Pid==0x05B7 || c.Pid==0x0C9D?"Cloud III Wireless":"Cloud II Core Wireless";return new Reading("HyperX "+n){Kind=DeviceKind.Headset,Experimental=true};}
 public static Reading ParseModern(byte[] r){if(r==null || r.Length<9 || !r.Take(6).SequenceEqual(new byte[]{12,2,3,1,0,6}) || r[6]>100)return null;int mv=(r[7]<<8)|r[8];if(mv==0 && r[6]==0 || mv!=0 && (mv<2500 || mv>5000))return null;return BatteryQuery.Headset("HyperX headset",r[6]);}
 public static Reading ParseCore(byte[] r){if(r==null || r.Length<5 || r[0]!=0x66 || r[1]!=0x89 && r[1]!=0x0D || r[2]==0 && r[3]==0 || r[4]>100)return null;return BatteryQuery.Headset("HyperX headset",r[4]);}
 public static Reading ParseCharging(byte[] r){if(r==null || r.Length<3 || r[0]!=0x66 || r[1]!=0x8A && r[1]!=0x0C || r[2]>2)return null;return BatteryQuery.Headset("HyperX headset",0,r[2]==1);}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return original;
  var r=Modern.Contains(c.Pid)?BatteryQuery.Ask(h,c,new byte[]{12,2,3,1,0,6},ParseModern,1200):BatteryQuery.Ask(h,c,new byte[]{0x66,0x89},ParseCore,1200);
  if(r!=null){if(Core.Contains(c.Pid)){var charging=BatteryQuery.Ask(h,c,new byte[]{0x66,0x8A},ParseCharging,600);if(charging!=null)r.Charging=charging.Charging;}r.Name=original.Name;return r;}
 }return original;}
}
