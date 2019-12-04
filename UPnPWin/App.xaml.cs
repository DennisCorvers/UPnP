using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace UPnPWin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if DEBUG
        public App()
        {
            Open.Nat.NatDiscoverer.TraceSource.Switch.Level = System.Diagnostics.SourceLevels.All;
        }
#endif
    }
}
