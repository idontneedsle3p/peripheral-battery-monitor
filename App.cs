using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;

static class Program {
 public static string Data=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"MyMchose");
 [STAThread] static int Main(string[] args){
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
  if(args.Length>0 && args[0]=="--render-test"){
   foreach(int count in new[]{0,1,3,8,20})foreach(float dpi in new[]{1f,1.25f,2f}){
    var readings=Enumerable.Range(0,count).Select(i=>new Reading("Test "+i){Kind=(DeviceKind)(i%5),Percent=i%2==0?100:0,IsStale=i%3==0}).ToArray();
    using(var bitmap=TaskbarView.DrawStrip(readings,dpi,false))if(bitmap.Width<=0 || bitmap.Height<=0)throw new InvalidOperationException("Invalid rendering size");
   }return 0;
  }
  if(args.Length>0 && args[0]=="--diagnose"){
   try{var lines=new System.Collections.Generic.List<string>();var cols=Hid.Enumerate();foreach(var c in cols)lines.Add(c.ToString());foreach(var r in Devices.Poll(cols,s=>lines.Add(s)))lines.Add(r.Name+" "+r.Display+" "+r.State);File.WriteAllLines(args[1],lines);return 0;}catch(Exception e){File.WriteAllText(args[1],e.ToString());return 1;}
  }
  bool created;using(var mutex=new Mutex(true,"Local\\MyMchoseBatteryPersonal",out created)){
   if(!created)return 0;
   Directory.CreateDirectory(Data);var window=new TrayApplication();
   if(args.Length>1 && (args[0]=="--smoke" || args[0]=="--monitor-test")){
    var finish=new System.Windows.Forms.Timer{Interval=6500};finish.Tick+=(s,e)=>{finish.Stop();window.VerifyTray(args[1]);if(args[0]=="--monitor-test")window.VerifyMonitors();window.Finish();finish.Dispose();};finish.Start();
   }
   Application.Run(window);return 0;
  }
 }
}
class TrayApplication:ApplicationContext {
 static readonly Color Accent=Color.FromArgb(111,226,190);
 Reading[] values=new Reading[0];
 readonly NotifyIcon[] tray=new NotifyIcon[1];readonly System.Collections.Generic.HashSet<string> warned=new System.Collections.Generic.HashSet<string>();
 readonly System.Windows.Forms.Timer timer=new System.Windows.Forms.Timer();
 readonly Strip strip;readonly ToolStripMenuItem refresh=new ToolStripMenuItem("Обновить"),status=new ToolStripMenuItem("Подключение…"){Enabled=false},devicesItem=new ToolStripMenuItem("Устройства");
 bool busy,quitting;ContextMenuStrip menu;ToolStripMenuItem widgetItem,startItem,monitorItem;DateTime nextPoll=DateTime.MinValue;
 public TrayApplication(){
  menu=new ContextMenuStrip();menu.Items.Add(devicesItem);devicesItem.DropDownOpening+=(s,e)=>BuildDeviceMenu();
  menu.Items.Add(status);menu.Items.Add(new ToolStripSeparator());refresh.Click+=(s,e)=>Poll();menu.Items.Add(refresh);
  widgetItem=new ToolStripMenuItem("Заряд на панели задач"){Checked=true,CheckOnClick=true};menu.Items.Add(widgetItem);
  startItem=new ToolStripMenuItem("Запускать с Windows"){CheckOnClick=true};menu.Items.Add(startItem);
  using(var key=Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run")){startItem.Checked=key!=null && key.GetValue("MyMchose")!=null;}
  startItem.CheckedChanged+=(s,e)=>{try{using(var key=Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run")){if(startItem.Checked)key.SetValue("MyMchose","\""+Application.ExecutablePath+"\" --tray");else key.DeleteValue("MyMchose",false);}}catch(Exception ex){MessageBox.Show(ex.Message,"Автозапуск");}};
  menu.Items.Add("Открыть журнал",null,(s,e)=>{string p=Path.Combine(Program.Data,"status.log");if(File.Exists(p))System.Diagnostics.Process.Start("notepad.exe",p);});menu.Items.Add(new ToolStripSeparator());menu.Items.Add("Выход",null,(s,e)=>{ExitThread();});
  tray[0]=new NotifyIcon{Icon=CommonIcon(),Visible=true,Text="YourBattery",ContextMenuStrip=menu};tray[0].DoubleClick+=(s,e)=>ShowMenu();
  strip=new Strip(()=>ShowMenu(),menu);widgetItem.CheckedChanged+=(s,e)=>{strip.Visible=widgetItem.Checked;SaveWidget();};
  LoadWidget();
  monitorItem=new ToolStripMenuItem("Мониторы");menu.Items.Insert(5,monitorItem);monitorItem.DropDownOpening+=(s,e)=>BuildMonitorMenu();BuildMonitorMenu();
  timer.Interval=1000;timer.Tick+=(s,e)=>{if(DateTime.UtcNow>=nextPoll)Poll();};
  // Creating the menu handle installs the WinForms synchronization context for async polling.
  var handle=menu.Handle;
  BuildDeviceMenu();if(widgetItem.Checked)strip.Show();timer.Start();Poll();
 }
 void ShowMenu(){if(!quitting)menu.Show(Cursor.Position);}
 protected override void ExitThreadCore(){
  if(quitting)return;quitting=true;timer.Stop();timer.Dispose();strip.Dispose();
  tray[0].Visible=false;var icon=tray[0].Icon;tray[0].Dispose();icon.Dispose();menu.Dispose();base.ExitThreadCore();
 }
 internal void VerifyTray(string path){
  if(Application.OpenForms.Count!=0)throw new InvalidOperationException("Unexpected application window");
  BuildDeviceMenu();File.WriteAllLines(path,new[]{"OpenForms="+Application.OpenForms.Count,"TrayIcons="+tray.Length,"DeviceCount="+values.Length}.Concat(menu.Items.Cast<ToolStripItem>().Select(i=>i.Text)).Concat(values.Select(r=>r.Name+": "+r.Display)));
 }
 internal void Finish(){strip.SaveVerification(Path.Combine(Program.Data,"taskbar-check.txt"));ExitThread();}
 internal void VerifyMonitors(){
  string saved=strip.Target;strip.Visible=true;strip.Target="all";strip.SaveVerification(Path.Combine(Program.Data,"monitors-all.txt"));
  foreach(var screen in Screen.AllScreens){strip.Target=screen.DeviceName;strip.SaveVerification(Path.Combine(Program.Data,"monitor-"+screen.DeviceName.Replace("\\"," ").Replace(".","").Trim()+".txt"));}
  strip.Visible=false;strip.SaveVerification(Path.Combine(Program.Data,"monitors-hidden.txt"));strip.Target=saved;strip.Visible=widgetItem.Checked;
 }
 void SaveWidget(){try{File.WriteAllLines(Path.Combine(Program.Data,"widget.txt"),new[]{widgetItem.Checked.ToString(),"0","0",strip.Target});}catch{}}
 void LoadWidget(){try{var p=File.ReadAllLines(Path.Combine(Program.Data,"widget.txt"));if(p.Length>3)strip.Target=p[3];widgetItem.Checked=bool.Parse(p[0]);}catch{}strip.MoveFinished=SaveWidget;}
 void BuildMonitorMenu(){
  while(monitorItem.DropDownItems.Count>0){var old=monitorItem.DropDownItems[0];monitorItem.DropDownItems.Remove(old);old.Dispose();}
  AddMonitorChoice("Все мониторы","all");AddMonitorChoice("Только основной","primary");monitorItem.DropDownItems.Add(new ToolStripSeparator());
  var screens=Screen.AllScreens.OrderBy(s=>s.DeviceName).ToArray();
  for(int i=0;i<screens.Length;i++){var s=screens[i];AddMonitorChoice("Монитор "+(i+1)+" · "+s.Bounds.Width+"×"+s.Bounds.Height+(s.Primary?" · основной":"")+" ("+s.DeviceName.Replace("\\\\.\\","")+")",s.DeviceName);}
  if(strip.Target!="all" && strip.Target!="primary" && !screens.Any(s=>s.DeviceName==strip.Target))monitorItem.DropDownItems.Add(new ToolStripMenuItem("Выбранный монитор отключён"){Enabled=false});
 }
 void AddMonitorChoice(string label,string target){var item=new ToolStripMenuItem(label){Checked=strip.Target==target};item.Click+=(s,e)=>{strip.Target=target;SaveWidget();BuildMonitorMenu();};monitorItem.DropDownItems.Add(item);}
 async void Poll(){
  if(busy || quitting)return;busy=true;refresh.Enabled=false;status.Text="Читаю заряд устройств…";nextPoll=DateTime.UtcNow.AddSeconds(30);
  try{
   var lines=new System.Collections.Generic.List<string>();var fresh=await Task.Run(()=>{var c=Hid.Enumerate();foreach(var h in c)if(h.Vid==0x2023 || h.Vid==0x3837)lines.Add(h.ToString());return Devices.Poll(c,s=>lines.Add(s));});
   if(quitting)return;
   values=fresh;
   foreach(var r in values){
    bool current=!r.IsStale && r.Time.HasValue && (DateTime.Now-r.Time.Value).TotalSeconds<10;
    if(current && r.Percent.HasValue){if(r.Percent>20 || r.Charging)warned.Remove(r.Id);
     if(r.Percent<=15 && !r.Charging && warned.Add(r.Id))tray[0].ShowBalloonTip(6000,"Пора зарядить "+r.Name,"Осталось "+r.Percent+"%",ToolTipIcon.Warning);}
    lines.Add(r.Name+" "+r.Display+" "+r.State);
   }
   File.WriteAllLines(Path.Combine(Program.Data,"status.log"),new[]{DateTime.Now.ToString("O")}.Concat(lines));Render();status.Text="Обновлено "+DateTime.Now.ToString("HH:mm:ss")+" · каждые 30 с · уведомления при ≤ 15%";
  }catch(Exception ex){if(!quitting)status.Text="Ошибка: "+ex.Message;}
  finally{busy=false;if(!quitting)refresh.Enabled=true;}
 }
 void Render(){
  string tip=values.Length==0?"YourBattery: нет устройств":string.Join("\n",values.Select(r=>r.Name+": "+r.Display+(r.IsStale?" *":"")));tray[0].Text=tip.Length>63?tip.Substring(0,60)+"…":tip;
  BuildDeviceMenu();strip.UpdateValues(values);
 }
 void BuildDeviceMenu(){
  while(devicesItem.DropDownItems.Count>0){var old=devicesItem.DropDownItems[0];devicesItem.DropDownItems.Remove(old);old.Dispose();}
  if(values.Length==0)devicesItem.DropDownItems.Add(new ToolStripMenuItem("Поддерживаемые устройства не найдены"){Enabled=false});
  foreach(var r in values){var item=new ToolStripMenuItem(r.Name+" · "+r.Display+(r.IsStale?" · старое значение":""));
   item.DropDownItems.Add(new ToolStripMenuItem(r.State){Enabled=false});item.DropDownItems.Add(new ToolStripMenuItem(r.Transport){Enabled=false});
   item.DropDownItems.Add(new ToolStripMenuItem(r.Time.HasValue?"Последний замер: "+r.Time.Value.ToString("HH:mm:ss"):"Нет замеров"){Enabled=false});devicesItem.DropDownItems.Add(item);}
 }
 [DllImport("user32.dll")]static extern bool DestroyIcon(IntPtr h);
 static Icon CommonIcon(){using(var large=new Bitmap(128,128)){
  using(var g=Graphics.FromImage(large))using(var pen=new Pen(Color.FromArgb(235,240,244),1.5f))using(var fill=new SolidBrush(Accent)){
   g.Clear(Color.Transparent);g.ScaleTransform(4,4);g.SmoothingMode=SmoothingMode.AntiAlias;pen.LineJoin=LineJoin.Round;
   g.DrawRectangle(pen,5,9,21,14);g.DrawLine(pen,28,13,28,19);g.FillRectangle(fill,8,12,6,8);g.FillRectangle(fill,17,12,6,8);
  }
  using(var small=new Bitmap(32,32)){using(var g=Graphics.FromImage(small)){g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawImage(large,0,0,32,32);}IntPtr h=small.GetHicon();try{return (Icon)Icon.FromHandle(h).Clone();}finally{DestroyIcon(h);}}
 }}
}
