using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// One HID poll feeds every view; taskbars are rediscovered after display/Explorer changes.
class Strip:IDisposable {
 [StructLayout(LayoutKind.Sequential)]struct Rect {public int Left,Top,Right,Bottom;}
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern IntPtr FindWindow(string c,string t);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern IntPtr FindWindowEx(IntPtr p,IntPtr a,string c,string t);
 [DllImport("user32.dll")]static extern bool GetWindowRect(IntPtr h,out Rect r);
 readonly Dictionary<IntPtr,TaskbarView> views=new Dictionary<IntPtr,TaskbarView>();
 readonly Action open;readonly ContextMenuStrip menu;readonly Timer timer=new Timer();
 Reading[] values=new Reading[0];
 bool visible=true,disposed;string target="all";
 public Action MoveFinished;
 public int Width{get{return 138;}}public int Height{get{return 26;}}public int Left{get{return 0;}}public int Top{get{return 0;}}public Point Location{get{return Point.Empty;}set{}}
 public string Target{get{return target;}set{target=value;Sync();}}
 public bool Visible{get{return visible;}set{visible=value;Sync();}}
 public Strip(Action action,ContextMenuStrip context){open=action;menu=context;timer.Interval=1000;timer.Tick+=(s,e)=>Sync();timer.Start();}
 public void Show(){Visible=true;}
 internal static bool WantsScreen(string selection,Screen screen){return selection=="all" || (selection=="primary"?screen.Primary:string.Equals(selection,screen.DeviceName,StringComparison.OrdinalIgnoreCase));}
 static List<IntPtr> Bars(){var list=new List<IntPtr>();var main=FindWindow("Shell_TrayWnd",null);if(main!=IntPtr.Zero)list.Add(main);IntPtr h=IntPtr.Zero;while((h=FindWindowEx(IntPtr.Zero,h,"Shell_SecondaryTrayWnd",null))!=IntPtr.Zero)list.Add(h);return list;}
 void Sync(){
  if(disposed)return;var desired=new List<IntPtr>();
  if(visible)foreach(var bar in Bars()){Rect r;if(!GetWindowRect(bar,out r))continue;var screen=Screen.FromRectangle(Rectangle.FromLTRB(r.Left,r.Top,r.Right,r.Bottom));if(WantsScreen(target,screen))desired.Add(bar);}
  foreach(var h in views.Keys.Where(h=>!desired.Contains(h)).ToArray()){views[h].Dispose();views.Remove(h);}
  foreach(var h in desired){TaskbarView view;if(!views.TryGetValue(h,out view)){view=new TaskbarView(h,open,menu);views.Add(h,view);view.Show();}view.UpdateValues(values);}
 }
 public void UpdateValues(Reading[] r){values=r;Sync();}
 public void SaveVerification(string path){
  Sync();var lines=new List<string>{"Selection="+target,"ViewCount="+views.Count};int i=0;
  foreach(var pair in views){string p=path+"."+(i++);pair.Value.SaveVerification(p);lines.Add(System.IO.File.ReadAllText(p));}
  System.IO.File.WriteAllLines(path,lines);using(var b=TaskbarView.DrawStrip(values,2,false))b.Save(path+".png");
 }
 public void Dispose(){if(disposed)return;disposed=true;timer.Stop();timer.Dispose();foreach(var view in views.Values)view.Dispose();views.Clear();}
}
