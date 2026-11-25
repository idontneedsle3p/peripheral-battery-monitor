using System;
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;

public sealed class Ajazz139BatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider {
 public string Id{get{return "ajazz-139";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public bool Matches(HidCollection c){bool id=(c.Vid==0xA8A4 || c.Vid==0xA8A5) && c.Pid==0x2255 || c.Vid==0x391D && c.Pid==0x1A03;return id && c.Page==0xFF01 && c.Usage==0x10 && (c.Input==65 && c.Output==65 || c.Vid==0xA8A5 && c.Input==64 && c.Output==64);}
 public Reading Describe(HidCollection c){return new Reading(c.Vid==0x391D?"VGN Dragonfly 3 Pro":"AJAZZ wireless mouse"){Kind=DeviceKind.Mouse,Experimental=true,Transport=c.Vid==0xA8A4?"USB":"USB-приёмник · 2,4 ГГц"};}
 public static byte[] Request(bool f0){var r=new byte[f0?64:65];if(f0)Array.Copy(new byte[]{0xF0,0x30,0xA5,0x0B,0x2E,1,1,1},r,8);else Array.Copy(new byte[]{0,0x55,0x30,0xA5,0x2E,0x2E},r,6);return r;}
 public static Reading Parse(byte[] r,bool f0,bool cable){if(r==null)return null;int at=r.Length==65 && r[0]==0?1:0;if(r.Length-at!=64)return null;if(f0){var header=new byte[]{0xF0,0x30,0xA5,0x95,0x2E,1,1,1};for(int i=0;i<header.Length;i++)if(r[at+i]!=header[i])return null;if(r[at+9]>1)return null;}else if(r[at]!=0xAA || r[at+1]!=0x30 || r[at+2]!=0xA5)return null;int level=r[at+8];if(level>100 || !cable && level==0)return null;return new Reading("AJAZZ mouse"){Kind=DeviceKind.Mouse,Experimental=true,Percent=level,Charging=r[at+9]!=0,Time=DateTime.Now,State="Подключено"};}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return original;bool f0=c.Output==64;var reading=BatteryQuery.Ask(h,c,Request(f0),r=>Parse(r,f0,c.Vid==0xA8A4),700);if(reading!=null){reading.Name=original.Name;reading.Transport=original.Transport;return reading;}}return original;}
}

public sealed class Ajazz179BatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider {
 public string Id{get{return "ajazz-179";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public bool Matches(HidCollection c){return c.Vid==0x249A && c.Pid==0x5C2F && c.Page==1 && c.Usage==0 && c.Input==33 && c.Output==33 && (c.Path??"").IndexOf("&mi_02",StringComparison.OrdinalIgnoreCase)>=0;}
 public Reading Describe(HidCollection c){return new Reading("AJAZZ wireless mouse"){Kind=DeviceKind.Mouse,Experimental=true};}
 public static byte[] Request(){var r=new byte[33];r[1]=0x10;return r;}
 public static Reading Parse(byte[] r){if(r==null || r.Length!=33 || r[0]!=0 || r[1]!=0x10 || r[2]!=0 || r[4]==0 || r[15]==0 || r[14]>100 || r[13]>2)return null;int sum=0;for(int i=5;i<32;i++)sum+=r[i];if((byte)sum!=r[32])return null;return new Reading("AJAZZ wireless mouse"){Kind=DeviceKind.Mouse,Experimental=true,Percent=r[14],Charging=r[13]==1,Time=DateTime.Now,State="Подключено"};}
 public Reading Read(HidCollection c,Action<string> log){using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return Describe(c);for(int i=0;i<3;i++){var reading=BatteryQuery.Ask(h,c,Request(),Parse,500);if(reading!=null)return reading;}}return Describe(c);}
}

public sealed class DarmosharkBatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider {
 public string Id{get{return "darmoshark-m3";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public bool Matches(HidCollection c){return c.Vid==0x248A && (c.Pid==0xFF30 || c.Pid==0xFF31) && c.Page==0x8C && c.Usage==1 && c.Feature>=20 && c.Feature<=4096 && c.Input>=4 && c.Input<=4096 && (c.Path??"").IndexOf("&mi_01",StringComparison.OrdinalIgnoreCase)>=0;}
 public Reading Describe(HidCollection c){return new Reading(c.Pid==0xFF31?"Darmoshark M3S":"Darmoshark M3"){Kind=DeviceKind.Mouse,Experimental=true};}
 public static bool Linked(byte[] r,ushort mouse){return r!=null && r.Length>=12 && r[0]==0x51 && r[1]==3 && r[3]==0x8A && r[4]==0x24 && (r[5]|r[6]<<8)==mouse && r[7]!=0;}
 public static int? Level(byte[] r,ushort mouse){return r!=null && r.Length>=12 && r[0]==0x51 && r[1]==6 && r[2]==1 && r[4]==0x8A && r[5]==0x24 && (r[6]|r[7]<<8)==mouse && r[11]<=100?(int?)r[11]:null;}
 public static bool NoLink(byte[] r){return r!=null && r.Length>=3 && r[0]==0x54 && r[1]==0xE4 && r[2]==2;}
 static byte[] Query(SafeFileHandle h,HidCollection c,byte op,Stopwatch total,ref bool away){var request=new byte[c.Feature];request[0]=0x51;request[1]=op;Hid.HidD_FlushQueue(h);if(total.ElapsedMilliseconds>=1000 || !Hid.HidD_SetFeature(h,request,request.Length))return null;long start=total.ElapsedMilliseconds;while(total.ElapsedMilliseconds<1000 && total.ElapsedMilliseconds-start<500){var input=Hid.Transfer(h,new byte[c.Input],false,20);if(NoLink(input)){away=true;return null;}var r=new byte[c.Feature];r[0]=0x51;if(Hid.HidD_GetFeature(h,r,r.Length) && r[1]==op)return r;}return null;}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);ushort mouse=(ushort)(c.Pid==0xFF31?0xFF18:0xFF12);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return original;var watch=Stopwatch.StartNew();bool away=false;for(int attempt=0;attempt<2 && !away && watch.ElapsedMilliseconds<1000;attempt++){if(!Linked(Query(h,c,3,watch,ref away),mouse))continue;var level=Level(Query(h,c,6,watch,ref away),mouse);if(level.HasValue){original.Percent=level;original.Time=DateTime.Now;original.State="Подключено";return original;}}}return original;}
}

