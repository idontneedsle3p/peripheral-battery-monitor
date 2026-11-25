using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Text;
using System.Threading;

public class HidCollection {
 public HidBatteryFeature BatteryFeature,BatteryInput;
 public string Path, Product,ContainerId; public ushort Vid,Pid,Page,Usage,Input,Output,Feature;
 public override string ToString(){return string.Format("{0:X4}:{1:X4} {2} page={3:X4} usage={4:X4} in={5} out={6} feature={7}",Vid,Pid,Product,Page,Usage,Input,Output,Feature);}
}
public static class Hid {
 [StructLayout(LayoutKind.Sequential)] struct Interface {public int Size;public Guid Guid;public int Flags;public IntPtr Reserved;}
 [StructLayout(LayoutKind.Sequential)] struct Attributes {public int Size;public ushort Vid,Pid,Version;}
 [StructLayout(LayoutKind.Sequential)] struct Caps {public ushort Usage,Page,Input,Output,Feature;[MarshalAs(UnmanagedType.ByValArray,SizeConst=17)]public ushort[] Reserved;public ushort Links,IB,IV,ID,OB,OV,OD,FB,FV,FD;}
 [StructLayout(LayoutKind.Sequential)] struct Overlap { public IntPtr Internal,InternalHigh;public uint Offset,OffsetHigh;public IntPtr Event; }
 [DllImport("hid.dll")] static extern void HidD_GetHidGuid(out Guid guid);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode)] static extern IntPtr SetupDiGetClassDevs(ref Guid g,IntPtr e,IntPtr p,uint f);
 [DllImport("setupapi.dll")] static extern bool SetupDiEnumDeviceInterfaces(IntPtr d,IntPtr dev,ref Guid g,uint i,ref Interface data);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode)] static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr d,ref Interface data,IntPtr detail,uint size,out uint needed,IntPtr dev);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,EntryPoint="SetupDiGetDeviceInterfaceDetailW")] static extern bool GetInterfaceDetailNode(IntPtr d,ref Interface data,IntPtr detail,uint size,out uint needed,ref WindowsDeviceProperties.DeviceInfo dev);
 [DllImport("setupapi.dll")] static extern bool SetupDiDestroyDeviceInfoList(IntPtr d);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern SafeFileHandle CreateFile(string p,uint access,uint share,IntPtr sec,uint disposition,uint flags,IntPtr template);
 [DllImport("hid.dll")] [return: MarshalAs(UnmanagedType.U1)] static extern bool HidD_GetAttributes(SafeFileHandle h,ref Attributes a);
 [DllImport("hid.dll")] [return: MarshalAs(UnmanagedType.U1)] static extern bool HidD_GetPreparsedData(SafeFileHandle h,out IntPtr p);
 [DllImport("hid.dll")] [return: MarshalAs(UnmanagedType.U1)] static extern bool HidD_FreePreparsedData(IntPtr p);
 [DllImport("hid.dll")] static extern int HidP_GetCaps(IntPtr p,out Caps c);
 [DllImport("hid.dll")] static extern int HidP_GetValueCaps(int reportType,IntPtr caps,ref ushort count,IntPtr preparsed);
 [DllImport("hid.dll")] static extern int HidP_GetUsageValue(int reportType,ushort page,ushort link,ushort usage,out uint value,IntPtr preparsed,byte[] report,uint length);
 static HidBatteryFeature FindBatteryFeature(IntPtr preparsed,ushort count,int reportType=2){
  if(count==0 || count>256)return null;IntPtr caps=Marshal.AllocHGlobal(count*72);
  try{if(HidP_GetValueCaps(reportType,caps,ref count,preparsed)!=0x110000)return null;var bytes=new byte[72];for(int i=0;i<count;i++){Marshal.Copy(IntPtr.Add(caps,i*72),bytes,0,72);var battery=HidBatteryFeature.ParseCaps(bytes);if(battery!=null)return battery;}return null;}finally{Marshal.FreeHGlobal(caps);}
 }
 public static int? ReadStandardBattery(HidCollection c){
  if(!new StandardHidBatteryDriver().Matches(c))return null;
  using(var handle=CreateFile(c.Path,0,3,IntPtr.Zero,3,0,IntPtr.Zero)){
   if(handle.IsInvalid)return null;IntPtr preparsed;if(!HidD_GetPreparsedData(handle,out preparsed))return null;
   try{foreach(int type in new[]{2,0}){var field=type==2?c.BatteryFeature:c.BatteryInput;int length=type==2?c.Feature:c.Input;if(field==null || length==0 || length>4096)continue;var report=new byte[length];report[0]=field.Report;bool got=type==2?HidD_GetFeature(handle,report,length):HidD_GetInputReport(handle,report,length);if(!got)continue;uint value;int status=HidP_GetUsageValue(type,6,field.Link,0x20,out value,preparsed,report,(uint)report.Length);if(status==0x110000 && value<=100)return (int)value;}return null;}finally{HidD_FreePreparsedData(preparsed);}
  }
 }
 [DllImport("hid.dll")] [return: MarshalAs(UnmanagedType.U1)] static extern bool HidD_GetProductString(SafeFileHandle h,byte[] b,int n);
 [DllImport("hid.dll")] [return: MarshalAs(UnmanagedType.U1)] public static extern bool HidD_SetFeature(SafeFileHandle h,byte[] b,int n);
 [DllImport("hid.dll")] [return: MarshalAs(UnmanagedType.U1)] public static extern bool HidD_GetFeature(SafeFileHandle h,byte[] b,int n);
 [DllImport("hid.dll")] [return: MarshalAs(UnmanagedType.U1)] public static extern bool HidD_GetInputReport(SafeFileHandle h,byte[] b,int n);
 [DllImport("hid.dll")] [return: MarshalAs(UnmanagedType.U1)] public static extern bool HidD_FlushQueue(SafeFileHandle h);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool WriteFile(SafeFileHandle h,IntPtr b,uint n,IntPtr count,IntPtr ov);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool ReadFile(SafeFileHandle h,IntPtr b,uint n,IntPtr count,IntPtr ov);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool GetOverlappedResult(SafeFileHandle h,IntPtr ov,out uint n,bool wait);
 [DllImport("kernel32.dll")] static extern bool CancelIoEx(SafeFileHandle h,IntPtr ov);
 public static List<HidCollection> Enumerate(){
  var result=new List<HidCollection>();Guid guid;HidD_GetHidGuid(out guid);
  IntPtr set=SetupDiGetClassDevs(ref guid,IntPtr.Zero,IntPtr.Zero,18);
  if(set==new IntPtr(-1))throw new InvalidOperationException("SetupAPI недоступен");
  try { for(uint i=0;;i++){
   var data=new Interface();data.Size=Marshal.SizeOf(typeof(Interface));
   if(!SetupDiEnumDeviceInterfaces(set,IntPtr.Zero,ref guid,i,ref data))break;
   uint need;SetupDiGetDeviceInterfaceDetail(set,ref data,IntPtr.Zero,0,out need,IntPtr.Zero);if(need==0)continue;
   IntPtr detail=Marshal.AllocHGlobal((int)need);
   try {Marshal.WriteInt32(detail,IntPtr.Size==8?8:6);
    var node=new WindowsDeviceProperties.DeviceInfo{Size=(uint)Marshal.SizeOf(typeof(WindowsDeviceProperties.DeviceInfo))};
    if(!GetInterfaceDetailNode(set,ref data,detail,need,out need,ref node))continue;
    string path=Marshal.PtrToStringUni(IntPtr.Add(detail,4));
    using(var h=CreateFile(path,0,3,IntPtr.Zero,3,0,IntPtr.Zero)){
     if(h.IsInvalid)continue;var a=new Attributes();a.Size=Marshal.SizeOf(typeof(Attributes));if(!HidD_GetAttributes(h,ref a))continue;
     IntPtr pp;if(!HidD_GetPreparsedData(h,out pp))continue;
     try{Caps c;if(HidP_GetCaps(pp,out c)<0)continue;byte[] name=new byte[512];HidD_GetProductString(h,name,name.Length);
      result.Add(new HidCollection{Path=path,ContainerId=WindowsDeviceProperties.ContainerId(set,ref node),Product=Encoding.Unicode.GetString(name).TrimEnd('\0'),Vid=a.Vid,Pid=a.Pid,Page=c.Page,Usage=c.Usage,Input=c.Input,Output=c.Output,Feature=c.Feature,BatteryFeature=FindBatteryFeature(pp,c.FV),BatteryInput=FindBatteryFeature(pp,c.IV,0)});
     }finally{HidD_FreePreparsedData(pp);}
    }
   }finally{Marshal.FreeHGlobal(detail);}
  }}finally{SetupDiDestroyDeviceInfoList(set);}return result;
 }
 public static byte[] Transfer(SafeFileHandle h,byte[] bytes,bool write,int timeout){
  using(var done=new EventWaitHandle(false,EventResetMode.ManualReset)){
   IntPtr buffer=Marshal.AllocHGlobal(bytes.Length),ov=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Overlap)));
   try {Marshal.Copy(bytes,0,buffer,bytes.Length);var o=new Overlap();o.Event=done.SafeWaitHandle.DangerousGetHandle();Marshal.StructureToPtr(o,ov,false);
    bool ok=write?WriteFile(h,buffer,(uint)bytes.Length,IntPtr.Zero,ov):ReadFile(h,buffer,(uint)bytes.Length,IntPtr.Zero,ov);
    if(!ok && Marshal.GetLastWin32Error()!=997)return null;
    if(!done.WaitOne(timeout)){CancelIoEx(h,ov);uint ignored;GetOverlappedResult(h,ov,out ignored,true);return null;}
    uint n;if(!GetOverlappedResult(h,ov,out n,false))return null;byte[] r=new byte[n];Marshal.Copy(buffer,r,0,r.Length);return r;
   }finally{Marshal.FreeHGlobal(ov);Marshal.FreeHGlobal(buffer);}
  }
 }
}
