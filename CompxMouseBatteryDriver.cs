using System;
using System.Collections.Generic;

public sealed class CompxMouseBatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider {
 static readonly Dictionary<uint,string> Models=new Dictionary<uint,string>{
  {0x3554F508,"Pulsar mouse"},{0x3554F509,"Pulsar mouse"},{0x3554F507,"Pulsar mouse"},{0x3554F57C,"Kysona M600"},{0x3554F57D,"Kysona M600"},{0x3554F5D5,"CompX wireless mouse"},
  {0x373B1085,"VXE R1 SE+"},{0x3554F58F,"VXE R1 SE+"},{0x3554F58A,"VXE Dragonfly R1"},{0x3554F58C,"VXE Dragonfly R1"},{0x3554F510,"LAMZU mouse"},{0x3554F50F,"LAMZU mouse"},
  {0x3554F527,"Zaopin mouse"},{0x373B10C9,"ATK mouse"},{0x37105406,"Pulsar TenZ"},{0x37103415,"Pulsar TenZ"}};
 public string Id{get{return "compx-mouse";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public bool Matches(HidCollection c){return Models.ContainsKey(((uint)c.Vid<<16)|c.Pid) && c.Page==0xFF02 && c.Usage==2 && c.Input==17 && c.Output==17;}
 public Reading Describe(HidCollection c){string model;Models.TryGetValue(((uint)c.Vid<<16)|c.Pid,out model);return new Reading(model??"CompX mouse"){Kind=DeviceKind.Mouse,Experimental=true};}
 public static byte[] Request(byte command){var r=new byte[17];r[0]=8;r[1]=command;Checksum(r);return r;}
 public static void Checksum(byte[] r){int sum=0;for(int i=0;i<16;i++)sum+=r[i];r[16]=(byte)(0x55-sum);}
 public static bool Valid(byte[] r,byte cmd){if(r==null || r.Length!=17 || r[0]!=8 || r[1]!=cmd)return false;int sum=0;foreach(byte b in r)sum+=b;return (sum&255)==0x55;}
 public static Reading ParsePower(byte[] r){if(!Valid(r,4) || r[6]>100 || r[7]>1 || r[6]==0 && r[7]==0 && r[8]==0 && r[9]==0)return null;return new Reading("CompX mouse"){Kind=DeviceKind.Mouse,Experimental=true,Percent=r[6],Charging=r[7]==1,State="Подключено",Time=DateTime.Now};}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return original;
  bool linked=false;var status=BatteryQuery.Ask(h,c,Request(3),r=>{if(!Valid(r,3) || r[6]>1)return null;linked=r[6]==1;return new Reading("status");},400);if(status==null || !linked)return original;
  var reading=BatteryQuery.Ask(h,c,Request(4),ParsePower,500);if(reading!=null){reading.Name=original.Name;return reading;}
 }return original;}
}
