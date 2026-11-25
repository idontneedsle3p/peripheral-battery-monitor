using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32.SafeHandles;

// Status-only requests. Exact receiver and collection checks keep these protocols separate.
public sealed class LogitechG321BatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider {
 public string Id{get{return "logitech-g321";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public bool Matches(HidCollection c){return c.Vid==0x046D && c.Pid==0x0B39 && c.Page==0xFF00 && c.Usage==0x212 && c.Input>=15 && c.Input<=4096 && c.Feature>=3 && c.Feature<=4096 && (c.Path??"").IndexOf("&mi_03",StringComparison.OrdinalIgnoreCase)>=0;}
 public Reading Describe(HidCollection c){return new Reading("Logitech G321 LIGHTSPEED"){Kind=DeviceKind.Headset,Experimental=true};}
 public static Reading Parse(byte[] r){if(r==null || r.Length<15 || r[0]!=0x27 || r[1]!=0x2B || r[2]!=0 || r[3]!=0x0B || r[14]>100)return null;return BatteryQuery.Headset("Logitech G321 LIGHTSPEED",r[14]);}
 public Reading Read(HidCollection c,Action<string> log){using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return Describe(c);var req=new byte[c.Feature];req[0]=0x24;req[1]=0x2B;Hid.HidD_FlushQueue(h);if(!Hid.HidD_SetFeature(h,req,req.Length))return Describe(c);var watch=Stopwatch.StartNew();while(watch.ElapsedMilliseconds<800){var r=Hid.Transfer(h,new byte[c.Input],false,Math.Max(1,800-(int)watch.ElapsedMilliseconds));if(r==null)break;var reading=Parse(r);if(reading!=null)return reading;}}return Describe(c);}
}

public sealed class LogitechG735BatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider {
 public string Id{get{return "logitech-g735";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public bool Matches(HidCollection c){return c.Vid==0x046D && c.Pid==0x0AD8 && c.Page==0xFF03 && c.Usage==1 && c.Input==271 && c.Output==271;}
 public Reading Describe(HidCollection c){return new Reading("Logitech G735"){Kind=DeviceKind.Headset,Experimental=true};}
 public static byte[] Request(byte index,bool discover){var r=new byte[271];var header=new byte[]{0xF2,1,5,0x1E,0xFA,0x82,0x40,0xCE,0,0,0,0x20,0,0,0,0};Array.Copy(header,r,header.Length);r[16]=index;r[17]=0x0B;if(discover){r[18]=1;r[19]=4;}return r;}
 public static bool IsReply(byte[] r,byte index){return r!=null && r.Length==271 && r[0]==0xF2 && r[1]==0 && r[16]==index && r[17]==0x0B;}
 public static Reading Parse(byte[] r,byte index){if(index==0 || !IsReply(r,index) || r[18]>100)return null;return BatteryQuery.Headset("Logitech G735",r[18],r[20]==1 || r[20]==2);}
 public Reading Read(HidCollection c,Action<string> log){using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return Describe(c);byte index=0;var found=BatteryQuery.Ask(h,c,Request(0,true),r=>{if(!IsReply(r,0) || r[18]==0 || r[18]==255)return null;index=r[18];return new Reading("feature");},500);if(found==null)return Describe(c);var reading=BatteryQuery.Ask(h,c,Request(index,false),r=>Parse(r,index),500);if(reading!=null)return reading;}return Describe(c);}
}

public sealed class RazerBlackShark2023BatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider,IBatteryPollingPolicy {
 public string Id{get{return "razer-blackshark-2023";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public TimeSpan MinimumPollInterval{get{return TimeSpan.FromMinutes(2);}}
 public bool Matches(HidCollection c){return c.Vid==0x1532 && c.Pid==0x0555 && c.Page==0xFF00 && c.Input==64 && c.Output==64;}
 public Reading Describe(HidCollection c){return new Reading("Razer BlackShark V2 Pro (2023)"){Kind=DeviceKind.Headset,Experimental=true};}
 public static byte[] Remote(bool on){var r=new byte[64];Array.Copy(new byte[]{2,0x80,7,0,0,0x50,0x41,0x0E,0,2,0xE1,(byte)(on?1:0)},r,12);return r;}
 public static byte[] Request(byte cmd){var r=new byte[64];Array.Copy(new byte[]{2,0x80,8,0,0,0x50,0x41,8,0,3,cmd,0},r,12);return r;}
 public static byte? Value(byte[] r,byte cmd){if(r==null || r.Length<16 || r[0]!=2 || r[1]!=0x0E || r[2]!=0x50 || r[3]!=0x49 || r[12]!=cmd || r[13]!=1 || r[14]!=1)return null;return r[15];}
 static byte? Query(SafeFileHandle h,HidCollection c,byte cmd){byte? value=null;try{if(Hid.Transfer(h,Remote(true),true,250)==null)return null;Thread.Sleep(35);for(int attempt=0;attempt<3;attempt++){if(Hid.Transfer(h,Remote(true),true,250)==null)break;Thread.Sleep(35);var result=BatteryQuery.Ask(h,c,Request(cmd),r=>{value=Value(r,cmd);return value.HasValue?new Reading("status"):null;},150);if(result!=null)break;}return value;}finally{Hid.Transfer(h,Remote(false),true,250);}}
 public Reading Read(HidCollection c,Action<string> log){using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return Describe(c);var level=Query(h,c,0x21);if(!level.HasValue || level>100)return Describe(c);var charging=Query(h,c,0x2A);return BatteryQuery.Headset("Razer BlackShark V2 Pro (2023)",level.Value,charging.HasValue && charging.Value!=0);} }
}

public sealed class RazerNariBatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider {
 public string Id{get{return "razer-nari";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public bool Matches(HidCollection c){return c.Vid==0x1532 && c.Pid==0x051C && c.Page==0xFF00 && c.Usage==1 && c.Feature>=64 && c.Feature<=4096;}
 public Reading Describe(HidCollection c){return new Reading("Razer Nari"){Kind=DeviceKind.Headset,Experimental=true};}
 public static Reading Parse(byte[] r){var prefix=new byte[]{0xFF,0x0F,5,0xFE,0x12,4,0x1F,8,5};if(r==null || r.Length<15)return null;for(int i=0;i<prefix.Length;i++)if(r[i]!=prefix[i])return null;int mv=r[12]<<8|r[13];if(mv<2500 || mv>5000)return null;byte state=r[11];if(state!=1 && state!=2 && state!=3 && state!=6 && state!=9)return null;if(state!=9 && r[14]>100)return null;var reading=BatteryQuery.Headset("Razer Nari",r[14]);if(state==9){reading.Percent=null;reading.Level=BatteryLevel.Unknown;reading.Charging=true;}return reading;}
 public Reading Read(HidCollection c,Action<string> log){using(var h=Hid.CreateFile(c.Path,0,3,IntPtr.Zero,3,0,IntPtr.Zero)){if(h.IsInvalid)return Describe(c);var req=new byte[c.Feature];Array.Copy(new byte[]{0xFF,0x0A,0,0xFD,4,0x12,0xF1,2,5},req,9);if(!Hid.HidD_SetFeature(h,req,req.Length))return Describe(c);for(int i=0;i<5;i++){Thread.Sleep(100);var r=new byte[c.Feature];r[0]=255;if(!Hid.HidD_GetFeature(h,r,r.Length))continue;var reading=Parse(r);if(reading!=null)return reading;}}return Describe(c);}
}
