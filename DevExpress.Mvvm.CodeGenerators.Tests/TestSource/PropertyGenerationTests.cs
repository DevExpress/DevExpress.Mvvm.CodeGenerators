using NUnit.Framework;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class GenerateProperties {
        [GenerateProperty(IsVirtual = true)]
        int property;

        int notProperty;

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

        [GenerateProperty]
        int? nullableInt;

        public int? NullableIntOldValue;
        public int? NullableIntNewValue;
        void OnNullableIntChanged(int? oldValue) =>
            NullableIntOldValue = oldValue;
        void OnNullableIntChanging(int? newValue) =>
            NullableIntNewValue = newValue;

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
        public void NullableValueType() {
            var generated = new GenerateProperties() { NullableInt = 1 };

            Assert.AreEqual(null, generated.NullableIntOldValue);
            Assert.AreEqual(1, generated.NullableIntNewValue);

            DoWith.PropertyChangedEvent(
                generated,
                () => {
                    DoWith.PropertyChangingEvent(
                        generated,
                        () => generated.NullableInt = 2,
                        e => {
                            Assert.AreEqual(1, generated.NullableInt);
                            Assert.AreEqual(null, generated.NullableIntOldValue);
                            Assert.AreEqual(1, generated.NullableIntNewValue);
                        }
                    );
                },
                e => {
                    Assert.AreEqual(2, generated.NullableInt);
                    Assert.AreEqual(null, generated.NullableIntOldValue);
                    Assert.AreEqual(2, generated.NullableIntNewValue);
                }
            );

            Assert.AreEqual(2, generated.NullableInt);
            Assert.AreEqual(1, generated.NullableIntOldValue);
            Assert.AreEqual(2, generated.NullableIntNewValue);
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
    }
}
