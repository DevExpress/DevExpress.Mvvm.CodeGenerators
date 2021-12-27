using NUnit.Framework;
using System;
using System.Reflection;
using DevExpress.Mvvm.CodeGenerators.MvvmLight;
using GalaSoft.MvvmLight.Helpers;
#if NETCOREAPP
using GalaSoft.MvvmLight.Command;
#else
using GalaSoft.MvvmLight.CommandWpf;
#endif

namespace MvvmLight.Mvvm.Tests {
    [GenerateViewModel]
    partial class GenerateCommands {
        [GenerateCommand]
        public void WithNoArg() { }
        [GenerateCommand]
        public void WithArg(int arg) { }
        [GenerateCommand]
        public void WithNullableArg(int? arg) { }
        public void SomeMethod() { }
        public void With2Args(int a, string str) { }

        [GenerateCommand(Name = "Command", CanExecuteMethod = "CanDoIt")]
        public void Method(int arg) { }
        public bool CanDoIt(int arg) => arg > 0;

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

            var expectedExecuteMethodWithNoArg = generated.GetType().GetMethod("WithNullableArg");
            var executeMethodWithNoArg = GetFieldValueMethod<MethodInfo, RelayCommand<int?>, WeakAction<int?>>(generated.WithNullableArgCommand, "_execute");
            Assert.AreEqual(expectedExecuteMethodWithNoArg, executeMethodWithNoArg);

            var method = GetFieldValueMethod<MethodInfo, RelayCommand<int>, WeakAction<int>>(generated.Command, "_execute");
            var expectedMethod = generated.GetType().GetMethod("Method");
            Assert.AreEqual(expectedMethod, method);

            var canMethod = GetFieldValueMethod<MethodInfo, RelayCommand<int>, WeakFunc<bool>>(generated.Command, "_canExecute");
            var expectedCanMethod = generated.GetType().GetMethod("CanDoIt");
            Assert.AreEqual(expectedCanMethod, canMethod);
        }
        static TResult GetFieldValueMethod<TResult, TCommand, T>(TCommand source, string fieldName) {
            var fieldInfo = source.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(fieldInfo);
            var fieldInsance = fieldInfo.GetValue(source);
            var weakMethod = typeof(T).GetProperty("Method", BindingFlags.NonPublic | BindingFlags.Instance);
            return (TResult)weakMethod.GetValue(fieldInsance);
        }
        [Test]
        public void ArgumentTypeForCommand() {
            var generated = new GenerateCommands();

            var noArgumentType = generated.WithNoArgCommand.GetType();
            Assert.IsEmpty(noArgumentType.GetGenericArguments());
            var expectedType = typeof(RelayCommand);
            Assert.AreEqual(expectedType, noArgumentType);

            var intArgumentType = generated.WithArgCommand.GetType().GetGenericArguments()[0];
            var intExpectedType = typeof(int);
            Assert.AreEqual(intExpectedType, intArgumentType);

            var nullableIntArgumentType = generated.WithNullableArgCommand.GetType().GetGenericArguments()[0];
            var nullableIntExpectedType = typeof(int?);
            Assert.AreEqual(nullableIntExpectedType, nullableIntArgumentType);
        }

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
