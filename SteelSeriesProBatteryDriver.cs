using System;
using System.Diagnostics;
using System.Linq;

public sealed class SteelSeriesProBatteryDriver:IBatteryDriver,IRequireBatteryReading {
 static readonly ushort[] Pro={0x12E0,0x12E5,0x225D},Omni={0x2290,0x2244,0x2270};
 public string Id{get{return "steelseries-pro";}}
 public bool Matches(HidCollection c){return c.Vid==0x1038 && c.Input>=16 && c.Output>=2 && (Pro.Contains(c.Pid)?c.Page>=0xFF00 && (c.Path??"").IndexOf("&mi_04",StringComparison.OrdinalIgnoreCase)>=0:Omni.Contains(c.Pid) && c.Page==0xFFC0 && c.Usage==1);}
 public Reading Describe(HidCollection c){return new Reading(!string.IsNullOrWhiteSpace(c.Product)?c.Product:Pro.Contains(c.Pid)?"SteelSeries Arctis Nova Pro Wireless":c.Pid==0x2290?"SteelSeries Arctis Nova Pro Omni":"SteelSeries Arctis Nova Elite"){Kind=DeviceKind.Headset,Experimental=true};}
 public static Reading Parse(ushort pid,byte[] r){
  bool pro=Pro.Contains(pid);if(!pro && !Omni.Contains(pid) || r==null || r.Length<(pro?16:17) || r[0]!=(pro?6:1) || r[1]!=0xB0 || r.Skip(2).All(b=>b==0))return null;
  if(r[15]==1 || r[15]>3)return null;var reading=new Reading("SteelSeries headset"){Kind=DeviceKind.Headset,Experimental=true,Time=DateTime.Now,State="Подключено",Charging=r[15]==2};
  if(pro){if(r[6]>8)return null;reading.Steps=r[6];reading.MaxSteps=8;}
  else{if(r[14]!=8 || r[6]>100)return null;reading.Percent=r[6];}return reading;
 }
 public Reading Read(HidCollection c,Action<string> log){var result=Describe(c);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){
  if(h.IsInvalid)return result;var request=new byte[c.Output];request[0]=(byte)(Pro.Contains(c.Pid)?6:1);request[1]=0xB0;Hid.HidD_FlushQueue(h);if(Hid.Transfer(h,request,true,250)==null)return result;
  var clock=Stopwatch.StartNew();while(clock.ElapsedMilliseconds<750){var r=Hid.Transfer(h,new byte[c.Input],false,Math.Max(1,750-(int)clock.ElapsedMilliseconds));if(r==null)break;var parsed=Parse(c.Pid,r);if(parsed==null)continue;parsed.Name=result.Name;return parsed;}
 }return result;}
}
