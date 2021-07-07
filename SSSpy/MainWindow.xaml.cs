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
using System.Windows.Threading;
using System.ComponentModel;

namespace SSSpy
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Visibility visibility;

        public Visibility Visibility
        {
            get { return visibility; }
            set
            {
                visibility = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
            }
        }
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowModel Model { get; private set; }

        public MainWindow()
        {
            Model = new MainWindowModel
            {
                Visibility = Visibility.Collapsed
            };
            InitializeComponent();
            DataContext = this;
        }

        public void Switch(string name)
        {
            string path = string.Format("Pages/{0}Page.xaml", name);
            PageFrame.Navigate(path);
        }

        public void Say(string text)
        {
            new Thread(() =>
            {
                Border b = null;
                Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                {
                    b = new Border();
                    b.Background = new SolidColorBrush(Colors.AliceBlue);
                    b.CornerRadius = new CornerRadius(5);
                    b.Margin = new Thickness(0, 5, 0, 5);
                    TextBlock tb = new TextBlock();
                    tb.Margin = new Thickness(10);
                    tb.Text = text;
                    b.Child = tb;
                    MessageStack.Children.Add(b);
                });
                Thread.Sleep(3000);
                Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                {
                    MessageStack.Children.Remove(b);
                });
            }).Start();
        }
    }
}
