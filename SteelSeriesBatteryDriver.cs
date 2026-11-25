using System;
using System.Diagnostics;
using System.Linq;

// Nova-family B0 status query; only whitelisted PIDs on FFC0:0001.
public sealed class SteelSeriesBatteryDriver:IBatteryDriver,IRequireBatteryReading {
 static readonly ushort[] SevenPlus={0x220E,0x2212,0x2216,0x2236},NovaFive={0x2232,0x2253},NovaThree={0x2269,0x226D},Buds={0x230A,0x2317};
 static readonly ushort[] DiscreteSeven={0x2202,0x2206,0x220A,0x223A,0x227A,0x22A4};
 static readonly ushort[] PercentSeven={0x22A1,0x227E,0x2258,0x229E,0x22AD,0x22A9,0x22A5,0x22A7};
 public string Id{get{return "steelseries-nova";}}
 public bool Matches(HidCollection c){return c.Vid==0x1038 && SevenPlus.Concat(NovaFive).Concat(NovaThree).Concat(Buds).Concat(DiscreteSeven).Concat(PercentSeven).Contains(c.Pid) && c.Page==0xFFC0 && c.Usage==1 && c.Input>=7 && c.Output>=2;}
 public Reading Describe(HidCollection c){string family=SevenPlus.Contains(c.Pid)?"Arctis 7+":NovaFive.Contains(c.Pid)?"Arctis Nova 5":NovaThree.Contains(c.Pid)?"Arctis Nova 3 Wireless":Buds.Contains(c.Pid)?"Arctis GameBuds":"Arctis Nova 7";return new Reading(string.IsNullOrWhiteSpace(c.Product)?"SteelSeries "+family:c.Product.Trim()){Experimental=true,Kind=DeviceKind.Headset};}
 public static Reading Parse(ushort pid,byte[] raw){
  if(raw==null)return null;int off=raw.Length>0 && raw[0]==0?1:0;var r=raw.Skip(off).ToArray();if(r.Length<7 || r[0]!=0xB0 || r.Skip(1).All(b=>b==0))return null;
  var result=new Reading("SteelSeries headset"){Kind=DeviceKind.Headset,Experimental=true,State="Подключено",Time=DateTime.Now};
  if(Buds.Contains(pid)){int left=r[3]==3?r[5]:-1,right=r[4]==3?r[6]:-1;if(left>100 || right>100 || left<0 && right<0)return null;result.Percent=left<0?right:right<0?left:Math.Min(left,right);return result;}
  if(NovaFive.Contains(pid) || NovaThree.Contains(pid)){if(r[1]==2 || r[1]>3 || r[3]>100 || r[4]>1)return null;result.Percent=r[3];result.Charging=r[4]==1;return result;}
  bool plus=SevenPlus.Contains(pid),discrete=plus || DiscreteSeven.Contains(pid);if(!plus && !discrete && !PercentSeven.Contains(pid))return null;
  if(plus?r[1]==1 || r[3]>1:r[3]==0 || r[3]>3)return null;
  if(discrete){if(r[2]>4)return null;result.Level=r[2]==0?BatteryLevel.Empty:r[2]==1?BatteryLevel.Low:r[2]==4?BatteryLevel.Full:BatteryLevel.Medium;}
  else{if(r[2]>100)return null;result.Percent=r[2];}result.Charging=plus?r[3]==1:r[3]==1 || r[3]==2;return result;
 }
 public Reading Read(HidCollection c,Action<string> log){var reading=Describe(c);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){
  if(h.IsInvalid)return reading;Hid.HidD_FlushQueue(h);var request=new byte[c.Output];request[1]=0xB0;if(Hid.Transfer(h,request,true,250)==null)return reading;
  var clock=Stopwatch.StartNew();while(clock.ElapsedMilliseconds<750){var data=Hid.Transfer(h,new byte[c.Input],false,Math.Max(1,750-(int)clock.ElapsedMilliseconds));if(data==null)break;var parsed=Parse(c.Pid,data);if(parsed==null)continue;parsed.Name=reading.Name;return parsed;}
 }return reading;}
}
