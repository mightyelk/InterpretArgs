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

        public string Name;
        public Type ValueType;

        public object? Value;
        public string? Description;
        public string? Example;

        public Parameter(string name, Type type)
        {
            Name = name;
            ValueType = type;
        }
    }
}
