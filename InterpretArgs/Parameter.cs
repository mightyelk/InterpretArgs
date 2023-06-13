using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpretArgs
{
    internal class Parameter
    {
        public bool IsSet;
        public bool Mandatory;

        public Type ValueType;
        public object Value;
        public string Name;
        public string Description;
        public string Example;

    }
}
