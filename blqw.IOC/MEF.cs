﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace blqw.IOC
{
    /// <summary>
    /// 用于执行MEF相关操作
    /// </summary>
    [Export("Component")]
    public sealed class MEF
    {
        const string GLOBAL_KEY = "O[ON}:z05i$*H75O[bJdnedei#('i_i^";

        /// <summary> 获取默认值
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// 是否正在初始化
        /// </summary>
        /// <returns></returns>
        private static bool IsInitializeing()
        {
            if (Monitor.IsEntered(GLOBAL_KEY))
            {
                return true;
            }
            if (Monitor.TryEnter(GLOBAL_KEY))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 插件容器
        /// </summary>
        [Export("MEF.PlugIns")]
        public static PlugInContainer PlugIns { get; } = Initializer();

        /// <summary> 初始化
        /// </summary>
        public static PlugInContainer Initializer()
        {
            if (IsInitialized || IsInitializeing())
            {
                return null;
            }
            var plugins = new PlugInContainer();
            try
            {
                if (Debugger.IsAttached)
                {
                    Debug.Listeners.Add(new ConsoleTraceListener(true));
                }
                var catalog = GetCatalog();
                plugins.AddCatalog(catalog);
            }
            finally
            {
                IsInitialized = true;
                if (Monitor.IsEntered(GLOBAL_KEY))
                    Monitor.Exit(GLOBAL_KEY);
            }
            return plugins;
        }
        
        /// <summary> 获取插件
        /// </summary>
        /// <returns></returns>
        private static ComposablePartCatalog GetCatalog()
        {
            var dir = new DirectoryCatalog(".").FullPath;
            var files = Directory.EnumerateFiles(dir, "*.dll", SearchOption.AllDirectories)
                .Union(Directory.EnumerateFiles(dir, "*.exe", SearchOption.AllDirectories));
            var logs = new AggregateCatalog();
            foreach (var file in files)
            {
                try
                {
                    var asmCat = new AssemblyCatalog(file);
                    if (asmCat.Parts.ToList().Count > 0)
                        logs.Catalogs.Add(asmCat);
                }
                catch (Exception)
                {
                }
            }
            return logs;
        }
        
    }
}