using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpretArgs
{
    public  class Argument
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ValueDescription { get; set; }

        public bool Mandatory { get; set; }
        public bool IsSet { get; private set; }
        public bool IsArray { get; set; }

        public Type TypeOfValue { get; private set; }

        public string StringVal { get; set; }
        public string[] StringArrayVal { get; set; }

        public int IntVal { get; set; }
        public int[] IntArrayVal { get; set; }

        public DateTime DateVal { get; set; }
        public bool BoolVal { get; set; }

        public Argument(Type typeOfValue)
        {
            TypeOfValue = typeOfValue;
            IsSet = false;
        }

        public void SetValue(object val)
        {
            Type t = val.GetType();

            if (IsArray)
            {
                var dummy= Array.CreateInstance(TypeOfValue, 0);
                if (!t.Equals(dummy.GetType()))
                    throw new InvalidCastException("Passed value does not match the arguments type.");
            }
            else if (!t.Equals(TypeOfValue))
                throw new InvalidCastException("Passed value does not match the arguments type.");

            if (t.Equals(typeof(string)))
                StringVal = (string)val;
            if (t.Equals(typeof(int)))
                IntVal = (int)val;
            if (t.Equals(typeof(bool)))
                BoolVal = (bool)val;
            if (t.Equals(typeof(DateTime)))
                DateVal = (DateTime)val;


            if (t.Equals(typeof(int[])))
                IntArrayVal = (int[])val;
            if (t.Equals(typeof(string[])))
                StringArrayVal = (string[])val;

            IsSet = true;
        }
    }

}
