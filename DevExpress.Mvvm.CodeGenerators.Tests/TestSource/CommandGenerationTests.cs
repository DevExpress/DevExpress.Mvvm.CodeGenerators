using NUnit.Framework;
using System;
using System.Reflection;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    [GenerateViewModel]
    partial class GenerateCommands {
        [GenerateCommand]
        public void WithNoArg() { }
        [GenerateCommand]
        public void WithArg(int arg) { }
        [GenerateCommand]
        public void WithNullableArg(int? arg) { }
        public void SomeMethod() { }

        [GenerateCommand(Name = "Command", CanExecuteMethod = "CanDoIt")]
        public void Method(int arg) { }
        public bool CanDoIt(int arg) => arg > 0;

#if !WINUI
        [GenerateCommand(Name = "CommandWithoutCommandManager", UseCommandManager = false)]
        public void WithoutManager() { }
        public void UpdateCommandWithoutManagerCommand() => CommandWithoutCommandManager.RaiseCanExecuteChanged();
#endif

#nullable enable
        [GenerateCommand]
        void WithNullableString1(string? str) { }
        [GenerateCommand]
        void WithNullableString2(string str) { }
        [GenerateCommand]
        void WithNullableString3(string? str) { }
        bool CanWithNullableString3(string str) => str.Length > 0;
        bool CanWithNullableString4(string? str) => str?.Length > 0;
#nullable disable
        [GenerateCommand]
        void WithNullableString4(string str) { }
        bool CanWithNullableString1(string str) => str?.Length > 0;
        bool CanWithNullableString2(string str) => str.Length > 0;
    }

    [TestFixture]
    public class CommandGenerationTests {
        [Test]
        public void CommandImplementation() {
            var generated = new GenerateCommands();

            Assert.IsNotNull(generated.GetType().GetProperty("WithNoArgCommand"));
            Assert.IsNotNull(generated.GetType().GetProperty("WithArgCommand"));
            Assert.IsNotNull(generated.GetType().GetProperty("WithNullableArgCommand"));

            Assert.IsNull(generated.GetType().GetProperty("With2ArgsCommand"));
            Assert.IsNull(generated.GetType().GetProperty("ReturnNoVoidCommand"));
            Assert.IsNull(generated.GetType().GetProperty("SomeMethodCommand"));
        }

        [Test]
        public void CallRequiredMethodForCommand() {
            var generated = new GenerateCommands();

            var executeMethodWithNoArg = GetFieldValue<Action<int?>, DelegateCommand<int?>>(generated.WithNullableArgCommand, "executeMethod");
            var expectedExecuteMethodWithNoArg = generated.GetType().GetMethod("WithNullableArg");
            Assert.AreEqual(expectedExecuteMethodWithNoArg, executeMethodWithNoArg.Method);

            var method = GetFieldValue<Action<int>, DelegateCommand<int>>(generated.Command, "executeMethod");
            var expectedMethod = generated.GetType().GetMethod("Method");
            Assert.AreEqual(expectedMethod, method.Method);

            var canMethod = GetFieldValue<Func<int, bool>, DelegateCommand<int>>(generated.Command, "canExecuteMethod");
            var expectedCanMethod = generated.GetType().GetMethod("CanDoIt");
            Assert.AreEqual(expectedCanMethod, canMethod.Method);

#if !WINUI
            var useCommandManager = GetFieldValue<bool, DelegateCommand<int>>(generated.Command, "useCommandManager");
            Assert.AreEqual(true, useCommandManager);

            useCommandManager = GetFieldValue<bool, DelegateCommand>(generated.CommandWithoutCommandManager, "useCommandManager");
            Assert.AreEqual(false, useCommandManager);

            var canExecuteMethod = GetFieldValue<Func<int, bool>, DelegateCommand>(generated.CommandWithoutCommandManager, "canExecuteMethod");
            Assert.IsNull(canExecuteMethod);
#endif
        }
        static TResult GetFieldValue<TResult, T>(T source, string fieldName) {
            var fieldInfo = source.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(fieldInfo);

            return (TResult)fieldInfo.GetValue(source);
        }
#if WINUI
        [Test]
        public void NoUseCommandManagerPropertyInWinUI() {
            Assert.IsNull(typeof(GenerateCommandAttribute).GetProperty(AttributesGenerator.UseCommandManager));
        }
#endif
        [Test]
        public void ArgumentTypeForCommand() {
            var generated = new GenerateCommands();

            var noArgumentType = generated.WithNoArgCommand.GetType();
            Assert.IsEmpty(noArgumentType.GetGenericArguments());
            var expectedType = typeof(DelegateCommand);
            Assert.AreEqual(expectedType, noArgumentType);

            var intArgumentType = generated.WithArgCommand.GetType().GetGenericArguments()[0];
            var intExpectedType = typeof(int);
            Assert.AreEqual(intExpectedType, intArgumentType);

            var nullableIntArgumentType = generated.WithNullableArgCommand.GetType().GetGenericArguments()[0];
            var nullableIntExpectedType = typeof(int?);
            Assert.AreEqual(nullableIntExpectedType, nullableIntArgumentType);
        }
#if !WINUI
        [Test]
        public void RaiseCanExecuteChanged() {
            var generated = new GenerateCommands();

            generated.CommandWithoutCommandManager.CanExecuteChanged += (s, e) => throw new Exception();
            Assert.Throws<Exception>(generated.UpdateCommandWithoutManagerCommand);
        }
#endif

        [Test]
        public void NullableReferenceType() {
            var generated = new GenerateCommands();

            Assert.IsTrue(generated.WithNullableString1Command.CanExecute("1"));
            Assert.IsTrue(generated.WithNullableString2Command.CanExecute("1"));
            Assert.IsTrue(generated.WithNullableString3Command.CanExecute("1"));
            Assert.IsTrue(generated.WithNullableString4Command.CanExecute("1"));

            Assert.IsFalse(generated.WithNullableString1Command.CanExecute(null));
            Assert.IsFalse(generated.WithNullableString2Command.CanExecute(""));
            Assert.IsTrue(generated.WithNullableString3Command.CanExecute(""));
            Assert.IsFalse(generated.WithNullableString4Command.CanExecute(null));
        }
    }
}
