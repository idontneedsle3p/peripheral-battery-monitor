using System;
using System.Threading;

public sealed class KeychronMouseBatteryDriver:IBatteryDriver,IRequireBatteryReading {
 public string Id{get{return "keychron-link-mouse";}}
 public bool Matches(HidCollection c){return c.Vid==0x3434 && (c.Pid==0xD030 || c.Pid==0xD031) && c.Page==0x008C && c.Feature>=13 && c.Feature<=4096;}
 public Reading Describe(HidCollection c){return new Reading("Keychron mouse"){Kind=DeviceKind.Mouse,Experimental=true};}
 public static string Model(ushort pid){switch(pid){case 0xD032:case 0xD033:return "Keychron M3";case 0xD035:return "Keychron M1";case 0xD036:case 0xD03E:return "Keychron M3 mini";case 0xD03B:return "Keychron M2";case 0xD03D:return "Keychron M2 mini";case 0xD03F:return "Keychron M6";case 0xD044:return "Keychron M7";default:return "Keychron mouse";}}
 public static Reading Parse(byte[] r){if(r==null || r.Length<13 || r[0]!=0x51 || r[1]!=6 || r[2]!=1 || r[3]!=0 || r[4]!=0x34 || r[5]!=0x34 || r[7]!=0xD0 || r[11]>100 || r[12]>1)return null;ushort pid=(ushort)(r[6]|r[7]<<8);return new Reading(Model(pid)){Kind=DeviceKind.Mouse,Experimental=true,Percent=r[11],Charging=r[12]==1,State="Подключено",Time=DateTime.Now};}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);using(var h=Hid.CreateFile(c.Path,0,3,IntPtr.Zero,3,0,IntPtr.Zero)){if(h.IsInvalid)return original;var request=new byte[c.Feature];request[0]=0x51;request[1]=6;if(!Hid.HidD_SetFeature(h,request,request.Length))return original;for(int attempt=0;attempt<5;attempt++){Thread.Sleep(40);var r=new byte[c.Feature];r[0]=0x51;if(!Hid.HidD_GetFeature(h,r,r.Length))continue;var reading=Parse(r);if(reading!=null)return reading;}}return original;}
}
