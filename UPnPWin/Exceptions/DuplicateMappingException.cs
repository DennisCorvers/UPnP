using Open.Nat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPnPWin.Exceptions
{
    public class DuplicateMappingException : Exception
    {
        public Mapping DuplicateMapping
        { get; }

        public DuplicateMappingException(Mapping duplicateMapping)
        { DuplicateMapping = duplicateMapping; }
    }
}
