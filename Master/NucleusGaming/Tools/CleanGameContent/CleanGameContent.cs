﻿using Nucleus.Gaming.Forms.NucleusMessageBox;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;

namespace Nucleus.Gaming
{
    public static class CleanGameContent
    {
        public static void CleanContentFolder(GenericGameInfo currentGameInfo)
        {
            
            string path = Path.Combine(GameManager.Instance.GetAppContentPath(), currentGameInfo.GUID);

            if (Directory.Exists(path))
            {
                string[] instances = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);            

                try
                {
                    KillRemainingGameProcess(currentGameInfo);

                    foreach (string instance in instances)
                    {
                        if (Directory.Exists(instance))
                        {
                            Directory.Delete(instance, true);
                        }
                    }
                }
                catch
                {
                    LogManager.Log("Nucleus will try to unlock one or more files in order to cleanup game content.");

                    try
                    {
                        foreach (string instance in instances)
                        {
                            bool exists = Directory.Exists(instance);

                            if (exists)
                            {
                                string[] subs = Directory.GetFileSystemEntries(instance, "*", SearchOption.AllDirectories);

                                foreach (string locked in subs)
                                {
                                    File.SetAttributes(locked, FileAttributes.Normal);
                                }
                            }

                            if (exists)
                            {
                                Directory.Delete(instance, true);
                                LogManager.Log("Game content cleaned.");
                            }
                        }
                    }
                    catch
                    {
                        LogManager.Log("Game content cleanup failed. One or more files can't be unlocked by Nucleus.");

                        System.Threading.Tasks.Task.Run(() =>
                        {
                            NucleusMessageBox.Show("Risk of crash!",
                                $"One or more files from {path} are locked\n" +
                                $"by the system or used by an other program \n" +
                                $"and Nucleus failed to unlock them.You can try\n" +
                                $"to delete/unlock the file(s) manually or restart\n" +
                                $" your computer to unlock the file(s) because it\n" +
                                $"could lead to a crash on game startup.\n" +
                                $" You can ignore this message and risk a crash or unexpected behaviors.", false);
                        });
                    }
                }
            }
        }

        private static void KillRemainingGameProcess(GenericGameInfo currentGameInfo)
        {
            try
            {
                Process[] procs = Process.GetProcesses();

                List<string> addtlProcsToKill = new List<string>();
                if (currentGameInfo.KillProcessesOnClose?.Length > 0)
                {
                    addtlProcsToKill = currentGameInfo.KillProcessesOnClose.ToList();
                }

                foreach (Process proc in procs)
                {
                    try
                    {
                        if ((currentGameInfo.LauncherExe != null && !currentGameInfo.LauncherExe.Contains("NucleusDefined") && proc.ProcessName.ToLower() == Path.GetFileNameWithoutExtension(currentGameInfo.LauncherExe.ToLower())) ||
                            addtlProcsToKill.Contains(proc.ProcessName, StringComparer.OrdinalIgnoreCase) || 
                            proc.ProcessName.ToLower() == Path.GetFileNameWithoutExtension(currentGameInfo.ExecutableName.ToLower()) || (currentGameInfo.Hook.ForceFocusWindowName != "" && proc.MainWindowTitle == currentGameInfo.Hook.ForceFocusWindowName))
                        {
                            LogManager.Log(string.Format("Killing process {0} (pid {1})", proc.ProcessName, proc.Id));
                            proc.Kill();
                        }

                    }
                    catch (Exception ex)
                    {
                        LogManager.Log(ex.InnerException + " " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(ex.InnerException + " " + ex.Message);
            }
        }
    }
}
