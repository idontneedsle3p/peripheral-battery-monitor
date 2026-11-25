using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows.Automation;

// A layered child of Explorer's taskbar: transparent pixels preserve its background.
class TaskbarView:NativeWindow,IDisposable {
 [StructLayout(LayoutKind.Sequential)]struct Rect {public int Left,Top,Right,Bottom;}
 [StructLayout(LayoutKind.Sequential)]struct PointN {public int X,Y;public PointN(int x,int y){X=x;Y=y;}}
 [StructLayout(LayoutKind.Sequential)]struct SizeN {public int X,Y;public SizeN(int x,int y){X=x;Y=y;}}
 [StructLayout(LayoutKind.Sequential,Pack=1)]struct Blend {public byte Op,Flags,Alpha,Format;}
 [StructLayout(LayoutKind.Sequential)]struct BitmapInfo {public uint Size;public int Width,Height;public ushort Planes,Bits;public uint Compression,ImageSize;public int Xppm,Yppm;public uint Used,Important;}
 [DllImport("gdi32.dll")]static extern IntPtr CreateDIBSection(IntPtr dc,ref BitmapInfo info,uint usage,out IntPtr bits,IntPtr section,uint offset);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern IntPtr FindWindow(string cls,string title);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern IntPtr FindWindowEx(IntPtr parent,IntPtr after,string cls,string title);
 [DllImport("user32.dll")]static extern bool GetWindowRect(IntPtr h,out Rect r);
 [DllImport("user32.dll")]static extern bool GetClientRect(IntPtr h,out Rect r);
 [DllImport("user32.dll")]static extern bool ScreenToClient(IntPtr h,ref PointN p);
 [DllImport("user32.dll")]static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int w,int height,uint flags);
 [DllImport("user32.dll")]static extern bool IsWindow(IntPtr h);
 [DllImport("user32.dll")]static extern bool IsWindowVisible(IntPtr h);
 [DllImport("user32.dll")]static extern bool ShowWindow(IntPtr h,int n);
 [DllImport("user32.dll")]static extern IntPtr GetParent(IntPtr h);
 [DllImport("user32.dll")]static extern uint GetDpiForWindow(IntPtr h);
 [DllImport("user32.dll")]static extern IntPtr GetDC(IntPtr h);
 [DllImport("user32.dll")]static extern int ReleaseDC(IntPtr h,IntPtr dc);
 [DllImport("gdi32.dll")]static extern IntPtr CreateCompatibleDC(IntPtr dc);
 [DllImport("gdi32.dll")]static extern IntPtr SelectObject(IntPtr dc,IntPtr obj);
 [DllImport("gdi32.dll")]static extern bool DeleteObject(IntPtr obj);
 [DllImport("gdi32.dll")]static extern bool DeleteDC(IntPtr dc);
 [DllImport("user32.dll",SetLastError=true)]static extern bool UpdateLayeredWindow(IntPtr h,IntPtr dst,IntPtr pos,ref SizeN size,IntPtr src,ref PointN source,int key,ref Blend blend,int flags);
 readonly Action open;readonly ContextMenuStrip menu;readonly Timer timer=new Timer();
 IntPtr taskbar;bool visible=true,disposed;float scale=1;bool light;
 Task<int?> clockProbe;int? trayFromRight;DateTime nextProbe=DateTime.MinValue;
 void ProbeClock(IntPtr bar,Rect rect){
  if(clockProbe!=null && clockProbe.IsCompleted){
   if(clockProbe.Status==TaskStatus.RanToCompletion && clockProbe.Result.HasValue)trayFromRight=clockProbe.Result;
   clockProbe=null;
  }
  if(clockProbe!=null || DateTime.UtcNow<nextProbe)return;
  nextProbe=DateTime.UtcNow.AddSeconds(10);
  clockProbe=Task.Run<int?>(()=>{
   try{
    var root=AutomationElement.FromHandle(bar);var elements=root.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.AutomationIdProperty,"SystemTrayIcon"));
    int? edge=null;foreach(AutomationElement element in elements){var r=element.Current.BoundingRectangle;if(!r.IsEmpty && r.Width>0 && r.Height>0 && r.Left>rect.Left && r.Right<=rect.Right+1 && r.Top>=rect.Top-1 && r.Bottom<=rect.Bottom+1){int x=(int)Math.Floor(r.Left);edge=edge.HasValue?Math.Min(edge.Value,x):x;}}
    return edge.HasValue?(int?)(rect.Right-edge.Value):null;
   }catch{return null;}
  });
 }
 Reading[] values=new Reading[0];
 public int Width{get{return (int)((values.Length==0?138:Math.Min(values.Length,6)*70+6)*scale);}}public int Height{get{return (int)(26*scale);}}
 public int Left{get{return 0;}}public int Top{get{return 0;}}public Point Location{get{return Point.Empty;}set{}}
 public bool Visible{get{return visible;}set{visible=value;if(!value && Handle!=IntPtr.Zero)ShowWindow(Handle,0);else Place();}}
 public TaskbarView(IntPtr parent,Action openWindow,ContextMenuStrip context){taskbar=parent;open=openWindow;menu=context;timer.Interval=1000;timer.Tick+=(s,e)=>Place();timer.Start();}
 public void Show(){Visible=true;}
 public void UpdateValues(Reading[] r){values=r;Place();}
 public void SaveVerification(string path){
  Rect r;GetWindowRect(Handle,out r);
  System.IO.File.WriteAllText(path,"IsWindow="+IsWindow(Handle)+"\nVisible="+IsWindowVisible(Handle)+"\nParentIsTaskbar="+(GetParent(Handle)==taskbar)+"\nRectangle="+r.Left+","+r.Top+","+r.Right+","+r.Bottom);
  using(var b=DrawStrip(values,2,false))b.Save(path+".png");
 }
 bool LightTheme(){try{using(var key=Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize")){return key!=null && Convert.ToInt32(key.GetValue("SystemUsesLightTheme",0))!=0;}}catch{return false;}}
 void Place(){
  if(disposed || !visible)return;
  IntPtr bar=taskbar;IntPtr tray=FindWindowEx(bar,IntPtr.Zero,"TrayNotifyWnd",null);if(tray==IntPtr.Zero)tray=FindWindowEx(bar,IntPtr.Zero,"ClockButton",null);
  if(bar==IntPtr.Zero || !IsWindow(bar)){if(Handle!=IntPtr.Zero)ShowWindow(Handle,0);return;}
  if(Handle==IntPtr.Zero || !IsWindow(Handle) || GetParent(Handle)!=bar){
   if(Handle!=IntPtr.Zero){if(IsWindow(Handle))DestroyHandle();else ReleaseHandle();}
   taskbar=bar;var p=new CreateParams{Caption="MyMchose.Taskbar",Parent=bar,Style=0x40000000,ExStyle=0x08080000,Width=124,Height=24};CreateHandle(p);
  }
  scale=GetDpiForWindow(bar)/96f;if(scale<=0)scale=1;light=LightTheme();
  Rect tr,br,client;GetWindowRect(bar,out br);GetClientRect(bar,out client);if(tray!=IntPtr.Zero)GetWindowRect(tray,out tr);else{ProbeClock(bar,br);tr=new Rect{Left=br.Right-(trayFromRight.HasValue?trayFromRight.Value:(int)(130*scale)),Top=br.Top};}
  if(br.Bottom-br.Top>br.Right-br.Left){ShowWindow(Handle,0);return;}
  var pos=new PointN(tr.Left,br.Top);ScreenToClient(bar,ref pos);
  int x=Math.Max(0,pos.X-Width-(int)(4*scale)),y=Math.Max(0,(client.Bottom-Height)/2);
  PaintBitmap();SetWindowPos(Handle,IntPtr.Zero,x,y,Width,Height,0x10 | (IsWindowVisible(bar)?0x40u:0x80u));
 }
 void PaintBitmap(){
  using(var bitmap=DrawStrip(values,scale,light)){
   IntPtr screen=GetDC(IntPtr.Zero),dc=CreateCompatibleDC(screen),bits;
   var info=new BitmapInfo{Size=40,Width=bitmap.Width,Height=-bitmap.Height,Planes=1,Bits=32};
   IntPtr hb=CreateDIBSection(screen,ref info,0,out bits,IntPtr.Zero,0);
   if(hb==IntPtr.Zero){DeleteDC(dc);ReleaseDC(IntPtr.Zero,screen);return;}
   var data=bitmap.LockBits(new Rectangle(0,0,bitmap.Width,bitmap.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppPArgb);
   try{byte[] row=new byte[bitmap.Width*4];for(int y=0;y<bitmap.Height;y++){Marshal.Copy(IntPtr.Add(data.Scan0,y*data.Stride),row,0,row.Length);Marshal.Copy(row,0,IntPtr.Add(bits,y*row.Length),row.Length);}}finally{bitmap.UnlockBits(data);}
   IntPtr old=SelectObject(dc,hb);
   try{var size=new SizeN(bitmap.Width,bitmap.Height);var source=new PointN(0,0);var blend=new Blend{Alpha=255,Format=1};if(!UpdateLayeredWindow(Handle,screen,IntPtr.Zero,ref size,dc,ref source,0,ref blend,2))System.Diagnostics.Debug.WriteLine("UpdateLayeredWindow: "+Marshal.GetLastWin32Error());}
   finally{SelectObject(dc,old);DeleteObject(hb);DeleteDC(dc);ReleaseDC(IntPtr.Zero,screen);}
  }
 }
 internal static Bitmap DrawStrip(Reading[] values,float scale,bool light){
  int width=(int)((values.Length==0?138:Math.Min(values.Length,6)*70+6)*scale),height=(int)(26*scale);
  using(var high=new Bitmap(width*4,height*4,PixelFormat.Format32bppPArgb)){
  using(var g=Graphics.FromImage(high)){
   g.Clear(Color.Transparent);g.ScaleTransform(scale*4,scale*4);g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.HighQuality;g.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.AntiAlias;
   using(var font=new Font("Segoe UI Variable Small",12.5f,FontStyle.Regular,GraphicsUnit.Pixel)){
    if(values.Length==0)using(var b=new SolidBrush(Color.Gray))g.DrawString("Нет устройств",font,b,3,5);
    for(int i=0;i<Math.Min(values.Length,6);i++){
     float x=3+i*70;var r=values[i];bool stale=r.IsStale;Color color=stale?Color.FromArgb(145,145,145):r.Percent<=15?Color.FromArgb(240,102,102):light?Color.FromArgb(30,30,30):Color.FromArgb(239,242,247);
     using(var pen=new Pen(color,1.3f))using(var brush=new SolidBrush(color)){
      pen.StartCap=pen.EndCap=System.Drawing.Drawing2D.LineCap.Round;pen.LineJoin=System.Drawing.Drawing2D.LineJoin.Round;
      using(var path=new System.Drawing.Drawing2D.GraphicsPath()){
       if(r.Kind==DeviceKind.Headset){
        path.AddBezier(x+1,14,x-0.5f,3,x+15.5f,3,x+14,14);g.DrawPath(pen,path);path.Reset();
        path.AddArc(x+0.5f,12,4,8,180,90);path.AddArc(x+0.5f,12,4,8,270,90);path.AddArc(x+0.5f,12,4,8,0,90);path.AddArc(x+0.5f,12,4,8,90,90);path.CloseFigure();g.DrawPath(pen,path);path.Reset();
        path.AddArc(x+10.5f,12,4,8,180,90);path.AddArc(x+10.5f,12,4,8,270,90);path.AddArc(x+10.5f,12,4,8,0,90);path.AddArc(x+10.5f,12,4,8,90,90);path.CloseFigure();g.DrawPath(pen,path);
       }else if(r.Kind==DeviceKind.Mouse){
        path.AddBezier(x+2,11,x+2,3,x+14,3,x+14,11);path.AddLine(x+14,11,x+14,16);path.AddBezier(x+14,16,x+14,23,x+2,23,x+2,16);path.CloseFigure();g.DrawPath(pen,path);
        g.DrawLine(pen,x+8,7.5f,x+8,11);
       }else{g.DrawRectangle(pen,x,8,16,11);g.DrawLine(pen,x+4,12,x+12,12);g.DrawLine(pen,x+4,15,x+12,15);}
      }
      string text=values.Length>6 && i==5?"+"+(values.Length-5):(r.Percent.HasValue?r.Percent+"%":"—");
      using(var format=(StringFormat)StringFormat.GenericTypographic.Clone()){
       format.FormatFlags|=StringFormatFlags.NoClip;
       // Outline glyphs retain smooth coverage on a transparent layered window.
       using(var glyphs=new System.Drawing.Drawing2D.GraphicsPath()){glyphs.AddString(text,font.FontFamily,(int)font.Style,font.Size,new PointF(x+23,5.3f),format);g.FillPath(brush,glyphs);}
      }
      if(stale)g.FillEllipse(brush,x+16,19,2,2);else if(r.Charging)g.DrawLine(pen,x+20,21,x+43,21);
     }
    }
   }
  }
  var bitmap=new Bitmap(width,height,PixelFormat.Format32bppPArgb);
  using(var g=Graphics.FromImage(bitmap))using(var attrs=new ImageAttributes()){
   g.CompositingMode=System.Drawing.Drawing2D.CompositingMode.SourceCopy;g.CompositingQuality=System.Drawing.Drawing2D.CompositingQuality.HighQuality;g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
   attrs.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);g.DrawImage(high,new Rectangle(0,0,width,height),0,0,high.Width,high.Height,GraphicsUnit.Pixel,attrs);
  }return bitmap;}
 }
 protected override void WndProc(ref Message m){
  if(m.Msg==0x201){open();return;}
  if(m.Msg==0x205){menu.Show(Cursor.Position);return;}
  if(m.Msg==0x21){m.Result=new IntPtr(3);return;}
  if(m.Msg==0x84){m.Result=new IntPtr(1);return;}
  base.WndProc(ref m);
 }
 public void Dispose(){if(disposed)return;disposed=true;timer.Stop();timer.Dispose();if(Handle!=IntPtr.Zero){if(IsWindow(Handle))DestroyHandle();else ReleaseHandle();}}
}
