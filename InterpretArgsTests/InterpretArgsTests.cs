using FluentAssertions;
using FluentAssertions.Execution;
using InterpretArgs;

namespace TestProject1;

public class InterpretArgsTests
{
    [Fact]
    public void AllTypesTest()
    {
        var interpreter = Interpreter.CreateInterpreter();

        var args = "-a -cid 12345 -file C:\\boot.ini -pi 3.1415927 -multint 1 2 3 4 -multstring asdf qwer yxcv 1234 -multfloat 1.2 3.4 5.6 -start 12.05.2022 -end 2023-12-24".Split(' ');

        interpreter.AddFlag("a", "Just a flag")
            .AddParameter<int[]>("multint")
            .AddParameter<string[]>("multstring")
            .AddParameter<string>("file")
            .AddParameter<int>("cid")
            .AddParameter<float>("pi", "-pi 1.234", "Define Pi" )
            .AddParameter<float[]>("multfloat")
            .AddParameter<DateTime>("start")
            .AddParameter<DateTime>("end")

            .OnError((e) =>
            {
                Console.WriteLine(e);
            })
            .SetArguments(args)
            .Run();

        using (new AssertionScope())
        {
            interpreter.ParameterValue<int>("cid").Should().Be(12345);
            interpreter.ParameterValue<bool>("a").Should().BeTrue();
            interpreter.ParameterValue<string>("file").Should().Be("C:\\boot.ini");
            interpreter.ParameterValue<float>("pi").Should().Be(3.1415927F);  //given argument is more precise than float can handle
            interpreter.ParameterValue<int[]>("multint").Should().BeEquivalentTo(new int[] { 1, 2, 3, 4 });
            interpreter.ParameterValue<string[]>("multstring").Should().BeEquivalentTo(new string[] { "asdf","qwer","yxcv","1234" });
            interpreter.ParameterValue<float[]>("multfloat").Should().BeEquivalentTo(new float[] { 1.2F, 3.4F, 5.6F });
            interpreter.ParameterValue<DateTime>("start").Should().Be(new DateTime(2022, 5, 12));
            interpreter.ParameterValue<DateTime>("end").Should().Be(new DateTime(2023, 12, 24));
        }
    }

    [Fact]
    public void ErrorHandlerTest ()
    {
        var i = Interpreter.CreateInterpreter();
        i.AddParameter<int>("number")
            .SetArguments("-number asdf")
            .OnError((e) =>
            {
                Console.WriteLine(e.Message);
            })
            .Run();
    }

    [Fact]
    public void MandatoryMissingTest()
    {
        var i = Interpreter.CreateInterpreter();
        i.AddMandatoryParameter<int>("number")
            .SetArguments("-file list.txt")
            .OnError((e) =>
            {
                Console.WriteLine(e.Message);
            });

        Assert.Throws<MandatoryParameterMissingException>(() => i.Run());
    }

    [Fact]
    public void UsageTextTest()
    {
        var interpreter = Interpreter.CreateInterpreter();

        var args = "-a -cid 12345 -file C:\\boot.ini -multint 1 2 3 4 -start 12.05.2022".Split(' ');

        interpreter.AddFlag("a", "Just a flag")
            .AddParameter<int>("cid", "[int]", "Integer Number")
            .AddParameter<string>("file", "[pdf]", "File to read")
            .AddParameter<int[]>("multint", "[numbers]", "Multiple integers")
            .AddParameter<DateTime>("start", "[date]", "Startdate")

            .OnError((e) =>
            {
                Console.WriteLine(e);
            })
            .SetArguments(args);


        var usage = interpreter.GetUsageText();
        var help = interpreter.GetHelpText();
 
    }
    
}