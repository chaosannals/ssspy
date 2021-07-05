using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace SSSpy
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void onStartup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, en) =>
            {
                "捕获了漏掉的异常".Log();
                en.ExceptionObject.ToString().Log();
            };
            AppDomain.CurrentDomain.ProcessExit += (s, en) => {
                LogExtends.Over();
            };
        }
    }
}
