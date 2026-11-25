using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public interface IBatterySource {Reading[] Poll(Action<string> log);}
public struct XInputBatteryReply {public uint Error;public byte Type,Level;}
public sealed class XInputSource:IBatterySource {
 [StructLayout(LayoutKind.Sequential)]struct BatteryInfo{public byte Type,Level;}
 [DllImport("xinput1_4.dll",EntryPoint="XInputGetBatteryInformation")]static extern uint GetBattery(uint index,byte deviceType,out BatteryInfo battery);
 readonly Func<uint,XInputBatteryReply> query;
 readonly Dictionary<uint,Reading> known=new Dictionary<uint,Reading>();
 public XInputSource():this(NativeQuery){}
 public XInputSource(Func<uint,XInputBatteryReply> reader){query=reader;}
 static XInputBatteryReply NativeQuery(uint index){BatteryInfo info;uint error=GetBattery(index,0,out info);return new XInputBatteryReply{Error=error,Type=info.Type,Level=info.Level};}
 public static Reading Parse(uint index,XInputBatteryReply reply){
  if(reply.Error!=0 || reply.Type==0)return null;
  BatteryLevel level=BatteryLevel.Unknown;
  if(reply.Type==1)level=BatteryLevel.Wired;
  else if((reply.Type==2 || reply.Type==3) && reply.Level<=3)level=(BatteryLevel)reply.Level;
  return new Reading("XInput controller "+(index+1)){Id="xinput:"+index,Kind=DeviceKind.Controller,Level=level,Experimental=true,State="Подключено",Time=DateTime.Now,Transport=reply.Type==1?"USB-кабель":"XInput"};
 }
 public Reading[] Poll(Action<string> log){
  var result=new List<Reading>();
  for(uint i=0;i<4;i++){
   Reading reading=null;bool disconnected=false;
   try{var reply=query(i);disconnected=reply.Error==1167 || reply.Error==0 && reply.Type==0;reading=Parse(i,reply);if(reading==null && !disconnected)log("XInput slot "+i+": "+reply.Error);}
   catch(Exception e){log("XInput: "+e.Message);}
   if(reading!=null)known[i]=reading;
   else {Reading previous;if(known.TryGetValue(i,out previous)){previous.IsStale=true;previous.State=disconnected?"Отключено":"Нет ответа · последнее значение";reading=previous;}}
   if(reading!=null)result.Add(reading);
  }
  return result.ToArray();
 }
}
