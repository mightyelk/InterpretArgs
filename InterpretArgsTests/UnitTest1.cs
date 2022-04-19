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
        public void NotAnArrayWithMultipleValues()
        {
            var interpreter = new InterpretArgs.ArgInterpreter();

            interpreter.RegisterArg("noarray", "", "", false, InterpretArgs.ArgInterpreter.ValueTypeEnum.String, false);
            

            Assert.ThrowsException<Exception>(()=> { 
                interpreter.SetArgs("-noarray a b c".Split(' ')); 
            });

            Assert.ThrowsException<Exception>(() => {
                interpreter.SetArgs("-noarray a b c -nextarg".Split(' '));
            });


        }

        [TestMethod]
        public void ArrayWithoutValues()
        {
            var interpreter = new InterpretArgs.ArgInterpreter();

            interpreter.RegisterArg("isarray", "", "", false, InterpretArgs.ArgInterpreter.ValueTypeEnum.String, true);

            Assert.ThrowsException<Exception>(() =>
            {
                interpreter.SetArgs("-isarray");
            });

            Assert.ThrowsException<Exception>(() =>
            {
                interpreter.SetArgs("-isarray", "-nextarg");
            });
        }

        [TestMethod]
        public void NotRegisteredArgument()
        {
            var interpreter = new InterpretArgs.ArgInterpreter();

            interpreter.RegisterArg("registered", "", "", false, InterpretArgs.ArgInterpreter.ValueTypeEnum.String, false);

            Assert.ThrowsException<Exception>(() =>
            {
                interpreter.SetArgs("-registered","yes","-unexpectedFlag");
            });
        }

        [TestMethod]
        public void WrongValueType()
        {
            var interpreter = new InterpretArgs.ArgInterpreter();

            interpreter.RegisterArg("aninteger", "", "", false, InterpretArgs.ArgInterpreter.ValueTypeEnum.Number, false);

            Assert.ThrowsException<InvalidCastException>(() =>
            {
                interpreter.SetArgs("-aninteger", "clearlynotanumber");
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

        [TestMethod]
        public void TestDefaultValues()
        {
            var interpreter = new InterpretArgs.ArgInterpreter();
            interpreter.RegisterArg("test", "", "", false, InterpretArgs.ArgInterpreter.ValueTypeEnum.String, false, "TEST");
            interpreter.RegisterArg("test2", "", "", false, InterpretArgs.ArgInterpreter.ValueTypeEnum.Number, false, 123);
            interpreter.RegisterArg("test3", "", "", false, InterpretArgs.ArgInterpreter.ValueTypeEnum.DateTime, false, new DateTime(2022,04,19));
            interpreter.RegisterArg("test4", "", "", false, InterpretArgs.ArgInterpreter.ValueTypeEnum.String, false, "TEST");
            interpreter.SetArgs("-test4","asdf");


            Assert.IsFalse(interpreter.Arguments["test"].IsSet);
            Assert.AreEqual(interpreter.Arguments["test"].StringVal, "TEST"); //Default value set above
            Assert.AreEqual(interpreter.Arguments["test2"].IntVal, 123 ); //Default value set above
            Assert.AreEqual(interpreter.Arguments["test3"].DateVal, new DateTime(2022,04,19)); //Default value set above

            Assert.IsTrue(interpreter.Arguments["test4"].IsSet);
            Assert.AreEqual(interpreter.Arguments["test4"].StringVal, "asdf"); //Default value set above is overwritten by arguments
        }
    }   
}
