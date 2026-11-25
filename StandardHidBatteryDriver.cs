using System;

public sealed class HidBatteryFeature {
 public byte Report;public ushort Link;
 public static HidBatteryFeature ParseCaps(byte[] c){
  // HIDP_VALUE_CAPS is 72 bytes on Windows; accept a scalar absolute 0..100 field only.
  if(c==null || c.Length!=72 || BitConverter.ToUInt16(c,0)!=6 || c[3]!=0 || c[12]!=0 || c[15]==0 || (BitConverter.ToUInt16(c,4)&3)!=2 || BitConverter.ToUInt16(c,18)<7 || BitConverter.ToUInt16(c,18)>32 || BitConverter.ToUInt16(c,20)!=1 || BitConverter.ToInt32(c,40)!=0 || BitConverter.ToInt32(c,44)!=100 || BitConverter.ToUInt16(c,56)!=0x20)return null;
  return new HidBatteryFeature{Report=c[2],Link=BitConverter.ToUInt16(c,6)};
 }
}
public sealed class StandardHidBatteryDriver:IBatteryDriver,IRequireBatteryReading {
 public string Id{get{return "standard-hid-battery";}}
 public bool Matches(HidCollection c){return c.BatteryFeature!=null && c.Feature>0 && c.Feature<=4096 || c.BatteryInput!=null && c.Input>0 && c.Input<=4096;}
 public Reading Describe(HidCollection c){return new Reading(string.IsNullOrWhiteSpace(c.Product)?"HID battery device":c.Product.Trim()){Kind=c.Page==1?(c.Usage==2?DeviceKind.Mouse:c.Usage==6?DeviceKind.Keyboard:c.Usage==4 || c.Usage==5?DeviceKind.Controller:DeviceKind.Other):DeviceKind.Other,Experimental=true,Transport="HID Battery Strength"};}
 public Reading Read(HidCollection c,Action<string> log){var r=Describe(c);int? level=Hid.ReadStandardBattery(c);if(level.HasValue){r.Percent=level;r.State="Подключено";r.Time=DateTime.Now;}return r;}
}
