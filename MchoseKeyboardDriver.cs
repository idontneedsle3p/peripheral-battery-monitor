using System;
using System.Diagnostics;

// GLW function-config read on the FF31:0074 collection. No configuration writes.
public sealed class MchoseKeyboardDriver:IBatteryDriver,IBatteryPollingPolicy {
 public string Id{get{return "mchose-glw-keyboard";}}
 public TimeSpan MinimumPollInterval{get{return TimeSpan.FromMinutes(10);}}
 public bool Matches(HidCollection c){return c.Vid==0x3837 && c.Pid==0x3033 && c.Page==0xFF31 && c.Usage==0x74 && c.Input==65 && c.Output==65 && (c.Product==null || c.Product.IndexOf("K87S",StringComparison.OrdinalIgnoreCase)<0);}
 public Reading Describe(HidCollection c){return new Reading(string.IsNullOrWhiteSpace(c.Product)?"MCHOSE keyboard":c.Product.Trim()){Kind=DeviceKind.Keyboard,Experimental=true};}
 public static byte[] Request(){var r=new byte[65];r[1]=0x55;r[2]=5;r[4]=56;r[5]=56;return r;}
 public static Reading Parse(byte[] r){
  if(r==null)return null;int offset=r.Length>0 && r[0]==0?1:0;
  if(r.Length-offset<41 || r[offset]!=0xAA || r[offset+1]!=5 || r[offset+2]!=0 || r[offset+5]!=0 || r[offset+6]!=0 || r[offset+7]!=0)return null;
  int length=r[offset+4];if(length<33 || length>56 || offset+8+length>r.Length)return null;
  int sum=0;for(int i=4;i<8+length;i++)sum+=r[offset+i];if((sum&255)!=r[offset+3])return null;
  byte battery=r[offset+40];int level=battery&127;if(level>100)return null;
  return new Reading("MCHOSE keyboard"){Kind=DeviceKind.Keyboard,Experimental=true,Percent=level,Charging=(battery&128)!=0,Time=DateTime.Now,State="Подключено"};
 }
 public Reading Read(HidCollection c,Action<string> log){
  var result=Describe(c);
  using(var h=Hid.CreateFile(c.Path,0xC0000000,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero)){
   if(h.IsInvalid){result.State="Приёмник занят или нет доступа";return result;}
   for(int attempt=0;attempt<2;attempt++){
    Hid.HidD_FlushQueue(h);if(Hid.Transfer(h,Request(),true,250)==null)continue;
    var wait=Stopwatch.StartNew();while(wait.ElapsedMilliseconds<500){
     var r=Hid.Transfer(h,new byte[65],false,Math.Max(1,500-(int)wait.ElapsedMilliseconds));if(r==null)break;
     log("MCHOSE keyboard RX "+BitConverter.ToString(r));var parsed=Parse(r);if(parsed==null)continue;parsed.Name=result.Name;return parsed;
    }
   }
  }return result;
 }
}
