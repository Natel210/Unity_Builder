using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using UnityBuilder.Res;

namespace UnityBuilder
{
    namespace Log
    {
        static public class Manager
        {
            static public Item.Base colsoleLog = new Item.Console();
            static public Item.Base debugOutputLog = new Item.VS_DebugOutput();
            static public Item.Base stringLog = new Item.String();
            static public Dictionary<string, Item.Base> logMap = new Dictionary<string, Item.Base>();
            static public void Init()
            {
                //Real Time Write
                colsoleLog.RealTimeWrite = true;
                debugOutputLog.RealTimeWrite = true;
                stringLog.RealTimeWrite = true;
            }
        }

        namespace Item
        {
            public abstract class Base
            {
                private List<string> outputStrings = new List<string>();
                protected List<string> OutputStrings { get => outputStrings; }

                private bool realTimeWrite = false;
                public bool RealTimeWrite { get => realTimeWrite; set => realTimeWrite = value; }

                public void Log_MainSection(string str)
                {
                    outputStrings.Add(string.Format(Res.String.Common.SectionForm, Res.String.Common.MainSection, str));
                    if (RealTimeWrite)
                        Write();
                }

                public void Log_MainSectionEnd()
                {
                    outputStrings.Add(Res.String.Common.MainSectionEnd);
                    if (RealTimeWrite)
                        Write();
                }

                public void Log_SubSection1(string str)
                {
                    outputStrings.Add(string.Format(Res.String.Common.SectionForm, Res.String.Common.SubSection1, str));
                    if (RealTimeWrite)
                        Write();
                }

                public void Log_SubSection2(string str)
                {
                    outputStrings.Add(string.Format(Res.String.Common.SectionForm, Res.String.Common.SubSection2, str));
                    if (RealTimeWrite)
                        Write();
                }

                public void Log_Text(string str)
                {
                    outputStrings.Add(string.Format(Res.String.Common.TextForm, Res.String.Common.TextLine, str));
                    if (RealTimeWrite)
                        Write();
                }

                public void Log_UserForm(string str)
                {
                    outputStrings.Add(str);
                    if (RealTimeWrite)
                        Write();
                }

                /// <summary>
                /// Use Write By Device Type
                /// </summary>
                public string[] Write()
                {
                    var tempStrings = WriteToDevice();
                    Clear();
                    return tempStrings;
                }

                /// <summary>
                /// take device output
                /// </summary>
                protected abstract string[] WriteToDevice();

                /// <summary>
                /// temp buf text clear
                /// </summary>
                public void Clear() { outputStrings.Clear(); }
            }

            public class Console : Base
            {
                protected override string[] WriteToDevice()
                {
                    foreach (var item in OutputStrings)
                        System.Console.WriteLine(item);
                    return OutputStrings.ToArray();
                }
            }

            public class TxtFile : Base
            {
                private string logPath;
                public string LogPath { get => logPath; set => logPath = value; }
                public TxtFile(string path) { LogPath = path; }

                protected override string[] WriteToDevice()
                {
                    using (var writer = new StreamWriter(LogPath, append: true))
                    {
                        foreach (var item in OutputStrings)
                            writer.WriteLine(item);
                    }
                    return OutputStrings.ToArray();
                }
            }

            public class TxtFile_Reverce : Base
            {
                private string logPath;
                public string LogPath { get => logPath; set => logPath = value; }
                public TxtFile_Reverce(string path) { LogPath = path; }

                protected override string[] WriteToDevice()
                {
                    string tempString = "";
                    foreach (var item in OutputStrings)
                        tempString += item + "\n";
                    if(File.Exists(logPath))
                        tempString += File.ReadAllText(logPath);
                    File.WriteAllText(logPath, tempString);
                    return OutputStrings.ToArray();
                }
            }

            public class VS_DebugOutput : Base
            {
                protected override string[] WriteToDevice()
                {
                    foreach (var item in OutputStrings)
                        Debug.WriteLine(item);
                    return OutputStrings.ToArray();
                }
            }

            public class String : Base
            {
                protected override string[] WriteToDevice()
                {
                    return OutputStrings.ToArray();
                }
            }
        }
    }
}
