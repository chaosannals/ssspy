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
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;
using SSSpy.Models;

namespace SSSpy.Pages
{
    public class SettingPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MsTypeInfo ssType;
        private string ssHost;
        private string ssUser;
        private string ssPass;
        private DatabaseInfo ssDatabase;
        private List<DatabaseInfo> ssDatabases = new List<DatabaseInfo>();
        private List<TableInfo> ssTables = new List<TableInfo>();

        public MsTypeInfo SsType
        {
            get { return ssType; }
            set
            {
                ssType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SsType"));
            }
        }

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
        public List<MsTypeInfo> Types { get; private set; }
        public SettingPageModel Model { get; private set; }

        public SettingPage()
        {
            Types = new List<MsTypeInfo>
            {
                new MsTypeInfo { id = 1, name = "Windows 认证" },
                new MsTypeInfo { id = 2, name = "SQL Server TCP" }
            };
            Model = new SettingPageModel
            {
                SsType = Types[0],
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
            using (var session = NewSession())
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
            using (var session = NewSession())
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
            new Thread(() =>
            {
                using (var session = NewSession())
                {
                    try
                    {
                        mw.Model.Visibility = Visibility.Visible;
                        var tables = session.Search<TableInfo>(
                            string.Format("SELECT * FROM {0}.sys.SysObjects WHERE [xtype]='U' ORDER BY [name]", Model.SsDatabase.name)
                        );
                        foreach (var ti in tables)
                        {
                            ti.rowCount = session.Count(string.Format("SELECT COUNT(*) FROM {0}.dbo.{1}", Model.SsDatabase.name, ti.name));
                        }
                        Model.SsTables = tables;
                    }
                    catch (Exception ex)
                    {
                        mw.Say(ex.Message);
                    }
                    finally
                    {
                        mw.Model.Visibility = Visibility.Collapsed;
                    }
                }
            }).Start();
        }

        public MsSQLSession NewSession()
        {
            if (Model.SsType.id == 1)
            {
                return new MsSQLSession(Model.SsHost, Model.SsUser, Model.SsPass);
            }
            return new MsSQLSession(Model.SsHost, Model.SsUser, Model.SsPass, false);
        }
    }
}
