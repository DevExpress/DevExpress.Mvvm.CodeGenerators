using NUnit.Framework;
using System.ComponentModel;

[DevExpress.Mvvm.CodeGenerators.GenerateViewModel(ImplementINotifyPropertyChanging = true)]
partial class GlobalClass {
    [DevExpress.Mvvm.CodeGenerators.GenerateProperty]
    public int testIntProperty;
    [DevExpress.Mvvm.CodeGenerators.GenerateProperty]
    public string testStringProperty;
}
namespace DevExpress.Mvvm.CodeGenerators.Tests {
    class ImplementedINPCingClass : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public int B { get; set; }
        protected void RaisePropertyChanging(PropertyChangingEventArgs e) => B = 1;
        protected void RaisePropertyChanging(string e) => B = 1;
        protected void RaisePropertyChanged(PropertyChangedEventArgs e) { }
    }
    [GenerateViewModel]
    partial class NotImplenmentedINPCing : ImplementedINPCingClass {
        [GenerateProperty]
        int a;
    }
    partial struct OuterClass {
        [GenerateViewModel]
        public partial class InnerClass {
            [GenerateProperty]
            int a;
            [GenerateViewModel]
            public partial class InnerClass2 {
                [GenerateProperty]
                int a;
            }
        }
    }
    class ViewModelParent1232 {
        public int a = 0;
        public void OnParentViewModelChanged(object o) { a = 1; }
    }
    class ViewModelParent {
        public int a = 0;
        public void OnParentViewModelChanged(object o) { a = 1; }
    }
    class MyClass {
        public string i;
        public MyClass(string k) {
            i = k;
        }

        public object ParentViewModel { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
    struct MyStruct {
        public int i;
        public MyStruct(int k) {
            i = k;
        }
    }

    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class GenericClassTest<T, TClass> {
        [GenerateProperty]
        T tNonNull;
#nullable enable
        [GenerateProperty]
        T tProperty;
        [GenerateProperty]
        TClass tClassProperty;

        public GenericClassTest(T val, TClass refer) {
            tProperty = val;
            tClassProperty = refer;
        }

        public T? TNonNullOldValue;
        public T? TNonNullNewValue;
        public T? TPropertyOldValue;
        public T? TPropertyNewValue;
        public TClass? TClassPropertyOldValue;
        public TClass? TClassPropertyNewValue;
        void OnTPropertyChanged(T oldValue) =>
            TPropertyOldValue = oldValue;
        void OnTPropertyChanging(T newValue) =>
            TPropertyNewValue = newValue;
        void OnTClassPropertyChanged(TClass oldValue) =>
            TClassPropertyOldValue = oldValue;
        void OnTClassPropertyChanging(TClass newValue) =>
            TClassPropertyNewValue = newValue;
        void OnTNonNullChanged(T oldValue) =>
            TNonNullOldValue = oldValue;
        void OnTNonNullChanging(T newValue) =>
            TNonNullNewValue = newValue;
#nullable disable
    }
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class GenerateProperties {
        [GenerateProperty(IsVirtual = true)]
        int property;
#nullable enable

        [GenerateProperty]
        MyStruct nonNullableStruct;
        public MyStruct? NonNullableStructOldValue = new MyStruct(0);
        public MyStruct? NonNullableStructNewValue = new MyStruct(-1);
        void OnNonNullableStructChanged(MyStruct oldValue) =>
            NonNullableStructOldValue = oldValue;
        void OnNonNullableStructChanging(MyStruct? newValue) =>
            NonNullableStructNewValue = newValue;

        [GenerateProperty]
        MyClass? nullableClass;
        public MyClass? NullableClassOldValue = new MyClass("Init value");
        void OnNullableClassChanged(MyClass oldValue) =>
            NullableClassOldValue = oldValue;
#nullable disable
        public MyClass NullableClassNewValue;
        void OnNullableClassChanging(MyClass newValue) =>
            NullableClassNewValue = newValue;

        [GenerateProperty(SetterAccessModifier = AccessModifier.Public)]
        int publicSet;
        [GenerateProperty(SetterAccessModifier = AccessModifier.Private)]
        int privateSet;
        [GenerateProperty(SetterAccessModifier = AccessModifier.Protected)]
        int protectedSet;
        [GenerateProperty(SetterAccessModifier = AccessModifier.Internal)]
        int internalSet;
        [GenerateProperty(SetterAccessModifier = AccessModifier.ProtectedInternal)]
        int protectedInternalSet;
        [GenerateProperty(SetterAccessModifier = AccessModifier.PrivateProtected)]
        int privateProtectedSet;

#nullable enable
        [GenerateProperty]
        int nonNullableInt;

        public int? NonNullableIntOldValue;
        public int? NonNullableIntNewValue;
        void OnNonNullableIntChanged(int oldValue) =>
            NonNullableIntOldValue = oldValue;
        void OnNonNullableIntChanging(int? newValue) =>
            NonNullableIntNewValue = newValue;
#nullable disable

        [GenerateProperty(OnChangedMethod = "OnChanged", OnChangingMethod = "OnChanging")]
        int noParameter;

        public bool ChangedMethodVisited;
        public bool ChangingMethodVisited;
        void OnChanged() =>
            ChangedMethodVisited = true;
        void OnChanging() =>
            ChangingMethodVisited = false;
        void OnChanging(int i) =>
            ChangingMethodVisited = true;

#nullable enable
        [GenerateProperty]
        string? nullableString;

        public string NullableStringOldValue = "Init value";
        void OnNullableStringChanged(string oldValue) =>
            NullableStringOldValue = oldValue;
#nullable disable
        public string NullableStringNewValue;
        void OnNullableStringChanging(string newValue) =>
            NullableStringNewValue = newValue;
    }
    [TestFixture]
    public class PropertyGenerationTests {
        [Test]
        public void PropertyImplementation() {
            var generated = new GenerateProperties();

            Assert.IsNotNull(generated.GetType().GetProperty("Property"));
            Assert.IsNull(generated.GetType().GetProperty("NotProperty"));
        }

        [Test]
        public void GetterAndSetter() {
            var generated = new GenerateProperties();
            var value = 1;

            generated.Property = value;
            Assert.AreEqual(value, generated.Property);
        }

        [Test]
        public void AccessModifier() {
            var generated = new GenerateProperties();

            Assert.IsTrue(generated.GetType().GetProperty("Property").GetGetMethod(true).IsVirtual);

            Assert.IsTrue(generated.GetType().GetProperty("Property").GetSetMethod(true).IsPublic);
            Assert.IsTrue(generated.GetType().GetProperty("PublicSet").GetSetMethod(true).IsPublic);
            Assert.IsTrue(generated.GetType().GetProperty("PrivateSet").GetSetMethod(true).IsPrivate);
            Assert.IsTrue(generated.GetType().GetProperty("ProtectedSet").GetSetMethod(true).IsFamily);
            Assert.IsTrue(generated.GetType().GetProperty("InternalSet").GetSetMethod(true).IsAssembly);
            Assert.IsTrue(generated.GetType().GetProperty("ProtectedInternalSet").GetSetMethod(true).IsFamilyOrAssembly);
            Assert.IsTrue(generated.GetType().GetProperty("PrivateProtectedSet").GetSetMethod(true).IsFamilyAndAssembly);
        }

        [Test]
        public void INPCInvoke() {
            var generated = new GenerateProperties { Property = 1 };

            DoWith.PropertyChangedEvent(
                generated,
                () => {
                    DoWith.PropertyChangingEvent(
                        generated,
                        () => generated.Property = 2,
                        e => {
                            Assert.AreEqual(nameof(GenerateProperties.Property), e.PropertyName);
                            Assert.AreEqual(1, generated.Property);
                        }
                    );
                },
                e => {
                    Assert.AreEqual(nameof(GenerateProperties.Property), e.PropertyName);
                    Assert.AreEqual(2, generated.Property);
                }
            );

            Assert.AreEqual(2, generated.Property);
        }
        [Test]
        public void NullableStruct() {
            var generated = new GenerateProperties() { NonNullableStruct = new MyStruct(1) };

            Assert.AreEqual(0, generated.NonNullableStructOldValue.Value.i);
            Assert.AreEqual(1, generated.NonNullableStructNewValue.Value.i);

            DoWith.PropertyChangedEvent(
                generated,
                () => {
                    DoWith.PropertyChangingEvent(
                        generated,
                        () => generated.NonNullableStruct = new MyStruct(2),
                        e => {
                            Assert.AreEqual(1, generated.NonNullableStruct.i);
                            Assert.AreEqual(0, generated.NonNullableStructOldValue.Value.i);
                            Assert.AreEqual(1, generated.NonNullableStructNewValue.Value.i);
                        }
                        );
                },
                e => {
                    Assert.AreEqual(2, generated.NonNullableStruct.i);
                    Assert.AreEqual(0, generated.NonNullableStructOldValue.Value.i);
                    Assert.AreEqual(2, generated.NonNullableStructNewValue.Value.i);
                }
                );

            Assert.AreEqual(2, generated.NonNullableStruct.i);
            Assert.AreEqual(1, generated.NonNullableStructOldValue.Value.i);
            Assert.AreEqual(2, generated.NonNullableStructNewValue.Value.i);
        }

        [Test]
        public void NullableValueType() {
            var generated = new GenerateProperties() { NonNullableInt = 1 };

            Assert.AreEqual(0, generated.NonNullableIntOldValue);
            Assert.AreEqual(1, generated.NonNullableIntNewValue);

            DoWith.PropertyChangedEvent(
                generated,
                () => {
                    DoWith.PropertyChangingEvent(
                        generated,
                        () => generated.NonNullableInt = 2,
                        e => {
                            Assert.AreEqual(1, generated.NonNullableInt);
                            Assert.AreEqual(0, generated.NonNullableIntOldValue);
                            Assert.AreEqual(1, generated.NonNullableIntNewValue);
                        }
                    );
                },
                e => {
                    Assert.AreEqual(2, generated.NonNullableInt);
                    Assert.AreEqual(0, generated.NonNullableIntOldValue);
                    Assert.AreEqual(2, generated.NonNullableIntNewValue);
                }
            );

            Assert.AreEqual(2, generated.NonNullableInt);
            Assert.AreEqual(1, generated.NonNullableIntOldValue);
            Assert.AreEqual(2, generated.NonNullableIntNewValue);
        }

        [Test]
        public void OnChangedMethodWithoutParameter() {
            var generated = new GenerateProperties();

            Assert.IsFalse(generated.ChangedMethodVisited);
            Assert.IsFalse(generated.ChangingMethodVisited);

            generated.NoParameter = 1;

            Assert.IsTrue(generated.ChangedMethodVisited);
            Assert.IsTrue(generated.ChangingMethodVisited);
        }
        [Test]
        public void NullableReferenceType() {
            var generated = new GenerateProperties() { NullableString = "1" };

            Assert.AreEqual("Init value", generated.NullableStringOldValue);
            Assert.AreEqual("1", generated.NullableStringNewValue);

            DoWith.PropertyChangedEvent(
                generated,
                () => {
                    DoWith.PropertyChangingEvent(
                        generated,
                        () => generated.NullableString = "2",
                        e => {
                            Assert.AreEqual("1", generated.NullableString);
                            Assert.AreEqual("Init value", generated.NullableStringOldValue);
                            Assert.AreEqual("1", generated.NullableStringNewValue);
                        }
                    );
                },
                e => {
                    Assert.AreEqual("2", generated.NullableString);
                    Assert.AreEqual("Init value", generated.NullableStringOldValue);
                    Assert.AreEqual("2", generated.NullableStringNewValue);
                }
            );

            Assert.AreEqual("2", generated.NullableString);
            Assert.AreEqual("Init value", generated.NullableStringOldValue);
            Assert.AreEqual("2", generated.NullableStringNewValue);
        }
        [Test]
        public void NullableClass() {
            var generated = new GenerateProperties() { NullableClass = new MyClass("1") };

            Assert.AreEqual("Init value", generated.NullableClassOldValue.i);
            Assert.AreEqual("1", generated.NullableClassNewValue.i);

            DoWith.PropertyChangedEvent(
                generated,
                () => {
                    DoWith.PropertyChangingEvent(
                        generated,
                        () => generated.NullableClass = new MyClass("2"),
                        e => {
                            Assert.AreEqual("1", generated.NullableClass.i);
                            Assert.AreEqual("Init value", generated.NullableClassOldValue.i);
                            Assert.AreEqual("1", generated.NullableClassNewValue.i);
                        }
                    );
                },
                e => {
                    Assert.AreEqual("2", generated.NullableClass.i);
                    Assert.AreEqual("Init value", generated.NullableClassOldValue.i);
                    Assert.AreEqual("2", generated.NullableClassNewValue.i);
                }
            );

            Assert.AreEqual("2", generated.NullableClass.i);
            Assert.AreEqual("Init value", generated.NullableClassOldValue.i);
            Assert.AreEqual("2", generated.NullableClassNewValue.i);

        }
        [Test]
        public void GenericClassGenerate() {
            var genClass = new GenericClassTest<int, string>(0, "Init Value") { TProperty = 1, TClassProperty = "1", TNonNull = 3 };

            Assert.AreEqual(0, genClass.TPropertyOldValue);
            Assert.AreEqual(1, genClass.TPropertyNewValue);
            Assert.AreEqual("Init Value", genClass.TClassPropertyOldValue);
            Assert.AreEqual("1", genClass.TClassPropertyNewValue);
            Assert.AreEqual(genClass.TNonNullOldValue, genClass.TNonNullNewValue);

            DoWith.PropertyChangedEvent(
                genClass,
                () => {
                    DoWith.PropertyChangingEvent(
                        genClass,
                        () => {
                            genClass.TProperty = 2;
                        },
                        e => {
                            Assert.AreEqual(1, genClass.TProperty);
                            Assert.AreEqual(0, genClass.TPropertyOldValue);
                            Assert.AreEqual(1, genClass.TPropertyNewValue);
                        }
                    );
                },
                e => {
                    Assert.AreEqual(2, genClass.TProperty);
                    Assert.AreEqual(0, genClass.TPropertyOldValue);
                    Assert.AreEqual(2, genClass.TPropertyNewValue);
                }
            );
            DoWith.PropertyChangedEvent(
                genClass,
                () => {
                    DoWith.PropertyChangingEvent(
                        genClass,
                        () => {
                            genClass.TClassProperty = "2";
                        },
                        e => {
                            Assert.AreEqual("1", genClass.TClassProperty);
                            Assert.AreEqual("Init Value", genClass.TClassPropertyOldValue);
                            Assert.AreEqual("1", genClass.TClassPropertyNewValue);
                        }
                    );
                },
                e => {
                    Assert.AreEqual("2", genClass.TClassProperty);
                    Assert.AreEqual("Init Value", genClass.TClassPropertyOldValue);
                    Assert.AreEqual("2", genClass.TClassPropertyNewValue);
                }
            );

            Assert.AreEqual("2", genClass.TClassProperty);
            Assert.AreEqual("1", genClass.TClassPropertyOldValue);
            Assert.AreEqual("2", genClass.TClassPropertyNewValue);
            Assert.AreEqual(2, genClass.TProperty);
            Assert.AreEqual(1, genClass.TPropertyOldValue);
            Assert.AreEqual(2, genClass.TPropertyNewValue);
        }
        [Test]
        public void DoNotGenerateRaiseMethod() {
            var generated = new NotImplenmentedINPCing();
            generated.A = 10;
            Assert.AreEqual(0, generated.B);
        }
        [Test]
        public void GenerateInnerClass() {

            var inner2 = new OuterClass.InnerClass.InnerClass2();
            var inner = new OuterClass.InnerClass();
            Assert.IsNotNull(inner2.GetType().GetProperty("A"));
            Assert.IsNotNull(inner.GetType().GetProperty("A"));
            Assert.AreEqual(typeof(OuterClass.InnerClass.InnerClass2), inner2.GetType().GetProperty("A").DeclaringType);
            Assert.AreEqual(typeof(OuterClass.InnerClass), inner2.GetType().GetProperty("A").DeclaringType.DeclaringType);
            Assert.AreEqual(typeof(OuterClass), inner2.GetType().GetProperty("A").DeclaringType.DeclaringType.DeclaringType);
        }
    }
    #region same class names
    namespace FirstNamespace {
        [GenerateViewModel]
        partial class PartialClass {
            [GenerateProperty]
            int a;
        }
        partial class PartialClass {
            [GenerateProperty]
            int b;
        }
    }
    namespace SecondNamespace {
        [GenerateViewModel]
        partial class PartialClass {
            [GenerateProperty]
            int c;
        }
    }
    #endregion
    [TestFixture]
    public class PartialTests {
        [Test]
        public void GeneratePartialClasses() {
            var a = new FirstNamespace.PartialClass();
            var b = new SecondNamespace.PartialClass();
            Assert.IsNotNull(a.GetType().GetProperty("A"));
            Assert.IsNotNull(a.GetType().GetProperty("B"));
            Assert.IsNotNull(b.GetType().GetProperty("C"));
        }
        [Test]
        public void GlobalNamespace() {
            var globalClass = new GlobalClass();
            Assert.AreEqual(null, globalClass.GetType().Namespace);
            Assert.IsNotNull(globalClass.GetType().GetProperty("TestIntProperty"));
            Assert.IsNotNull(globalClass.GetType().GetProperty("TestStringProperty"));
        }
    }
}
