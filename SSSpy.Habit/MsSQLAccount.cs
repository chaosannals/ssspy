using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSSpy.Habit
{
    public class MsSQLAccount
    {
        public int Type { get; set; }
        public string Host { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
