using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using InfinityScript;
using System.IO;

namespace LSDGobalBan
{
    public class LSDGobalBan : BaseScript
    {
        List<Entity> Entitys = new List<Entity>();

        public string server = "lgbs.codeascript.de";
        public int port = 21965;
        public string password = "";
        public string nowbanmessage = "You have a LSD Global Ban!";
        public string connectedbannmessage = "You have a LSD Global Ban!";
        public string whitlistmessage = "You are not on the WhiteList!";
        public string whitelist = "false";

        List<string> Admins = new List<string>();


        public LSDGobalBan()
            : base()
        {
            server = getDvar<string>("LGBS_hostname");
            port = getDvar<int>("LGBS_port");
            password = getDvar<string>("LGBS_password");
            nowbanmessage = getDvar<string>("LGBS_NowBanMessage");
            connectedbannmessage = getDvar<string>("LGBS_ReconnectBanMessage");
            whitlistmessage = getDvar<string>("LGBS_WhiteListMessage");
            whitelist = getDvar<string>("LGBS_aktivate_whitelist");
            hashadmins();
            base.PlayerConnected += connected;
            base.PlayerDisconnected += discconnect;
        }

        public void connected(Entity player)
        {
            Entitys.Add(player);
            int hiho = 0;
            player.OnInterval(2000, (entity) =>
            {
                if (hiho == 0)
                {
                    hiho = 1;
                    return true;
                }
                else if (hiho == 1)
                {
                    if (whitelist == "true")
                    {
                        TcpClient client = new TcpClient(server, port);
                        StreamWriter w = new StreamWriter(client.GetStream());
                        StreamReader r = new StreamReader(client.GetStream());
                        w.WriteLine("wl|" + entity.GUID.ToString() + "|" + entity.Name.ToString().Replace("|", "<strich>") + "|" + entity.IP.ToString().Split(new char[] { ':' })[0]);
                        w.Flush();
                        string s = r.ReadLine();
                        //Log.Write(LogLevel.All, s);
                        if (s == "no")
                        {

                        }
                        else if (s == "wlyes")
                        {
                            Entitys.Remove(entity);
                            Utilities.ExecuteCommand("dropclient " + player.Call<int>("getentitynumber", new Parameter[0]) + " \"" + whitlistmessage + "\"");
                        }
                        else
                        {
                            Entitys.Remove(entity);
                            Utilities.ExecuteCommand("dropclient " + player.Call<int>("getentitynumber", new Parameter[0]) + " \"" + connectedbannmessage + "\"");
                        }
                        client.Close();
                    }
                    else
                    {
                        TcpClient client = new TcpClient(server, port);
                        StreamWriter w = new StreamWriter(client.GetStream());
                        StreamReader r = new StreamReader(client.GetStream());
                        w.WriteLine(entity.GUID.ToString() + "|" + entity.Name.ToString().Replace("|", "<strich>") + "|" + entity.IP.ToString().Split(new char[] { ':' })[0]);
                        w.Flush();
                        string s = r.ReadLine();
                        //Log.Write(LogLevel.All, s);
                        if (s == "no")
                        {

                        }
                        else
                        {
                            Entitys.Remove(entity);
                            Utilities.ExecuteCommand("dropclient " + player.Call<int>("getentitynumber", new Parameter[0]) + " \"" + connectedbannmessage + "\"");
                        }
                        client.Close();
                    }
                    if (IsEntetyAdmin(player))
                    {
                        string text4 = "^0L^1S^3D^2-GlobalBanSystemAdmin ^1<admin>^2 Connected!";
                        text4 = text4.Replace("<admin>", player.Name.ToString());
                        Utilities.SayAll("^0L^1S^3D^7-^2GlobalBanSystem", text4);
                    }
                    hiho = 2;
                    return true;

                }
                else if (hiho == 40)
                {
                    string text4 = "Hello, <admin> it give a ReportSystem with the Chat Command !report [PART OF NAME] [REASON]";
                    text4 = text4.Replace("<admin>", player.Name.ToString());
                    Utilities.SayAll("^0L^1S^3D^7-^2GlobalBanSystem", text4);
                    hiho = hiho + 1;
                    return true;
                }
                else if (hiho == 41)
                {
                    return false;
                }
                else
                {

                    hiho = hiho + 1;
                    return true;
                }
                //return true;
            });
        }

        public void discconnect(Entity player)
        {
            Entitys.Remove(player);
        }

