using InterpretArgs;
using System;
using System.Collections.Generic;

public interface IInterpreter
{
    IInterpreter AddParameter<T>(string name);
    IInterpreter AddParameter<T>(string name, string example, string helpText);
    IInterpreter AddFlag(string flag);
    IInterpreter AddFlag(string flag, string helpText);
    IInterpreter SetArguments(params string[] arguments);
    IInterpreter SetArguments(string arguments);
    IInterpreter OnError(Action<Exception> onErrorAction);
    void Run();
    T ParameterValue<T>(string parameterName);
}

