using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public sealed class BluetoothDeviceNode {
 public string InstanceId,ContainerId,Name;
 public int? Battery;
 public bool? Connected;
 public uint? ClassOfDevice;
 public bool HasProblem;
}

// Read-only SetupAPI properties. The battery value is maintained by Windows, not queried over radio here.
public static class WindowsDeviceProperties {
 [StructLayout(LayoutKind.Sequential)] public struct DeviceInfo {public uint Size;public Guid ClassGuid;public uint DevInst;public IntPtr Reserved;}
 [StructLayout(LayoutKind.Sequential)] struct PropertyKey {public Guid Format;public uint Id;public PropertyKey(string guid,uint id){Format=new Guid(guid);Id=id;}}
 static readonly PropertyKey Container=new PropertyKey("8C7ED206-3F8A-4827-B3AB-AE9E1FAEFC6C",2),Battery=new PropertyKey("104EA319-6EE2-4701-BD47-8DDBF425BBE5",2),Connected=new PropertyKey("83DA6326-97A6-4088-9453-A1923F573B29",15),ClassOfDevice=new PropertyKey("2BD67D8B-8BEB-48D5-87E0-6CDA3428040A",10),FriendlyName=new PropertyKey("A45C254E-DF1C-4EFD-8020-67D146A850E0",14),Description=new PropertyKey("A45C254E-DF1C-4EFD-8020-67D146A850E0",2);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true,EntryPoint="SetupDiGetClassDevsW")] static extern IntPtr GetDevices(IntPtr classGuid,string enumerator,IntPtr parent,uint flags);
 [DllImport("setupapi.dll",SetLastError=true)] static extern bool SetupDiEnumDeviceInfo(IntPtr set,uint index,ref DeviceInfo info);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true,EntryPoint="SetupDiGetDeviceInstanceIdW")] static extern bool GetInstanceId(IntPtr set,ref DeviceInfo info,StringBuilder id,uint size,out uint required);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true,EntryPoint="SetupDiGetDevicePropertyW")] static extern bool GetProperty(IntPtr set,ref DeviceInfo info,ref PropertyKey key,out uint type,byte[] value,uint size,out uint required,uint flags);
 [DllImport("setupapi.dll")] static extern bool SetupDiDestroyDeviceInfoList(IntPtr set);
 [DllImport("cfgmgr32.dll")] static extern uint CM_Get_DevNode_Status(out uint status,out uint problem,uint devInst,uint flags);
 public static string NormalizeContainer(string value){Guid id;return Guid.TryParse(value,out id) && id!=Guid.Empty?id.ToString("D"):null;}
 static byte[] Read(IntPtr set,ref DeviceInfo info,PropertyKey key,uint wanted,int capacity){uint type,needed;var value=new byte[capacity];if(!GetProperty(set,ref info,ref key,out type,value,(uint)value.Length,out needed,0) || type!=wanted || needed>capacity)return null;Array.Resize(ref value,(int)needed);return value;}
 public static string ContainerId(IntPtr set,ref DeviceInfo info){var value=Read(set,ref info,Container,13,16);return value!=null && value.Length==16?NormalizeContainer(new Guid(value).ToString()):null;}
 static string Text(IntPtr set,ref DeviceInfo info,PropertyKey key){var value=Read(set,ref info,key,18,2048);return value==null?null:Encoding.Unicode.GetString(value).TrimEnd('\0');}
 public static List<BluetoothDeviceNode> ReadBluetoothNodes(){
  var result=new List<BluetoothDeviceNode>();
  foreach(string enumerator in new[]{"BTHENUM","BTHLE","BTHLEDEVICE"}){
   IntPtr set=GetDevices(IntPtr.Zero,enumerator,IntPtr.Zero,6);if(set==new IntPtr(-1))throw new Win32Exception(Marshal.GetLastWin32Error());
   try{for(uint index=0;;index++){
    var info=new DeviceInfo{Size=(uint)Marshal.SizeOf(typeof(DeviceInfo))};
    if(!SetupDiEnumDeviceInfo(set,index,ref info)){int error=Marshal.GetLastWin32Error();if(error!=259)throw new Win32Exception(error);break;}
    uint needed;var instance=new StringBuilder(1024);if(!GetInstanceId(set,ref info,instance,(uint)instance.Capacity,out needed))continue;
    var node=new BluetoothDeviceNode{InstanceId=instance.ToString(),ContainerId=ContainerId(set,ref info),Name=Text(set,ref info,FriendlyName)??Text(set,ref info,Description)};
    var battery=Read(set,ref info,Battery,3,1);if(battery!=null && battery.Length==1)node.Battery=battery[0];
    var connected=Read(set,ref info,Connected,17,1);if(connected!=null && connected.Length==1)node.Connected=connected[0]!=0;
    var cod=Read(set,ref info,ClassOfDevice,7,4);if(cod!=null && cod.Length==4)node.ClassOfDevice=BitConverter.ToUInt32(cod,0);
    uint status,problem;node.HasProblem=CM_Get_DevNode_Status(out status,out problem,info.DevInst,0)!=0 || (status&0x400)!=0;
    result.Add(node);
   }}finally{SetupDiDestroyDeviceInfoList(set);}
  }return result;
 }
}