        public override EventEat OnSay3(Entity player, ChatType type, string name, ref string message)
        {
            if (message.StartsWith("!gban"))
            {
                string[] array = message.Split(new char[]
			    {
				    ' '
			    });
                ban(array, player);
                return EventEat.EatGame;
            }
            if (message.StartsWith("!LGBS"))
            {
                tell(player, "^2The Server used Version ^3 1.0.0.2");
                return EventEat.EatGame;
            }
            if (message.StartsWith("!gkick"))
            {
                string[] array = message.Split(new char[]
			    {
				    ' '
			    });
                kick(array, player);
                return EventEat.EatGame;
            }
            if (message.Equals("!report"))
            {
                tell(player, "!report [Name] [reason]");
                return EventEat.EatGame;
            }
            if (message.StartsWith("!report"))
            {
                try
                {
                    string[] array = message.Split(new char[]
			        {
				        ' '
			        });
                    string text = "";
                    for (int i = 2; i < array.Length; i++)
                    {
                        text = text + " " + array[i];
                    }

                    Entity reportplayer = FindEntByName(array[1]);
                    report(reportplayer, player,text);
                }
                catch (Exception ex)
                {

                }
                return EventEat.EatGame;
            }
            return EventEat.EatNone;

        }
        public bool IsEntetyAdmin(Entity player)
        {
            bool isadmin = false;
            string[] lines = System.IO.File.ReadAllLines(System.IO.Directory.GetCurrentDirectory() + "\\admin.list");
            foreach (string line_loopVariable in lines)
            {
                string line = line_loopVariable;
                if (line == player.GUID.ToString())
                {
                    isadmin = true;
                }
            }
            foreach (string getc in Admins)
            {
                if (getc == player.GUID.ToString())
                {
                    isadmin = true;
                }
            }
            return isadmin;
        }

        public void hashadmins()
        {
            TcpClient client = new TcpClient(server, port);
            StreamReader r = new StreamReader(client.GetStream());
            StreamWriter w = new StreamWriter(client.GetStream());
            w.WriteLine("admins");
            w.Flush();
            string geta = r.ReadLine();
            string[] getb = geta.Split(new char[]
			    {
				    '|'
			    });
            foreach(string getc in getb)
            {
                if (getc == "none" || getc == "" || getc == "admins")
                { }
                else
                {
                    Admins.Add(getc);
                }
            }
        }

