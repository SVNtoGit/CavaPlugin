﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Styx;
using Styx.Helpers;
using Styx.Loaders;
using Styx.Patchables;
using Styx.Plugins;
using Styx.Common;
using Styx.CommonBot;
using Styx.Pathing;
using Styx.WoWInternals;
using Styx.WoWInternals.Misc;
using Styx.WoWInternals.Misc.DBC;
using Styx.WoWInternals.World;
using Styx.WoWInternals.WoWCache;
using Styx.WoWInternals.WoWObjects;

using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;

using CavaHPlugin;
using CavaHProfile;
using CavaHQB;

namespace CavaPlugin
{
    public class CavaPlugin : HBPlugin
    {
        public bool hasBeenInitialized = false;
        public bool hasBeenInitialized2 = false;
        public bool hasBeenInitialized3 = false;
        public static bool hasBeenInitialized4 = false;
        public bool cavaupdated = false;
        
        #region Overrides except pulse
        public override string Author { get { return "Cava"; } }
        public override Version Version { get { return new Version(3, 0, 0); } }
        public override string Name { get { return "CavaPlugin"; } }
        public override bool WantButton { get { return true; } }
        public override string ButtonText { get { return "Cava Profiles"; } }
        public override void OnButtonPress()
        {
            bool isRunningantes = TreeRoot.IsRunning;
            if (isRunningantes)
            {
                MessageBox.Show("Bot is running, stop bot before initiate Cava Plugin", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                abreJanela();
            }
        }
        private Cava_Plugin_Updater UpdaterPlugin;
        private Cava_QB_Updater UpdaterQB;
        private Cava_Profile_Updater UpdaterProfile;
        public string pathToCavaArmageddoner = Path.Combine(Utilities.AssemblyDirectory + @"\Default Profiles\Cava\Scripts\Armageddoner\");
        static void UpdaterArmageddoner(string f)
        {
            //ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.FileName = "TortoiseProc.exe";
            //startInfo.Arguments = f;
            //Process.Start(startInfo);
            Process p = new Process();
            p.StartInfo.FileName = "TortoiseProc.exe";
            p.StartInfo.Arguments = f;
            p.Start();
            p.WaitForExit();
            if (p.ExitCode == 0)
            {
                hasBeenInitialized4 = true;
            }

        }

        public override void Initialize()
        {
            if (!hasBeenInitialized)
            {
                Logging.Write(Colors.Teal, "Loaded Cava Plugin v" + Version.ToString());
                Logging.Write(Colors.Teal, "Please Wait While [Cava Plugin] Check For Updates, This Can Take Several Minutes");
                hasBeenInitialized = true;
                try
                {
                    UpdaterPlugin = new Cava_Plugin_Updater("http://cavaplugin.googlecode.com/svn/trunk/", "CavaPlugin");
                    if (UpdaterPlugin.UpdateAvailable())
                    {
                        Logging.Write("[Cava Plugin] Update to $" + UpdaterPlugin.GetNewestRev().ToString() + " is available! You are on $" + UpdaterPlugin.CurrentRev.ToString());
                        Logging.Write("[Cava Plugin] Starting update process");
                        if (UpdaterPlugin.Update())
                        {
                            Logging.Write("[Cava Plugin] is now up to date! Please reload HB");
                            cavaupdated = true;
                        }
                        else
                        {
                            Logging.Write("[Cava Plugin] Encountered an error trying to auto-update. Please update manually");
                        }
                    }
                    else
                    {
                        Logging.Write("[Cava Plugin] is at Rev $" + UpdaterPlugin.CurrentRev.ToString() + " and up to date!");
                    }
                }
                catch (Exception ex)
                {
                    Logging.Write(Colors.Teal, "Unable to run [Cava Plugin] update process");
                    Logging.Write(LogLevel.Diagnostic, "[Cava Plugin]: Exception " + ex.Message);
                }
                try
                {
                    UpdaterQB = new Cava_QB_Updater("http://cavaqbs.googlecode.com/svn/trunk/", "");
                    if (UpdaterQB.UpdateAvailable())
                    {
                        Logging.Write("[Cava QB Updater] Update to $" + UpdaterQB.GetNewestRev().ToString() + " is available! You are on $" + UpdaterQB.CurrentRev.ToString());
                        Logging.Write("[Cava QB Updater] Starting update process");
                        if (UpdaterQB.Update())
                        {
                            Logging.Write("[Cava QB Updater] is now up to date! Please reload HB");
                            cavaupdated = true;
                        }
                        else
                        {
                            Logging.Write("[Cava QB Updater] Encountered an error trying to auto-update. Please update manually");
                        }
                    }
                    else
                    {
                        Logging.Write("[Cava QB Updater] is at Rev $" + UpdaterQB.CurrentRev.ToString() + " and up to date!");
                    }
                }
                catch (Exception ex)
                {
                    Logging.Write(Colors.Teal, "Unable to run [Cava QB Updater] update process");
                    Logging.Write(LogLevel.Diagnostic, "[Cava QB Updater]: Exception " + ex.Message);
                }
                try
                {
                    UpdaterProfile = new Cava_Profile_Updater("http://cavaprofiles.googlecode.com/svn/trunk/", "");
                    if (UpdaterProfile.UpdateAvailable())
                    {
                        Logging.Write("[Cava Profile Updater] Update to $" + UpdaterProfile.GetNewestRev().ToString() + " is available! You are on $" + UpdaterProfile.CurrentRev.ToString());
                        Logging.Write("[Cava Profile Updater] Starting update process");
                        if (UpdaterProfile.Update())
                        {
                            Logging.Write("[Cava Profile Updater] is now up to date!");
                        }
                        else
                        {
                            Logging.Write("[Cava Profile Updater] Encountered an error trying to auto-update. Please update manually");
                        }
                    }
                    else
                    {
                        Logging.Write("[Cava Profile Updater] is at Rev $" + UpdaterProfile.CurrentRev.ToString() + " and up to date!");
                    }
                }
                catch (Exception ex)
                {
                    Logging.Write(Colors.Teal,"Unable to run [Cava Profile Updater] update process");
                    Logging.Write(LogLevel.Diagnostic, "[Cava Profile Updater]: Exception " + ex.Message);
                }
                CPGlobalSettings.Instance.Load();
                if (CPGlobalSettings.Instance.Armageddoner && CPGlobalSettings.Instance.AllowUpdate)
                {
                    UpdaterArmageddoner("/command:\"update\" /path:\"" + pathToCavaArmageddoner + "\" /closeonend:1");
                }
              }
            //duplo ignore, bot corre 2 vezes o Initialize 
            if (!hasBeenInitialized2)
            {
                hasBeenInitialized2 = true;
                return;
            }
            if (!hasBeenInitialized3)
            {
                hasBeenInitialized3 = true;
                return;
            }
            abreJanela();
        }

        public override void Dispose()
        {
            Logging.Write(Colors.Teal, "CavaPlugin Disposed");
        }
        #endregion

        #region Privates/Publics
        private void abreJanela()
        {
            if (cavaupdated)
            {
                MessageBox.Show("Cava Plugin/Quest Behaviors has been updated a restart is required.", "RESTART REQUIRED", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            var mainCavaForm = new CavaForm();
            mainCavaForm.ShowDialog();
        }

        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        #endregion

        #region Quests
        public List<WoWUnit> MobDocZapnozzle { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 36608)).OrderBy(ret => ret.Distance).ToList(); } }
        public List<WoWUnit> MobArctanus { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 34292)).OrderBy(ret => ret.Distance).ToList(); } }
        public List<WoWUnit> MobTidecrusher { get { return ObjectManager.GetObjectsOfType<WoWUnit>().Where(ret => (ret.Entry == 38750 && ret.IsAlive)).OrderBy(ret => ret.Distance).ToList(); } }

        #endregion

        #region Override Pulse
        public override void Pulse()
        {
            if (Me.Race == WoWRace.Goblin && Me.HasAura("Near Death!") && Me.ZoneId == 4720 && MobDocZapnozzle.Count > 0)
            {
                MobDocZapnozzle[0].Interact();
                Thread.Sleep(1000);
                Lua.DoString("RunMacroText('/click QuestFrameCompleteQuestButton')");
            }

            if (Me.QuestLog.GetQuestById(13884) != null && !Me.QuestLog.GetQuestById(13884).IsCompleted && !Me.HasAura(65178) && MobArctanus.Count > 0)
            {
                MobArctanus[0].Interact();
                Thread.Sleep(1000);
            }

            if (Me.QuestLog.GetQuestById(24950) != null && !Me.QuestLog.GetQuestById(24950).IsCompleted && MobTidecrusher.Count > 0)
            {
                MobTidecrusher[0].Interact();
                MobTidecrusher[0].Face();
                Styx.CommonBot.Routines.RoutineManager.Current.Pull();
            }
        }
        #endregion

    }
}