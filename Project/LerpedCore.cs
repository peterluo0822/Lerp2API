using Lerp2API.Communication.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = Lerp2API.DebugHandler.Debug;

namespace Lerp2API
{
    public class LerpedCore : MonoBehaviour
    {
        private static Dictionary<string, object> _storedInfo;
        private static string storePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "L2API.sav");
        public const string UnityBoot = "UNITY_STARTED_UP", enabledDebug = "ENABLE_DEBUG", loggerPath = "LOG_PATH", consoleSymLinkPath = "CONSOLE_SYMLINK_PATH",
                            defaultLogFilePath = "Logs/Console.log";
        public static GameObject lerpedCore;
        public static string SystemTime
        {
            get
            {
                return DateTime.Now.ToString("h:mm:ss.ffffff");
            }
        }

        public static float UnityTick
        {
            get
            {
                return Time.fixedDeltaTime;
            }
        }

        public static bool CheckUnityVersion(int mainVer, int subVer)
        {
            char[] separator = new char[] { '.' };
            int[] numArray = Application.unityVersion.Split(separator).Select(x => GetVersionValue(x)).ToArray();
            return ((mainVer < numArray[0]) || ((mainVer == numArray[0]) && (subVer <= numArray[1])));
        }

        private static int GetVersionValue(string strNumber)
        {
            int result = 0;
            int.TryParse(strNumber, out result);
            return result;
        }

        public static bool GetBool(string key)
        {
            return storedInfo.ContainsKey(key) && ((bool)storedInfo[key]);
        }

        public static void SetBool(string key, bool value)
        {
            if (!storedInfo.ContainsKey(key))
                storedInfo.Add(key, value);
            else
                storedInfo[key] = value;
            JSONHelpers.SerializeToFile(storePath, storedInfo, true);
        }

        public static string GetString(string key)
        {
            if (storedInfo.ContainsKey(key))
                return (string)storedInfo[key];
            else
                return "";
        }

        public static void SetString(string key, string value)
        {
            if (!storedInfo.ContainsKey(key))
                storedInfo.Add(key, value);
            else
                storedInfo[key] = value;
            JSONHelpers.SerializeToFile(storePath, storedInfo, true);
        }

        public static int GetInt(string key)
        {
            if (storedInfo.ContainsKey(key))
                return (int)storedInfo[key];
            else
                return 0;
        }

        public static void SetInt(string key, int value)
        {
            if (!storedInfo.ContainsKey(key))
                storedInfo.Add(key, value);
            else
                storedInfo[key] = value;
            JSONHelpers.SerializeToFile(storePath, storedInfo, true);
        }

        public static long GetLong(string key)
        {
            if (storedInfo.ContainsKey(key))
                return (long)storedInfo[key];
            else
                return 0;
        }

        public static void SetLong(string key, long value)
        {
            if (!storedInfo.ContainsKey(key))
                storedInfo.Add(key, value);
            else
                storedInfo[key] = value;
            JSONHelpers.SerializeToFile(storePath, storedInfo, true);
        }

        private static Dictionary<string, object> storedInfo
        {
            get
            {
                if (_storedInfo == null)
                    if (File.Exists(storePath))
                        _storedInfo = JSONHelpers.DeserializeFromFile<Dictionary<string, object>>(storePath);
                    else
                        _storedInfo = new Dictionary<string, object>();
                return _storedInfo;
            }
            set
            {
                _storedInfo = value;
            }
        }

        public static GameObject AutoHookCore()
        {
            GameObject core = GameObject.Find("Lerp2Core");
            if (core == null)
                core = new GameObject("Lerp2Core");
            return core;
        }

        public static void HookThis(Type type, int waitSecs = 1)
        {
            int i = DateTime.Now.Second + waitSecs; //Wait desired seconds for lerpedCore to be assigned 
            if (lerpedCore == null) lerpedCore = AutoHookCore();
            //while (lerpedCore == null && DateTime.Now.Second < i) { }
            if (lerpedCore.GetComponent(type) == null)
                lerpedCore.AddComponent(type);
        }