public sealed class WindowsBluetoothBatterySource:IBatterySource {
 readonly Func<IEnumerable<BluetoothDeviceNode>> enumerate;
 readonly Dictionary<string,Reading> known=new Dictionary<string,Reading>(StringComparer.OrdinalIgnoreCase);
 public WindowsBluetoothBatterySource():this(()=>WindowsDeviceProperties.ReadBluetoothNodes()){}
 public WindowsBluetoothBatterySource(Func<IEnumerable<BluetoothDeviceNode>> reader){enumerate=reader;}
 static bool BluetoothNode(BluetoothDeviceNode n){return n!=null && n.InstanceId!=null && (n.InstanceId.StartsWith("BTHENUM\\",StringComparison.OrdinalIgnoreCase) || n.InstanceId.StartsWith("BTHLE\\",StringComparison.OrdinalIgnoreCase) || n.InstanceId.StartsWith("BTHLEDEVICE\\",StringComparison.OrdinalIgnoreCase));}
 static bool Root(BluetoothDeviceNode n){return n.InstanceId.StartsWith("BTHENUM\\DEV_",StringComparison.OrdinalIgnoreCase) || n.InstanceId.StartsWith("BTHLE\\DEV_",StringComparison.OrdinalIgnoreCase);}
 public static DeviceKind Kind(uint cod){uint major=(cod>>8)&31,minor=(cod>>2)&63;if(major==4)return DeviceKind.Headset;if(major==5){if((minor&15)==1 || (minor&15)==2)return DeviceKind.Controller;if((minor>>4)==1)return DeviceKind.Keyboard;if((minor>>4)==2)return DeviceKind.Mouse;}return DeviceKind.Other;}
 public static Reading[] Decode(IEnumerable<BluetoothDeviceNode> nodes){
  var readings=new List<Reading>();
  foreach(var group in nodes.Where(BluetoothNode).GroupBy(n=>WindowsDeviceProperties.NormalizeContainer(n.ContainerId)??n.InstanceId,StringComparer.OrdinalIgnoreCase)){
   var allRoots=group.Where(Root).ToArray();var roots=allRoots.Where(n=>!n.HasProblem).ToArray();if(allRoots.Length>0 && roots.Length==0)continue;var rootStates=roots.Where(n=>n.Connected.HasValue).ToArray();
   // A present/paired devnode can remain after disconnect. Unknown connection is insufficient.
   bool connected=rootStates.Length>0?rootStates.Any(n=>n.Connected==true):group.Any(n=>!n.HasProblem && n.Connected==true);if(!connected)continue;
   var candidate=group.Where(n=>!n.HasProblem && n.Battery.HasValue && n.Battery>=0 && n.Battery<=100 && n.Connected!=false).OrderByDescending(n=>n.Connected==true).ThenBy(n=>n.InstanceId,StringComparer.OrdinalIgnoreCase).FirstOrDefault();if(candidate==null)continue;
   string name=roots.Select(n=>n.Name).FirstOrDefault(n=>!string.IsNullOrWhiteSpace(n))??candidate.Name;
   if(string.IsNullOrWhiteSpace(name))name="Bluetooth device";
   foreach(string suffix in new[]{" Hands-Free AG"," Hands-Free"," Avrcp Transport"," Stereo"," Audio Gateway"})if(name.EndsWith(suffix,StringComparison.OrdinalIgnoreCase) && name.Length>suffix.Length){name=name.Substring(0,name.Length-suffix.Length);break;}
   var cod=roots.Concat(group).FirstOrDefault(n=>n.ClassOfDevice.HasValue);var kind=cod==null?DeviceKind.Other:Kind(cod.ClassOfDevice.Value);
   if(kind==DeviceKind.Other)kind=BluetoothBatterySource.GuessKind(name);
   readings.Add(new Reading(name){Id="bt-windows:"+group.Key,PhysicalId=WindowsDeviceProperties.NormalizeContainer(candidate.ContainerId),Kind=kind,Percent=candidate.Battery,Time=DateTime.Now,State="Подключено · данные Windows",Transport="Bluetooth · Windows",Experimental=true});
  }return readings.ToArray();
 }
 public Reading[] Poll(Action<string> log){foreach(var r in known.Values){r.IsStale=true;r.Charging=false;r.State="Нет ответа · последнее значение";}try{var nodes=enumerate().ToArray();log("Windows Bluetooth device nodes: "+nodes.Length);foreach(var r in Decode(nodes))known[r.Id]=r;}catch(Exception e){log("Windows Bluetooth properties: "+e.Message);}return known.Values.ToArray();}
}
