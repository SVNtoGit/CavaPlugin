using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using Styx;
using Styx.Helpers;
using Styx.Plugins;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.POI;
using Styx.CommonBot.Profiles;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace CavaPlugin
{
    // ReSharper disable UnusedMember.Global
    public class CavaPlugin : HBPlugin
    // ReSharper restore UnusedMember.Global
    {
        private bool _hasBeenInitialized;
        private bool _hasBeenInitialized2;
        private bool _hasBeenInitialized3;
        private bool _cavaupdated;
        private bool _botRunning = true;
        private bool _gotGuildInvite;
        private bool _gotPartyInvite;
        private bool _gotTradeinvite;
        private bool _gotDuelinvite;
        private bool _erro;
        private int _nVezesBotUnstuck;
        private static Thread _recomecar;
        private Stopwatch _ultimoSemStuck;
        private Stopwatch _summonpettime;
        private Stopwatch _mountedTime;
        private int _refusetime;
        private WoWPoint _ultimoLocal;
        
        private Stopwatch _asLastSavedTimer; 
        private WoWPoint _asLastSavedPosition;
        private bool _asLastSavedPositionTrigger;
        
        private bool _onbotstart = true; 
        private readonly Stopwatch _refuseguildtimer = new Stopwatch();
        private readonly Stopwatch _refusepartytimer = new Stopwatch();
        private readonly Stopwatch _refusetradetimer = new Stopwatch();
        private readonly Stopwatch _refusedueltimer = new Stopwatch();

        //languages
        private static CultureInfo _ci;
        private static readonly string Str = Assembly.GetExecutingAssembly().FullName.Remove(Assembly.GetExecutingAssembly().FullName.IndexOf(','));
        private readonly Assembly _assembly = Assembly.Load(Str);
        private static ResourceManager _rm;
        #region Overrides except pulse
        static readonly SoundPlayer Player = new SoundPlayer();
        public override string Author { get { return "Cava"; } }
        public override Version Version { get { return new Version(4, 2, 0); } }
        public override string Name { get { return "CavaPlugin"; } }
        public override bool WantButton { get { return true; } }
        public override string ButtonText { get { return "Cava Profiles"; } }
 
        public override void OnButtonPress()
        {
            var isRunningantes = TreeRoot.IsRunning;
            if (isRunningantes)
            {
                // ReSharper disable ResourceItemNotResolved
                MessageBox.Show(_rm.GetString("Bot_is_running_stop_bot_before_initiate_Cava_Plugin", _ci), _rm.GetString("error", _ci), MessageBoxButtons.OK, MessageBoxIcon.Error);
                // ReSharper restore ResourceItemNotResolved
                Player.SoundLocation = PathToCavaPlugin + "Sounds\\Error.wav";
                Player.Play();
                return;
            }
            AbreJanela();
            Player.SoundLocation = PathToCavaPlugin + "Sounds\\Close.wav";
            Player.Play();
            //MessageBox.Show("To Start CavaPlugin load profile Cava_Starter_Profiles.xml", "WELCOME TO CAVAPLUGIN", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private const string PbLoadProfile = "dXNpbmcgU3lzdGVtOw0KdXNpbmcgU3lzdGVtLkNvbXBvbmVudE1vZGVsOw0KdXNpbmcgU3lzdGVtLkRyYXdpbmcuRGVzaWduOw0KdXNpbmcgU3lzdGVtLklPOw0KdXNpbmcgU3lzdGVtLk5ldDsNCnVzaW5nIFN5c3RlbS5TZWN1cml0eS5DcnlwdG9ncmFwaHk7DQp1c2luZyBTeXN0ZW0uVGV4dDsNCnVzaW5nIFN5c3RlbS5YbWw7DQovL3VzaW5nIFN5c3RlbS5YbWwuTGlucTsNCnVzaW5nIFN0eXguQ29tbW9uLkhlbHBlcnM7DQp1c2luZyBTdHl4LkNvbW1vbkJvdC5Qcm9maWxlczsNCi8vdXNpbmcgU3R5eC5IZWxwZXJzOw0KdXNpbmcgU3R5eC5UcmVlU2hhcnA7DQoNCm5hbWVzcGFjZSBIaWdoVm9sdHouQ29tcG9zaXRlcw0Kew0KDQoJI3JlZ2lvbiBFbmFibGVQcm9maWxlQWN0aW9uDQoNCiAgICBwdWJsaWMgc2VhbGVkIGNsYXNzIEVuYWJsZVByb2ZpbGVBY3Rpb24gOiBQQkFjdGlvbg0KICAgIHsNCiAgICAgICAgI3JlZ2lvbiBMb2FkUHJvZmlsZVR5cGUgZW51bQ0KDQogICAgICAgIHB1YmxpYyBlbnVtIExvYWRQcm9maWxlVHlwZQ0KICAgICAgICB7DQogICAgICAgICAgICBIb25vcmJ1ZGR5LA0KICAgICAgICAgICAgUHJvZmVzc2lvbmJ1ZGR5DQogICAgICAgIH0NCg0KICAgICAgICAjZW5kcmVnaW9uDQoNCiAgICAgICAgcHJpdmF0ZSByZWFkb25seSBXYWl0VGltZXIgX2xvYWRQcm9maWxlVGltZXIgPSBuZXcgV2FpdFRpbWVyKFRpbWVTcGFuLkZyb21TZWNvbmRzKDUpKTsNCiAgICAgICAgcHJpdmF0ZSBib29sIF9sb2FkZWRQcm9maWxlOw0KDQogICAgICAgIHB1YmxpYyBFbmFibGVQcm9maWxlQWN0aW9uKCkNCiAgICAgICAgew0KICAgICAgICAgICAgUHJvcGVydGllc1siUGF0aCJdID0gbmV3IE1ldGFQcm9wKA0KICAgICAgICAgICAgICAgICJQYXRoIiwNCiAgICAgICAgICAgICAgICB0eXBlb2YgKHN0cmluZyksDQogICAgICAgICAgICAgICAgbmV3IEVkaXRvckF0dHJpYnV0ZSgNCiAgICAgICAgICAgICAgICAgICAgdHlwZW9mIChQcm9wZXJ0eUJhZy5GaWxlTG9jYXRpb25FZGl0b3IpLA0KICAgICAgICAgICAgICAgICAgICB0eXBlb2YgKFVJVHlwZUVkaXRvcikpLA0KICAgICAgICAgICAgICAgIG5ldyBEaXNwbGF5TmFtZUF0dHJpYnV0ZShQYi5TdHJpbmdzWyJBY3Rpb25fQ29tbW9uX1BhdGgiXSkpOw0KDQogICAgICAgICAgICBQcm9wZXJ0aWVzWyJQcm9maWxlVHlwZSJdID0gbmV3IE1ldGFQcm9wKA0KICAgICAgICAgICAgICAgICJQcm9maWxlVHlwZSIsDQogICAgICAgICAgICAgICAgdHlwZW9mIChMb2FkUHJvZmlsZVR5cGUpLA0KICAgICAgICAgICAgICAgIG5ldyBEaXNwbGF5TmFtZUF0dHJpYnV0ZShQYi5TdHJpbmdzWyJBY3Rpb25fTG9hZFByb2ZpbGVBY3Rpb25fUHJvZmlsZVR5cGUiXSkpOw0KDQogICAgICAgICAgICBQcm9wZXJ0aWVzWyJJc0xvY2FsIl0gPSBuZXcgTWV0YVByb3AoDQogICAgICAgICAgICAgICAgIklzTG9jYWwiLA0KICAgICAgICAgICAgICAgIHR5cGVvZiAoYm9vbCksDQogICAgICAgICAgICAgICAgbmV3IERpc3BsYXlOYW1lQXR0cmlidXRlKFBiLlN0cmluZ3NbIkFjdGlvbl9Mb2FkUHJvZmlsZUFjdGlvbl9Jc0xvY2FsIl0pKTsNCg0KICAgICAgICAgICAgUGF0aCA9ICIiOw0KICAgICAgICAgICAgUHJvZmlsZVR5cGUgPSBMb2FkUHJvZmlsZVR5cGUuSG9ub3JidWRkeTsNCiAgICAgICAgICAgIElzTG9jYWwgPSB0cnVlOw0KICAgICAgICB9DQoNCiAgICAgICAgW1BiWG1sQXR0cmlidXRlXQ0KICAgICAgICBwdWJsaWMgTG9hZFByb2ZpbGVUeXBlIFByb2ZpbGVUeXBlDQogICAgICAgIHsNCiAgICAgICAgICAgIGdldCB7IHJldHVybiAoTG9hZFByb2ZpbGVUeXBlKSBQcm9wZXJ0aWVzWyJQcm9maWxlVHlwZSJdLlZhbHVlOyB9DQogICAgICAgICAgICBzZXQgeyBQcm9wZXJ0aWVzWyJQcm9maWxlVHlwZSJdLlZhbHVlID0gdmFsdWU7IH0NCiAgICAgICAgfQ0KDQogICAgICAgIFtQYlhtbEF0dHJpYnV0ZV0NCiAgICAgICAgcHVibGljIHN0cmluZyBQYXRoDQogICAgICAgIHsNCiAgICAgICAgICAgIGdldCB7IHJldHVybiAoc3RyaW5nKSBQcm9wZXJ0aWVzWyJQYXRoIl0uVmFsdWU7IH0NCiAgICAgICAgICAgIHNldCB7IFByb3BlcnRpZXNbIlBhdGgiXS5WYWx1ZSA9IHZhbHVlOyB9DQogICAgICAgIH0NCg0KICAgICAgICBbUGJYbWxBdHRyaWJ1dGVdDQogICAgICAgIHB1YmxpYyBib29sIElzTG9jYWwNCiAgICAgICAgew0KICAgICAgICAgICAgZ2V0IHsgcmV0dXJuIChib29sKSBQcm9wZXJ0aWVzWyJJc0xvY2FsIl0uVmFsdWU7IH0NCiAgICAgICAgICAgIHNldCB7IFByb3BlcnRpZXNbIklzTG9jYWwiXS5WYWx1ZSA9IHZhbHVlOyB9DQogICAgICAgIH0NCg0KICAgICAgICBwdWJsaWMgc3RyaW5nIEFic29sdXRlUGF0aA0KICAgICAgICB7DQogICAgICAgICAgICBnZXQNCiAgICAgICAgICAgIHsNCiAgICAgICAgICAgICAgICBpZiAoIUlzTG9jYWwpDQogICAgICAgICAgICAgICAgICAgIHJldHVybiBQYXRoOw0KDQogICAgICAgICAgICAgICAgcmV0dXJuIHN0cmluZy5Jc051bGxPckVtcHR5KFBiLkN1cnJlbnRQcm9maWxlLlhtbFBhdGgpDQogICAgICAgICAgICAgICAgICAgID8gc3RyaW5nLkVtcHR5DQogICAgICAgICAgICAgICAgICAgIDogU3lzdGVtLklPLlBhdGguQ29tYmluZShTeXN0ZW0uSU8uUGF0aC5HZXREaXJlY3RvcnlOYW1lKFBiLkN1cnJlbnRQcm9maWxlLlhtbFBhdGgpLCBQYXRoKTsNCiAgICAgICAgICAgIH0NCiAgICAgICAgfQ0KDQogICAgICAgIHB1YmxpYyBvdmVycmlkZSBzdHJpbmcgTmFtZQ0KICAgICAgICB7DQogICAgICAgICAgICBnZXQgeyByZXR1cm4gUGIuU3RyaW5nc1siQWN0aW9uX0xvYWRQcm9maWxlQWN0aW9uX05hbWUiXTsgfQ0KICAgICAgICB9DQoNCiAgICAgICAgcHVibGljIG92ZXJyaWRlIHN0cmluZyBUaXRsZQ0KICAgICAgICB7DQogICAgICAgICAgICBnZXQgeyByZXR1cm4gc3RyaW5nLkZvcm1hdCgiezB9OiB7MX0iLCBOYW1lLCBQYXRoKTsgfQ0KICAgICAgICB9DQoNCiAgICAgICAgcHVibGljIG92ZXJyaWRlIHN0cmluZyBIZWxwDQogICAgICAgIHsNCiAgICAgICAgICAgIGdldCB7IHJldHVybiBQYi5TdHJpbmdzWyJBY3Rpb25fTG9hZFByb2ZpbGVBY3Rpb25fSGVscCJdOyB9DQogICAgICAgIH0NCg0KICAgICAgICBwcm90ZWN0ZWQgb3ZlcnJpZGUgUnVuU3RhdHVzIFJ1bihvYmplY3QgY29udGV4dCkNCiAgICAgICAgew0KICAgICAgICAgICAgaWYgKCFJc0RvbmUpDQogICAgICAgICAgICB7DQogICAgICAgICAgICAgICAgaWYgKCFfbG9hZGVkUHJvZmlsZSkNCiAgICAgICAgICAgICAgICB7DQogICAgICAgICAgICAgICAgICAgIGlmIChMb2FkKCkpDQogICAgICAgICAgICAgICAgICAgIHsNCiAgICAgICAgICAgICAgICAgICAgICAgIF9sb2FkUHJvZmlsZVRpbWVyLlJlc2V0KCk7DQogICAgICAgICAgICAgICAgICAgIH0NCiAgICAgICAgICAgICAgICAgICAgX2xvYWRlZFByb2ZpbGUgPSB0cnVlOw0KICAgICAgICAgICAgICAgIH0gLy8gV2UgbmVlZCB0byB3YWl0IGZvciBhIHByb2ZpbGUgdG8gbG9hZCBiZWNhdXNlIHRoZSBwcm9maWxlIG1pZ2h0IGJlIGxvYWRlZCBhc3luY2hyb25vdXNseQ0KICAgICAgICAgICAgICAgIGlmIChfbG9hZFByb2ZpbGVUaW1lci5Jc0ZpbmlzaGVkIHx8DQogICAgICAgICAgICAgICAgICAgICghc3RyaW5nLklzTnVsbE9yRW1wdHkoUHJvZmlsZU1hbmFnZXIuWG1sTG9jYXRpb24pICYmDQogICAgICAgICAgICAgICAgICAgICBQcm9maWxlTWFuYWdlci5YbWxMb2NhdGlvbi5FcXVhbHMoQWJzb2x1dGVQYXRoKSkpDQogICAgICAgICAgICAgICAgew0KICAgICAgICAgICAgICAgICAgICBJc0RvbmUgPSB0cnVlOw0KICAgICAgICAgICAgICAgIH0NCiAgICAgICAgICAgIH0NCiAgICAgICAgICAgIHJldHVybiBSdW5TdGF0dXMuRmFpbHVyZTsNCiAgICAgICAgfQ0KICAgICAgICBwdWJsaWMgc3RyaW5nIERlY3J5cHQoc3RyaW5nIGNpcGhlclRleHQpDQogICAgICAgIHsNCiAgICAgICAgICAgIHZhciBjaXBoZXJCeXRlcyA9IENvbnZlcnQuRnJvbUJhc2U2NFN0cmluZyhjaXBoZXJUZXh0KTsNCiAgICAgICAgICAgIHVzaW5nICh2YXIgZW5jcnlwdG9yID0gQWVzLkNyZWF0ZSgpKQ0KICAgICAgICAgICAgew0KICAgICAgICAgICAgICAgIHZhciBwZGIgPSBuZXcgUmZjMjg5OERlcml2ZUJ5dGVzKEVudmlyb25tZW50LlVzZXJOYW1lLA0KICAgICAgICAgICAgICAgIG5ldyBieXRlW10geyAweDQ5LCAweDc2LCAweDYxLCAweDZlLCAweDIwLCAweDRkLCAweDY1LCAweDY0LCAweDc2LCAweDY1LCAweDY0LCAweDY1LCAweDc2IH0pOw0KICAgICAgICAgICAgICAgIGlmIChlbmNyeXB0b3IgPT0gbnVsbCkgcmV0dXJuIGNpcGhlclRleHQ7DQogICAgICAgICAgICAgICAgZW5jcnlwdG9yLktleSA9IHBkYi5HZXRCeXRlcygzMik7DQogICAgICAgICAgICAgICAgZW5jcnlwdG9yLklWID0gcGRiLkdldEJ5dGVzKDE2KTsNCiAgICAgICAgICAgICAgICB1c2luZyAodmFyIG1zID0gbmV3IE1lbW9yeVN0cmVhbSgpKQ0KICAgICAgICAgICAgICAgIHsNCiAgICAgICAgICAgICAgICAgICAgdXNpbmcgKHZhciBjcyA9IG5ldyBDcnlwdG9TdHJlYW0obXMsIGVuY3J5cHRvci5DcmVhdGVEZWNyeXB0b3IoKSwgQ3J5cHRvU3RyZWFtTW9kZS5Xcml0ZSkpDQogICAgICAgICAgICAgICAgICAgIHsNCiAgICAgICAgICAgICAgICAgICAgICAgIGNzLldyaXRlKGNpcGhlckJ5dGVzLCAwLCBjaXBoZXJCeXRlcy5MZW5ndGgpOw0KICAgICAgICAgICAgICAgICAgICAgICAgY3MuQ2xvc2UoKTsNCiAgICAgICAgICAgICAgICAgICAgfQ0KICAgICAgICAgICAgICAgICAgICBjaXBoZXJUZXh0ID0gRW5jb2RpbmcuVW5pY29kZS5HZXRTdHJpbmcobXMuVG9BcnJheSgpKTsNCiAgICAgICAgICAgICAgICB9DQogICAgICAgICAgICB9DQogICAgICAgICAgICByZXR1cm4gY2lwaGVyVGV4dDsNCiAgICAgICAgfQ0KDQogICAgICAgIHByaXZhdGUgc3RhdGljIHN0cmluZyBHZXRkYXRhKHN0cmluZyBzZXR0aW5nc3BhdGgsIHN0cmluZyBjaGVja2l0KQ0KICAgICAgICB7DQogICAgICAgICAgICB2YXIgZCA9IG5ldyBYbWxEb2N1bWVudCgpOw0KICAgICAgICAgICAgZC5Mb2FkKHNldHRpbmdzcGF0aCk7DQogICAgICAgICAgICB2YXIgbiA9IGQuR2V0RWxlbWVudHNCeVRhZ05hbWUoY2hlY2tpdCk7DQogICAgICAgICAgICByZXR1cm4gblswXSAhPSBudWxsID8gblswXS5Jbm5lclRleHQgOiAiIjsNCiAgICAgICAgfQ0KDQogICAgICAgIHB1YmxpYyBib29sIExvYWQoKQ0KICAgICAgICB7DQogICAgICAgICAgICB2YXIgYWJzUGF0aCA9IEFic29sdXRlUGF0aDsNCg0KICAgICAgICAgICAgaWYgKElzTG9jYWwgJiYgIXN0cmluZy5Jc051bGxPckVtcHR5KFByb2ZpbGVNYW5hZ2VyLlhtbExvY2F0aW9uKSAmJg0KICAgICAgICAgICAgICAgIFByb2ZpbGVNYW5hZ2VyLlhtbExvY2F0aW9uLkVxdWFscyhhYnNQYXRoLCBTdHJpbmdDb21wYXJpc29uLkN1cnJlbnRDdWx0dXJlSWdub3JlQ2FzZSkpDQogICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlOw0KICAgICAgICAgICAgdHJ5DQogICAgICAgICAgICB7DQogICAgICAgICAgICAgICAgUHJvZmVzc2lvbmJ1ZGR5LkRlYnVnKA0KICAgICAgICAgICAgICAgICAgICAiTG9hZGluZyBQcm9maWxlIDp7MH0sIHByZXZpb3VzIHByb2ZpbGUgd2FzIHsxfSIsDQogICAgICAgICAgICAgICAgICAgIFBhdGgsDQogICAgICAgICAgICAgICAgICAgIFByb2ZpbGVNYW5hZ2VyLlhtbExvY2F0aW9uID8/ICJbTm8gUHJvZmlsZV0iKTsNCiAgICAgICAgICAgICAgICBpZiAoc3RyaW5nLklzTnVsbE9yRW1wdHkoUGF0aCkpDQogICAgICAgICAgICAgICAgew0KICAgICAgICAgICAgICAgICAgICBQcm9maWxlTWFuYWdlci5Mb2FkRW1wdHkoKTsNCiAgICAgICAgICAgICAgICB9DQogICAgICAgICAgICAgICAgZWxzZSBpZiAoIUlzTG9jYWwpDQogICAgICAgICAgICAgICAgew0KICAgICAgICAgICAgICAgICAgICBpZiAoUGF0aC5Db250YWlucygiY2F2YXByb2Zlc3Npb25zIikpDQogICAgICAgICAgICAgICAgICAgIHsNCiAgICAgICAgICAgICAgICAgICAgICAgIHZhciBwYXRodG9jYXZhc2V0dGluZ3MgPSBTeXN0ZW0uSU8uUGF0aC5Db21iaW5lKEFwcERvbWFpbi5DdXJyZW50RG9tYWluLkJhc2VEaXJlY3RvcnksIHN0cmluZy5Gb3JtYXQoQCJTZXR0aW5nc1xDYXZhUGx1Z2luXE1haW4tU2V0dGluZ3MueG1sIikpOw0KICAgICAgICAgICAgICAgICAgICAgICAgdmFyIHVybCA9IHN0cmluZy5Gb3JtYXQoImh0dHA6Ly9jYXZhcHJvZmlsZXMub3JnL2luZGV4LnBocD91c2VyPXswfSZwYXNzdz17MX0iLCBHZXRkYXRhKHBhdGh0b2NhdmFzZXR0aW5ncywgIkNwTG9naW4iKSwgRGVjcnlwdChHZXRkYXRhKHBhdGh0b2NhdmFzZXR0aW5ncywgIkNwUGFzc3dvcmQiKSkpOw0KICAgICAgICAgICAgICAgICAgICAgICAgdmFyIHJlcXVlc3QgPSAoSHR0cFdlYlJlcXVlc3QpIFdlYlJlcXVlc3QuQ3JlYXRlKHVybCk7DQogICAgICAgICAgICAgICAgICAgICAgICByZXF1ZXN0LkFsbG93QXV0b1JlZGlyZWN0ID0gZmFsc2U7DQogICAgICAgICAgICAgICAgICAgICAgICByZXF1ZXN0LkNvb2tpZUNvbnRhaW5lciA9IG5ldyBDb29raWVDb250YWluZXIoKTsNCiAgICAgICAgICAgICAgICAgICAgICAgIHZhciByZXNwb25zZSA9IChIdHRwV2ViUmVzcG9uc2UpIHJlcXVlc3QuR2V0UmVzcG9uc2UoKTsNCiAgICAgICAgICAgICAgICAgICAgICAgIHZhciBjb29raWVzID0gcmVxdWVzdC5Db29raWVDb250YWluZXI7DQogICAgICAgICAgICAgICAgICAgICAgICByZXNwb25zZS5DbG9zZSgpOw0KICAgICAgICAgICAgICAgICAgICAgICAgdHJ5DQogICAgICAgICAgICAgICAgICAgICAgICB7DQogICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVxdWVzdCA9DQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIChIdHRwV2ViUmVxdWVzdCkNCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIFdlYlJlcXVlc3QuQ3JlYXRlKFBhdGggKyAiL2ZpbGUiKTsNCiAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXF1ZXN0LkFsbG93QXV0b1JlZGlyZWN0ID0gZmFsc2U7DQogICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVxdWVzdC5Db29raWVDb250YWluZXIgPSBjb29raWVzOw0KICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJlc3BvbnNlID0gKEh0dHBXZWJSZXNwb25zZSkgcmVxdWVzdC5HZXRSZXNwb25zZSgpOw0KICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhciBkYXRhID0gcmVzcG9uc2UuR2V0UmVzcG9uc2VTdHJlYW0oKTsNCiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzdHJpbmcgaHRtbDsNCiAgICAgICAgICAgICAgICAgICAgICAgICAgICB1c2luZyAodmFyIHNyID0gbmV3IFN0cmVhbVJlYWRlcihkYXRhKSkNCiAgICAgICAgICAgICAgICAgICAgICAgICAgICB7DQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGh0bWwgPSBzci5SZWFkVG9FbmQoKTsNCiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9DQogICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVzcG9uc2UuQ2xvc2UoKTsNCiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgcHJvZmlsZXBhdGggPQ0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBuZXcgTWVtb3J5U3RyZWFtKA0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgRW5jb2RpbmcuVVRGOC5HZXRCeXRlcyhFbmNvZGluZy5VVEY4LkdldFN0cmluZyhDb252ZXJ0LkZyb21CYXNlNjRTdHJpbmcoaHRtbCkpKSk7DQogICAgICAgICAgICAgICAgICAgICAgICAgICAgUHJvZmlsZU1hbmFnZXIuTG9hZE5ldyhwcm9maWxlcGF0aCk7DQogICAgICAgICAgICAgICAgICAgICAgICB9DQogICAgICAgICAgICAgICAgICAgICAgICBjYXRjaCAoRXhjZXB0aW9uIGV4KQ0KICAgICAgICAgICAgICAgICAgICAgICAgew0KICAgICAgICAgICAgICAgICAgICAgICAgICAgIFByb2Zlc3Npb25idWRkeS5FcnIoIkRvZXMgbm90IGhhdmUgYWNjZXNzIHRvIFByb2ZpbGUgezB9LiBQbGVhc2UgY2hlY2sgaWYgeW91IGhhdmUgUHJvZmVzc2lvbiBhY2Nlc3MgRXJyb3IgY29kZTogezF9IixQYXRoLCBleCk7DQogICAgICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlOw0KICAgICAgICAgICAgICAgICAgICAgICAgfQ0KICAgICAgICAgICAgICAgICAgICB9DQogICAgICAgICAgICAgICAgICAgIGVsc2UNCiAgICAgICAgICAgICAgICAgICAgew0KICAgICAgICAgICAgICAgICAgICAgICAgdmFyIHJlcSA9IFdlYlJlcXVlc3QuQ3JlYXRlKFBhdGgpOw0KICAgICAgICAgICAgICAgICAgICAgICAgcmVxLlByb3h5ID0gbnVsbDsNCiAgICAgICAgICAgICAgICAgICAgICAgIHVzaW5nIChXZWJSZXNwb25zZSByZXMgPSByZXEuR2V0UmVzcG9uc2UoKSkNCiAgICAgICAgICAgICAgICAgICAgICAgIHsNCiAgICAgICAgICAgICAgICAgICAgICAgICAgICB1c2luZyAodmFyIHN0cmVhbSA9IHJlcy5HZXRSZXNwb25zZVN0cmVhbSgpKQ0KICAgICAgICAgICAgICAgICAgICAgICAgICAgIHsNCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgUHJvZmlsZU1hbmFnZXIuTG9hZE5ldyhzdHJlYW0pOw0KICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0NCiAgICAgICAgICAgICAgICAgICAgICAgIH0NCiAgICAgICAgICAgICAgICAgICAgfQ0KICAgICAgICAgICAgICAgIH0NCiAgICAgICAgICAgICAgICBlbHNlIGlmIChGaWxlLkV4aXN0cyhhYnNQYXRoKSkNCiAgICAgICAgICAgICAgICB7DQogICAgICAgICAgICAgICAgICAgIFByb2ZpbGVNYW5hZ2VyLkxvYWROZXcoYWJzUGF0aCwgdHJ1ZSk7DQogICAgICAgICAgICAgICAgfQ0KICAgICAgICAgICAgICAgIGVsc2UNCiAgICAgICAgICAgICAgICB7DQogICAgICAgICAgICAgICAgICAgIFByb2Zlc3Npb25idWRkeS5FcnIoInswfTogezF9IiwgUGIuU3RyaW5nc1siRXJyb3JfVW5hYmxlVG9GaW5kUHJvZmlsZSJdLCBQYXRoKTsNCiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlOw0KICAgICAgICAgICAgICAgIH0NCiAgICAgICAgICAgIH0NCiAgICAgICAgICAgIGNhdGNoIChFeGNlcHRpb24gZXgpDQogICAgICAgICAgICB7DQogICAgICAgICAgICAgICAgUHJvZmVzc2lvbmJ1ZGR5LkVycigiezB9IiwgZXgpOw0KICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTsNCiAgICAgICAgICAgIH0NCiAgICAgICAgICAgIHJldHVybiB0cnVlOw0KICAgICAgICB9DQoNCiAgICAgICAgcHVibGljIG92ZXJyaWRlIG9iamVjdCBDbG9uZSgpDQogICAgICAgIHsNCiAgICAgICAgICAgIHJldHVybiBuZXcgRW5hYmxlUHJvZmlsZUFjdGlvbiB7UGF0aCA9IFBhdGgsIFByb2ZpbGVUeXBlID0gUHJvZmlsZVR5cGUsIElzTG9jYWwgPSBJc0xvY2FsfTsNCiAgICAgICAgfQ0KDQogICAgICAgIHB1YmxpYyBvdmVycmlkZSB2b2lkIFJlc2V0KCkNCiAgICAgICAgew0KICAgICAgICAgICAgX2xvYWRlZFByb2ZpbGUgPSBmYWxzZTsNCiAgICAgICAgICAgIGJhc2UuUmVzZXQoKTsNCiAgICAgICAgfQ0KICAgIH0NCgkjZW5kcmVnaW9uDQp9";
        // ReSharper disable PossiblyMistakenUseOfParamsMethod
        private readonly string _pathToPbLoadProfile = Path.Combine(Utilities.AssemblyDirectory + @"\Bots\Professionbuddy\Composites\EnableProfileAction.cs");
        private static readonly string PathToCavaPlugin = Path.Combine(Utilities.AssemblyDirectory + @"\Plugins\CavaPlugin\");
        private static readonly string PathToCavaProfiles = Path.Combine(Utilities.AssemblyDirectory + @"\Default Profiles\Cava\");
        private static readonly string PathToCavaQBs = Path.Combine(Utilities.AssemblyDirectory + @"\Quest Behaviors\Cava\");
        // ReSharper restore PossiblyMistakenUseOfParamsMethod
        static bool UpdaterCava(string f, string stuff)
        {
            var p = new Process {StartInfo = {FileName = "TortoiseProc.exe", Arguments = f}};
            try
            {
                p.Start();
                p.WaitForExit();
                if (p.ExitCode == 0)
                {
                    return true;
                }
                switch (stuff)
                {
                    case "AllowUpdate":
                        CPGlobalSettings.Instance.AllowUpdate = false;
                        break;
                    case "PBMiningBlacksmithing":
                        CPGlobalSettings.Instance.PBMiningBlacksmithing = false;
                        break;
                }
            }

            catch (Exception ex)
            {
                // ReSharper disable ResourceItemNotResolved
                Err(_rm.GetString("Unable_to_run_TortoiseSVN", _ci));
                // ReSharper restore ResourceItemNotResolved
                Err("Exception " + ex.Message);
            }
            return false;
        }

        public string Decrypt(string cipherText)
        {
            var cipherBytes = Convert.FromBase64String(cipherText);
            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(Environment.UserName,
                new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                if (encryptor == null) return cipherText;
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
        public override void OnEnable()
        {
            CPGlobalSettings.Instance.Load();
            CPsettings.Instance.Load();
            if (!CPGlobalSettings.Instance.languageselected)
            {
                Form getlanguage = new Language(); 
                getlanguage.ShowDialog();
            }
            switch (CPGlobalSettings.Instance.language)
            {
                default:
                    _ci = new CultureInfo("en-US");
                    _rm = new ResourceManager("Lang", _assembly);
                    break;
                case 0:
                    _ci = new CultureInfo("en-US");
                    _rm = new ResourceManager("Lang", _assembly);
                    break;
                case 1:
                    _ci = new CultureInfo("da");
                    _rm = new ResourceManager("Lang.da", _assembly);
                    break;
                case 2:
                    _ci = new CultureInfo("de");
                    _rm = new ResourceManager("Lang.de", _assembly);
                    break;
                case 3:
                    _ci = new CultureInfo("pt-PT");
                    _rm = new ResourceManager("Lang.pt", _assembly);
                    break;
                case 4:
                    _ci = new CultureInfo("ru-RU");
                    _rm = new ResourceManager("Lang.ru", _assembly);
                    break;
            }
            
            //_ci = new CultureInfo("en-US");
            //_rm = new ResourceManager("Lang", _assembly);
            BotEvents.OnBotStartRequested += _OnBotStart;
            if (!_hasBeenInitialized)
            {
                if (File.Exists(PathToCavaPlugin + "CavaPlugin.ver") || 
                    File.Exists(PathToCavaPlugin + "Cava_Plugin_V3_Updater.ver"))
                {
                    // ReSharper disable ResourceItemNotResolved
                    MessageBox.Show(_rm.GetString("Welcome_to_CavaPlugin", _ci) + Environment.NewLine +  
                        _rm.GetString("Please_download_and_update_your_version_to_latest_one", _ci) + Environment.NewLine +
                        _rm.GetString("This_version_have_some_problems_with_TortoiseSVN", _ci) + Environment.NewLine +
                        _rm.GetString("Download_latest_version_from_HB_Forum", _ci) , _rm.GetString("information", _ci), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Player.SoundLocation = PathToCavaPlugin + "Sounds\\Information.wav";
                    Player.Play();
                    return;
                }
                Debug(_rm.GetString("Loading_CavaPlugin", _ci));
                Debug(_rm.GetString("Please_Wait_While_CavaPlugin_Check_For_Updates", _ci));
                //update Plugin
                if (CavaPluginUpdater.UpdateAvailable("http://cavaplugin.googlecode.com/svn/trunk/", "Plugin.ver"))
                {
                    var newrev =
                    CavaPluginUpdater.GetNewestRev("http://cavaplugin.googlecode.com/svn/trunk/").ToString(CultureInfo.InvariantCulture);
                    Debug(_rm.GetString("Cava_Plugin_Update_to_0_is_available_You_are_on_1", _ci), newrev,CavaPluginUpdater.GetCurrentRev("Plugin.ver").ToString(CultureInfo.InvariantCulture));
                    Debug(_rm.GetString("Starting_update_process", _ci));
                    if (UpdaterCava("/command:\"update\" /path:\"" + PathToCavaPlugin + "\" /closeonend:1", ""))
                    {
                        _cavaupdated = true;
                        CavaPluginUpdater.WriteNewRevFile("Plugin.ver", newrev);
                        Debug(_rm.GetString("is_at_Rev_0_and_up_to_date", _ci), newrev);
                    }
                    else
                    {
                        Err(_rm.GetString("There_is_a_problem_updating", _ci) + " CavaPlugin.");
                        Player.SoundLocation = PathToCavaPlugin + "Sounds\\Error2.wav";
                        Player.Play();
                        _erro = true;
                    }
                }
                if (!Directory.Exists(PathToCavaProfiles) || !Directory.Exists(PathToCavaQBs))
                {
                    MessageBox.Show(_rm.GetString("Theres_an_error_with_Cava_Quest_Behaviors_or_Cava_profiles", _ci),
                        _rm.GetString("information", _ci), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Player.SoundLocation = PathToCavaPlugin + "Sounds\\Information.wav";
                    Player.Play();
                    return;
                }

                //fazer update de quest behaviors
                if (CavaPluginUpdater.UpdateAvailable("http://cavaqbs.googlecode.com/svn/trunk/Cava/",
                                                      "QuestBehaviors.ver"))
                {
                    var newrev =
                        CavaPluginUpdater.GetNewestRev("http://cavaqbs.googlecode.com/svn/trunk/Cava/")
                                         .ToString(CultureInfo.InvariantCulture);
                    Debug("Cava Quest Behaviors " + _rm.GetString("Update_to_0_are_available_You_are_on_1", _ci), newrev,
                          CavaPluginUpdater.GetCurrentRev("QuestBehaviors.ver").ToString(CultureInfo.InvariantCulture));
                    Debug(_rm.GetString("Starting_update_process", _ci));
                    if (UpdaterCava("/command:\"update\" /path:\"" + PathToCavaQBs + "\" /closeonend:1", ""))
                    {
                        _cavaupdated = true;
                        CavaPluginUpdater.WriteNewRevFile("QuestBehaviors.ver", newrev);
                        Debug("Quest Behaviors " + _rm.GetString("are_at_Rev_0_and_up_to_date", _ci), newrev);
                    }
                    else
                    {
                        Err(_rm.GetString("There_is_a_problem_updating", _ci) + " Cava Quest Behaviors.");
                        Player.SoundLocation = PathToCavaPlugin + "Sounds\\Error2.wav";
                        Player.Play();
                        _erro = true;
                    }
                }

                //fazer update de profiles
                if (CavaPluginUpdater.UpdateAvailable("http://cavaprofiles.googlecode.com/svn/trunk/",
                                                      "Profiles.ver"))
                {
                    var newrev = CavaPluginUpdater.GetNewestRev("http://cavaprofiles.googlecode.com/svn/trunk/").ToString(CultureInfo.InvariantCulture);
                    Debug("Quest Profiles " + _rm.GetString("Update_to_0_are_available_You_are_on_1", _ci), newrev,
                          CavaPluginUpdater.GetCurrentRev("Profiles.ver").ToString(CultureInfo.InvariantCulture));
                    Debug(_rm.GetString("Starting_update_process", _ci));
                    if (UpdaterCava("/command:\"update\" /path:\"" + PathToCavaProfiles + "\" /closeonend:1", ""))
                    {
                        CavaPluginUpdater.WriteNewRevFile("Profiles.ver", newrev);
                        Debug("Cava Profiles " + _rm.GetString("are_at_Rev_0_and_up_to_date", _ci), newrev);
                    }
                    else
                    {
                        Err(_rm.GetString("There_is_a_problem_updating", _ci) + " Cava Profiles.");
                        Player.SoundLocation = PathToCavaPlugin + "Sounds\\Error2.wav";
                        Player.Play();
                        _erro = true;
                    }
                }
                //fazer update de Armageddoner
                var url = string.Format("http://cavaprofiles.org/index.php?user={0}&passw={1}", CPGlobalSettings.Instance.CpLogin, Decrypt(CPGlobalSettings.Instance.CpPassword));
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.AllowAutoRedirect = false;
                request.CookieContainer = new CookieContainer();
                var response = (HttpWebResponse)request.GetResponse();
                var cookies = request.CookieContainer;
                response.Close();
                try
                {
                    request =(HttpWebRequest)WebRequest.Create("http://cavaprofiles.org/index.php/profiles/profiles-list/armageddoner/6-armagedonner-user-1/file");
                    request.AllowAutoRedirect = false;
                    request.CookieContainer = cookies;
                    response = (HttpWebResponse) request.GetResponse();
                    response.Close();
                    if (response.StatusCode.ToString() == "OK") //is armageddoner
                    {
                        Log("Armageddoner Access Tested and Passed");
                        CPGlobalSettings.Instance.ArmaPanelBack = true;
                    }
                    else
                    {
                        CPGlobalSettings.Instance.ArmaPanelBack = false;
                        CPsettings.Instance.AntiStuckSystem = false;
                        CPsettings.Instance.CheckAllowSummonPet = false;
                        CPsettings.Instance.guildInvitescheck = false;
                        CPsettings.Instance.refuseguildInvitescheck = false;
                        CPsettings.Instance.refusepartyInvitescheck = false;
                        CPsettings.Instance.refusetradeInvitescheck = false;
                        CPsettings.Instance.refuseduelInvitescheck = false;
                        CPsettings.Instance.RessAfterDie = false;
                    }
                }
                catch (Exception)
                {
                    CPGlobalSettings.Instance.ArmaPanelBack = false;
                }
                //fazer update de PBs
                //mining+blacksmithing 1 to 600
                try
                {
                    request = (HttpWebRequest)WebRequest.Create("http://cavaprofiles.org/index.php/profiles/profiles-list/cavaprofessions/mining/13-miningblacksmithing600/file");
                    request.AllowAutoRedirect = false;
                    request.CookieContainer = cookies;
                    response = (HttpWebResponse)request.GetResponse();
                    response.Close();
                    if (response.StatusCode.ToString() == "OK") //is profession min,bs600
                    {
                        Log("Profession Owner Access Tested and Passed for Mining/Blacksmithing 1 to 600");
                        CPGlobalSettings.Instance.ProfMinBlack600 = true;
                    }
                    else
                    {
                        CPGlobalSettings.Instance.ProfMinBlack600 = false;
                    }
                }
                catch (Exception)
                {
                    CPGlobalSettings.Instance.ProfMinBlack600 = false;
                }
                var fi = new FileInfo(_pathToPbLoadProfile);
                if (!File.Exists(_pathToPbLoadProfile) || fi.Length != 9591)//
                {
                    var file = new StreamWriter(_pathToPbLoadProfile);
                    file.Write(Encoding.UTF8.GetString(Convert.FromBase64String(PbLoadProfile)));
                    file.Close();      
                }/*
                else
                {
                    Log(fi.Length.ToString(CultureInfo.InvariantCulture));
                }*/

                if (!_erro)
                {
                    Debug(_cavaupdated ? _rm.GetString("is_now_up_to_date_Please_reload_HB", _ci) : _rm.GetString("is_up_to_date_and_ready", _ci));
                }
                if (_cavaupdated && CPGlobalSettings.Instance.AutoShutdownWhenUpdate)
                {
                    Debug(_rm.GetString("Auto_Shutdown_in_progress_at", _ci) + " " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    StyxWoW.Sleep(5000);
                    Environment.Exit(0);
                }
                _hasBeenInitialized = true;
                CPGlobalSettings.Instance.Save();
                CPsettings.Instance.Save();   
                _mountedTime = new Stopwatch();
                _summonpettime = new Stopwatch();
                _ultimoSemStuck = new Stopwatch();
                _asLastSavedTimer = new Stopwatch();
            }
            //duplo ignore, bot corre 2 vezes o Initialize
            if (!_hasBeenInitialized2)
            {
                _hasBeenInitialized2 = true;
                return;
            }
            if (!_hasBeenInitialized3)
            {
                _hasBeenInitialized3 = true;
                return;
            }
            AbreJanela();
        }

        public override void OnDisable()
        {
            BotEvents.OnBotStartRequested -= _OnBotStart;
            Log(_rm.GetString("CavaPlugin_Disposed", _ci));
            if (!_botRunning) return;
            if (CPsettings.Instance.guildInvitescheck || CPsettings.Instance.refuseguildInvitescheck)
            {
                Lua.Events.DetachEvent("GUILD_INVITE_REQUEST", RotinaGuildInvites);
            }
            if (CPsettings.Instance.refusepartyInvitescheck)
            {
                Lua.Events.DetachEvent("PARTY_INVITE_REQUEST", RotinaPartyInvites);
            }
            if (CPsettings.Instance.refusetradeInvitescheck)
            {
                Lua.Events.DetachEvent("TRADE_SHOW", RotinaTradeInvites);
            }
            if (CPsettings.Instance.refuseduelInvitescheck)
            {
                Lua.Events.DetachEvent("DUEL_REQUESTED", RotinaDuelInvites);
            }
        }

        private void _OnBotStart(EventArgs args)
        {
            if (_onbotstart)
            {
                CPsettings.Instance.Load();
                _botRunning = true;
                if (ProfileManager.CurrentProfile.Name != null && !ProfileManager.CurrentProfile.Name.Contains("[Cava]") && _botRunning)
                {
                    _botRunning = false;
                }
                Log(_botRunning ? _rm.GetString("Is_now_ENABLED", _ci) : _rm.GetString("Is_now_DISABLED", _ci));
                if (_botRunning)
                {
                    Log(CPsettings.Instance.AntiStuckSystem
                            ? _rm.GetString("System_Anti-Stuck_Enabled", _ci)
                            : _rm.GetString("System_Anti-Stuck_Disabled", _ci));
                    _mountedTime.Restart();
                    _recomecar = new Thread(_Recomecar);
                    _asLastSavedTimer.Restart();

                    if (CPsettings.Instance.CheckAllowSummonPet)
                    {
                        //var numMinipets = Lua.GetReturnVal<int>("return C_PetBattles.GetNumPets(1)", 0);
                        var numMinipets = Lua.GetReturnVal<int>("return GetNumCompanions('CRITTER')", 0);
                        if (numMinipets > 0)
                        {
                            Log(_rm.GetString("Summon_Random_Pet_Enabled", _ci));
                            Lua.DoString("RunMacroText('/randompet')");
                            _summonpettime.Restart();
                        }
                        else 
                        {
                            Log(_rm.GetString("Dont_have_any_Pet_to_summom_disabling_Summon_Random_Pet", _ci));
                            CPsettings.Instance.CheckAllowSummonPet = false;

                        }
                    }
                    else
                    {
                        Log(_rm.GetString("Summon_Random_Pet_Disabled", _ci));
                    }

                    if (CPsettings.Instance.guildInvitescheck || CPsettings.Instance.refuseguildInvitescheck)
                    {
                        if (CPsettings.Instance.guildInvitescheck)
                        {
                            Log(_rm.GetString("Accept_lvl_25_guild_invite_Enabled", _ci));
                        }
                        if (CPsettings.Instance.refuseguildInvitescheck)
                        {
                            Log(_rm.GetString("Refuse_guild_invites_Enabled", _ci));
                        }
                        Lua.Events.AttachEvent("GUILD_INVITE_REQUEST", RotinaGuildInvites);
                    }
                    if (!CPsettings.Instance.guildInvitescheck || !CPsettings.Instance.refuseguildInvitescheck)
                    {
                        if (!CPsettings.Instance.guildInvitescheck)
                        {
                            Log(_rm.GetString("Accept_lvl_25_guild_invite_Disabled", _ci));
                        }
                        if (!CPsettings.Instance.refuseguildInvitescheck)
                        {
                            Log(_rm.GetString("Refuse_guild_invites_Disabled", _ci));
                        }
                    }

                    if (CPsettings.Instance.refusepartyInvitescheck)
                    {
                        Log(_rm.GetString("Refuse_party_invites_Enabled", _ci));
                        Lua.Events.AttachEvent("PARTY_INVITE_REQUEST", RotinaPartyInvites);
                    }
                    else 
                    {
                        Log(_rm.GetString("Refuse_party_invites_Disabled", _ci));
                    }

                    if (CPsettings.Instance.refusetradeInvitescheck)
                    {
                        Log(_rm.GetString("Refuse_trade_invites_Enabled", _ci));
                        Lua.Events.AttachEvent("TRADE_SHOW", RotinaTradeInvites);
                    }
                    else 
                    {
                        Log(_rm.GetString("Refuse_trade_invites_Disabled", _ci));
                    }

                    if (CPsettings.Instance.refuseduelInvitescheck)
                    {
                        Log(_rm.GetString("Refuse_duel_invites_Enabled", _ci));
                        Lua.Events.AttachEvent("DUEL_REQUESTED", RotinaDuelInvites);
                    }
                    else 
                    {
                        Log(_rm.GetString("Refuse_duel_invites_Disabled", _ci));
                        // ReSharper restore ResourceItemNotResolved
 
                    }
                }
                _onbotstart = false;
            }
            else
            { _onbotstart = true; }
        }

        private static int RandomNumber(int min, int max)
        {
            var random = new Random();
            return random.Next(min, max);
        }
        private void RotinaGuildInvites(object sender, LuaEventArgs e)
        {
            var guildName = e.Args[1].ToString();
            var guildLevel = Convert.ToInt32(e.Args[2]);
            if (CPsettings.Instance.guildInvitescheck && guildLevel >= 25)
            {
                Log("Accepting guild invite from {0}", guildName);
                Lua.DoString("AcceptGuild()");
                Lua.DoString("StaticPopup_Hide(\"GUILD_INVITE_REQUEST\")");
            }
            if (CPsettings.Instance.refuseguildInvitescheck || guildLevel < 25)
            {
                _refuseguildtimer.Reset();
                _refuseguildtimer.Start();
                _refusetime = RandomNumber(3000, 8000);
                _gotGuildInvite = true;
                Log("Declining guild invite from {0} lvl {1} in " + _refusetime / 1000 + " seconds", guildName, guildLevel);
            }
        }

        private void RotinaPartyInvites(object sender, LuaEventArgs e)
        {
            var userInviter = e.Args[1].ToString();
            _refusepartytimer.Reset();
            _refusepartytimer.Start();
            _refusetime = RandomNumber(3000, 8000);
            _gotPartyInvite = true;
            Log("[CavaPlugin - Declining party invite from {0} in " + _refusetime / 1000 + " seconds", userInviter);
        }

        private void RotinaTradeInvites(object sender, LuaEventArgs e)
        {
            _refusetradetimer.Reset();
            _refusetradetimer.Start();
            _refusetime = RandomNumber(3000, 8000);
            _gotTradeinvite = true;
            Log("[CavaPlugin - Declining trade in " + _refusetime / 1000 + " seconds");
        }

        private void RotinaDuelInvites(object sender, LuaEventArgs e)
        {
            var userInviter = e.Args[1].ToString();
            _refusedueltimer.Reset();
            _refusedueltimer.Start();
            _refusetime = RandomNumber(3000, 8000);
            _gotDuelinvite = true;
            Log("[CavaPlugin - Declining duel invite from {0} in " + _refusetime / 1000 + " seconds", userInviter);
        }

        private static void _Recomecar()
        {
            TreeRoot.Stop();
            StyxWoW.Sleep(2000);
            TreeRoot.Start();
        }

        private static bool IsObjectiveComplete(int objectiveId, uint questId)
        {
            if (Me.QuestLog.GetQuestById(questId) == null)
            {
                return false;
            }
            var returnVal = Lua.GetReturnVal<int>(string.Format("return GetQuestLogIndexByID({0})", questId), 0);
            return Lua.GetReturnVal<bool>(string.Format("return GetQuestLogLeaderBoard({0},{1})", objectiveId, returnVal), 2);
        }

        private String NewCavaProfilePath
        {
            get
            {
                var directory = Utilities.AssemblyDirectory + @"\Default Profiles\Cava\Scripts\";
                return (Path.Combine(directory, _profileName));
            }
        }

        private String _profileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@"Plugins\CavaPlugin\Settings\Main-Settings.xml"));
        #endregion


        #region Logging
        // ReSharper disable MemberCanBePrivate.Global
        public static void Log(string format, params object[] args)
        {
            Log(Colors.SkyBlue, format, args);
        }
        public static void Log(Color color, string format, params object[] args)
        {
            // ReSharper disable LocalizableElement
            Logging.Write(color, "[CavaPlugin]:" + format, args);
        }

        public void Debug(string format, params object[] args)
        {
            Debug(Colors.Teal, format, args);
        }
        public void Debug(Color color, string format, params object[] args)
        {
            Logging.Write(color, "[CavaPlugin]" + Version + ": " + format, args);
        }
 
        public static void Err(string format, params object[] args)
        {
            Err(Colors.Red, format, args);
        }
        public static void Err(Color color, string format, params object[] args)
        {
            Logging.Write(color, "Err: " + format, args);
            Player.SoundLocation = PathToCavaPlugin + "Sounds\\Error2.wav";
            Player.Play();

        }
        // ReSharper restore LocalizableElement
        // ReSharper restore MemberCanBePrivate.Global
        #endregion
        
        #region Utils
        #endregion

        #region Privates/Publics
        private void AbreJanela()
        {
            if (_cavaupdated)
            {
                // ReSharper disable LocalizableElement
                MessageBox.Show("Cava Plugin/Quest Behaviors has been updated a restart is required.", "RESTART REQUIRED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // ReSharper restore LocalizableElement
                Player.SoundLocation = PathToCavaPlugin + "Sounds\\notify.wav";
                Player.Play();
                Environment.Exit(0);
            }
            var mainCavaForm = new CavaForm();
            mainCavaForm.ShowDialog();
        }

        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        #endregion

        #region Quests

        private static List<WoWUnit> MobKingGennGreymane { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 36332)).OrderBy(ret => ret.Distance).ToList(); } }
        private static List<WoWUnit> MobDocZapnozzle { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 36608)).OrderBy(ret => ret.Distance).ToList(); } }
        private static List<WoWUnit> MobArctanus { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 34292)).OrderBy(ret => ret.Distance).ToList(); } }
        private static List<WoWUnit> MobTidecrusher { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 38750 && ret.IsAlive)).OrderBy(ret => ret.Distance).ToList(); } }
        private static List<WoWUnit> MobElectromental { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 21729 && ret.IsAlive && !ret.HasAura(37136))).OrderBy(ret => ret.Distance).ToList(); } }
        private static List<WoWUnit> MobNetherWhelp { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 20021 && ret.IsAlive)).OrderBy(ret => ret.Distance).ToList(); } }
        private static List<WoWUnit> MobProtoNetherDrake { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 21821 && ret.IsAlive)).OrderBy(ret => ret.Distance).ToList(); } }
        private static List<WoWUnit> MobAdolescentNetherDrake { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 21817 && ret.IsAlive)).OrderBy(ret => ret.Distance).ToList(); } }
        private static List<WoWUnit> MobMatureNetherDrake { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 21820 && ret.IsAlive)).OrderBy(ret => ret.Distance).ToList(); } }
        private static List<WoWUnit> MobKoiKoiSpirit { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 22226 && ret.IsAlive)).OrderBy(ret => ret.Distance).ToList(); } }
        private static List<WoWUnit> MobWitheredCorpse { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 20561 && ret.Distance < 16 && ret.HasAura(31261))).OrderBy(ret => ret.Distance).ToList(); } }
        private static List<WoWUnit> MobGlacierIce { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 49233 && ret.IsAlive)).OrderBy(ret => ret.Distance).ToList(); } }
        private static WoWItem ItemCelebrationPack { get { return (StyxWoW.Me.CarriedItems.FirstOrDefault(i => i.Entry == 90918)); } }
        //private static WoWItem ItemHs { get { return (StyxWoW.Me.CarriedItems.FirstOrDefault(i => i.Entry == 6948)); } }
        private static WoWItem ItemThisShiv { get { return (StyxWoW.Me.CarriedItems.FirstOrDefault(i => i.Entry == 55883)); } }

        #endregion

        #region Override Pulse
        public override void Pulse()
        {
            if (ProfileManager.CurrentOuterProfile.Name == "Mining Blacksmithing 1 to 300 by Cava" ||
                ProfileManager.CurrentOuterProfile.Name == "Mining Blacksmithing 1 to 600 by Cava")
            {
                Log("Loading '{0}'", ProfileManager.CurrentOuterProfile.Name);
                StyxWoW.Sleep(3000);
                BotBase pbBotBase;
                BotManager.Instance.Bots.TryGetValue("ProfessionBuddy", out pbBotBase);
                if (pbBotBase != null && BotManager.Current != pbBotBase)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        new Action(
                            () =>
                            {
                                TreeRoot.Stop();
                                BotManager.Instance.SetCurrent(pbBotBase);
                                StyxWoW.Sleep(2000);
                                if (ProfileManager.CurrentOuterProfile.Name == "Mining Blacksmithing 1 to 600 by Cava") { _profileName = "Prof\\MB\\[PB]MB(Cava).xml"; }
                                if (ProfileManager.CurrentOuterProfile.Name == "Mining Blacksmithing 1 to 300 by Cava") { _profileName = "Prof\\MB\\Free[PB]MB(Cava).xml"; }
                                ProfileManager.LoadNew(NewCavaProfilePath, false);
                                TreeRoot.Start();
                            }));
                }
                else
                {
                    if (ProfileManager.CurrentOuterProfile.Name == "Mining Blacksmithing 1 to 600 by Cava")
                    {
                        ProfileManager.LoadNew(Utilities.AssemblyDirectory + @"\Default Profiles\Cava\Scripts\Prof\MB\[PB]MB(Cava).xml", false);
                    }
                    if (ProfileManager.CurrentOuterProfile.Name == "Mining Blacksmithing 1 to 300 by Cava")
                    {
                        ProfileManager.LoadNew(Utilities.AssemblyDirectory + @"\Default Profiles\Cava\Scripts\Prof\MB\Free[PB]MB(Cava).xml", false);
                    }
                }
            }
            if (Me.IsDead && !Me.HasAura(8326) && CPsettings.Instance.RessAfterDie)
            {
                StyxWoW.Sleep(5000);
                Log("Anti-Bug Release System");
                Lua.DoString("RunMacroText('/click StaticPopup1Button1')");
                Lua.DoString(string.Format("RunMacroText(\"{0}\")", "/script RepopMe()"));
            }
            if (_botRunning)
            {
                if (_gotGuildInvite && _refuseguildtimer.ElapsedMilliseconds >= _refusetime)
                {
                    Lua.DoString("DeclineGuild()");
                    Lua.DoString("StaticPopup_Hide(\"GUILD_INVITE\")");
                    _gotGuildInvite = false;
                }

                if (_gotPartyInvite && _refusepartytimer.ElapsedMilliseconds >= _refusetime)
                {
                    Lua.DoString("DeclineGroup()");
                    Lua.DoString("StaticPopup_Hide(\"PARTY_INVITE\")");
                    _gotPartyInvite = false;
                }

                if (_gotTradeinvite && _refusetradetimer.ElapsedMilliseconds >= _refusetime)
                {
                    Lua.DoString("CancelTrade()");
                    _gotTradeinvite = false;
                }

                if (_gotDuelinvite && _refusedueltimer.ElapsedMilliseconds >= _refusetime)
                {
                    Lua.DoString("CancelDuel()");
                    Lua.DoString("StaticPopup_Hide(\"DUEL_REQUESTED\")");
                    _gotDuelinvite = false;
                }

                if (CPsettings.Instance.CheckAllowSummonPet && _summonpettime.Elapsed.TotalMinutes > 30)
                {
                    Log("Summoning Random pet.");
                    Lua.DoString("RunMacroText('/randompet')");
                    _summonpettime.Restart();
                }

                if (Me.Race == WoWRace.Goblin && Me.HasAura("Near Death!") && Me.ZoneId == 4720 && MobDocZapnozzle.Count > 0)
                {
                    MobDocZapnozzle[0].Interact();
                    StyxWoW.Sleep(1000);
                    Lua.DoString("RunMacroText('/click QuestFrameCompleteQuestButton')");
                }
                if (Me.Race == WoWRace.Worgen && Me.HasAura(68631) && Me.ZoneId == 4714 && MobKingGennGreymane.Count > 0)
                {
                    MobKingGennGreymane[0].Interact();
                    StyxWoW.Sleep(1000);
                    Lua.DoString("RunMacroText('/click QuestFrameCompleteQuestButton')");
                }
                if (Me.QuestLog.GetQuestById(13884) != null && !Me.QuestLog.GetQuestById(13884).IsCompleted && !Me.HasAura(65178) && MobArctanus.Count > 0)
                {
                    MobArctanus[0].Interact();
                    StyxWoW.Sleep(1000);
                }
                if (Me.QuestLog.GetQuestById(24950) != null && !Me.QuestLog.GetQuestById(24950).IsCompleted && MobTidecrusher.Count > 0)
                {
                    MobTidecrusher[0].Interact();
                    MobTidecrusher[0].Face();
                    Styx.CommonBot.Routines.RoutineManager.Current.Pull();
                }
                if (Me.QuestLog.GetQuestById(10584) != null && !Me.QuestLog.GetQuestById(10584).IsCompleted && MobElectromental.Count > 0)
                {
                    MobElectromental[0].Interact();
                    MobElectromental[0].Face();
                    Lua.DoString("UseItemByName(30656)");
                    StyxWoW.Sleep(4000);
                }
                if (Me.QuestLog.GetQuestById(10609) != null && !Me.QuestLog.GetQuestById(10609).IsCompleted && MobNetherWhelp.Count > 0)
                {
                    MobNetherWhelp[0].Interact();
                    MobNetherWhelp[0].Face();
                    Lua.DoString("UseItemByName(30742)");
                    StyxWoW.Sleep(4000);
                }
                if (Me.QuestLog.GetQuestById(10609) != null && !Me.QuestLog.GetQuestById(10609).IsCompleted && IsObjectiveComplete(1, 10609) && MobProtoNetherDrake.Count > 0)
                {
                    MobProtoNetherDrake[0].Interact();
                    MobProtoNetherDrake[0].Face();
                    Lua.DoString("UseItemByName(30742)");
                    StyxWoW.Sleep(4000);
                }
                if (Me.QuestLog.GetQuestById(10609) != null && !Me.QuestLog.GetQuestById(10609).IsCompleted && IsObjectiveComplete(2, 10609) && MobAdolescentNetherDrake.Count > 0)
                {
                    MobAdolescentNetherDrake[0].Interact();
                    MobAdolescentNetherDrake[0].Face();
                    Lua.DoString("UseItemByName(30742)");
                    StyxWoW.Sleep(4000);
                }
                if (Me.QuestLog.GetQuestById(10609) != null && !Me.QuestLog.GetQuestById(10609).IsCompleted && IsObjectiveComplete(3, 10609) && MobMatureNetherDrake.Count > 0)
                {
                    MobMatureNetherDrake[0].Interact();
                    MobMatureNetherDrake[0].Face();
                    Lua.DoString("UseItemByName(30742)");
                    StyxWoW.Sleep(4000);
                }
                if (Me.QuestLog.GetQuestById(10830) != null && !Me.QuestLog.GetQuestById(10830).IsCompleted && MobKoiKoiSpirit.Count > 0)
                {
                    MobKoiKoiSpirit[0].Interact();
                    MobKoiKoiSpirit[0].Face();
                    Styx.CommonBot.Routines.RoutineManager.Current.Pull();
                }
                if (Me.QuestLog.GetQuestById(10345) != null && !Me.Combat && MobWitheredCorpse.Count > 0)
                {
                    WoWMovement.MoveStop();
                    Lua.DoString("UseItemByName(29473)");
                    StyxWoW.Sleep(500);
                }
                if (Me.QuestLog.GetQuestById(28632) != null && !Me.QuestLog.GetQuestById(28632).IsCompleted && !Me.Combat && MobGlacierIce.Count > 0)
                {
                    MobGlacierIce[0].Interact();
                }
                if (Me.QuestLog.GetQuestById(11794) != null && Me.QuestLog.GetQuestById(11794).IsCompleted && !Me.Combat && !Me.HasAura(46078))
                {
                    WoWMovement.MoveStop();
                    Lua.DoString("UseItemByName(35125)");
                    StyxWoW.Sleep(500);
                }
                if (Me.ZoneId == 616 && Me.CurrentTarget != null && Me.CurrentTarget.Entry == 41031)
                {
                    if (ItemThisShiv != null)
                    {
                        if(Me.CurrentTarget.Distance < 6)
                        {
                            WoWMovement.MoveStop();
                            Lua.DoString("UseItemByName(55883)");
                            StyxWoW.Sleep(500);
                        }
                    }
                    else
                    {
                         Blacklist.Add(Me.CurrentTarget, BlacklistFlags.Combat, TimeSpan.FromSeconds(120));
                    }
                }
                if (Me.IsAlive && !Me.HasAura(132700) && !Me.IsOnTransport && !Me.OnTaxi && !Me.Mounted && !Me.IsCasting && !Me.Combat && ItemCelebrationPack != null)
                {
                    Log("Using Celebration Package 9Th Aniversary at " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    WoWMovement.MoveStop();
                    Lua.DoString("UseItemByName(90918)");
                    StyxWoW.Sleep(500);
                }
                
                if (CPsettings.Instance.AntiStuckSystem )
                {
                    if (_asLastSavedPosition.Distance(Me.Location) > 35)
                    {
                        _asLastSavedPosition = Me.Location;
                        _asLastSavedTimer.Restart();
                        _asLastSavedPositionTrigger = false;
                    }
                    if (!Me.Mounted || Me.OnTaxi)
                    {
                        _mountedTime.Restart();
                    }

                    if (_asLastSavedTimer.ElapsedMilliseconds > 6000 && _mountedTime.ElapsedMilliseconds > 30000 && BotPoi.Current.Location.DistanceSqr(Me.Location) > 10 && !_asLastSavedPositionTrigger)
                    {  //movimento menor que 35 nos ultimos 6 segundos, mounted mais de 30 segundos,a mais de 10 do objectivo
                        Log("[AntiStuck] Char is Mounted for more than 6 secs and stuck : Forcing Dismount..." + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                        Mount.Dismount();
                        StyxWoW.Sleep(2000);
                        Lua.DoString("Dismount()");
                        _mountedTime.Restart();
                        _asLastSavedPositionTrigger = true;
                    }
                    if (Me.IsAlive && Me.Mounted && !Me.OnTaxi && _mountedTime.ElapsedMilliseconds > 600000)
                    {
                        Log("[AntiStuck] Char is Mounted for more than 10 min : Forcing Dismount..." + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                        Mount.Dismount();
                        StyxWoW.Sleep(2000);
                        Lua.DoString("Dismount()");
                        _mountedTime.Restart();
                    }
                    if (!TreeRoot.IsRunning && _ultimoSemStuck.ElapsedMilliseconds > 30000)
                    {
                        Logging.Write(@"[CavaPlugin-AntiStuck] LastPosition reseted, bot is not running (but pulse is called ???)");
                        _ultimoSemStuck.Restart();
                        return;
                    }
                    if (_ultimoLocal.Distance(Me.Location) > 10f)
                    {
                        _ultimoSemStuck.Restart();
                        _ultimoLocal = Me.Location;
                        _nVezesBotUnstuck = 0;
                        return;
                    }
                    if (Me.IsAlive && Me.IsAFKFlagged && !Me.IsCasting && !Me.IsMoving && !Me.Combat && !Me.OnTaxi && _nVezesBotUnstuck == 0)
                    {
                        Log("[CavaPlugin-AntiStuck] I'm AFK flagged, Anti-Afking at " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                        WoWMovement.Move(WoWMovement.MovementDirection.JumpAscend, TimeSpan.FromMilliseconds(100));
                        StyxWoW.Sleep(2000);
                        KeyboardManager.KeyUpDown((char)KeyboardManager.eVirtualKeyMessages.VK_SPACE);
                        StyxWoW.Sleep(2000);
                        KeyboardManager.AntiAfk();
                        StyxWoW.Sleep(2000);
                        Mount.Dismount();
                        StyxWoW.Sleep(2000);
                        Lua.DoString("Dismount()");
                        StyxWoW.Sleep(2000);
                        StyxWoW.ResetAfk();
                        _nVezesBotUnstuck++;
                    }
                    if (Styx.CommonBot.Frames.AuctionFrame.Instance.IsVisible || Styx.CommonBot.Frames.MailFrame.Instance.IsVisible)
                    {
                        _ultimoSemStuck.Restart();
                        _ultimoLocal = Me.Location;
                        return;
                    }
                    if (Me.HasAura("Resurrection Sickness"))
                    {
                        _ultimoSemStuck.Restart();
                        return;
                    }
                    if (_ultimoSemStuck.ElapsedMilliseconds > 300000 && _nVezesBotUnstuck == 0)
                    {
                        Log("[CavaPlugin-AntiStuck] not moving last 5 min : Forcing Dismount..." + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                        Mount.Dismount();
                        StyxWoW.Sleep(2000);
                        Lua.DoString("Dismount()");
                        _nVezesBotUnstuck++;
                    }
                    if (_ultimoSemStuck.ElapsedMilliseconds > 600000 && _nVezesBotUnstuck == 1)
                    {
                        Log("[CavaPlugin-AntiStuck] not moving last 10 min : Restarting bot..." + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                        _recomecar.Start();
                        _nVezesBotUnstuck++;
                    }
                    if (_ultimoSemStuck.ElapsedMilliseconds > 900000 && Me.ZoneId != 1519 && Me.ZoneId != 1637)
                    {
                        Log("[CavaPlugin-AntiStuck] not moving last 15 min : Closing WOW Game..." + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                        Lua.DoString(@"ForceQuit()");
                    }
                  }
            }
        }
        #endregion
    }
}