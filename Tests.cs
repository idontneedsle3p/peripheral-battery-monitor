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
  Check(Devices.ParseMouse(new byte[3])==null,"AX5 truncated");Console.WriteLine("10 protocol checks passed");return 0;
 }
}
