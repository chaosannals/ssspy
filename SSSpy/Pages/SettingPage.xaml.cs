using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using SSSpy.Models;

namespace SSSpy.Pages
{
    public class SettingPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string ssHost;
        private string ssUser;
        private string ssPass;
        private DatabaseInfo ssDatabase;
        private List<DatabaseInfo> ssDatabases = new List<DatabaseInfo>();
        private List<TableInfo> ssTables = new List<TableInfo>();

        public string SsHost
        {
            get { return ssHost; }
            set
            {
                ssHost = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SsHost"));
            }
        }

        public string SsUser
        {
            get { return ssUser; }
            set
            {
                ssUser = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SsUser"));
            }
        }

        public string SsPass
        {
            get { return ssPass; }
            set
            {
                ssPass = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SsPass"));
            }
        }

        public DatabaseInfo SsDatabase
        {
            get { return ssDatabase; }
            set
            {
                ssDatabase = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SsDatabase"));
            }
        }

        public List<DatabaseInfo> SsDatabases
        {
            get { return ssDatabases; }
            set
            {
                ssDatabases = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SsDatabases"));
            }
        }

        public List<TableInfo> SsTables
        {
            get { return ssTables; }
            set
            {
                ssTables = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SsTables"));
            }
        }
    }

    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        public SettingPageModel Model { get; private set; }

        public SettingPage()
        {
            Model = new SettingPageModel
            {
                SsHost = "(local)",
                SsUser = Environment.MachineName + "/" + Environment.UserName,
                SsPass = "",
            };
            InitializeComponent();
            DataContext = this;
        }

        private void onClickSaveButton(object sender, RoutedEventArgs e)
        {
            MainWindow mw = Window.GetWindow(this) as MainWindow;
            mw.Say("保存成功");
        }

        private void onClickTestButton(object sender, RoutedEventArgs e)
        {
            MainWindow mw = Window.GetWindow(this) as MainWindow;
            using (var session = new MsSQLSession(Model.SsHost, Model.SsUser, Model.SsPass))
            {
                try
                {
                    session.EnsureConnection();
                    mw.Say("测试链接数据库成功");
                }
                catch (Exception ex)
                {
                    mw.Say(ex.Message);
                }
            }
        }

        private void onClickShowButton(object sender, RoutedEventArgs e)
        {
            MainWindow mw = Window.GetWindow(this) as MainWindow;
            using (var session = new MsSQLSession(Model.SsHost, Model.SsUser, Model.SsPass))
            {
                try
                {
                    Model.SsDatabases = session.Search<DatabaseInfo>(
                        "SELECT * FROM MASTER.DBO.SYSDATABASES ORDER BY [name]"
                    );
                    if (Model.SsDatabases.Count > 0)
                    {
                        Model.SsDatabase = Model.SsDatabases[0];
                    }
                }
                catch (Exception ex)
                {
                    mw.Say(ex.Message);
                }
            }
        }

        private void onClickShowTablesButton(object sender, RoutedEventArgs e)
        {
            MainWindow mw = Window.GetWindow(this) as MainWindow;
            using (var session = new MsSQLSession(Model.SsHost, Model.SsUser, Model.SsPass))
            {
                try
                {
                    Model.SsTables = session.Search<TableInfo>(
                        string.Format("SELECT * FROM {0}.sys.SysObjects WHERE [xtype]='U' ORDER BY [name]", Model.SsDatabase.name)
                    );
                }
                catch (Exception ex)
                {
                    mw.Say(ex.Message);
                }
            }
        }
    }
}
