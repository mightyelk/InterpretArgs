using InterpretArgs;
using System;
using System.Collections.Generic;

public interface IInterpreter
{
    IInterpreter AddParameter<T>(string name);
    IInterpreter AddParameter<T>(string name, string example, string helpText);
    IInterpreter AddParameter<T>(string name, string example, string helpText, bool mandatory);
    IInterpreter AddMandatoryParameter<T>(string name);
    IInterpreter AddFlag(string flag);
    IInterpreter AddFlag(string flag, string helpText);
    IInterpreter SetArguments(params string[] arguments);
    IInterpreter SetArguments(string arguments);
    IInterpreter OnError(Action<Exception> onErrorAction);
    void Run();
    string GetHelpText();
    string GetUsageText();
    T ParameterValue<T>(string parameterName);
}

