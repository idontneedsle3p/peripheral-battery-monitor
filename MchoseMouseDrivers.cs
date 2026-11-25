using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

// Status-only implementations based on the protocol references in SUPPORTED_DEVICES.md.
public sealed class MchoseClassicDriver:IBatteryDriver {
 static readonly ushort[] RealtekPids={0x1001,0x100A,0x100B,0x100C,0x1012};
 public string Id{get{return "mchose-classic";}}
 public bool Matches(HidCollection c){return (c.Vid==0x5253 && c.Pid==0x1020 || c.Vid==0x3837 && RealtekPids.Contains(c.Pid)) && c.Page==0xFF01 && c.Usage==1 && c.Feature>=65;}
 public Reading Describe(HidCollection c){return new Reading(string.IsNullOrWhiteSpace(c.Product)?"MCHOSE mouse":c.Product.Trim()){Kind=DeviceKind.Mouse,Experimental=true};}
 public static byte[] Request(byte report,int length){var r=Enumerable.Repeat((byte)255,length).ToArray();r[0]=report;r[1]=0xF9;return r;}
 public static Reading Parse(byte[] r,ushort usbVid){
  if(r==null || r.Length<13 || (r[0]!=0x11 && r[0]!=0x12) || r[1]!=0xF9)return null;
  int vid=(r[2]^255)|((r[3]^255)<<8);
  if(usbVid==0x5253?vid!=0x5253:vid!=0x3837 && vid!=0x41E4)return null;
  int level=r[11]^255,charge=r[12]^255;if(level>100 || charge>2)return null;
  return new Reading("MCHOSE mouse"){Kind=DeviceKind.Mouse,Experimental=true,Percent=level,Charging=charge==1,Time=DateTime.Now,State="Подключено"};
 }
 public Reading Read(HidCollection c,Action<string> log){
  var result=Describe(c);
  using(var h=Hid.CreateFile(c.Path,0,3,IntPtr.Zero,3,0,IntPtr.Zero)){
   if(h.IsInvalid){result.State="Приёмник занят или нет доступа";return result;}
   foreach(byte report in new byte[]{0x12,0x11})for(int attempt=0;attempt<2;attempt++){
    var req=Request(report,c.Feature);if(!Hid.HidD_SetFeature(h,req,req.Length))break;Thread.Sleep(120);
    var r=new byte[c.Feature];r[0]=report;if(!Hid.HidD_GetFeature(h,r,r.Length) || r[0]!=report)continue;
    log("MCHOSE classic RX "+BitConverter.ToString(r));var parsed=Parse(r,c.Vid);if(parsed==null)continue;
    parsed.Name=result.Name;if((r[10]^255)==0 && parsed.Charging)parsed.Transport="USB-кабель";return parsed;
   }
  }return result;
 }
}
public sealed class MchoseModernDriver:IBatteryDriver {
 static readonly ushort[] Pids={0x1014,0x1018,0x1016,0x102C,0x102D,0x4028};
 public string Id{get{return "mchose-modern";}}
 public bool Matches(HidCollection c){return c.Vid==0x3837 && Pids.Contains(c.Pid) && c.Page==0xFF01 && c.Usage==1 && c.Input==64 && c.Output==64;}
 public Reading Describe(HidCollection c){return new Reading(string.IsNullOrWhiteSpace(c.Product)?"MCHOSE mouse":c.Product.Trim()){Kind=DeviceKind.Mouse,Experimental=true,Transport=c.Pid==0x4028?"USB-кабель":"USB-приёмник · 2,4 ГГц"};}
 public static byte[] Request(){var r=new byte[64];r[0]=0x4D;r[1]=1;r[2]=1;r[5]=9;r[8]=8;return r;}
 public static Reading Parse(byte[] r){
  if(r==null || r.Length<23 || r[0]!=0x4D || r[1]!=1 || r[4]!=0 || r[5]!=9 || r[6]!=0 || r[7]!=0)return null;
  int length=r[3];if(length<14 || 8+length>=r.Length || r[8]!=0x37 || r[9]!=0x38)return null;
  if((r[2]&1)!=0){byte sum=0;for(int i=2;i<8+length;i++)sum^=r[i];if(sum!=r[8+length])return null;}
  if(r[18]==0 || r[18]>2 || r[19]>2 || r[20]>100)return null;
  return new Reading("MCHOSE mouse"){Kind=DeviceKind.Mouse,Experimental=true,Percent=r[20],Charging=r[19]==1,Time=DateTime.Now,State="Подключено"};
 }
 public Reading Read(HidCollection c,Action<string> log){
  var result=Describe(c);
  using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){
   if(h.IsInvalid){result.State="Приёмник занят или нет доступа";return result;}
   Hid.HidD_FlushQueue(h);var budget=Stopwatch.StartNew();
   for(int attempt=0;attempt<3 && budget.ElapsedMilliseconds<1200;attempt++){
    if(Hid.Transfer(h,Request(),true,200)==null)continue;
    var wait=Stopwatch.StartNew();while(wait.ElapsedMilliseconds<250 && budget.ElapsedMilliseconds<1200){
     var r=Hid.Transfer(h,new byte[64],false,Math.Max(1,250-(int)wait.ElapsedMilliseconds));if(r==null)break;
     log("MCHOSE modern RX "+BitConverter.ToString(r));var parsed=Parse(r);if(parsed==null)continue;parsed.Name=result.Name;parsed.Transport=result.Transport;return parsed;
    }
   }
  }return result;
 }
}
