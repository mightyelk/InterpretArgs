using InterpretArgs;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

public class Interpreter : IInterpreter
{
    private List<string> commandlineArgs;
    private List<Parameter> parameters;
    private Action<Exception>? onErrorAction;

    private Interpreter()
    {
        commandlineArgs = new();
        parameters = new();
    }

    public static IInterpreter CreateInterpreter()
    {
        return new Interpreter();
    }
    public IInterpreter AddParameter<T>(string name)
    {
        return AddParameter<T>(name, "", "", false);
    }
    public IInterpreter AddParameter<T>(string name, string example, string helpText)
    {
        return AddParameter<T>(name,example , helpText, false);
    }
    public IInterpreter AddParameter<T>(string name, string example, string helpText, bool mandatory)
    {
        parameters.Add(new Parameter(name, typeof(T))
        {
            Description = helpText,
            Example = example,
            Mandatory= mandatory
        });
        return this;
    }

    public IInterpreter AddMandatoryParameter<T>(string name)
    {
        return AddParameter<T>(name, "", "",true);
    }


    public IInterpreter AddFlag(string flag)
    {
        return AddFlag(flag, "");
    }
    public IInterpreter AddFlag(string flag, string helpText) 
    {
        parameters.Add(new Parameter(flag, typeof(bool))
        {
            Description=helpText,
        });
        return this;
    }

    public IInterpreter SetArguments(params string[] arguments)
    {
        commandlineArgs.AddRange(arguments);
        return this;
    }
    public IInterpreter SetArguments(string arguments)
    {
        commandlineArgs.AddRange(arguments.Split(' '));
        return this;
    }

    public IInterpreter OnError(Action<Exception> onErrorAction)
    {
        this.onErrorAction = onErrorAction;
        return this;
    }



    /// <summary>
    /// Assigns the values from arguments to the parameters.
    /// </summary>
    /// <exception cref="MandatoryParameterMissingException" />
    public void Run()
    {
        try
        {
            AssignValues();

            CheckMandatoryParameters();
                
        }
        catch (MandatoryParameterMissingException)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (onErrorAction != null)
            {
                onErrorAction(ex);
            }
            else
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }

    private void CheckMandatoryParameters()
    {
        var unsetMandatoryParameters = parameters.Where(p => p.Mandatory && !p.IsSet).Select(p=> p.Name);
        if (unsetMandatoryParameters.Any())
        {
            throw new MandatoryParameterMissingException(unsetMandatoryParameters.ToArray());
        }
    }

    private void AssignValues()
    {
        var iterator = commandlineArgs.GetTwoWayEnumerator();

        while(iterator.MoveNext())
        {
            var arg = iterator.Current;

            if (!arg.StartsWith("-")) 
            {
                continue;
            }

            var parameter=parameters.Where(p=> p.Name.Equals(arg.Substring(1), StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (parameter is null)
                continue;

            parameter.IsSet = true;

            if (parameter.ValueType == typeof(bool))
            {
                parameter.Value = true;
                continue;
            }

            iterator.MoveNext();
            var next = iterator.Current;

            if (parameter.ValueType== typeof(string))
            {
                parameter.Value = next;
                parameter.ValueType = typeof(string);
                continue;
            }

            if (parameter.ValueType == typeof(int))
            {
                parameter.Value = int.Parse(next);
                parameter.ValueType = typeof(int);
                continue;
            }
            if (parameter.ValueType == typeof(float))
            {

                CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.CurrencyDecimalSeparator = ".";
                parameter.Value = float.Parse(next, System.Globalization.NumberStyles.Any, ci);
                parameter.ValueType = typeof(float);
                continue;
            }
            if (parameter.ValueType == typeof(DateTime))
            {
                parameter.Value = DateTime.Parse(next);
                parameter.ValueType = typeof(DateTime);
                continue;
            }

            if (parameter.ValueType == typeof(int[]))
            {
                var values = new List<string>();
                while (!iterator.Current.StartsWith("-"))
                {
                    values.Add(iterator.Current);
                    if (!iterator.MoveNext())
                        break;
                }

                iterator.MovePrevious();

                parameter.ValueType = typeof(int[]);
                parameter.Value = values.ConvertAll<int>((s) => { return int.Parse(s); });
                continue;
            }
            if (parameter.ValueType == typeof(string[]))
            {
                var values = new List<string>();
                while (!iterator.Current.StartsWith("-"))
                {
                    values.Add(iterator.Current);
                    if (!iterator.MoveNext())
                        break;
                }

                iterator.MovePrevious();

                parameter.ValueType = typeof(string[]);
                parameter.Value = values.ToArray();
                continue;
            }
            if (parameter.ValueType == typeof(float[]))
            {
                var values = new List<string>();
                while (!iterator.Current.StartsWith("-"))
                {
                    values.Add(iterator.Current);
                    if (!iterator.MoveNext())
                        break;
                }

                iterator.MovePrevious();

                parameter.ValueType = typeof(float[]);
                CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.CurrencyDecimalSeparator = ".";
                parameter.Value = values.ConvertAll<float>((s) => { return float.Parse(s, System.Globalization.NumberStyles.Any, ci); });
                continue;
            }

        }
    }

    public T ParameterValue<T>(string parameterName)
    {
        var param = parameters
            .Where(p => p.Name.Equals(parameterName, StringComparison.InvariantCultureIgnoreCase))
            .FirstOrDefault();

        if (param is null)
            return Activator.CreateInstance<T>();

        if (param.ValueType.IsArray)
        {
            var elementType=param.ValueType.GetElementType();
            if (elementType is null)
                return Activator.CreateInstance<T>();
            var listType = typeof(List<>).MakeGenericType(elementType);


            //String does not implement iconvertible?!...
            if (elementType.Equals(typeof(string)))
            {
                var stringArray = param.Value as string[];
                var asArray = (T?)Convert.ChangeType(stringArray, typeof(T));
                if (asArray is null)
                    return Activator.CreateInstance<T>();
                return asArray;
            }
            
            var list = Convert.ChangeType(param.Value, listType) as IList;
            if (list is null)
                return Activator.CreateInstance<T>();
            var array = Array.CreateInstance(elementType, list.Count);

            list.CopyTo(array, 0);
            return (T)Convert.ChangeType(array, typeof(T));

        }

        if (param.Value is null)
            return Activator.CreateInstance<T>();
        return (T)param.Value;
    }

    public string GetUsageText()
    {
        var sb = new StringBuilder();

        foreach (var param in parameters)
        {
            sb.Append("-"+param.Name);
            if (!param.ValueType.Equals(typeof(bool)))
                sb.Append(" " + param.Example);
            sb.Append(' ');
        }
        return sb.ToString();
    }

    public string GetHelpText()
    {
        var sb = new StringBuilder();

        foreach (var param in parameters)
        {
            sb.Append("-" + param.Name);
            sb.AppendLine("\t" + param.Description);
        }
        return sb.ToString();
    }


}
