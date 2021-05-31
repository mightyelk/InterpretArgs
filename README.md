# About

InterpretArgs is a C# (.Net) library for easy to use command line parameters.

## Usage

Don't forget to add a reference to InterpretArgs.dll
```c#
using InterpretArgs;

static void Main(string[] args)
{
    //create instance
    var interpreter = new InterpretArgs.ArgInterpreter();
    //register valid parameters with name and expected type

    interpreter.RegisterDefault("Default parameter is the first thing after the exe, like a filename", ArgInterpreter.ValueTypeEnum.String);
    interpreter.RegisterFlag("test","Flags are on/off boolean");
    interpreter.RegisterArg("cmd","Arg is a named parameter with value behind like -cmd CUT", "Description for help", false, ArgInterpreter.ValueTypeEnum.String, false);
    interpreter.RegisterArg("arr", "Arrays have multiple values after the parameter like -pages 1 2 3", "Description", true, ArgInterpreter.ValueTypeEnum.Number, true);

    //pass the actual environment arguments to interpreter
    interpreter.SetArgs(args);

    //do something
    if (interpreter.Arguments["test"].IsSet)
    {
    }
    //Default parameter
    if (System.IO.File.Exists(interpreter.Arguments[""].StringVal))
    {
    }
    //array
    foreach(var page in interpreter.Arguments["pages"].IntArrayVal) 
    { 
    }
}
```

## License
[MIT](https://choosealicense.com/licenses/mit/)