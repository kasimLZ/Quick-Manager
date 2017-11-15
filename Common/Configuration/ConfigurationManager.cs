using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CoreConsoleTest.ReadJson
{
    public static class ConfigurationManager
    {
        /// <summary>
        /// 配置文件
        /// </summary>
        private static IConfiguration _configuration = null;

        /// <summary>
        /// 配置监听文件列表
        /// </summary>
        private static List<KeyValuePair<string, FileSystemWatcher>> FileListeners = new List<KeyValuePair<string, FileSystemWatcher>>();

        /// <summary>
        /// 默认路径
        /// </summary>
        private static string _defaultPath = Directory.GetCurrentDirectory() + @"\appsettings.json";

        /// <summary>
        /// 最终配置文件路径
        /// </summary>
        private static string _configPath = null;

        /// <summary>
        /// 配置节点关键字
        /// </summary>
        private static string _configSection = "AppSettings";

        /// <summary>
        /// 配置外连接的后缀
        /// </summary>
        private static string _configUrlPostfix = "Url";

        /// <summary>
        /// 最终修改时间戳
        /// </summary>
        private static long _timeStamp = 0L;

        /// <summary>
        /// 配置外链关键词，例如：AppSettings.Url
        /// </summary>
        private static string _configUrlSection = _configSection + "." + _configUrlPostfix;

        /// <summary>
        /// 配置缓存
        /// </summary>
        private static NameValueCollection _settingCache = new NameValueCollection();

        static ConfigurationManager()
        {
            ConfigFinder(_defaultPath);
        }

        /// <summary>
        /// 确定配置文件路径
        /// </summary>
        private static void ConfigFinder(string filepath)
        {
            _configPath = filepath;
            IConfigurationBuilder configBuilder = new ConfigurationBuilder();
            FileInfo info = new FileInfo(_configPath);
            while (info != null && info.Exists)
            {
                //防止循环链路
                if (FileListeners.Any(a => a.Key.Equals(info.FullName))) break;

                configBuilder = configBuilder.AddJsonFile(info.FullName, true, false);
                FileListeners.Add(CreateListener(info));
                IConfiguration config = configBuilder.Build();

                var childpath = config.GetSection(_configUrlSection).Value;
                info = (!string.IsNullOrEmpty(childpath) && !_configPath.Equals(childpath)) ? new FileInfo(_configPath = childpath) : null;
            }
            _configuration = configBuilder.Build();
        }


        /// <summary>
        /// 添加监听树节点
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static KeyValuePair<string, FileSystemWatcher> CreateListener(FileInfo info)
        {

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.BeginInit();
            watcher.Path = info.DirectoryName;
            watcher.Filter = info.Name;
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size;
            watcher.Changed += new FileSystemEventHandler(ConfigChangeListener);
            watcher.EndInit();

            return new KeyValuePair<string, FileSystemWatcher>(info.FullName, watcher);

        }

        private static void ConfigChangeListener(object sender, FileSystemEventArgs e)
        {
            long time = TimeStamp();
            lock (FileListeners)
            {
                if (time > _timeStamp)
                {
                    _timeStamp = time;
                    //释放所有监听对象
                    FileListeners.ForEach(a => a.Value.Dispose());
                    FileListeners.Clear();
                    ConfigFinder(_defaultPath);
                }
            }
        }

        private static long TimeStamp()
        {
            return (long)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 100);
        }

        public static string ConfigSection
        {
            get { return _configSection; }
            set { _settingCache["_configSection"] = value; }
        }

        public static string ConfigUrlPostfix
        {
            get { return _configUrlPostfix; }
            set { _settingCache["_configUrlPostfix"] = value; }
        }

        public static string DefaultPath
        {
            get { return _defaultPath; }
            set { _settingCache["_defaultPath"] = value; }
        }

        public static string AppSettings(string key)
        {
            return _configuration.GetSection(_configSection + ":" + key).Value;
        }

        /// <summary>
        /// 手动刷新配置，修改配置后，请手动调用此方法，以便更新配置参数
        /// </summary>
        public static void RefreshConfiguration()
        {
            lock (FileListeners)
            {
                BindingFlags flag = BindingFlags.Static | BindingFlags.NonPublic;
                //修改配置
                foreach (var setting in _settingCache.AllKeys)
                {
                    FieldInfo f_key = typeof(ConfigurationManager).GetField(setting, flag);
                    if (f_key != null) f_key.SetValue(null, _settingCache[setting]);
                }
                if (_settingCache.Count > 0) _configUrlSection = _configSection + "." + _configUrlPostfix;
                _settingCache.Clear();

                FileListeners.ForEach(a => a.Value.Dispose());
                ConfigFinder(_defaultPath);
            }
        }

    }
}
