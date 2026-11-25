using System;
using System.Diagnostics;
using System.Linq;

// Battery buckets from native input reports, not inferred percentages.
public sealed class PlayStationBatteryDriver:IBatteryDriver,IRequireBatteryReading {
 public string Id{get{return "sony-playstation";}}
 public bool Matches(HidCollection c){return c.Vid==0x054C && new ushort[]{0x05C4,0x09CC,0x0CE6,0x0DF2}.Contains(c.Pid) && c.Page==1 && c.Usage==5 && (c.Input==64 || c.Input==78);}
 public Reading Describe(HidCollection c){return new Reading(c.Pid==0x0DF2?"Sony DualSense Edge":c.Pid==0x0CE6?"Sony DualSense":"Sony DualShock 4"){Kind=DeviceKind.Controller,Experimental=true,Transport=c.Input==78?"Bluetooth":"USB-кабель"};}
 public static uint BluetoothCrc(byte[] r){uint crc=0xFFFFFFFF;crc=Update(crc,0xA1);for(int i=0;i<74;i++)crc=Update(crc,r[i]);return ~crc;}
 static uint Update(uint crc,byte value){crc^=value;for(int i=0;i<8;i++)crc=(crc>>1)^((crc&1)!=0?0xEDB88320u:0);return crc;}
 public static Reading Parse(ushort pid,bool bluetooth,byte[] r){
  bool ds4=pid==0x05C4 || pid==0x09CC;if(!ds4 && pid!=0x0CE6 && pid!=0x0DF2 || r==null || r.Length!=(bluetooth?78:64) || r[0]!=(bluetooth?(ds4?0x11:0x31):1))return null;
  if(bluetooth){uint sent=(uint)r[74]|(uint)r[75]<<8|(uint)r[76]<<16|(uint)r[77]<<24;if(sent!=BluetoothCrc(r))return null;}
  byte status=r[ds4?(bluetooth?32:30):(bluetooth?54:53)];int steps=status&15;bool charging;
  if(ds4){bool cable=(status&16)!=0;if(steps>11 || !cable && steps>10)return null;charging=cable && steps<11;steps=Math.Min(steps,10);}
  else{int state=status>>4;if(steps>10 || state>2)return null;charging=state==1;if(state==2)steps=10;}
  return new Reading("Sony controller"){Kind=DeviceKind.Controller,Experimental=true,Steps=steps,MaxSteps=10,Charging=charging,Time=DateTime.Now,State="Подключено"};
 }
 public Reading Read(HidCollection c,Action<string> log){var reading=Describe(c);bool bt=c.Input==78;
  using(var h=Hid.CreateFile(c.Path,0x80000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){
   if(h.IsInvalid)return reading;
   if(bt && c.Feature>=2 && c.Feature<=512){var calibration=new byte[c.Feature];calibration[0]=5;Hid.HidD_GetFeature(h,calibration,calibration.Length);}
   Hid.HidD_FlushQueue(h);var watch=Stopwatch.StartNew();while(watch.ElapsedMilliseconds<500){var r=Hid.Transfer(h,new byte[c.Input],false,Math.Max(1,500-(int)watch.ElapsedMilliseconds));if(r==null)break;var parsed=Parse(c.Pid,bt,r);if(parsed==null)continue;parsed.Name=reading.Name;parsed.Transport=reading.Transport;log(reading.Name+": "+parsed.Display);return parsed;}
  }return reading;
 }
}
