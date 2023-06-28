using System;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace UnityBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Manager.Init();
            Log.Manager.logMap.Add(Res.String.Path.VersionTextKey, new Log.Item.TxtFile_Reverce(Res.String.Path.VersionTextValue));

            //Before Version Chacking
            string[] allVersion = null;
            string lastVersion = null;
            if (File.Exists(Res.String.Path.VersionTextValue))
            {
                allVersion = File.ReadAllLines(Res.String.Path.VersionTextValue);
                if (allVersion.Length > 0)
                    lastVersion = allVersion[0];
            }
            else
            {
                allVersion = null;
                lastVersion = null;
            }

            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string CurrentVersion = currentAssembly.GetName().Version.ToString();
            if (lastVersion != CurrentVersion)
            {
                if (Log.Manager.logMap.ContainsKey(Res.String.Path.VersionTextKey))
                {
                    Log.Manager.logMap[Res.String.Path.VersionTextKey].RealTimeWrite = true;
                    Log.Manager.logMap[Res.String.Path.VersionTextKey].Log_UserForm(CurrentVersion);
                }
            }

            if (args.Length == 2)
            {
                UnityBuilder.rootPath = args[0];
                UnityBuilder.projectName = args[1];
                UnityBuilder.Work();
            }
            else
                Log.Manager.colsoleLog.Log_UserForm(Res.String.LogText.ArgumentCountFile);
        }
    }
}
