using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;

namespace TaskInstanceQuery
{
 
    public class Settings 
    {
        public string ProcessToCheck { get; set; } = "example.exe";
        public string ServersToQuery { get; set; } = "localhost";
        public bool CheckLocal { get; set; } = true;
    }


}
