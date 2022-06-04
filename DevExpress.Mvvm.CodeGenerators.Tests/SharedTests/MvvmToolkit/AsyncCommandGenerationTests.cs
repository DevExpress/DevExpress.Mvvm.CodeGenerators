using NUnit.Framework;
using System;
using System.Reflection;
using System.Threading.Tasks;
using DevExpress.Mvvm.CodeGenerators.MvvmToolkit;
using Microsoft.Toolkit.Mvvm.Input;

namespace MvvmToolkit.Mvvm.Tests {
    [GenerateViewModel]
    partial class GenerateAsyncCommands {
        readonly Task<int> task = new(() => 1);
        [GenerateCommand]
        public Task WithNoArg() => Task.CompletedTask;
        [GenerateCommand]
        public Task WithArg(int arg) => Task.CompletedTask;
        [GenerateCommand]
        public Task WithNullableArg(int? arg) => Task.CompletedTask;
        public Task SomeMethod() => Task.CompletedTask;

        [GenerateCommand(Name = "MyAsyncCommand", CanExecuteMethod = "CanDoIt")]
        public Task Method(int arg) => Task.CompletedTask;
        public bool CanDoIt(int arg) => arg > 0;

        [GenerateCommand]
        public Task<int> GenericTask() => task;
    }

    [TestFixture]
    public class AsyncCommandGenerationTests {
        [Test]
        public void CallRequiredMethodForAsyncCommand() {
            var generated = new GenerateAsyncCommands();

            var method = GetFieldValue<Action<int>, RelayCommand<int>>(generated.MyAsyncCommand, "execute");
            StringAssert.Contains("MyAsyncCommand", method.Method.Name);

            var canMethod = GetFieldValue<Predicate<int>, RelayCommand<int>>(generated.MyAsyncCommand, "canExecute");
            var expectedCanMethod = generated.GetType().GetMethod("CanDoIt").Name;
            Assert.AreEqual(expectedCanMethod, canMethod.Method.Name);

            var canExecuteMethod = GetFieldValue<Predicate<int>, RelayCommand>(generated.GenericTaskCommand, "canExecute");
            Assert.IsNull(canExecuteMethod);
        }

        static TResult GetFieldValue<TResult, T>(T source, string fieldName) {
            var fieldInfo = source.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(fieldInfo);

            return (TResult)fieldInfo.GetValue(source);
        }
        [Test]
        public void AsyncCommandImplementation() {
            var generated = new GenerateAsyncCommands();

            Assert.IsNotNull(generated.GetType().GetProperty("WithNoArgCommand"));
            Assert.IsNotNull(generated.GetType().GetProperty("WithArgCommand"));
            Assert.IsNotNull(generated.GetType().GetProperty("WithNullableArgCommand"));

            Assert.IsNull(generated.GetType().GetProperty("With2ArgsCommand"));
            Assert.IsNull(generated.GetType().GetProperty("ReturnNoTaskCommand"));
            Assert.IsNull(generated.GetType().GetProperty("SomeMethodCommand"));
        }

        [Test]
        public void ArgumentTypeForAsyncCommand() {
            var generated = new GenerateAsyncCommands();

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
    }
}
