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
 static void SaveImage(Bitmap bitmap,string path){string full=Path.GetFullPath(path);Directory.CreateDirectory(Path.GetDirectoryName(full));bitmap.Save(full,System.Drawing.Imaging.ImageFormat.Png);}
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
  if(args.Length>0 && args[0]=="--preview"){
   using(var f=new Dashboard(true)){f.Show();f.Refresh();using(var b=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(b,new Rectangle(Point.Empty,b.Size));SaveImage(b,args[1]);}f.Close();}return 0;
  }
  bool created;using(var mutex=new Mutex(true,"Local\\MyMchoseBatteryPersonal",out created)){
   if(!created){MessageBox.Show("Программа уже работает. Открой её через значок с зарядом в трее.","YourBattery");return 0;}
   Directory.CreateDirectory(Data);var window=new Dashboard(false);
   if(args.Length>1 && (args[0]=="--smoke" || args[0]=="--monitor-test")){
    var finish=new System.Windows.Forms.Timer{Interval=6500};finish.Tick+=(s,e)=>{finish.Stop();using(var b=new Bitmap(window.Width,window.Height)){window.DrawToBitmap(b,new Rectangle(Point.Empty,b.Size));SaveImage(b,args[1]);}if(args[0]=="--monitor-test")window.VerifyMonitors();window.Finish();finish.Dispose();};finish.Start();
   }
   Application.Run(window);return 0;
  }
 }
}
class Dashboard:Form {
 internal static Color Bg=Color.FromArgb(18,22,29),PanelBg=Color.FromArgb(28,34,44),Muted=Color.FromArgb(148,161,180),Accent=Color.FromArgb(111,226,190);
 Reading[] values=new Reading[0];
 readonly FlowLayoutPanel cards=new FlowLayoutPanel();
 readonly NotifyIcon[] tray=new NotifyIcon[1];readonly System.Collections.Generic.HashSet<string> warned=new System.Collections.Generic.HashSet<string>();
 readonly System.Windows.Forms.Timer timer=new System.Windows.Forms.Timer();
 readonly Label footer;readonly Button refresh;readonly Strip strip;
 bool busy,quitting,preview;ContextMenuStrip menu;ToolStripMenuItem widgetItem,startItem,monitorItem;DateTime nextPoll=DateTime.MinValue;
 internal static Label TextLabel(string text,int x,int y,int w,int h,float size,Color color,FontStyle style){return new Label{Text=text,Location=new Point(x,y),Size=new Size(w,h),ForeColor=color,BackColor=Color.Transparent,Font=new Font("Segoe UI",size,style)};}
 internal static Button MakeButton(string text,int x,int y,int w){var b=new Button{Text=text,Location=new Point(x,y),Size=new Size(w,34),FlatStyle=FlatStyle.Flat,BackColor=PanelBg,ForeColor=Color.White,Cursor=Cursors.Hand,Font=new Font("Segoe UI",9)};b.FlatAppearance.BorderColor=Color.FromArgb(58,71,88);return b;}
 public Dashboard(bool isPreview){
  preview=isPreview;Text="YourBattery";ClientSize=new Size(510,414);BackColor=Bg;ForeColor=Color.White;Font=new Font("Segoe UI",10);FormBorderStyle=FormBorderStyle.FixedSingle;MaximizeBox=false;StartPosition=FormStartPosition.CenterScreen;AutoScaleMode=AutoScaleMode.None;
  Controls.Add(TextLabel("YourBattery",26,20,440,36,22,Color.White,FontStyle.Bold));
  Controls.Add(TextLabel("Заряд устройств под рукой.",28,62,450,24,10,Muted,FontStyle.Regular));
  cards.Location=new Point(26,103);cards.Size=new Size(458,226);cards.FlowDirection=FlowDirection.TopDown;cards.WrapContents=false;cards.AutoScroll=true;Controls.Add(cards);
  refresh=MakeButton("Обновить",26,341,110);refresh.Click+=(s,e)=>Poll();Controls.Add(refresh);
  var hide=MakeButton("Свернуть в трей",147,341,160);hide.Click+=(s,e)=>Hide();Controls.Add(hide);
  var settings=MakeButton("Настройки",318,341,166);settings.Click+=(s,e)=>menu.Show(settings,new Point(0,settings.Height));Controls.Add(settings);
  footer=TextLabel("Обновление каждые 30 с · предупреждение при ≤ 15%",27,386,465,20,8,Muted,FontStyle.Regular);Controls.Add(footer);
  if(preview){values=new[]{new Reading("MCHOSE X9"){Id="preview-x9",Kind=DeviceKind.Headset,Percent=40,State="Подключена",Time=DateTime.Now},new Reading("MCHOSE AX5 PRO MAX"){Id="preview-ax5",Kind=DeviceKind.Mouse,Percent=77,State="Подключена",Time=DateTime.Now},new Reading("Пример клавиатуры"){Id="preview-kb",Kind=DeviceKind.Keyboard,Percent=65,State="Демонстрация интерфейса",Time=DateTime.Now}};Render();return;}
  menu=new ContextMenuStrip();menu.Items.Add("Открыть",null,(s,e)=>Open());menu.Items.Add("Обновить",null,(s,e)=>Poll());
  widgetItem=new ToolStripMenuItem("Заряд на панели задач"){Checked=true,CheckOnClick=true};menu.Items.Add(widgetItem);
  startItem=new ToolStripMenuItem("Запускать с Windows"){CheckOnClick=true};menu.Items.Add(startItem);
  using(var key=Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run")){startItem.Checked=key!=null && key.GetValue("MyMchose")!=null;}
  startItem.CheckedChanged+=(s,e)=>{try{using(var key=Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run")){if(startItem.Checked)key.SetValue("MyMchose","\""+Application.ExecutablePath+"\" --tray");else key.DeleteValue("MyMchose",false);}}catch(Exception ex){MessageBox.Show(ex.Message,"Автозапуск");}};
  menu.Items.Add("Открыть журнал",null,(s,e)=>{string p=Path.Combine(Program.Data,"status.log");if(File.Exists(p))System.Diagnostics.Process.Start("notepad.exe",p);});menu.Items.Add(new ToolStripSeparator());menu.Items.Add("Выход",null,(s,e)=>{quitting=true;Close();});
  tray[0]=new NotifyIcon{Icon=CommonIcon(),Visible=true,Text="YourBattery",ContextMenuStrip=menu};tray[0].DoubleClick+=(s,e)=>Open();
  strip=new Strip(()=>Open(),menu);widgetItem.CheckedChanged+=(s,e)=>{strip.Visible=widgetItem.Checked;SaveWidget();};
  LoadWidget();
  monitorItem=new ToolStripMenuItem("Мониторы");menu.Items.Insert(3,monitorItem);monitorItem.DropDownOpening+=(s,e)=>BuildMonitorMenu();BuildMonitorMenu();
  timer.Interval=1000;timer.Tick+=(s,e)=>{if(DateTime.UtcNow>=nextPoll)Poll();};
  Shown+=(s,e)=>{if(Environment.GetCommandLineArgs().Contains("--tray"))Hide();if(widgetItem.Checked)strip.Show();timer.Start();Poll();};
  FormClosing+=(s,e)=>{if(!quitting && e.CloseReason==CloseReason.UserClosing){e.Cancel=true;Hide();return;}timer.Stop();foreach(var t in tray){t.Visible=false;var icon=t.Icon;t.Dispose();if(icon!=SystemIcons.Application)icon.Dispose();}strip.Dispose();menu.Dispose();};
  Resize+=(s,e)=>{if(WindowState==FormWindowState.Minimized){Hide();WindowState=FormWindowState.Normal;}};
 }
 void Open(){Show();WindowState=FormWindowState.Normal;Activate();}
 internal void Finish(){if(strip!=null)strip.SaveVerification(Path.Combine(Program.Data,"taskbar-check.txt"));quitting=true;Close();}
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
  if(busy || quitting)return;busy=true;refresh.Enabled=false;footer.Text="Читаю заряд устройств…";nextPoll=DateTime.UtcNow.AddSeconds(30);
  try{
   var lines=new System.Collections.Generic.List<string>();var fresh=await Task.Run(()=>{var c=Hid.Enumerate();foreach(var h in c)if(h.Vid==0x2023 || h.Vid==0x3837)lines.Add(h.ToString());return Devices.Poll(c,s=>lines.Add(s));});
   if(quitting || IsDisposed)return;
   values=fresh;
   foreach(var r in values){
    bool current=!r.IsStale && r.Time.HasValue && (DateTime.Now-r.Time.Value).TotalSeconds<10;
    if(current && r.Percent.HasValue){if(r.Percent>20 || r.Charging)warned.Remove(r.Id);
     if(r.Percent<=15 && !r.Charging && warned.Add(r.Id))tray[0].ShowBalloonTip(6000,"Пора зарядить "+r.Name,"Осталось "+r.Percent+"%",ToolTipIcon.Warning);}
    lines.Add(r.Name+" "+r.Display+" "+r.State);
   }
   File.WriteAllLines(Path.Combine(Program.Data,"status.log"),new[]{DateTime.Now.ToString("O")}.Concat(lines));Render();footer.Text="Обновлено "+DateTime.Now.ToString("HH:mm:ss")+" · каждые 30 с · уведомления при ≤ 15%";
  }catch(Exception ex){if(!quitting && !IsDisposed)footer.Text="Ошибка: "+ex.Message;}
  finally{busy=false;if(!quitting && !IsDisposed)refresh.Enabled=true;}
 }
 void Render(){
  int scroll=cards.VerticalScroll.Value;cards.SuspendLayout();
  while(cards.Controls.Count>0){var old=cards.Controls[0];cards.Controls.Remove(old);old.Dispose();}
  if(values.Length==0)cards.Controls.Add(TextLabel("Поддерживаемые устройства не найдены.\nПодключи устройство и нажми «Обновить».",0,0,430,80,11,Muted,FontStyle.Regular));
  foreach(var r in values){
   var card=new Panel{Size=new Size(435,101),BackColor=PanelBg,Margin=new Padding(0,0,0,12)};cards.Controls.Add(card);
   card.Controls.Add(TextLabel(KindLabel(r.Kind),16,12,280,18,8,Muted,FontStyle.Bold));
   var name=TextLabel(r.Name,16,33,300,26,12,Color.White,FontStyle.Bold);name.AutoEllipsis=true;card.Controls.Add(name);
   card.Controls.Add(TextLabel(r.State+(r.Charging?" · зарядка":""),16,66,300,20,9,Muted,FontStyle.Regular));
   card.Controls.Add(TextLabel(r.Percent.HasValue?r.Percent+"%":"—",315,26,118,40,23,r.IsStale?Muted:r.Percent<=15?Color.Salmon:Accent,FontStyle.Bold));
   var time=TextLabel(r.Time.HasValue?r.Time.Value.ToString("HH:mm:ss"):"Нет данных",310,72,115,18,8,Muted,FontStyle.Regular);time.TextAlign=ContentAlignment.TopRight;card.Controls.Add(time);
  }
  cards.ResumeLayout();cards.VerticalScroll.Value=Math.Min(scroll,cards.VerticalScroll.Maximum);
  if(!preview && tray[0]!=null){string tip=values.Length==0?"YourBattery: нет устройств":string.Join("\n",values.Select(r=>r.Name+": "+r.Display+(r.IsStale?" *":"")));tray[0].Text=tip.Length>63?tip.Substring(0,60)+"…":tip;}
  if(strip!=null)strip.UpdateValues(values);
 }
 static string KindLabel(DeviceKind kind){switch(kind){case DeviceKind.Headset:return "ГАРНИТУРА";case DeviceKind.Mouse:return "МЫШЬ";case DeviceKind.Keyboard:return "КЛАВИАТУРА";case DeviceKind.Controller:return "КОНТРОЛЛЕР";default:return "УСТРОЙСТВО";}}
 [DllImport("user32.dll")]static extern bool DestroyIcon(IntPtr h);
 static Icon CommonIcon(){using(var large=new Bitmap(128,128)){
  using(var g=Graphics.FromImage(large))using(var pen=new Pen(Color.FromArgb(235,240,244),1.5f))using(var fill=new SolidBrush(Accent)){
   g.Clear(Color.Transparent);g.ScaleTransform(4,4);g.SmoothingMode=SmoothingMode.AntiAlias;pen.LineJoin=LineJoin.Round;
   g.DrawRectangle(pen,5,9,21,14);g.DrawLine(pen,28,13,28,19);g.FillRectangle(fill,8,12,6,8);g.FillRectangle(fill,17,12,6,8);
  }
  using(var small=new Bitmap(32,32)){using(var g=Graphics.FromImage(small)){g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawImage(large,0,0,32,32);}IntPtr h=small.GetHicon();try{return (Icon)Icon.FromHandle(h).Clone();}finally{DestroyIcon(h);}}
 }}
}
