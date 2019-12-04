using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UPnPWin.Utils
{
    internal struct ThreadSafeBool
    {
        private int m_value;

        public bool Value
        {
            get { return (Interlocked.CompareExchange(ref m_value, 1, 1) == 1); }
            set
            {
                if (value) Interlocked.CompareExchange(ref m_value, 1, 0);
                else Interlocked.CompareExchange(ref m_value, 0, 1);
            }
        }
    }
}
