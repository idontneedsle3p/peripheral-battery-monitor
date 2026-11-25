using System;

// Read-only fixed status query for PRO X 2 firmware with the legacy Centurion response.
public sealed class LogitechProX2BatteryDriver:IBatteryDriver,IRequireBatteryReading {
 public string Id{get{return "logitech-prox2";}}
 public bool Matches(HidCollection c){return c.Vid==0x046D && c.Pid==0x0AF7 && c.Page==0xFFA0 && c.Usage==1 && c.Input==64 && c.Output==64;}
 public Reading Describe(HidCollection c){return new Reading("Logitech G PRO X 2 LIGHTSPEED"){Kind=DeviceKind.Headset,Experimental=true};}
 public static Reading Parse(byte[] r){if(r==null || r.Length<13 || r[0]!=0x51 || r[1]!=0x0B || r[2]!=0 || r[8]!=4 || r[10]>100 || r[12]>3)return null;return BatteryQuery.Headset("Logitech G PRO X 2 LIGHTSPEED",r[10],r[12]==2);}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return original;return BatteryQuery.Ask(h,c,new byte[]{0x51,8,0,3,0x1A,0,3,0,4,0x0A},Parse,1200)??original;}}
}
