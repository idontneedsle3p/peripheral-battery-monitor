using System;
class Tests {
 static int checks;
 static void Check(bool b,string name){checks++;if(!b)throw new Exception(name);}
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
  L.Set("en");Check(L.T("Обновить")=="Refresh","English menu translation");
  Check(L.T("Нет ответа · последнее значение")=="No response · last known value","English stale status");
  Check(new Reading("test"){Percent=60,Charging=true}.Display=="60% · charging","English charging state");
  Check(L.T("MCHOSE X9")=="MCHOSE X9","device names unchanged");
  string settings=System.IO.Path.GetTempFileName();try{L.Save(settings);L.Set("ru");L.Load(settings);Check(L.Language=="en","saved language survives reload");}finally{System.IO.File.Delete(settings);}
  L.Set("ru");Check(L.T("Обновить")=="Обновить","switch back to Russian");Check(new Reading("test"){Percent=60,Charging=true}.Display=="60% · зарядка","Russian charging state");
  var classic=new MchoseClassicDriver();Check(classic.Matches(new HidCollection{Vid=0x5253,Pid=0x1020,Page=0xFF01,Usage=1,Feature=65}),"classic receiver matched");
  Check(!classic.Matches(new HidCollection{Vid=0x5253,Pid=0x9999,Page=0xFF01,Usage=1,Feature=65}),"unknown classic PID excluded");
  byte[] status=MchoseClassicDriver.Request(0x12,65);Check(MchoseClassicDriver.Parse(status,0x5253)==null,"classic request echo rejected");
  status[2]=(byte)(0x53^255);status[3]=(byte)(0x52^255);status[10]=(byte)(9^255);status[11]=(byte)(7^255);status[12]=255;
  Check(MchoseClassicDriver.Parse(status,0x5253).Percent==7,"classic low percentage not multiplied");
  status[12]=254;Check(MchoseClassicDriver.Parse(status,0x5253).Charging,"classic charging");status[11]=(byte)(101^255);Check(MchoseClassicDriver.Parse(status,0x5253)==null,"classic invalid percentage");
  status[11]=255;Check(MchoseClassicDriver.Parse(status,0x3837)==null,"classic wrong identity");Check(MchoseClassicDriver.Parse(new byte[4],0x5253)==null,"classic truncated reply");
  var modern=new MchoseModernDriver();Check(modern.Matches(new HidCollection{Vid=0x3837,Pid=0x1014,Page=0xFF01,Usage=1,Input=64,Output=64}),"modern receiver matched");
  Check(!modern.Matches(new HidCollection{Vid=0x3837,Pid=0x100C,Page=0xFF01,Usage=1,Input=64,Output=64}),"classic and modern IDs do not overlap");
  byte[] frame=MchoseModernDriver.Request();Check(frame[8]==8 && MchoseModernDriver.Parse(frame)==null,"modern request and echo rejection");
  frame[3]=14;frame[8]=0x37;frame[9]=0x38;frame[18]=2;frame[19]=1;frame[20]=83;ModernChecksum(frame);
  Check(MchoseModernDriver.Parse(frame).Percent==83 && MchoseModernDriver.Parse(frame).Charging,"modern battery and charging offsets");
  frame[22]^=1;Check(MchoseModernDriver.Parse(frame)==null,"modern checksum mismatch");ModernChecksum(frame);
  frame[3]=64;Check(MchoseModernDriver.Parse(frame)==null,"modern oversized length");frame[3]=14;frame[18]=0;ModernChecksum(frame);Check(MchoseModernDriver.Parse(frame)==null,"modern unlinked receiver not zero battery");
  frame[18]=2;frame[20]=101;ModernChecksum(frame);Check(MchoseModernDriver.Parse(frame)==null,"modern invalid percentage");
  frame[20]=50;frame[7]=1;ModernChecksum(frame);Check(MchoseModernDriver.Parse(frame)==null,"modern sequence mismatch");
  var ax5=new MchoseAx5Driver();Check(ax5.Matches(new HidCollection{Vid=0x2023,Pid=0xF013,Page=0xFFFF,Feature=65}),"A5 shared receiver matched");Check(!ax5.Matches(new HidCollection{Vid=0x2023,Pid=0xF023,Page=0xFFFF,Feature=65}),"AX5 bootloader excluded");
  var kb=new MchoseKeyboardDriver();Check(kb.Matches(new HidCollection{Vid=0x3837,Pid=0x3033,Page=0xFF31,Usage=0x74,Input=65,Output=65,Product="MCHOSE G75 V2 2.4G"}),"GLW keyboard collection matched");
  Check(!kb.Matches(new HidCollection{Vid=0x3837,Pid=0x3033,Page=0xFF31,Usage=0x74,Input=65,Output=65,Product="MCHOSE K87S"}),"K87S excluded");
  Check(MchoseKeyboardDriver.Parse(MchoseKeyboardDriver.Request())==null,"keyboard echo rejected");
  byte[] keyboard=new byte[65];keyboard[1]=0xAA;keyboard[2]=5;keyboard[5]=56;keyboard[41]=85;KeyboardChecksum(keyboard);
  Check(MchoseKeyboardDriver.Parse(keyboard).Percent==85,"keyboard battery offset");
  keyboard[41]=0x80|60;KeyboardChecksum(keyboard);Check(MchoseKeyboardDriver.Parse(keyboard).Charging && MchoseKeyboardDriver.Parse(keyboard).Percent==60,"keyboard charging bit");
  keyboard[4]^=1;Check(MchoseKeyboardDriver.Parse(keyboard)==null,"keyboard checksum rejected");KeyboardChecksum(keyboard);
  Check(MchoseKeyboardDriver.Parse(new byte[10])==null,"keyboard truncated reply");
  keyboard[41]=127;KeyboardChecksum(keyboard);Check(MchoseKeyboardDriver.Parse(keyboard)==null,"keyboard invalid percentage");
  keyboard[41]=0;KeyboardChecksum(keyboard);Check(MchoseKeyboardDriver.Parse(keyboard).Percent==0,"keyboard zero battery valid");
  byte[] unnumbered=new byte[64];Array.Copy(keyboard,1,unnumbered,0,64);Check(MchoseKeyboardDriver.Parse(unnumbered).Percent==0,"keyboard unnumbered payload");
  Check(new MchoseX9Driver().Matches(new HidCollection{Vid=0x3837,Pid=0x600A,Page=0xFF90,Usage=1,Input=64,Output=64}),"V9 Turbo receiver matched");
  Check(new MchoseX9Driver().Describe(new HidCollection{Pid=0x600A}).Experimental,"V9 Turbo experimental label");
  var slow=new FakeDriver{Interval=TimeSpan.FromMinutes(10)};var scheduled=new BatteryMonitor(slow);scheduled.Poll(cols,s=>{});int calls=slow.Calls;
  var cachedReadings=scheduled.Poll(cols,s=>{});Check(slow.Calls==calls && cachedReadings[0].Percent==50,"slow polling preserves cached readings");
  scheduled.Poll(cols,s=>{},true);Check(slow.Calls==calls+3,"manual refresh bypasses cooldown");
  scheduled.Poll(new System.Collections.Generic.List<HidCollection>(),s=>{});calls=slow.Calls;scheduled.Poll(cols,s=>{});Check(slow.Calls==calls+3,"reconnect bypasses cooldown");
  var gamepad=XInputSource.Parse(0,new XInputBatteryReply{Type=2,Level=1});Check(!gamepad.Percent.HasValue && gamepad.Level==BatteryLevel.Low && gamepad.LowBattery,"XInput low level never invented percentage");
  Check(XInputSource.Parse(0,new XInputBatteryReply{Type=3,Level=3}).Level==BatteryLevel.Full,"XInput NiMH full");
  Check(XInputSource.Parse(1,new XInputBatteryReply{Type=1,Level=0}).Level==BatteryLevel.Wired,"wired controller not empty battery");
  Check(XInputSource.Parse(0,new XInputBatteryReply{Type=255,Level=0}).Level==BatteryLevel.Unknown,"unknown type not empty battery");
  Check(XInputSource.Parse(0,new XInputBatteryReply{Type=2,Level=200}).Level==BatteryLevel.Unknown,"invalid XInput level not shown");
  Check(XInputSource.Parse(0,new XInputBatteryReply{Error=1167})==null,"disconnected XInput slot hidden");
  Check(XInputSource.Parse(0,new XInputBatteryReply{Type=0})==null,"battery type disconnected hidden");
  bool padConnected=true;var pads=new XInputSource(i=>new XInputBatteryReply{Error=padConnected && i==0?0u:1167u,Type=2,Level=2});
  var padReadings=pads.Poll(s=>{});Check(padReadings.Length==1 && padReadings[0].Id=="xinput:0","XInput only connected slots displayed");
  padConnected=false;padReadings=pads.Poll(s=>{});Check(padReadings.Length==1 && padReadings[0].IsStale && padReadings[0].State=="Отключено","XInput disconnection retains stale state");
  padConnected=true;Check(!pads.Poll(s=>{})[0].IsStale,"XInput reconnection restores fresh state");
  Check(new XInputSource(i=>new XInputBatteryReply{Error=1167}).Poll(s=>{}).Length==0,"no phantom XInput devices");
  L.Set("en");Check(gamepad.Display=="Low battery" && gamepad.ShortDisplay=="Low","English discrete levels");L.Set("ru");Check(gamepad.Display=="Низкий заряд","Russian discrete levels");
  Console.WriteLine(checks+" protocol, registry and localization checks passed");return 0;
 }
 static void KeyboardChecksum(byte[] r){int sum=0;for(int i=5;i<65;i++)sum+=r[i];r[4]=(byte)sum;}
 static void ModernChecksum(byte[] r){r[8+r[3]]=0;for(int i=2;i<8+r[3];i++)r[8+r[3]]^=r[i];}
}
class FakeDriver:IBatteryDriver,IBatteryPollingPolicy {
 public TimeSpan Interval;public TimeSpan MinimumPollInterval{get{return Interval;}}
 public bool Fail,Throw;public int Calls,Level=50;
 public string Id{get{return "test";}}
 public bool Matches(HidCollection c){return c.Vid==123;}
 public Reading Describe(HidCollection c){return new Reading("Identical model"){Kind=DeviceKind.Mouse};}
 public Reading Read(HidCollection c,Action<string> log){Calls++;if(Throw)throw new Exception("test failure");var r=Describe(c);if(!Fail){r.Percent=Level;r.Time=DateTime.Now;r.Charging=true;r.State="Подключено";}return r;}
}
