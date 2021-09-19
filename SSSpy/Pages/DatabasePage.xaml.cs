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
using System.Threading;
using System.ComponentModel;
using Darkit.Logging;
using SSSpy.Models;
using SSSpy.Models.Habits;

namespace SSSpy.Pages
{
    /// <summary>
    /// 数据库页逻辑
    /// </summary>
    public class DatabasePageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private DatabaseInfo database = null;
        private List<DatabaseInfo> databases = new List<DatabaseInfo>();
        private List<TableInfo> tables = new List<TableInfo>();

        public DatabaseInfo Database
        {
            get { return database; }
            set
            {
                database = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Database"));
            }
        }

        public List<DatabaseInfo> Databases
        {
            get { return databases; }
            set
            {
                databases = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Databases"));
            }
        }

        public List<TableInfo> Tables
        {
            get { return tables; }
            set
            {
                tables = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tables"));
            }
        }
    }

    /// <summary>
    /// DatabasePage.xaml 的交互逻辑
    /// </summary>
    public partial class DatabasePage : Page
    {
        public SQLServerAccount Account { get; set; }
        public DatabasePageModel Model { get; private set; }

        public DatabasePage()
        {
            Model = new DatabasePageModel();
            InitializeComponent();
            DataContext = this;
        }

        private void onClickShowButton(object sender, RoutedEventArgs e)
        {
            MainWindow mw = Window.GetWindow(this) as MainWindow;
            using (var session = NewSession())
            {
                try
                {
                    Model.Databases = session.Search<DatabaseInfo>(
                        "SELECT * FROM MASTER.DBO.SYSDATABASES ORDER BY [name]"
                    );
                    Log.Info("database count: {0}", Model.Databases.Count);
                    if (Model.Databases.Count > 0)
                    {
                        Model.Database = Model.Databases[0];
                    }
                }
                catch (Exception ex)
                {
                    mw.Say(ex.Message);
                }
            }
        }

        /// <summary>
        /// 获取指定数据库的表信息。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                            string.Format("SELECT * FROM {0}.sys.SysObjects WHERE [xtype]='U' ORDER BY [name]", Model.Database.name)
                        );
                        foreach (var ti in tables)
                        {
                            ti.rowCount = session.Count(string.Format("SELECT COUNT(*) FROM {0}.dbo.{1}", Model.Database.name, ti.name));
                        }
                        Model.Tables = tables;
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

            if (Account.Type == 1)
            {
                return new MsSQLSession(Account.Host, Account.User, Account.Pass);
            }
            return new MsSQLSession(Account.Host, Account.User, Account.Pass, false);
        }
    }
}
