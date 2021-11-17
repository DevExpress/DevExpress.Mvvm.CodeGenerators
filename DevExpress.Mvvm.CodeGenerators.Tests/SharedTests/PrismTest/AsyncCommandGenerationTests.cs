using NUnit.Framework;
using System;
using System.Reflection;
using System.Threading.Tasks;
using DevExpress.Mvvm.CodeGenerators.Prism;
using Prism.Commands;

namespace Prism.Mvvm.Tests {
    [GenerateViewModel]
    partial class GenerateAsyncCommands {
        [GenerateCommand]
        public Task WithNoArg() => Task.CompletedTask;
        [GenerateCommand]
        public Task WithArg(int? arg) => Task.CompletedTask;
        public Task SomeMethod() => Task.CompletedTask;

        [GenerateCommand(Name = "MyAsyncCommand", CanExecuteMethod = "CanDoIt")]
        public Task Method(int? arg) => Task.CompletedTask;
        public bool CanDoIt(int? arg) => arg > 0;
    }

    [TestFixture]
    public class AsyncCommandGenerationTests {
        [Test]
        public void AsyncCommandImplementation() {
            var generated = new GenerateAsyncCommands();

            Assert.IsNotNull(generated.GetType().GetProperty("WithNoArgCommand"));
            Assert.IsNotNull(generated.GetType().GetProperty("WithArgCommand"));

            Assert.IsNull(generated.GetType().GetProperty("With2ArgsCommand"));
            Assert.IsNull(generated.GetType().GetProperty("ReturnNoTaskCommand"));
            Assert.IsNull(generated.GetType().GetProperty("SomeMethodCommand"));
        }

        [Test]
        public void CallRequiredCanExecuteForAsyncCommand() {
            var generated = new GenerateAsyncCommands();

            var canMethod = GetFieldValue<Func<int?, bool>, DelegateCommand<int?>>(generated.MyAsyncCommand, "_canExecuteMethod");
            var expectedCanMethod = generated.GetType().GetMethod("CanDoIt");
            Assert.AreEqual(expectedCanMethod, canMethod.Method);
        }

        static TResult GetFieldValue<TResult, T>(T source, string fieldName) {
            var fieldInfo = source.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(fieldInfo);

            return (TResult)fieldInfo.GetValue(source);
        }

        [Test]
        public void ArgumentTypeForAsyncCommand() {
            var generated = new GenerateAsyncCommands();

            var noArgumentType = generated.WithNoArgCommand.GetType();
            Assert.IsEmpty(noArgumentType.GetGenericArguments());
            var expectedType = typeof(DelegateCommand);
            Assert.AreEqual(expectedType, noArgumentType);

            var intArgumentType = generated.WithArgCommand.GetType().GetGenericArguments()[0];
            var intExpectedType = typeof(int?);
            Assert.AreEqual(intExpectedType, intArgumentType);
        }
    }
}
