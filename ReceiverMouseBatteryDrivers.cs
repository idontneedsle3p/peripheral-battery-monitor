using System;
using System.Threading;
using Microsoft.Win32.SafeHandles;

public sealed class SinoWealthBatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider {
 public string Id{get{return "sinowealth-receiver";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public bool Matches(HidCollection c){return c.Vid==0x258A && c.Pid==0x002F && c.Page==0xFF00 && c.Usage==1 && c.Feature==8 && c.Input==0 && c.Output==0 && (c.Path??"").IndexOf("&mi_01",StringComparison.OrdinalIgnoreCase)>=0;}
 public Reading Describe(HidCollection c){string name=(c.Product??"").Trim();if(name.Length==0 || name.IndexOf("receiver",StringComparison.OrdinalIgnoreCase)>=0 || name.IndexOf("2.4g",StringComparison.OrdinalIgnoreCase)>=0)name="Wireless mouse";return new Reading(name){Kind=DeviceKind.Mouse,Experimental=true};}
 public static bool Linked(byte[] r){return r!=null && r.Length==8 && r[0]==5 && r[1]==0x80 && r[2]==1;}
 public static int? Level(byte[] r){return r!=null && r.Length==8 && r[0]==5 && r[1]==0x90 && r[2]==0x11 && r[3]<=100?(int?)r[3]:null;}
 public static int? ReadLevel(Func<byte,byte[]> ask){if(!Linked(ask(0x80)))return null;return Level(ask(0x90));}
 static byte[] Query(SafeFileHandle h,byte command){var r=new byte[8];r[0]=5;r[1]=command;if(!Hid.HidD_SetFeature(h,r,r.Length))return null;Thread.Sleep(100);r=new byte[8];r[0]=5;return Hid.HidD_GetFeature(h,r,r.Length)?r:null;}
 public Reading Read(HidCollection c,Action<string> log){var reading=Describe(c);using(var h=Hid.CreateFile(c.Path,0,3,IntPtr.Zero,3,0,IntPtr.Zero)){if(h.IsInvalid)return reading;var level=ReadLevel(cmd=>Query(h,cmd));if(level.HasValue){reading.Percent=level;reading.Time=DateTime.Now;reading.State="Подключено";}}return reading;}
}

public sealed class WlMouseBatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider {
 public string Id{get{return "wlmouse-beast";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public bool Matches(HidCollection c){return c.Vid==0x36A7 && (c.Pid==0xA887 || c.Pid==0xA885 || c.Pid==0xA868 || c.Pid==0xA880) && c.Page==0xFFFF && c.Feature==65;}
 public Reading Describe(HidCollection c){string name=c.Pid==0xA887?"WLMOUSE Beast X":c.Pid==0xA885?"WLMOUSE Beast X Mini":c.Pid==0xA868?"WLMOUSE Beast X Mini Pro":"WLMOUSE Beast X Max";return new Reading(name){Kind=DeviceKind.Mouse,Experimental=true};}
 public static byte[] Request(){var r=new byte[65];Array.Copy(new byte[]{0,0,0,2,2,0,0x83},r,7);return r;}
 public static Reading Parse(byte[] r){if(r==null || r.Length!=65)return null;for(int at=0;at<=1;at++){if(at==1 && r[0]!=0)break;if((r[at]==0xA1 || r[at]==0xA2) && r[at+3]==2 && r[at+4]==0 && r[at+5]==0x83 && r[at+7]<=100)return new Reading("WLMOUSE mouse"){Kind=DeviceKind.Mouse,Experimental=true,Percent=r[at+7],Charging=r[at+6]!=0,Time=DateTime.Now,State="Подключено"};}return null;}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);using(var h=Hid.CreateFile(c.Path,0,3,IntPtr.Zero,3,0,IntPtr.Zero)){if(h.IsInvalid)return original;var request=Request();if(!Hid.HidD_SetFeature(h,request,request.Length))return original;for(int i=0;i<16;i++){Thread.Sleep(50);var r=new byte[65];if(!Hid.HidD_GetFeature(h,r,r.Length))continue;var reading=Parse(r);if(reading!=null){reading.Name=original.Name;return reading;}}}return original;}
}

public sealed class NzxtLiftBatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider {
 public string Id{get{return "nzxt-lift-elite";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public bool Matches(HidCollection c){return c.Vid==0x1E71 && (c.Pid==0x2101 || c.Pid==0x2129) && c.Page==0xFFCA && c.Usage==1 && (c.Input==64 || c.Input==65) && (c.Output==64 || c.Output==65);}
 public Reading Describe(HidCollection c){return new Reading("NZXT Lift Elite"){Kind=DeviceKind.Mouse,Experimental=true,Transport=c.Pid==0x2129?"USB":"USB-приёмник · 2,4 ГГц"};}
 public static byte[] Request(int length){if(length!=64 && length!=65)return null;var r=new byte[length];Array.Copy(new byte[]{0x4E,2,0x81,0,0xB0},0,r,length==65?1:0,5);return r;}
 public static Reading Parse(byte[] r){if(r==null || r.Length!=64 && r.Length!=65)return null;int at=r.Length==65 && r[0]==0?1:0;if(r[at]!=0x4E || r[at+1]!=2 || r[at+2]!=0x97)return null;int level=r[at+17]|r[at+18]<<8;if(level>100)return null;return new Reading("NZXT Lift Elite"){Kind=DeviceKind.Mouse,Experimental=true,Percent=level,Charging=(r[at+22]&1)!=0,Time=DateTime.Now,State="Подключено"};}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return original;var reading=BatteryQuery.Ask(h,c,Request(c.Output),Parse,1500);if(reading!=null){reading.Transport=original.Transport;return reading;}}return original;}
}

public sealed class EndgameGearBatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider {
 public string Id{get{return "endgame-4k";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public bool Matches(HidCollection c){return c.Vid==0x3367 && c.Pid==0x1970 && c.Page==0xFF01 && c.Usage==2 && c.Feature==64;}
 public Reading Describe(HidCollection c){return new Reading("Endgame Gear 4k wireless mouse"){Kind=DeviceKind.Mouse,Experimental=true};}
 public static byte[] Request(byte command,byte target){var r=new byte[64];r[0]=0xA1;r[1]=command;r[2]=target;return r;}
 public static int? Level(byte[] r){return r!=null && r.Length==64 && r[0]==0xA1 && r[1]==1 && r[16]<=100?(int?)r[16]:null;}
 public static bool Asleep(byte[] r){return r!=null && r.Length==64 && r[0]==0xA1 && (r[1]==8 || r[1]==0x11);}
 public static string Model(byte[] r){if(r==null || r.Length!=64 || r[0]!=0xA1 || r[1]!=1 || r[16]!=0x67 || r[17]!=0x33)return null;switch(r[18]|r[19]<<8){case 0x1972:return "Endgame Gear OP1w 4k";case 0x1984:return "Endgame Gear OP1w 4k v2";case 0x1968:return "Endgame Gear XM2w 4k";case 0x1982:return "Endgame Gear XM2w 4k v2";default:return null;}}
 static byte[] Query(SafeFileHandle h,byte command,byte target){var request=Request(command,target);if(!Hid.HidD_SetFeature(h,request,request.Length))return null;for(int i=0;i<3;i++){Thread.Sleep(150+i*100);var r=new byte[64];r[0]=0xA1;if(!Hid.HidD_GetFeature(h,r,r.Length))return null;if(r[0]!=0xA1)return null;if(r[1]!=3)return r;}return null;}
 public static int? ReadLevel(Func<byte,byte,byte[]> query){var reply=query(0xB4,0);var level=Level(reply);if(level.HasValue || Asleep(reply))return level;query(0x0F,1);query(0x0F,0x0F);return Level(query(0xB4,0));}
 public Reading Read(HidCollection c,Action<string> log){var reading=Describe(c);using(var h=Hid.CreateFile(c.Path,0,3,IntPtr.Zero,3,0,IntPtr.Zero)){if(h.IsInvalid)return reading;var level=ReadLevel((cmd,target)=>Query(h,cmd,target));if(level.HasValue){reading.Percent=level;reading.Time=DateTime.Now;reading.State="Подключено";string name=Model(Query(h,0x0E,0));if(name!=null)reading.Name=name;}}return reading;}
}
