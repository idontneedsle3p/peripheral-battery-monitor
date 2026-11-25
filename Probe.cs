using System;
using System.IO;
using System.Text;
class Probe {
 static int Main(string[] args){try{
  var text=new StringBuilder();var colls=Hid.Enumerate();foreach(var c in colls)text.AppendLine(c.ToString());
  if(args.Length>1 && args[1]=="--poll")foreach(var r in Devices.Poll(colls,s=>text.AppendLine(s)))text.AppendLine(r.Name+" "+r.Display+" "+r.State);
  File.WriteAllText(args[0],text.ToString(),Encoding.UTF8);return 0;
 }catch(Exception e){File.WriteAllText(args[0],e.ToString());return 1;}}
}
