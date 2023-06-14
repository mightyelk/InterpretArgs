# About

InterpretArgs is a C# (.Net Core) library for easy to use command line parameters.

## Usage

Don't forget to add a reference to InterpretArgs.dll
```c#
var interpreter = Interpreter.CreateInterpreter();

interpreter.AddFlag("a", "Just a flag")
            .AddParameter<int[]>("pages", "-pages 1 2 4", "Pages to print")
            .AddParameter<string>("file")
            .AddParameter<DateTime>("start")
            .OnError((e) =>
            {
                Console.WriteLine(e);
            })
            .SetArguments(Environment.GetCommandLineArgs())
            .Run();

if (interpreter.ParameterValue<int>("cid")==12345)
{
	//Do something
}
```


## License
[MIT](https://choosealicense.com/licenses/mit/)
