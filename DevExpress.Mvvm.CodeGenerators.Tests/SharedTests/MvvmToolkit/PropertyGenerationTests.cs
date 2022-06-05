using NUnit.Framework;
using System.ComponentModel;
using DevExpress.Mvvm.CodeGenerators.MvvmToolkit;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace MvvmToolkit.Mvvm.Tests {
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class GenerateProperties {
        [GenerateProperty(IsVirtual = true)]
        int property;
    }
    [GenerateViewModel]
    partial class WithTwoMvvmAttribute {
        [DevExpress.Mvvm.CodeGenerators.GenerateProperty]
        int dxProperty;
        [DevExpress.Mvvm.CodeGenerators.GenerateCommand]
        void DxMethod() { dxProperty++; }
    }

    [GenerateViewModel]
    partial class Broadcast : ObservableRecipient {
        public Broadcast(IMessenger messenger) : base(messenger) {
        }
        [GenerateProperty(Broadcast = true)]
        int value;
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
        public void DoNotGenerateDxMembers() {
            var generated = new WithTwoMvvmAttribute();

            Assert.IsNull(generated.GetType().GetProperty("DxProperty"));
            Assert.IsNull(generated.GetType().GetProperty("DxMethodCommand"));
        }
#if !FRAMEWORK
        [Test]
        public void BroadcastProperty() {
            var messenger = new StrongReferenceMessenger();
            var broadcast = new Broadcast(messenger) { IsActive = true };

            int messageCount = 0;
            messenger.Register<PropertyChangedMessage<int>>(this, (r, m) => {
                Assert.AreEqual(r, this);
                Assert.AreEqual(0, m.OldValue);
                Assert.AreEqual(1, m.NewValue);
                Assert.AreEqual("Value", m.PropertyName);
                Assert.AreEqual(broadcast, m.Sender);
                Assert.AreEqual(1, broadcast.Value);
                messageCount++;
            });
            broadcast.Value = 1;
            Assert.AreEqual(1, messageCount);
        }
#endif
    }
}
