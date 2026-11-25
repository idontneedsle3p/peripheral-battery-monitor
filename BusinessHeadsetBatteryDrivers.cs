using System;
using System.Linq;
using System.Threading;

public sealed class JabraLinkBatteryDriver:IBatteryDriver,IRequireBatteryReading {
 static int sequence=0x20;static readonly ushort[] Pids={0x2E50,0x2E51,0x2E56,0x2E57};
 public string Id{get{return "jabra-link390";}}
 public bool Matches(HidCollection c){return c.Vid==0x0B0E && Pids.Contains(c.Pid) && c.Page==0xFF00 && c.Usage==5 && c.Input==64 && c.Output==64;}
 public Reading Describe(HidCollection c){return new Reading("Jabra Link 390 headset"){Kind=DeviceKind.Headset,Experimental=true,Transport="Jabra Link 390"};}
 public static byte[] Request(byte seq){var r=new byte[64];r[0]=5;r[1]=4;r[3]=seq;r[4]=0x46;r[5]=0x12;r[6]=2;return r;}
 public static Reading Parse(byte[] r,byte seq){if(r==null || r.Length<9 || r[0]!=5 || r[1]!=0 || r[2]!=4 || r[3]!=seq || r[4]>>6!=3 || (r[4]&63)<8 || (r[4]&63)+1>r.Length || r[5]!=0x12 || r[6]!=2 || r[8]>100)return null;return BatteryQuery.Headset("Jabra headset",r[8],(r[7]&1)!=0);}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);byte seq=(byte)Interlocked.Increment(ref sequence);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return original;var r=BatteryQuery.Ask(h,c,Request(seq),raw=>Parse(raw,seq));if(r!=null){r.Name=original.Name;r.Transport=original.Transport;return r;}}return original;}
}
public sealed class HyperXAlphaBatteryDriver:IBatteryDriver,IRequireBatteryReading {
 public string Id{get{return "hyperx-alpha";}}
 public bool Matches(HidCollection c){return c.Vid==0x03F0 && c.Pid==0x098D && c.Page==0xFF43 && c.Usage==0x202 && c.Input>=4 && c.Output>=4;}
 public Reading Describe(HidCollection c){return new Reading("HyperX Cloud Alpha Wireless"){Kind=DeviceKind.Headset,Experimental=true};}
 public static Reading Parse(byte[] r,byte cmd){if(cmd!=3 && cmd!=0x0B && cmd!=0x0C || r==null || r.Length<4 || r[0]!=0x21 || r[2]!=cmd || cmd==0x0B && r[3]>100 || (cmd==3 || cmd==0x0C) && r[3]>1)return null;return BatteryQuery.Headset("HyperX Cloud Alpha Wireless",cmd==0x0B?r[3]:0,cmd==0x0C && r[3]==1);}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return original;
  // Link response value 1 means the paired headset is absent; do not treat it as 1%.
  bool offline=false;var r=BatteryQuery.Ask(h,c,new byte[]{0x21,0xBB,3},raw=>{var parsed=Parse(raw,3);if(parsed!=null)offline=raw[3]==1;return parsed;});if(r==null || offline)return original;
  var battery=BatteryQuery.Ask(h,c,new byte[]{0x21,0xBB,0x0B},raw=>Parse(raw,0x0B));if(battery==null)return original;var charge=BatteryQuery.Ask(h,c,new byte[]{0x21,0xBB,0x0C},raw=>Parse(raw,0x0C));battery.Charging=charge!=null && charge.Charging;return battery;
 } }
}
