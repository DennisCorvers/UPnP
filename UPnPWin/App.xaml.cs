using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace UPnPWin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Substring(0, args.Name.IndexOf(','));

            if (dllName.EndsWith(".resources"))
                return null;

            var asm = Assembly.GetExecutingAssembly();
            dllName = $"{asm.GetName().Name}.Libs.{dllName}.dll";

            using (Stream stream = asm.GetManifestResourceStream(dllName))
            {
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);

                return Assembly.Load(buffer);
            }
        }

        public App()
        {
#if DEBUG
            Open.Nat.NatDiscoverer.TraceSource.Switch.Level = System.Diagnostics.SourceLevels.All;
#endif
        }
    }
}
