using System;
class Tests {
 static void Check(bool b,string name){if(!b)throw new Exception(name);}
 static int Main(){
  Check(Devices.ParseX9(new byte[]{0x55,0x65,40,2})==40,"X9 real reply");
  Check(!Devices.ParseX9(new byte[]{0x55,0x65,1,0}).HasValue,"X9 echo");
  Check(!Devices.ParseX9(new byte[]{0x55,0x65,101,2}).HasValue,"X9 range");
  Check(!Devices.ParseX9(new byte[]{0x55,0x65}).HasValue,"X9 truncated");
  byte[] r={0,0xA1,0,2,2,0,0x83,0,77};Check(Devices.ParseMouse(r).Percent==77,"AX5 real reply");
  r[7]=1;Check(Devices.ParseMouse(r).Charging,"AX5 charging");r[8]=101;Check(Devices.ParseMouse(r)==null,"AX5 range");
  r[8]=77;r[1]=0;Check(Devices.ParseMouse(r)==null,"AX5 echo");r[1]=0xA1;r[6]=0x81;Check(Devices.ParseMouse(r)==null,"AX5 wrong command");
  Check(Devices.ParseMouse(new byte[3])==null,"AX5 truncated");
  var fake=new FakeDriver();var monitor=new BatteryMonitor(fake);
  var a=new HidCollection{Path="a",Vid=123};var b=new HidCollection{Path="b",Vid=123};var c=new HidCollection{Path="c",Vid=123};
  var cols=new System.Collections.Generic.List<HidCollection>{a,b,c,a,new HidCollection{Path="unsupported",Vid=999}};
  var readings=monitor.Poll(cols,s=>{});Check(readings.Length==3,"dynamic count and duplicate collection");Check(fake.Calls==3,"unsupported devices never queried");
  Check(readings[0].Id!=readings[1].Id,"identical models retain distinct identities");
  fake.Fail=true;readings=monitor.Poll(cols,s=>{});Check(readings[0].Percent==50 && readings[0].IsStale && !readings[0].Charging,"failed read retains explicitly stale battery");
  readings=monitor.Poll(new System.Collections.Generic.List<HidCollection>(),s=>{});Check(readings.Length==3 && readings[0].State=="Отключено","unplugged devices retained as disconnected");
  fake.Fail=false;readings=monitor.Poll(cols,s=>{});Check(!readings[0].IsStale && readings[0].Percent==50,"reconnection restores fresh values");
  Check(new BatteryMonitor(fake).Poll(new System.Collections.Generic.List<HidCollection>(),s=>{}).Length==0,"empty discovery has no phantom devices");
  fake.Throw=true;readings=new BatteryMonitor(fake).Poll(cols,s=>{});Check(readings.Length==3 && readings[0].State=="Ошибка чтения","driver exceptions isolated");
  fake.Throw=false;fake.Level=101;readings=new BatteryMonitor(fake).Poll(cols,s=>{});Check(!readings[0].Percent.HasValue,"invalid driver percentage rejected");
  Check(!new MchoseAx5Driver().Matches(new HidCollection{Vid=0x2023,Pid=0xF024,Feature=65,Page=1}),"non-vendor collections not queried");
  Console.WriteLine("20 protocol and registry checks passed");return 0;
 }
}
class FakeDriver:IBatteryDriver {
 public bool Fail,Throw;public int Calls,Level=50;
 public string Id{get{return "test";}}
 public bool Matches(HidCollection c){return c.Vid==123;}
 public Reading Describe(HidCollection c){return new Reading("Identical model"){Kind=DeviceKind.Mouse};}
 public Reading Read(HidCollection c,Action<string> log){Calls++;if(Throw)throw new Exception("test failure");var r=Describe(c);if(!Fail){r.Percent=Level;r.Time=DateTime.Now;r.Charging=true;r.State="Подключено";}return r;}
}
