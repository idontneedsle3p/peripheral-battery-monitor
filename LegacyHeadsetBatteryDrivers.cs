using System;
using System.Linq;

public sealed class SteelSeriesClassicBatteryDriver:IBatteryDriver,IRequireBatteryReading {
 static readonly ushort[] One={0x12B3,0x12B6,0x12D7},Seven={0x1260,0x12AD,0x1252};
 public string Id{get{return "steelseries-classic";}}
 public bool Matches(HidCollection c){return c.Vid==0x1038 && c.Input>=5 && c.Output>=3 && (One.Contains(c.Pid) || Seven.Contains(c.Pid)?c.Page==0xFF43 && c.Usage==0x202:(c.Pid==0x12C2 || c.Pid==0x1290) && c.Page>=0xFF00 && (c.Path??"").IndexOf("&mi_00",StringComparison.OrdinalIgnoreCase)>=0);}
 public Reading Describe(HidCollection c){return new Reading(!string.IsNullOrWhiteSpace(c.Product)?c.Product:One.Contains(c.Pid)?"SteelSeries Arctis 1 Wireless":Seven.Contains(c.Pid)?"SteelSeries Arctis 7 / Pro":c.Pid==0x12C2?"SteelSeries Arctis 9":"SteelSeries Arctis Pro Wireless"){Kind=DeviceKind.Headset,Experimental=true};}
 public static Reading Parse(ushort pid,byte[] raw){if(raw==null)return null;int offset=raw.Length>0 && raw[0]==0?1:0;var r=raw.Skip(offset).ToArray();
  if(One.Contains(pid)){if(r.Length<4 || r[0]!=6 || r[1]!=0x12 || r[2]==1 || r[3]>100)return null;return BatteryQuery.Headset("SteelSeries headset",r[3]);}
  if(Seven.Contains(pid)){if(r.Length<3 || r[0]!=6 || r[1]!=0x18 || r[2]==0 || r[2]>100)return null;return BatteryQuery.Headset("SteelSeries headset",r[2]);}
  if(pid==0x12C2){if(r.Length<5 || r[3]<100 || r[3]>154 || r[4]>1)return null;return new Reading("SteelSeries Arctis 9"){Kind=DeviceKind.Headset,Experimental=true,Steps=r[3]-100,MaxSteps=54,Charging=r[4]==1,State="Подключено",Time=DateTime.Now};}return null;
 }
 public static Reading ParseProLevel(byte[] raw){if(raw==null)return null;int offset=raw.Length>1 && raw[0]==0?1:0;if(raw.Length<=offset || raw[offset]>4)return null;return new Reading("SteelSeries Arctis Pro Wireless"){Kind=DeviceKind.Headset,Experimental=true,Steps=raw[offset],MaxSteps=4,Time=DateTime.Now,State="Подключено"};}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return original;
  Reading r;if(c.Pid==0x1290){bool connected=false;var status=BatteryQuery.Ask(h,c,new byte[]{0,0x41,0xAA},raw=>{int offset=raw.Length>1 && raw[0]==0?1:0;if(raw.Length<=offset || raw[offset]>2)return null;connected=raw[offset]!=2;return new Reading("status");});if(status==null || !connected)return original;r=BatteryQuery.Ask(h,c,new byte[]{0,0x40,0xAA},ParseProLevel);}
  else r=BatteryQuery.Ask(h,c,One.Contains(c.Pid)?new byte[]{6,0x12}:Seven.Contains(c.Pid)?new byte[]{6,0x18}:new byte[]{0,0x20},raw=>Parse(c.Pid,raw));
  if(r!=null){r.Name=original.Name;return r;}
 }return original;}
}
public sealed class HyperXCloudTwoBatteryDriver:IBatteryDriver,IRequireBatteryReading {
 public string Id{get{return "hyperx-cloud2-kingston";}}
 public bool Matches(HidCollection c){return c.Vid==0x0951 && c.Pid==0x1718 && c.Page>=0xFF00 && c.Output==62 && c.Input>=8;}
 public Reading Describe(HidCollection c){return new Reading("HyperX Cloud II Wireless (Kingston)"){Kind=DeviceKind.Headset,Experimental=true};}
 public static byte[] Request(byte command){var r=new byte[62];byte[] header={6,0,2,0,0x9A,0,0,0x68,0x4A,0x8E,0x0A,0,0,0,0xBB,command};Array.Copy(header,r,header.Length);return r;}
 public static Reading Parse(byte[] r,byte command){if(r==null || r.Length<8 || r[0]!=0x0B || r[2]!=0xBB || r[3]!=command || command==2 && r[7]>100 || command!=2 && command!=3 || command==3 && r[4]>2)return null;return BatteryQuery.Headset("HyperX Cloud II Wireless (Kingston)",command==2?r[7]:0,command==3 && r[4]==1);}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return original;
  var prime=new byte[c.Input];prime[0]=6;Hid.HidD_GetInputReport(h,prime,prime.Length);var r=BatteryQuery.Ask(h,c,Request(2),raw=>Parse(raw,2));if(r==null)return original;
  Hid.HidD_GetInputReport(h,prime,prime.Length);var charge=BatteryQuery.Ask(h,c,Request(3),raw=>Parse(raw,3));r.Charging=charge!=null && charge.Charging;return r;
 } }
}
