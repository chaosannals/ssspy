using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Darkit.Logging;

namespace SSSpy
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void onStartup(object sender, StartupEventArgs e)
        {
            Log.Init();
            AppDomain.CurrentDomain.UnhandledException += (s, en) =>
            {
                Log.Error("捕获了漏掉的异常：{0}", en.ExceptionObject.ToString());
            };
            AppDomain.CurrentDomain.ProcessExit += (s, en) => {
                Log.Quit();
            };
        }
    }
}