        private Entity FindEntByName(string name)
        {
            int num = 0;
            Entity result = null;
            foreach (Entity current in this.Entitys)
            {
                if (0 <= current.Name.ToString().IndexOf(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = current;
                    num++;
                }
            }
            if (num > 1)
            {
                return null;
            }
            if (num == 1)
            {
                return result;
            }
            return null;
        }

        private void report(Entity reportplayer, Entity player,string text)
        {
            if (reportplayer == null)
            {
                tell(player, "^2Dieser Spieler wurde nicht gefunden");
                return;
            }
            System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient(server, port);
            System.IO.StreamWriter w = new System.IO.StreamWriter(client.GetStream());
            w.WriteLine("report|" + reportplayer.Name.ToString().Replace("|", "<strich>") + "[" + reportplayer.GUID + "/" + reportplayer.IP.ToString().Split(new char[] { ':' })[0] + "] wurde von " + player.Name.ToString().Replace("|", "<strich>") + "[" + player.GUID + "/" + player.IP.ToString().Split(new char[] { ':' })[0] + "] wegen \"" + text.Replace("|", "<strich>") + "\" reportet! Server: " + Call<string>("getdvar", "sv_hostname").Replace("|", "<strich>"));// Call<string>("getdvar", "sv_hostname") + "<->" + reportplayer.Name.ToString() + "[" + reportplayer.GUID.ToString() + "] wurde von " + player.Name.ToString() + "[" + player.GUID.ToString() + "] wegen >" + text + "< reportet");
            w.Flush();
            client.Close();
            string text4 = "^1<cheater>^2 wurde wegen ^1<reason>^2 reportet!";
            text4 = text4.Replace("<cheater>", reportplayer.Name.ToString());
            text4 = text4.Replace("<reason>", text);
            //Utilities.ExecuteCommand("say " + text4);
            //Utilities.SayAll("", text4);
            Utilities.RawSayAll(text4);
            return;
        }

        private void ban(string[] array, Entity player)
        {

            if (IsEntetyAdmin(player))
            {
                if (array[1] == null || array[2] == null)
                {
                    tell(player, "!gban [USER] [reason]");
                }
                else
                {
                    try
                    {
                        Entity badplayer = FindEntByName(array[1]);
                        if (badplayer == null)
                        {
                            tell(player, "^2Diese Spieler ist nicht vorhanden.");
                            return;
                        }

                        TcpClient client = new TcpClient(server, port);
                            StreamWriter w = new StreamWriter(client.GetStream());
                            StreamReader r = new StreamReader(client.GetStream());
                            string text = "";
                            for (int i = 2; i < array.Length; i++)
                            {
                                text = text + " " + array[i];
                            }
                            w.WriteLine("add|" + password + "|" + badplayer.GUID + "|" + badplayer.Name.ToString().Replace("|", "<strich>") + "|" + badplayer.IP.ToString().Split(new char[] { ':' })[0] + "|" + text + "|" + player.Name.ToString() + "[" + player.GUID.ToString() + "]");
                            w.Flush();
                            string s = r.ReadLine();
                            client.Close();
                            Utilities.ExecuteCommand("dropclient " + badplayer.Call<int>("getentitynumber", new Parameter[0]) + " \"" + nowbanmessage + "\"");
                            string text4 = "^2<playername> ^3has been GobalBanned ^7by ^1<kicker>";
                            if (text == "")
                            {
                                text4 = "^2<playername> ^3has been GobalBanned ^7by ^1<kicker>";
                            }
                            else
                            {
                                text4 = "^2<playername> ^3has been GobalBanned ^7by ^1<kicker> ^2reason:^7<reason>";
                            }
                            text4 = text4.Replace("<playername>", badplayer.Name.ToString());
                            text4 = text4.Replace("<kicker>", player.Name.ToString());
                            text4 = text4.Replace("<reason>", text);
                            Utilities.SayAll("^0L^1S^3D^7-GlobalBanSystem", text4);
                            if (s == "OK")
                            {
                                tell(player, "Der Server hat den Eintrag erfolgreich übernommen!");
                            }
                            else
                            {
                                tell(player, "Der Server hat den Eintrag ^1nicht^7 erfolgreich übernommen!");
                            }                        
                    }
                    catch (Exception ex)
                    {
                        Log.Write(LogLevel.All, ex.ToString());
                    }
                }
            }
            else
            {
                tell(player, "willst du den Ban haben?");
                
            }
            return;
        }

        private void kick(string[] array, Entity player)
        {

            if (IsEntetyAdmin(player))
            {
                if (array[1] == null || array[2] == null)
                {
                    tell(player, "!gkick [USER] [reason]");
                }
                else
                {
                    try
                    {
                        Entity badplayer = FindEntByName(array[1]);
                        if (badplayer == null)
                        {
                            tell(player, "^2Diese Spieler ist nicht vorhanden.");
                            return;
                        }

                        string text = "";
                        for (int i = 2; i < array.Length; i++)
                        {
                            text = text + " " + array[i];
                        }
                        Utilities.ExecuteCommand("dropclient " + badplayer.Call<int>("getentitynumber", new Parameter[0]) + " \"^2LGBS-Kick:^7" + text + "\"");
                        string text4 = "^2<playername> ^3has been kicked ^7by ^1<kicker>";
                        if (text == "")
                        {
                            text4 = "^2<playername> ^3has been kicked ^7by ^1<kicker>";
                        }
                        else
                        {
                            text4 = "^2<playername> ^3has been kicked ^7by ^1<kicker> ^2reason:^7<reason>";
                        }
                        text4 = text4.Replace("<playername>", badplayer.Name.ToString());
                        text4 = text4.Replace("<kicker>", player.Name.ToString());
                        text4 = text4.Replace("<reason>", text);
                        Utilities.SayAll("^0L^1S^3D^2-GlobalBanSystem", text4);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(LogLevel.All, ex.ToString());
                    }
                }
            }
            else
            {
                tell(player, "willst du den kick haben?");

            }
            return;
        }

        private void tell(Entity player, string message)
        {
            Utilities.SayTo(player, "^0L^1S^3D^2-GlobalBanSystem", message);
        }
        private void setDvar(string dvar, object value)
        {
            Call("setdvar", dvar, value.ToString());
        }
        private T getDvar<T>(string dvar)
        {
            // would switch work here? - no
            if (typeof(T) == typeof(int))
            {
                return Call<T>("getdvarint", dvar);
            }
            else if (typeof(T) == typeof(float))
            {
                return Call<T>("getdvarfloat", dvar);
            }
            else if (typeof(T) == typeof(string))
            {
                return Call<T>("getdvar", dvar);
            }
            else if (typeof(T) == typeof(Vector3))
            {
                return Call<T>("getdvarvector", dvar);
            }
            else
            {
                return default(T);
            }
        }

    }
}
