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
        int? nullableValue;

        public int? NullableOldValue;
        public int? NullableNewValue;
        void OnNullableValueChanged(int? oldValue) =>
            NullableOldValue = oldValue;
        void OnNullableValueChanging(int? newValue) =>
            NullableNewValue = newValue;

        [GenerateProperty(OnChangedMethod = "OnChanged", OnChangingMethod = "OnChanging")]
        int noParameter;

        public bool ChangedMethodVisited;
        public bool ChangingMethodVisited;
        void OnChanged() =>
            ChangedMethodVisited = true;
        void OnChanging() =>
            ChangingMethodVisited = true;
        void OnChanging(int i) =>
            ChangingMethodVisited = true;
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
        public void NullableAnnotation() {
            var generated = new GenerateProperties() { NullableValue = 1 };

            Assert.AreEqual(null, generated.NullableOldValue);
            Assert.AreEqual(1, generated.NullableNewValue);

            DoWith.PropertyChangedEvent(
                generated,
                () => {
                    DoWith.PropertyChangingEvent(
                        generated,
                        () => generated.NullableValue = 2,
                        e => {
                            Assert.AreEqual(1, generated.NullableValue);
                            Assert.AreEqual(null, generated.NullableOldValue);
                            Assert.AreEqual(1, generated.NullableNewValue);
                        }
                    );
                },
                e => {
                    Assert.AreEqual(2, generated.NullableValue);
                    Assert.AreEqual(null, generated.NullableOldValue);
                    Assert.AreEqual(2, generated.NullableNewValue);
                }
            );

            Assert.AreEqual(2, generated.NullableValue);
            Assert.AreEqual(1, generated.NullableOldValue);
            Assert.AreEqual(2, generated.NullableNewValue);
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
    }
}
