using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Nextension
{
    public class GuiCommand : NSingleton<GuiCommand>
    {
        [SerializeField] private GuiCommandUIManager _uiManager;
        [SerializeField] private bool _usePersistentSaveFile;
        [SerializeField] private string[] _defaultCommands;

        private static Dictionary<string, NAction<CommandData>> _cmdListeners = new();

        public static readonly Dictionary<string, string> CommandInfos = new();

        protected override void onInitialized()
        {
            foreach (var cmdInput in _defaultCommands)
            {
                _uiManager.addSavedCmd(cmdInput, false);
            }
            foreach (var cmdInput in readSavedCommandFromFile())
            {
                _uiManager.addSavedCmd(cmdInput, true);
            }
        }

        private static List<Exception> __invokeCmd(CommandData cmdData)
        {
            List<Exception> errors = new();
            if (_cmdListeners.TryGetValue(cmdData.CmdName, out var listeners) && listeners.Count > 0)
            {
                foreach (var listener in listeners)
                {
                    try
                    {
                        listener.Invoke(cmdData);
                    }
                    catch (Exception e)
                    {
                        errors.Add(e);
                    }
                }
            }
            else
            {
                errors.Add(new Exception("Invalid command"));
            }
            return errors;
        }

        public static void register(string cmdName, Action<CommandData> action)
        {
            if (!_cmdListeners.TryGetValue(cmdName, out var listener))
            {
                listener = new NAction<CommandData>();
                _cmdListeners.Add(cmdName, listener);
            }
            listener.add(action);
        }
        
        public static void unregister(string cmdName, Action<CommandData> action)
        {
            if (_cmdListeners.TryGetValue(cmdName, out var actions))
            {
                actions.remove(action);
            }
        }
        
        public static List<Exception> runCmd(string cmdInput)
        {
            var cmdData = new CommandData(cmdInput);
            return __invokeCmd(cmdData);
        }

        public static bool addCmdInfo(string arg0, string info)
        {
            if (CommandInfos.ContainsKey(arg0))
            {
                return false;
            }
            CommandInfos.Add(arg0, info);
            return true;
        }
        
        public static bool addCmdInfo(string arg0, string arg1, string info)
        {
            var cmdPrefix = $"{arg0} {arg1}";
            if (CommandInfos.ContainsKey(cmdPrefix))
            {
                return false;
            }
            CommandInfos.Add(cmdPrefix, info);
            return true;
        }
        
        public static bool addCmdInfo(string arg0, string arg1, string arg2, string info)
        {
            var cmdPrefix = $"{arg0} {arg1} {arg2}";
            if (CommandInfos.ContainsKey(cmdPrefix))
            {
                return false;
            }
            CommandInfos.Add(cmdPrefix, info);
            return true;
        }

        public static string getCmdInfo(string cmdInput)
        {
            var cmdData = new CommandData(cmdInput);
            var args = cmdData.args;

            var cmdPrefix = new StringBuilder(args[0]);
            if (CommandInfos.TryGetValue(cmdPrefix.ToString(), out var info))
            {
                return info;
            }
            for (int i = 1; i < args.Length; i++)
            {
                cmdPrefix.Append(" ");
                cmdPrefix.Append(cmdData.args[i]);
                if (CommandInfos.TryGetValue(cmdPrefix.ToString(), out info))
                {
                    return info;
                }
            }
            return "No description";
        }
    
        public static Span<string> readSavedCommandFromFile()
        {
            if (!Instance._usePersistentSaveFile) return Span<string>.Empty;

            try
            {
                var fileName = "SavedCmdInputJson.json";
                var path = Path.Combine(Application.persistentDataPath, fileName);
                if (!File.Exists(path)) return Span<string>.Empty;
                var json = File.ReadAllText(path);
                var savedInputJson = JsonUtility.FromJson<SavedCmdInputJson>(json);
                return savedInputJson.cmdInputs.asSpan();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return Span<string>.Empty;
            }
        }

        public static void storeSavedCommandToFile(IEnumerable<string> cmdInputs)
        {
            if (!Instance._usePersistentSaveFile) return;
            var savedInputJson = new SavedCmdInputJson();
            foreach (var cmdInput in cmdInputs)
            {
                savedInputJson.cmdInputs.Add(cmdInput);
            }

            var fileName = "SavedCmdInputJson.json";
            var path = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (savedInputJson.cmdInputs.Count > 0)
            {
                File.WriteAllText(path, JsonUtility.ToJson(savedInputJson));
            }
        }
    }

    public class CommandData
    {
        public readonly string[] args;
        public string CmdName => args[0];
        internal CommandData(string input)
        {
            var matches = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+");
            var args = new List<string>();

            foreach (Match m in matches)
            {
                args.Add(m.Value.Trim('"'));
            }

            this.args = args.ToArray();
        }
    }

    public class SavedCmdInputJson
    {
        public List<string> cmdInputs = new();
    }
}
