using System;
using System.IO;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace UnityBuilder
{
    static class UnityBuilder
    {
        public static string rootPath { get; set; } = @"";
        public static string projectName { get; set; } = @"";
        public static List<string> headerList = new List<string> ();
        public static List<string> cppList = new List<string>();
        public static List<string> unityBuildFileList = new List<string>();
        public static Log.Item.Base log = Log.Manager.colsoleLog;
        public static string Language = "KR";

        public static void Work()
        {
            log.Log_MainSection(Res.String.Section.UnityBuild);
            if (string.Empty == rootPath)
            {
                Log.Manager.colsoleLog.Log_Text(Res.String.LogText.InvalidArgument1);
                return;
            }
            if (string.Empty == projectName)
            {
                Log.Manager.colsoleLog.Log_Text(Res.String.LogText.InvalidArgument2);
                return;
            }
            UnityBuildFileLoad();
            LoadHeader();
            LoadCpp();
            if(CheckChangedHeaderCpp())
                SetChangedHeaderCpp();
            SetProjectProperty();
            log.Log_MainSectionEnd();
        }

        private static void UnityBuildFileLoad()
        {
            log.Log_SubSection1(Res.String.Section.FileLoad);

            string tempUnityBuildDirPath = rootPath + @"\" + Res.String.Common.UnityBuildDirectoryName;
            DirectoryInfo tempDirInfo = new DirectoryInfo(tempUnityBuildDirPath);
            if (!tempDirInfo.Exists)
            {
                log.Log_Text(Res.String.LogText.FileLoad_NotDirExist);
                tempDirInfo.Create();
            }
            else
                log.Log_Text(Res.String.LogText.FileLoad_DirExist);

            string tempUnityBuildFilePath = rootPath + Res.String.Common.UnityBuildDirectoryName + Res.String.Common.UnityBuildFileName + Res.String.Common.HeaderFileExtension;

            if (!File.Exists(tempUnityBuildFilePath))
            {
                log.Log_Text(string.Format(Res.String.Common.NoExistsFile, Res.String.Common.UnityBuildFileName, Res.String.Common.HeaderFileExtension) );
                using (File.Create(tempUnityBuildFilePath)) { }
            }
            else
                log.Log_Text(string.Format(Res.String.Common.ExistsFile, Res.String.Common.UnityBuildFileName, Res.String.Common.HeaderFileExtension));

            var a = File.ReadAllLines(tempUnityBuildFilePath);
            var b = a.ToList();

            unityBuildFileList = File.ReadAllLines(tempUnityBuildFilePath).ToList();

        }

        private static void LoadHeader()
        {
            log.Log_SubSection1(Res.String.Section.CheckHeader);
            string[] files = Directory.GetFiles(rootPath, Res.String.Common.HeaderFile_wild, SearchOption.AllDirectories);
            foreach (var file in files)
                headerList.Add(Path.GetRelativePath(rootPath, file));
        }

        private static void LoadCpp()
        {
            log.Log_SubSection1(Res.String.Section.CheckCpp);
            string[] files = Directory.GetFiles(rootPath, Res.String.Common.CppFile_wild, SearchOption.AllDirectories);
            foreach (var file in files)
                cppList.Add(Path.GetRelativePath(rootPath, file));
        }

        private static bool CheckChangedHeaderCpp()
        {
            log.Log_SubSection1(Res.String.Section.changeHCpp);
            log.Log_Text(Res.String.LogText.CompareFiles);
            //ChangeFileStartLog
            bool result = false;
            List<string> tempChangedList = new List<string>();
            log.Log_Text(Res.String.LogText.CheckUnityBuildHeader);
            foreach (var item in headerList)
            {
                if (item.Contains(Res.String.Common.UnityBuildFileName + Res.String.Common.HeaderFileExtension))
                    continue;
                if (-1 == unityBuildFileList.FindIndex(x => x == ("#include \"" + item + "\"")))
                {
                    result |= true;
                    System.Console.WriteLine(String.Format("  Header file  add : {0}.", item));
                }
            }
            System.Console.WriteLine("* check UnityBuild.h from cpp.");
            foreach (var item in cppList)
            {
                if (item.Contains("UnityBuild.cpp"))
                    continue;
                if (-1 == unityBuildFileList.FindIndex(x => x == ("#include \"" + item + "\"")))
                {
                    result |= true;
                    System.Console.WriteLine(String.Format("  Cpp file add : {0}.", item));
                }
            }
            System.Console.WriteLine("* check h&cpp from UnityBuild.h.");
            foreach (var item in unityBuildFileList)
            {
                if (item.Contains("UnityBuild.h"))
                    continue;
                else if (item.Contains("UnityBuild.cpp"))
                    continue;
                else if (item.Contains(@"#pragma once"))
                    continue;

                if (item.Contains(".h"))
                {
                    if (-1 == headerList.FindIndex(x => item == ("#include \"" + x + "\"")))
                    {
                        result |= true;
                        System.Console.WriteLine(String.Format("  Header file  remove : {0}.", item));
                    }
                }
                else if (item.Contains(".cpp"))
                {
                    if (-1 == cppList.FindIndex(x => item == ("#include \"" + x + "\"")))
                    {
                        result |= true;
                        System.Console.WriteLine(String.Format("  Cpp file remove : {0}.", item));
                    }
                }
            }

            if (result)
                System.Console.WriteLine("* update - UnityBuild.h.");
            else
                System.Console.WriteLine("* skip update");
            return result;
        }

        private static void SetChangedHeaderCpp()
        {
            System.Console.WriteLine("----- update h cpp -----");
            string tempUnityBuildFilePath = rootPath + @"UnityBuild\UnityBuild.h";
            StreamWriter writer;
            writer = File.CreateText(tempUnityBuildFilePath);
            writer.Write("");
            writer.WriteLine(@"#pragma once");
            foreach (var item in headerList)
                writer.WriteLine("#include \"" + item + "\"");
            foreach (var item in cppList)
                writer.WriteLine("#include \"" + item + "\"");
            writer.Close();
        }

        private static void SetProjectProperty()
        {
            System.Console.WriteLine("----- set project property -----");
            System.Console.WriteLine(String.Format("* cheock {0}.vcxproj.", projectName));
            string tempUnityBuildHeaderPath = rootPath + @"\" + projectName + ".vcxproj";
            if (!File.Exists(tempUnityBuildHeaderPath))
            {
                System.Console.WriteLine(String.Format("  not exist {0}.vcxproj.", projectName));
                return;
            }
            System.Console.WriteLine("* enter excluded option check.");
            var lineList = File.ReadAllLines(tempUnityBuildHeaderPath).ToList();
            bool result = false;
            foreach (var item in headerList)
            {
                if (item.Contains("UnityBuild.h"))
                    continue;

                string tempInclude = "    <ClInclude Include=\"" + item + "\" />";
                var finsIndex = lineList.FindIndex(x => x == tempInclude);
                if (-1 != finsIndex)
                {
                    result |= true;
                    System.Console.WriteLine(String.Format("  Create : {0} => excluded.", item));
                    lineList[finsIndex] =
                        "    <ClInclude Include = \"" + item + "\">\n"
                        + "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'ReleaseUnity|Win32\'\">true</ExcludedFromBuild>\n"
                        + "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'ReleaseUnity|x64\'\">true</ExcludedFromBuild>\n"
                        + "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'DebugUnity|Win32\'\">true</ExcludedFromBuild>\n"
                        + "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'DebugUnity|x64\'\">true</ExcludedFromBuild>\n"
                        + "    </ClInclude>";
                    continue;
                }
                tempInclude = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'DebugUnity|x64\'\">false</ExcludedFromBuild>";
                finsIndex = lineList.FindIndex(x => x == tempInclude);
                if (-1 != finsIndex)
                {
                    result |= true;
                    System.Console.WriteLine(String.Format("  Modify : {0} => DebugUnity|x64.", item));
                    lineList[finsIndex] = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'DebugUnity|x64\'\">true</ExcludedFromBuild>";
                    continue;
                }

                tempInclude = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'DebugUnity|Win32\'\">false</ExcludedFromBuild>";
                finsIndex = lineList.FindIndex(x => x == tempInclude);
                if (-1 != finsIndex)
                {
                    result |= true;
                    System.Console.WriteLine(String.Format("  Modify : {0} => DebugUnity|Win32.", item));
                    lineList[finsIndex] = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'DebugUnity|Win32\'\">true</ExcludedFromBuild>";
                    continue;
                }

                tempInclude = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'ReleaseUnity|x64\'\">false</ExcludedFromBuild>";
                finsIndex = lineList.FindIndex(x => x == tempInclude);
                if (-1 != finsIndex)
                {
                    result |= true;
                    System.Console.WriteLine(String.Format("  Modify : {0} => ReleaseUnity|x64.", item));
                    lineList[finsIndex] = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'ReleaseUnity|x64\'\">true</ExcludedFromBuild>";
                    continue;
                }

                tempInclude = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'ReleaseUnity|Win32\'\">false</ExcludedFromBuild>";
                finsIndex = lineList.FindIndex(x => x == tempInclude);
                if (-1 != finsIndex)
                {
                    result |= true;
                    System.Console.WriteLine(String.Format("  Modify : {0} => ReleaseUnity|Win32.", item));
                    lineList[finsIndex] = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'ReleaseUnity|Win32\'\">true</ExcludedFromBuild>";
                    continue;
                }
            }
            foreach (var item in cppList)
            {
                if (item.Contains("UnityBuild.cpp"))
                    continue;

                string tempInclude = "    <ClInclude Include=\"" + item + "\" />";
                var finsIndex = lineList.FindIndex(x => x == tempInclude);
                if (-1 != finsIndex)
                {
                    result |= true;
                    System.Console.WriteLine(String.Format("  Create : {0} => excluded", item));
                    lineList[finsIndex] =
                        "    <ClInclude Include = \"" + item + "\">\n"
                        + "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'ReleaseUnity|Win32\'\">true</ExcludedFromBuild>\n"
                        + "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'ReleaseUnity|x64\'\">true</ExcludedFromBuild>\n"
                        + "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'DebugUnity|Win32\'\">true</ExcludedFromBuild>\n"
                        + "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'DebugUnity|x64\'\">true</ExcludedFromBuild>\n"
                        + "    </ClInclude>";
                    continue;
                }
                tempInclude = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'DebugUnity|x64\'\">false</ExcludedFromBuild>";
                finsIndex = lineList.FindIndex(x => x == tempInclude);
                if (-1 != finsIndex)
                {
                    result |= true;
                    System.Console.WriteLine(String.Format("  Modify : {0} => DebugUnity|x64", item));
                    lineList[finsIndex] = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'DebugUnity|x64\'\">true</ExcludedFromBuild>";
                    continue;
                }

                tempInclude = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'DebugUnity|Win32\'\">false</ExcludedFromBuild>";
                finsIndex = lineList.FindIndex(x => x == tempInclude);
                if (-1 != finsIndex)
                {
                    result |= true;
                    System.Console.WriteLine(String.Format("  Modify : {0} => DebugUnity|Win32", item));
                    lineList[finsIndex] = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'DebugUnity|Win32\'\">true</ExcludedFromBuild>";
                    continue;
                }

                tempInclude = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'ReleaseUnity|x64\'\">false</ExcludedFromBuild>";
                finsIndex = lineList.FindIndex(x => x == tempInclude);
                if (-1 != finsIndex)
                {
                    result |= true;
                    System.Console.WriteLine(String.Format("  Modify : {0} => ReleaseUnity|x64", item));
                    lineList[finsIndex] = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'ReleaseUnity|x64\'\">true</ExcludedFromBuild>";
                    continue;
                }

                tempInclude = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'ReleaseUnity|Win32\'\">false</ExcludedFromBuild>";
                finsIndex = lineList.FindIndex(x => x == tempInclude);
                if (-1 != finsIndex)
                {
                    result |= true;
                    System.Console.WriteLine(String.Format("  Modify : {0} => DebugUnity|Win32", item));
                    lineList[finsIndex] = "      <ExcludedFromBuild Condition=\"\'$(Configuration)|$(Platform)\'==\'ReleaseUnity|Win32\'\">true</ExcludedFromBuild>";
                    continue;
                }
            }
            if (result)
            {
                System.Console.WriteLine(String.Format("* modify : {0}.vcxproj", projectName));
                File.WriteAllLines(tempUnityBuildHeaderPath, lineList.ToArray());
            }
            else
                System.Console.WriteLine(String.Format("* unredacted {0}.vcxproj", projectName));
        }
    }
}
