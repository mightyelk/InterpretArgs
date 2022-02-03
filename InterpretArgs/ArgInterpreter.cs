using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace InterpretArgs
{
    public class ArgInterpreter
    {
        public enum ValueTypeEnum
        {
            String,
            Number,
            DateTime,
        }

        /// <summary>
        /// returns a formatted usage page
        /// </summary>
        /// <param name="exeFileName">name of executing assembly for first line of usage page: ASSEMBLY.EXE [params]</param>
        /// <returns></returns>
        public string GetUsage(string exeFileName)
        {
            var output = new List<string>();

            output.Add(exeFileName.ToUpper());

            foreach(var a in Arguments)
            {
                var arg = a.Value;
                if (string.IsNullOrWhiteSpace(arg.Name))
                {
                    output.Add(arg.ValueDescription);
                    continue;
                }
                
                string line = (arg.Mandatory) ? "-" + arg.Name + "{0}" : "[-" + arg.Name + "{0}]";
                if (!arg.TypeOfValue.Equals(typeof(Boolean)))
                    line = string.Format(line, string.Format((arg.IsArray) ? " {0} {0}..." : " {0}", arg.ValueDescription));
                else
                    line = string.Format(line, "");

                output.Add(line);

            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            foreach (var a in Arguments)
            {
                var arg = a.Value;
                if (string.IsNullOrWhiteSpace(arg.Name))
                    continue;

                sb.AppendLine(string.Format("\t-{0} {1}\t{2}", arg.Name, arg.ValueDescription, arg.Description));
            }

            return string.Join(" ", output.ToArray()) + sb.ToString(); ;
        }

        
        //for future
        public string[] ArgumentDelimiters { get; set; }


        public Dictionary<string, Argument> Arguments { get; private set; }

       
        public ArgInterpreter()
        {
            Arguments = new Dictionary<string, Argument>();
            ArgumentDelimiters = new string[] { "-" };
        }

        /// <summary>
        /// Registers a flag (boolean) with the interpreter
        /// </summary>
        /// <param name="name">Name of parameter without - or  /</param>
        /// <param name="description">Description for the generated usage page.</param>
        public void RegisterFlag(string name, string description)
        {
            RegisterArg(name, null, description, false, typeof(bool), false);
        }


        /// <summary>
        /// Registers the default parameter with the interpreter. The default parameter is the one without a dash/slash before it e.g. "tool.exe /a file.txt" file.txt would be the default parameter.
        /// </summary>
        /// <param name="valueDescription">Describes the passed value for a parameter, e.g. filename or pagenumber.</param>
        /// <param name="valueType">Type of value after parameter.</param>
        public void RegisterDefault(string valueDescription,ValueTypeEnum valueType)
        {
            RegisterArg("", valueDescription, null, false, valueType, false);
        }

        /// <summary>
        /// Registers a parameter with the interpreter.
        /// </summary>
        /// <param name="name">Name of parameter without - or  /</param>
        /// <param name="valueDescription">Describes the passed value for a parameter, e.g. filename or pagenumber.</param>
        /// <param name="description">Description for the generated usage page.</param>
        /// <param name="mandatory">Is this parameter mandatory?</param>
        /// <param name="valueType">Type of value after parameter.</param>
        /// <param name="isArray">True when multiple values are allowed for parameter.</param>
        public void RegisterArg(string name, string valueDescription, string description, bool mandatory, ValueTypeEnum valueType, bool isArray)
        {
            Type t = typeof(string);
            switch (valueType)
            {
                case ValueTypeEnum.DateTime: t = typeof(DateTime); break;
                //case ValueTypeEnum.Flag: t = typeof(Boolean); break;
                case ValueTypeEnum.String: t = typeof(string); break;
                case ValueTypeEnum.Number: t = typeof(int); break;
            }
            RegisterArg(name, valueDescription, description, mandatory, t, isArray);
        }



        private void RegisterArg(string name, string valueDescription, string description, bool mandatory, Type valueType, bool isArray)
        {
            if (Arguments.ContainsKey(name.ToLower()))
                throw new Exception("Argument already registerd.");

            
            Argument arg = new Argument(valueType); 

            arg.Name = name.ToLower();
            arg.Description = description;
            arg.Mandatory = mandatory;
            arg.IsArray = isArray;
            arg.ValueDescription = valueDescription;
            Arguments.Add(arg.Name, arg);
        }


        /// <summary>
        /// Evaluates the passed string array.
        /// </summary>
        /// <param name="args">Usually the args[] of the main program.</param>
        public void SetArgs(params string[] args)
        { 
            Argument lastArg = null;
            for (int i = 0; i < args.Length; i++)
            {
                //default argument(s) without name
                if (i==0 & !ArgumentDelimiters.Contains(args[0].Substring(0, 1)))
                {
                    if (!Arguments.ContainsKey("")) //no default argument given?
                        continue;

                    Argument arg = Arguments[""];
                    lastArg = arg;
                    if (arg.IsArray)
                    {
                        var z = FindAllValues(args, -1, arg.TypeOfValue);
                        arg.SetValue(z);
                        continue;
                    }
                    
                    arg.SetValue(args[0]);
                    continue;
                }

                //arg starts with one of the specified characters
                if (ArgumentDelimiters.Contains(args[i].Substring(0, 1)))
                {
                    string name = args[i].Substring(1);

                    int whiteSpaceIndex = name.IndexOfAny(new char[] { ' ', '\t' });

                    if (whiteSpaceIndex > 0)
                        name = name.Substring(0, whiteSpaceIndex);

                    if (!Arguments.ContainsKey(name))
                    {
                        throw new Exception(String.Format("Not registered argument passed '{0}'", name));
                    }
                    Argument arg = Arguments[name];
                    lastArg = arg;


                    //just a flag argument without any parameters beeing passed
                    if (arg.TypeOfValue.Equals(typeof(bool)))
                    {
                        arg.SetValue(true);
                        continue;
                    }

                    //is there a value (argument without a specified character) behind the argument? 
                    if (!arg.IsArray && args.Length > i + 1 && !ArgumentDelimiters.Contains(args[i + 1].Substring(0, 1)))
                    {
                        //does that value match the type?
                        if (TryGetValue(args[i + 1], arg.TypeOfValue, out object value))
                        {
                            arg.SetValue(value);
                            i++;
                            continue;
                        }
                        throw new InvalidCastException("Passed value does not match the arguments type.");
                    }

                    //are there multiple values without a specified charagter behind the argument?
                    if (arg.IsArray 
                        && args.Length > i + 1
                        && !ArgumentDelimiters.Contains(args[i + 1].Substring(0, 1)))
                    {
                        var z = FindAllValues(args, i, arg.TypeOfValue);
                        arg.SetValue(z);
                        i += z.Length;
                        continue;
                    }
                    
                    
                    

                    //again something went wrong if code gets here
                    //Array without values?
                    throw new Exception(String.Format("Missing values after argument '{0}'.", name));

                }


                //something went wrong if the code gets here
                //last argument wasn't an array but there are values (not new arguments) behind
                if (!lastArg.IsArray)
                {
                    throw new Exception(String.Format("Unexpected value '{0}' behind argument '{1}'", args[i], lastArg.Name));
                }
            }
        }


        /// <summary>
        /// Returns true if all mandatory parameters are set
        /// </summary>
        /// <returns></returns>
        public bool CheckForMandatoryArgs()
        {
            foreach (var a in Arguments)
                if (a.Value.Mandatory & !a.Value.IsSet)
                    return false;
            return true;
        }

        /// <summary>
        /// Function iterates the command line arguments after the array parameter and tries to fetch all values into an array.
        /// </summary>
        /// <param name="args">All command line arguments</param>
        /// <param name="currentPos">Current position (index) of argument</param>
        /// <param name="elementType">Type to look for</param>
        /// <returns></returns>
        private Array FindAllValues(string[] args, int currentPos, Type elementType) 
        {
            //find all values
            int n = currentPos + 1;
            List<string> arrayValues = new List<string>();
            while (n < args.Length && !ArgumentDelimiters.Contains(args[n].Substring(0, 1)))
                arrayValues.Add(args[n++]);


            //make a new array of according type
            var z = Array.CreateInstance(elementType, arrayValues.Count);

            for (n = 0; n < arrayValues.Count; n++)
            {
                if (TryGetValue(arrayValues[n], elementType, out object value))
                {
                    z.SetValue(value, n);
                }
                else
                    throw new Exception("Invalid argument parameter.");
            }
            return z;
        }

        /// <summary>
        /// Returns true if the input value [s] could be cast into type [argType] 
        /// </summary>
        /// <param name="s">input value</param>
        /// <param name="argType">type of return value</param>
        /// <param name="value">return value</param>
        /// <returns></returns>
        private bool TryGetValue(string s, Type argType, out object value)
        {
            string typName = argType.FullName;


            if (Regex.IsMatch(typName, "System.[U]?Int[16|32|64]*"))
            {
                if ( int.TryParse(s, out int v))
                {
                    value = v;
                    return true;
                }
            }

            if (argType.Equals(typeof(DateTime)))
            {
                if (DateTime.TryParse(s, out DateTime d))
                {
                    value = d;
                    return true;
                }
            }
            if (argType.Equals(typeof(System.String)))
            {
                value = s;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Returns an Argument
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Argument GetArgument(string name)
        {
            return Arguments[name];
        }
        /// <summary>
        /// Returns the default Argument
        /// </summary>
        /// <returns></returns>
        public Argument GetDefaultArgument()
        {
            return Arguments[""];
        }
    }
}
