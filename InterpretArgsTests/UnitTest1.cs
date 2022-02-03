using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InterpretArgsTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var interpreter = new InterpretArgs.ArgInterpreter();

            interpreter.RegisterDefault("filename", InterpretArgs.ArgInterpreter.ValueTypeEnum.String);

            interpreter.RegisterFlag("test","Testparameter");

            interpreter.RegisterArg("name","Username", "User who runs this",false, InterpretArgs.ArgInterpreter.ValueTypeEnum.String,false);

            interpreter.RegisterArg("arr", "index", "", true, InterpretArgs.ArgInterpreter.ValueTypeEnum.Number, true);

            interpreter.SetArgs("default -test -name jo -arr 1 2 3".Split(' '));

            Assert.AreEqual(interpreter.Arguments[""].StringVal, "default");
            Assert.IsTrue(interpreter.Arguments["test"].BoolVal);
            Assert.AreEqual(interpreter.Arguments["name"].StringVal, "jo");
            Assert.IsTrue(interpreter.Arguments["name"].IsSet);
            CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, interpreter.Arguments["arr"].IntArrayVal);



            interpreter = new InterpretArgs.ArgInterpreter();
            interpreter.RegisterArg("", "username",  "", true, InterpretArgs.ArgInterpreter.ValueTypeEnum.String, true);

            interpreter.SetArgs("a.txt b.gif c.jpg".Split(' '));

            CollectionAssert.AreEqual(new string[] { "a.txt", "b.gif", "c.jpg" }, interpreter.Arguments[""].StringArrayVal);

            //interpreter.SetArgs(new string[] { "-mult3", "1", "a", "3" });

            
        }


        [TestMethod]
        public void TestMethod2()
        {
            var interpreter = new InterpretArgs.ArgInterpreter();

            interpreter.RegisterDefault("filename", InterpretArgs.ArgInterpreter.ValueTypeEnum.String);

            interpreter.RegisterFlag("test", "Testparameter");
            Assert.ThrowsException<Exception>(()=> { interpreter.RegisterFlag("test", "Testparameter"); });


            interpreter.RegisterArg("noarray", "", "", false, InterpretArgs.ArgInterpreter.ValueTypeEnum.String, false);
            interpreter.RegisterArg("isarray", "", "", false, InterpretArgs.ArgInterpreter.ValueTypeEnum.String, true);


            Assert.ThrowsException<Exception>(()=> { 
                interpreter.SetArgs("-noarray a b c".Split(' ')); 
            });

            Assert.ThrowsException<Exception>(() =>
            {
                interpreter.SetArgs("-isarray".Split(' '));
            });




        }

        [TestMethod]
        public void TestUsage()
        {
            var interpreter = new InterpretArgs.ArgInterpreter();

            interpreter.RegisterDefault("filename", InterpretArgs.ArgInterpreter.ValueTypeEnum.String);

            interpreter.RegisterFlag("test","For testing");

            interpreter.RegisterArg("outputfile","filename","results are written into",false,InterpretArgs.ArgInterpreter.ValueTypeEnum.String,false);

            interpreter.RegisterArg("pages", "number","page numbers",true, InterpretArgs.ArgInterpreter.ValueTypeEnum.Number, true);

            string s = interpreter.GetUsage("Unit.exe");

            Assert.AreEqual(
@"UNIT.EXE filename [-test] [-outputfile filename] -pages number number...
	-test 	For testing
	-outputfile filename	results are written into
	-pages number	page numbers
", s);
        }
    }   
}
