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

namespace SSSpy.Pages
{
    /// <summary>
    /// IndexPage.xaml 的交互逻辑
    /// </summary>
    public partial class IndexPage : Page
    {
        public IndexPage()
        {
            InitializeComponent();
        }

        private void OnClickSwitchButton(object sender, RoutedEventArgs e)
        {
            string page = PageManager.GetToPage(sender as DependencyObject);
            MainWindow mw = Window.GetWindow(this) as MainWindow;
            mw.Switch(page);
        }
    }
}