        public static void ChangeRequiredASM(string ASMName, string ASMFolder)
        {
            string destFile = Path.Combine(Application.streamingAssetsPath, "RequiredASM/" + ASMName + ".dll"),
                   originFile = ASMFolder + "/" + ASMName + ".dll",
                   destFolder = Path.Combine(Application.streamingAssetsPath, "RequiredASM");
            if (!File.Exists(destFile))
            {
                if (!Directory.Exists(destFolder))
                    Directory.CreateDirectory(destFolder);
                if (File.Exists(originFile))
                    File.Copy(originFile, destFile);
            }
        }
    }

    public class ConsoleListener
    {
        public static void StartConsole()
        {
            //Debug.LogFormat("Attemping to open the console in {0}.", Application.dataPath);
            string console = Path.Combine(Application.dataPath, "Lerp2API/Console/Lerp2Console.exe"),
                   curLoc = Assembly.GetExecutingAssembly().Location;
            if (!File.Exists(console))
            {
                Debug.LogErrorFormat("Console app couldn't be found in its default path ({0}).", console);
                return;
            }
            if (Application.isEditor)
                LerpedCore.ChangeRequiredASM("UnityEngine", Path.Combine(Path.GetDirectoryName(Application.dataPath), "Library/UnityAssemblies"));
            string[] mklinkspaths = new string[2] { Path.Combine(Path.GetDirectoryName(console), "Lerp2API.dll"), Path.Combine(Path.GetDirectoryName(console), "UnityEngine.dll") };
            bool domklink = mklinkspaths.All(x => !File.Exists(x)), 
                 redomklink = LerpedCore.GetString(LerpedCore.consoleSymLinkPath) != Path.GetDirectoryName(curLoc);
            string mklinks = (!File.Exists(mklinkspaths[0]) ? string.Format(@"mklink ""{0}"" ""{1}""", mklinkspaths[0], curLoc) : "")
                + (domklink ? " & " : "")
                + (!File.Exists(mklinkspaths[1]) ? string.Format(@"mklink ""{0}"" ""{1}""", mklinkspaths[1], Path.Combine(Application.streamingAssetsPath, "RequiredASM/UnityEngine.dll")) : "");
            if (domklink || redomklink)
            {
                LerpedCore.SetString(LerpedCore.consoleSymLinkPath, Path.GetDirectoryName(curLoc));
                using (Process p = new Process())
                { //This is a little bit problematic, because it can cause a crash due to a recompilation on the UnityEngine.dll (because of the smybolic link)
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.Arguments = string.Format(@"/c " + mklinks);
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.Verb = "runas";
                    p.Start();
                    p.WaitForExit();
                }
            }
            using (Process p = new Process())
            {
                p.StartInfo.FileName = console;
                p.StartInfo.Arguments = string.Format(@"-projectPath={0}{1}", Application.dataPath, Application.isEditor ? " -editor" : "");
                p.Start();
            }
        }
    }

    public class ConsoleSender
    {
        //private static List<string> paths = new List<string>();
        //public static PipeClient l2dStream;
        public SocketClient l2dClient;
        private ConsoleLogger logger;
        public ConsoleSender(string path)
        {
            l2dClient = new SocketClient();
            l2dClient.DoConnection();

            if (!File.Exists(path))
                File.Create(path);

            logger = new ConsoleLogger(path);
        }
        public void SendMessage(LogType lt, string ls, string st)
        {
            //if (!paths.Contains(path))
            //    paths.Add(path);
            //File.AppendAllText(path, Environment.NewLine + JsonUtility.ToJson(new ConsoleMessage(lt, ls, st)));
            //if(l2dStream == null)
            //    l2dStream = new PipeClient(GetServerID());

            //l2dStream.SendMessage(string.Format("{0}\n{1}", ls, st));

            if (l2dClient == null)
            {
                l2dClient = new SocketClient();
                l2dClient.DoConnection();
            }

            l2dClient.WriteLine(ls);
            logger.SendLog(ls);

            //Tengo que quitar el path, tengo que ver lo de los colores...
        }
        /*public static void Quit()
        {
            //Tengo q comprimir el log... Todo esto cuando tenga una carpeta q se llame logs y este codigo este dentro de la api
            //Al tener la api ya no me va a hacer falta tener q estar diciendole q se ejecute todos los eventos, ni los archivos definidos en otros scripts ni nada
            //Ni tampoco estar diciendole a esta clase donde esta el ejecutable pq debe ir siempre acompaņado de la api al tenerlo como referencia dentro de si
            //Y todo estar mas compactado tanto las clases como los metodos...
            foreach (string p in paths)
                if (File.Exists(p))
                    File.Delete(p);
        }*/

        //Obsolete?
        /*private static string GetServerID()
        {
            string path = Path.Combine(Application.dataPath, "Lerp2API/Console/ServerID.GUID");
            if (File.Exists(path))
                return File.ReadAllText(path);
            else
            {
                Debug.LogErrorFormat("File '{0}' doesn't exists. Server ID couldn't be retrieved!", path);
                return "";
            }
        }*/
    }

    public class ConsoleMessage
    {
        public LogType logType;
        public string logString, stackTrace;
        public ConsoleMessage(LogType lt, string ls, string st)
        {
            logType = lt;
            logString = ls;
            stackTrace = st;
        }
    }

    public class ConsoleLogger
    {
        public string path = "";
        public ConsoleLogger(string path)
        {
            this.path = path;
        }
        public void SendLog(string msg)
        {
            File.AppendAllText(path, msg);
        }

        public void ClearLog()
        {
            File.WriteAllText(path, "");
        }
    }
}