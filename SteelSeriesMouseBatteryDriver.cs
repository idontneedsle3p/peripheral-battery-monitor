using System;
using System.Linq;

public sealed class SteelSeriesMouseBatteryDriver:IBatteryDriver,IRequireBatteryReading {
 static readonly ushort[] Pids={0x1838,0x183A,0x1878,0x187A,0x1890,0x1892,0x1852,0x1854,0x185C,0x185E,0x1858,0x185A,0x1840,0x1842,0x1860};
 static readonly ushort[] Wired={0x183A,0x187A,0x1892,0x1854,0x185E,0x185A,0x1842};
 public string Id{get{return "steelseries-aerox";}}
 public bool Matches(HidCollection c){return c.Vid==0x1038 && Pids.Contains(c.Pid) && c.Input>=2 && c.Output>=2 && (c.Path??"").IndexOf("&mi_03",StringComparison.OrdinalIgnoreCase)>=0;}
 public Reading Describe(HidCollection c){return new Reading(string.IsNullOrWhiteSpace(c.Product)?"SteelSeries Aerox / Prime Wireless":c.Product.Trim()){Kind=DeviceKind.Mouse,Experimental=true,Transport=Wired.Contains(c.Pid)?"USB-кабель":"USB-приёмник · 2,4 ГГц"};}
 public static Reading Parse(ushort pid,byte[] raw){
  if(!Pids.Contains(pid) || raw==null)return null;int offset=raw.Length>0 && raw[0]==0?1:0;var r=raw.Skip(offset).ToArray();byte command=(byte)(Wired.Contains(pid)?0x92:0xD2);if(r.Length<2 || r[0]!=command)return null;
  var result=new Reading("SteelSeries mouse"){Kind=DeviceKind.Mouse,Experimental=true,State="Подключено",Time=DateTime.Now};
  if(pid==0x1890 || pid==0x1892){if(r.Length<3 || r[1]>2 || r[2]>100 || pid==0x1890 && r[1]==0 && r[2]==0)return null;result.Percent=r[2];result.Charging=r[1]==1;}
  else{int level=r[1]&127;if(level<1 || level>21)return null;result.Steps=level-1;result.MaxSteps=20;result.Charging=(r[1]&128)!=0;}return result;
 }
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return original;var r=BatteryQuery.Ask(h,c,new byte[]{0,(byte)(Wired.Contains(c.Pid)?0x92:0xD2)},raw=>Parse(c.Pid,raw));if(r!=null){r.Name=original.Name;r.Transport=original.Transport;return r;}}return original;}
}