public sealed class AttackSharkBatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider {
 public string Id{get{return "attackshark-passive";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public bool Matches(HidCollection c){return c.Vid==0x1D57 && (c.Pid==0xFA60 || c.Pid==0xFA65 || c.Pid==0x2015) && c.Page==0x000A && c.Input==5 && (c.Path??"").IndexOf("&mi_02",StringComparison.OrdinalIgnoreCase)>=0;}
 public Reading Describe(HidCollection c){return new Reading("Wireless mouse"){Kind=DeviceKind.Mouse,Experimental=true,Transport=c.Pid==0x2015?"USB":"USB-приёмник · 2,4 ГГц"};}
 public static string Model(ushort pid,byte model){if(pid==0xFA65){if(model==0x30 || model==0x3D || model==0x50)return "Delux M900 Pro";if(model==0x73)return "Inphic IN10R MAX";if(model==0x74)return "Inphic IN10 MAX";return "Wireless mouse";}switch(model){case 0x55:return "Attack Shark X11";case 0x4E:return "Attack Shark X3 Max";case 0xE5:return "Attack Shark V3 Pro";case 0x24:return "Attack Shark V6";case 0x10:return "Attack Shark X6 / R1";case 0x47:return "IO Nova SE";default:return "Attack Shark mouse";}}
 public static Reading Parse(byte[] r,ushort pid){if(r==null || r.Length!=5 || r[0]!=3 || r[2]!=0x40 && r[2]!=0x41 || r[3]<1 || r[3]>3 || r[4]>100)return null;var reading=new Reading(Model(pid,r[1])){Kind=DeviceKind.Mouse,Experimental=true,Charging=r[3]==3,Percent=r[4],Time=DateTime.Now,State="Подключено",Transport=pid==0x2015?"USB":"USB-приёмник · 2,4 ГГц"};if(pid==0xFA60 && r[1]==0x10){if(r[4]>10)return null;reading.Percent=null;reading.Steps=r[4];reading.MaxSteps=10;}if(pid==0xFA65 && r[3]==2){reading.Percent=100;reading.Charging=false;}return reading;}
 public static bool Same(byte[] a,byte[] b){if(a==null || b==null || a.Length!=5 || b.Length!=5)return false;for(int i=0;i<5;i++)if(a[i]!=b[i])return false;return true;}
 public static bool NeedsConfirmation(byte[] r){return r!=null && r.Length==5 && r[3]==1 && r[4]==100;}
 public Reading Read(HidCollection c,Action<string> log){using(var h=Hid.CreateFile(c.Path,0x80000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return Describe(c);Hid.HidD_FlushQueue(h);var watch=Stopwatch.StartNew();byte[] previous=null;Reading last=null;while(watch.ElapsedMilliseconds<5500){var r=Hid.Transfer(h,new byte[c.Input],false,Math.Max(1,5500-(int)watch.ElapsedMilliseconds));if(r==null)break;var reading=Parse(r,c.Pid);if(reading==null)continue;if(Same(previous,r))return reading;previous=r;last=reading;}return last!=null && !NeedsConfirmation(previous)?last:Describe(c);}}
}
