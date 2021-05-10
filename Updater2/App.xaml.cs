﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Updater2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string exepath = AppContext.BaseDirectory;
        public static bool openingDS4W;
        private string launchExeName;
        private string launchExePath;
        private MainWindow mwd;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            mwd = new MainWindow();
            launchExePath = Path.Combine(exepath, "DS4Windows.exe");
            for (int i=0, arlen = e.Args.Length; i < arlen; i++)
            {
                string temp = e.Args[i];
                if (temp.Contains("-skipLang"))
                {
                    mwd.downloadLang = false;
                }
                else if (temp.Equals("-autolaunch"))
                {
                    mwd.autoLaunchDS4W = true;
                }
                else if (temp.Equals("-user"))
                {
                    mwd.forceLaunchDS4WUser = true;
                }
                else if (temp.Equals("--launchExe"))
                {
                    if ((i+1) < arlen)
                    {
                        i++;
                        temp = e.Args[i];
                        string tempPath = Path.Combine(exepath, temp);
                        if (File.Exists(tempPath))
                        {
                            launchExeName = temp;
                            launchExePath = tempPath;
                        }
                    }
                }
            }

            mwd.Show();
        }

        public App()
        {
            //Debug.WriteLine(CultureInfo.CurrentCulture);
            this.Exit += (s, e) =>
            {
                string currentUpdaterPath = Path.Combine(exepath, "Update Files", "DS4Windows", "DS4Updater.exe");
                string tempNewUpdaterPath = Path.Combine(exepath, "DS4Updater NEW.exe");

                string fileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.exe";
                string version = Path.Combine(AppContext.BaseDirectory, fileName);
                if (File.Exists(exepath + "\\Update Files\\DS4Windows\\DS4Updater.exe")
                    && FileVersionInfo.GetVersionInfo(exepath + "\\Update Files\\DS4Windows\\DS4Updater.exe").FileVersion.CompareTo(version) != 0)
                {
                    File.Move(currentUpdaterPath, tempNewUpdaterPath);
                    //Directory.Delete(exepath + "\\Update Files", true);

                    //string tempFilePath = Path.GetTempFileName();
                    string tempFilePath = Path.Combine(Path.GetTempPath(), "UpdateReplacer.bat");
                    using (StreamWriter w = new StreamWriter(tempFilePath))
                    {
                        w.WriteLine("@echo off"); // Turn off echo
                        w.WriteLine("@echo Attempting to replace updater, please wait...");
                        w.WriteLine("@ping -n 4 127.0.0.1 > nul"); //Its silly but its the most compatible way to call for a timeout in a batch file, used to give the main updater time to cleanup and exit.
                        w.WriteLine("@del \"" + exepath + "\\DS4Updater.exe" + "\"");
                        w.WriteLine("@ren \"" + exepath + "\\DS4Updater NEW.exe" + "\" \"DS4Updater.exe\"");
                        w.Close();
                    }

                    Process.Start(tempFilePath);
                }
                else if (File.Exists(tempNewUpdaterPath))
                {
                    File.Delete(tempNewUpdaterPath);
                }

                if (Directory.Exists(exepath + "\\Update Files"))
                {
                    Directory.Delete(exepath + "\\Update Files", true);
                }
            };

            this.Exit += (s, e) =>
            {
                if (openingDS4W)
                {
                    AutoOpenDS4();
                }
            };
        }

        private void AutoOpenDS4()
        {
            string finalLaunchExePath = exepath;
            if (File.Exists(launchExePath))
                finalLaunchExePath = launchExePath;

            if (mwd.forceLaunchDS4WUser)
            {
                // Attempt to launch program as a normal user
                Util.StartProcessInExplorer(finalLaunchExePath);
            }
            else
            {
                // Attempt to launch Explorer with folder open
                Process.Start(finalLaunchExePath);
            }
        }
    }
}
