using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

public sealed class MchoseX9Driver : IBatteryDriver {
 public string Id {get{return "mchose-x9";}}
 public bool Matches(HidCollection c){return c.Vid==0x3837 && c.Pid==0x6045 && c.Page==0xFF90 && c.Usage==1 && c.Input==64 && c.Output==64;}
 public Reading Describe(HidCollection c){return new Reading("MCHOSE X9"){Kind=DeviceKind.Headset};}
 public Reading Read(HidCollection c,Action<string> log){var x9=Describe(c);
    try{using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){
     if(h.IsInvalid){x9.State="Приёмник занят или нет доступа";return x9;}
     Hid.HidD_FlushQueue(h);byte[] req=new byte[64];req[0]=0x55;req[1]=0x65;req[2]=1;
     if(Hid.Transfer(h,req,true,500)==null){x9.State="Ошибка запроса";return x9;}
     var clock=Stopwatch.StartNew();while(clock.ElapsedMilliseconds<1500){var reply=Hid.Transfer(h,new byte[64],false,Math.Max(1,1500-(int)clock.ElapsedMilliseconds));if(reply==null)break;log("X9 RX "+BitConverter.ToString(reply));int? p=Devices.ParseX9(reply);if(p.HasValue){x9.Percent=p;x9.State="Подключена";x9.Time=DateTime.Now;break;}}
    }}catch(Exception e){x9.State="Ошибка чтения";log(e.Message);}
 return x9; }
}
public sealed class MchoseAx5Driver : IBatteryDriver {
 public string Id {get{return "mchose-ax5";}}
 public bool Matches(HidCollection c){return c.Vid==0x2023 && (c.Pid==0xF024 || c.Pid==0xF022) && c.Feature==65 && c.Page==0xFFFF;}
 public Reading Describe(HidCollection c){return new Reading("MCHOSE AX5 PRO MAX"){Kind=DeviceKind.Mouse,Transport=c.Pid==0xF022?"USB-кабель":"USB-приёмник · 2,4 ГГц"};}
 public Reading Read(HidCollection c,Action<string> log){var mouse=Describe(c);
    try{using(var h=Hid.CreateFile(c.Path,0,3,IntPtr.Zero,3,0,IntPtr.Zero)){
     if(h.IsInvalid){mouse.State="Приёмник занят или нет доступа";return mouse;}
     for(int i=0;i<3;i++){byte[] req=new byte[65];req[3]=2;req[4]=2;req[6]=0x83;
      if(!Hid.HidD_SetFeature(h,req,req.Length)){mouse.State="Ошибка запроса";break;}Thread.Sleep(30);
      byte[] r=new byte[65];if(!Hid.HidD_GetFeature(h,r,r.Length))continue;log("AX5 RX "+BitConverter.ToString(r));var parsed=Devices.ParseMouse(r);
      if(parsed!=null){mouse=parsed;if(c.Pid==0xF022)mouse.Transport="USB-кабель";break;}Thread.Sleep(60);
     }
    }}catch(Exception e){mouse.State="Ошибка чтения";log(e.Message);}
 return mouse; }
}
