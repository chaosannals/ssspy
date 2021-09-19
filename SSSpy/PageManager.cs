using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SSSpy
{
    public class PageManager : DependencyObject
    {
        public static readonly DependencyProperty ToPageProperty =
            DependencyProperty.RegisterAttached("ToPage", typeof(string), typeof(PageManager));

        public static string GetToPage(DependencyObject obj)
        {
            return obj.GetValue(ToPageProperty) as string;
        }

        public static void SetToPage(DependencyObject obj, string value)
        {
            obj.SetValue(ToPageProperty, value);
        }
    }
}
