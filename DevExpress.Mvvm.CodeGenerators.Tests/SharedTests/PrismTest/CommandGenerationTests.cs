using NUnit.Framework;
using System;
using System.Reflection;
using DevExpress.Mvvm.CodeGenerators.Prism;
using Prism.Commands;
using System.Collections.Generic;
using System.Linq;

namespace Prism.Mvvm.Tests {
    [GenerateViewModel]
    partial class GenerateCommands {
        [GenerateCommand]
        public void WithNoArg() { }
        public bool CanWithNoArg() => true;
        [GenerateCommand]
        public void WithNullableArg(int? i) { }
        bool CanWithNullableArg(int? i) => true;

        [GenerateCommand(Name = "Command", CanExecuteMethod = "CanDoIt")]
        public void Method(int? arg) { }
        public bool CanDoIt(int? arg) => true;

        [GenerateCommand(ObservesCanExecuteProperty = nameof(CanExecuteProperty))]
        public void WithObservesCanExecute() { }
        [GenerateProperty]
        bool canExecuteProperty;

        public int CanExecuteCount = 0;
        [GenerateCommand(ObservesProperties = new[] { nameof(ObservesPropertyA), nameof(ObservesPropertyB)})]
        void WithObservesProperty() { }
        public bool CanWithObservesProperty() {
            CanExecuteCount++;
            return true;
        }
        [GenerateProperty]
        public bool observesPropertyA, observesPropertyB, notObservesPropertyC;

        [GenerateCommand(CanExecuteMethod = "CanWithNoArg", ObservesCanExecuteProperty = nameof(CanExecuteProperty))]
        public void WithCanExecuteAndCanExecuteProperty() { }

        [GenerateCommand(ObservesCanExecuteProperty = "CanExecuteProperty", ObservesProperties = new[] { nameof(ObservesPropertyA), nameof(ObservesPropertyB) })]
        void WithCanExecutePropertyAndOservesProperty() { }

        public void SomeMethod() { }
        [First]
        [Second]
        [Third]
        [GenerateCommand]
        void AttributeTest() { }
    }
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class FirstAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SecondAttribute : Attribute { }
    public sealed class ThirdAttribute : Attribute { }

    [TestFixture]
    public class CommandGenerationTests {
        [Test]
        public void CommandImplementation() {
            var generated = new GenerateCommands();

            Assert.IsNotNull(generated.GetType().GetProperty("WithNoArgCommand"));
            Assert.IsNotNull(generated.GetType().GetProperty("Command"));
            Assert.IsNotNull(generated.GetType().GetProperty("WithObservesCanExecuteCommand"));
            Assert.IsNotNull(generated.GetType().GetProperty("WithObservesPropertyCommand"));
            Assert.IsNotNull(generated.GetType().GetProperty("WithCanExecuteAndCanExecutePropertyCommand"));

            Assert.IsNull(generated.GetType().GetProperty("SomeMethodCommand"));
        }
        [Test]
        public void CallRequiredMethodForCommand() {
            var generated = new GenerateCommands();

            var method = GetFieldValue<Action<int?>, DelegateCommand<int?>>(generated.Command, "_executeMethod");
            var expectedMethod = generated.GetType().GetMethod("Method");
            Assert.AreEqual(expectedMethod, method.Method);

            var canMethod = GetFieldValue<Func<int?, bool>, DelegateCommand<int?>>(generated.Command, "_canExecuteMethod");
            var expectedCanMethod = generated.GetType().GetMethod("CanDoIt");
            Assert.AreEqual(expectedCanMethod, canMethod.Method);
        }
        [Test]
        public void IgnoreCanExecuteMethodWithCanExecuteProperty() {
            var generated = new GenerateCommands();

            var canPropertyMethod = GetFieldValue<Func<bool>, DelegateCommand>(generated.WithCanExecuteAndCanExecutePropertyCommand, "_canExecuteMethod");
            var expectedCanPropertyMethod = generated.GetType().GetMethod("CanWithNoArg");
            Assert.AreNotEqual(expectedCanPropertyMethod, canPropertyMethod.Method);
        }
        static TResult GetFieldValue<TResult, T>(T source, string fieldName) {
            var fieldInfo = source.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(fieldInfo);
            return (TResult)fieldInfo.GetValue(source);
        }
        [Test]
        public void ArgumentTypeForCommand() {
            var generated = new GenerateCommands();

            var noArgumentType = generated.WithNoArgCommand.GetType();
            Assert.IsEmpty(noArgumentType.GetGenericArguments());
            var expectedType = typeof(DelegateCommand);
            Assert.AreEqual(expectedType, noArgumentType);

            var intArgumentType = generated.WithNullableArgCommand.GetType().GetGenericArguments()[0];
            var intExpectedType = typeof(int?);
            Assert.AreEqual(intExpectedType, intArgumentType);

            var nullableIntArgumentType = generated.WithNullableArgCommand.GetType().GetGenericArguments()[0];
            var nullableIntExpectedType = typeof(int?);
            Assert.AreEqual(nullableIntExpectedType, nullableIntArgumentType);
        }
        [Test]
        public void ObservesCanExecuteProperty() {
            var generated = new GenerateCommands();

            generated.CanExecuteProperty = false;
            Assert.IsFalse(generated.WithObservesCanExecuteCommand.CanExecute());
            generated.CanExecuteProperty = true;
            Assert.IsTrue(generated.WithObservesCanExecuteCommand.CanExecute());
        }
        [Test]
        public void ObservesProperties() {
            var generated = new GenerateCommands();

            var expressions = (HashSet<string>)typeof(DelegateCommandBase).GetField("_observedPropertiesExpressions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(generated.WithObservesPropertyCommand);
            Assert.IsTrue(expressions.Contains($"() => value({generated.GetType().FullName}).ObservesPropertyA"));
            Assert.IsTrue(expressions.Contains($"() => value({generated.GetType().FullName}).ObservesPropertyB"));

            var expressionsWithCanExecuteProperty = (HashSet<string>)typeof(DelegateCommandBase).GetField("_observedPropertiesExpressions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(generated.WithCanExecutePropertyAndOservesPropertyCommand);
            Assert.AreEqual(3, expressionsWithCanExecuteProperty.Count);
            Assert.IsTrue(expressionsWithCanExecuteProperty.Contains($"() => value({generated.GetType().FullName}).CanExecuteProperty"));
        }
        [Test]
        public void AttributeGenerationTest() {
            var generated = new GenerateCommands();

            var attributes = generated.GetType().GetProperty("AttributeTestCommand").GetCustomAttributes().ToList();
            Assert.AreEqual(2, attributes.Count);
            Assert.IsTrue(attributes[0] is FirstAttribute);
            Assert.IsTrue(attributes[1] is ThirdAttribute);
        }
    }
}
