using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

public sealed class BluetoothBatterySource:IBatterySource {
 static readonly Guid Service=new Guid("0000180f-0000-1000-8000-00805f9b34fb"),Characteristic=new Guid("00002a19-0000-1000-8000-00805f9b34fb");
 readonly Dictionary<string,Reading> known=new Dictionary<string,Reading>();
 readonly Func<Action<string>,IEnumerable<Reading>> reader;int cursor;
 public BluetoothBatterySource(){reader=ReadPaired;}
 public BluetoothBatterySource(Func<Action<string>,IEnumerable<Reading>> query){reader=query;}
 static T Wait<T>(IAsyncOperation<T> operation,int timeout){
  var watch=Stopwatch.StartNew();try{while(operation.Status==AsyncStatus.Started && watch.ElapsedMilliseconds<timeout)Thread.Sleep(15);if(operation.Status==AsyncStatus.Started){operation.Cancel();throw new TimeoutException("Bluetooth operation timed out");}return operation.GetResults();}finally{try{operation.Close();}catch{}}
 }
 public static Reading Parse(string id,string name,byte[] value){if(value==null || value.Length!=1 || value[0]>100)return null;return new Reading(string.IsNullOrWhiteSpace(name)?"Bluetooth LE device":name){Id="ble:"+id,Kind=GuessKind(name),Percent=value[0],Time=DateTime.Now,State="Подключено",Transport="Bluetooth LE",Experimental=true};}
 static DeviceKind GuessKind(string name){string n=(name??"").ToLowerInvariant();if(n.Contains("mouse") || n.Contains("мыш"))return DeviceKind.Mouse;if(n.Contains("keyboard") || n.Contains("клавиат"))return DeviceKind.Keyboard;if(n.Contains("head") || n.Contains("buds") || n.Contains("науш"))return DeviceKind.Headset;return DeviceKind.Other;}
 IEnumerable<Reading> ReadPaired(Action<string> log){
  var result=new List<Reading>();var devices=Wait(DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelectorFromPairingState(true)),2000);log("Bluetooth LE paired devices: "+devices.Count);var total=Stopwatch.StartNew();
  for(int visited=0;visited<devices.Count && total.ElapsedMilliseconds<12000;visited++){
   int index=cursor++%devices.Count;var info=devices[index];
   try{using(var device=Wait(BluetoothLEDevice.FromIdAsync(info.Id),1000)){
    if(device==null || device.ConnectionStatus!=BluetoothConnectionStatus.Connected)continue;
    var services=Wait(device.GetGattServicesForUuidAsync(Service,BluetoothCacheMode.Cached),1000);if(services.Status!=GattCommunicationStatus.Success)continue;
    foreach(var service in services.Services)using(service){
     var chars=Wait(service.GetCharacteristicsForUuidAsync(Characteristic,BluetoothCacheMode.Cached),1000);if(chars.Status!=GattCommunicationStatus.Success)continue;
     foreach(var ch in chars.Characteristics){if((ch.CharacteristicProperties&GattCharacteristicProperties.Read)==0)continue;
      var response=Wait(ch.ReadValueAsync(BluetoothCacheMode.Uncached),1200);if(response.Status!=GattCommunicationStatus.Success || response.Value==null || response.Value.Length!=1)continue;
      byte value;using(var data=DataReader.FromBuffer(response.Value))value=data.ReadByte();
      var reading=Parse(info.Id+":"+service.AttributeHandle+":"+ch.AttributeHandle,info.Name+(services.Services.Count>1?" #"+service.AttributeHandle:""),new[]{value});if(reading!=null)result.Add(reading);
     }
    }
   }}catch(Exception e){log("Bluetooth "+info.Name+": "+e.Message);}
  }return result;
 }
 public Reading[] Poll(Action<string> log){
  // Keep last measurements explicit when a radio/device stops answering; never fabricate 0%.
  foreach(var r in known.Values){r.IsStale=true;r.State="Нет ответа · последнее значение";}
  try{foreach(var r in reader(log)){if(r!=null && r.Id!=null)known[r.Id]=r;}}catch(Exception e){log("Bluetooth: "+e.Message);}
  return known.Values.ToArray();
 }
}
