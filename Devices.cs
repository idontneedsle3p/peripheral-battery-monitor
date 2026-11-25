using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

public class Reading {
 public string Name,State="Нет ответа",Transport="USB-приёмник · 2,4 ГГц";public int? Percent;public bool Charging;public DateTime? Time;
 public Reading(string name){Name=name;}
 public string Display {get{return Percent.HasValue?Percent+"%"+(Charging?" · зарядка":""):"—";}}
}
public static class Devices {
 public static int? ParseX9(byte[] r){if(r.Length>=4 && r[0]==0x55 && r[1]==0x65 && r[3]==2 && r[2]<=100)return r[2];return null;}
 public static Reading ParseMouse(byte[] r){
  if(r.Length<9 || r[0]!=0 || r[1]!=0xA1 || r[3]!=2 || r[5]!=0 || r[6]!=0x83 || r[7]>1 || r[8]>100)return null;
  return new Reading("MCHOSE AX5 PRO MAX"){Percent=r[8],Charging=r[7]==1,State="Подключена",Time=DateTime.Now};
 }
 public static Reading[] Poll(List<HidCollection> colls,Action<string> log){
  Reading x9=new Reading("MCHOSE X9"),mouse=new Reading("MCHOSE AX5 PRO MAX");
  foreach(var c in colls){
   if(c.Vid==0x3837 && c.Pid==0x6045 && c.Page==0xFF90 && c.Usage==1 && c.Input==64 && c.Output==64){
    try{using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){
     if(h.IsInvalid){x9.State="Приёмник занят или нет доступа";continue;}
     Hid.HidD_FlushQueue(h);byte[] req=new byte[64];req[0]=0x55;req[1]=0x65;req[2]=1;
     if(Hid.Transfer(h,req,true,500)==null){x9.State="Ошибка запроса";continue;}
     var clock=Stopwatch.StartNew();while(clock.ElapsedMilliseconds<1500){var reply=Hid.Transfer(h,new byte[64],false,Math.Max(1,1500-(int)clock.ElapsedMilliseconds));if(reply==null)break;log("X9 RX "+BitConverter.ToString(reply));int? p=ParseX9(reply);if(p.HasValue){x9.Percent=p;x9.State="Подключена";x9.Time=DateTime.Now;break;}}
    }}catch(Exception e){x9.State="Ошибка чтения";log(e.Message);}
   }
   if(c.Vid==0x2023 && (c.Pid==0xF024 || c.Pid==0xF022) && c.Feature==65){
    try{using(var h=Hid.CreateFile(c.Path,0,3,IntPtr.Zero,3,0,IntPtr.Zero)){
     if(h.IsInvalid){mouse.State="Приёмник занят или нет доступа";continue;}
     for(int i=0;i<3;i++){byte[] req=new byte[65];req[3]=2;req[4]=2;req[6]=0x83;
      if(!Hid.HidD_SetFeature(h,req,req.Length)){mouse.State="Ошибка запроса";break;}Thread.Sleep(30);
      byte[] r=new byte[65];if(!Hid.HidD_GetFeature(h,r,r.Length))continue;log("AX5 RX "+BitConverter.ToString(r));var parsed=ParseMouse(r);
      if(parsed!=null){mouse=parsed;if(c.Pid==0xF022)mouse.Transport="USB-кабель";break;}Thread.Sleep(60);
     }
    }}catch(Exception e){mouse.State="Ошибка чтения";log(e.Message);}
   }
  }return new[]{x9,mouse};
 }
}
