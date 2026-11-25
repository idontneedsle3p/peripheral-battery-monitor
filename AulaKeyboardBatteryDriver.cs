using System;

public sealed class AulaKeyboardBatteryDriver:IBatteryDriver,IRequireBatteryReading,IBatteryIdentityProvider,IBatteryPollingPolicy {
 public string Id{get{return "aula-by-keyboard";}}
 public string Identity(HidCollection c){return LogitechBatterySource.PhysicalKey(c.Path);}
 public TimeSpan MinimumPollInterval{get{return TimeSpan.FromMinutes(1);}}
 public bool Matches(HidCollection c){return (c.Vid==0x3554 && (c.Pid==0xFA09 || c.Pid==0xFB40) || c.Vid==0x41E4 && c.Pid==0x2001) && (c.Page==0xFF02 || c.Page==0xFF04) && c.Usage==2 && c.Input==20 && c.Output==20;}
 public Reading Describe(HidCollection c){return new Reading(string.IsNullOrWhiteSpace(c.Product)?"AULA / BY wireless keyboard":c.Product.Trim()){Kind=DeviceKind.Keyboard,Experimental=true};}
 public static byte[] Request(byte packet){var r=new byte[20];r[0]=0x13;r[1]=0x4A;r[2]=packet;r[19]=(byte)(r[0]+r[1]+r[2]);return r;}
 public static Reading Parse(byte[] r){if(r==null || r.Length!=20 || r[0]!=0x13 || (r[1]&127)!=0x4A || r[5]<1 || r[5]>100)return null;int sum=0;for(int i=0;i<19;i++)sum+=r[i];if(r[19]!=(byte)sum && !(r[19]==0 && r[2]==1 && r[3]==0))return null;return new Reading("AULA / BY keyboard"){Kind=DeviceKind.Keyboard,Experimental=true,Percent=r[5],Charging=r[6]>>4==1,Time=DateTime.Now,State="Подключено"};}
 public Reading Read(HidCollection c,Action<string> log){var original=Describe(c);using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){if(h.IsInvalid)return original;for(byte packet=0;packet<2;packet++){var reading=BatteryQuery.Ask(h,c,Request(packet),Parse,1200);if(reading!=null){reading.Name=original.Name;return reading;}}}return original;}
}
