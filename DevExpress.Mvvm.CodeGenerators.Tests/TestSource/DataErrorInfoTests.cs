using NUnit.Framework;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    [GenerateViewModel(ImplementIDataErrorInfo = true)]
    partial class ClassWithDataErrorInfo {
        public const string MaxLengthAttributeErrorMessage = "ErrorMessage from MaxLengthAttribute";
        public const string MinLengthAttributeErrorMessage = "ErrorMessage from MinLengthAttribute";
        public const string RequiredAttributeErrorMessage = "ErrorMessage from RequiredAttribute";

        [GenerateProperty]
        string withoutErrorInfo;

        [GenerateProperty]
        [MaxLength(5, ErrorMessage = MaxLengthAttributeErrorMessage)]
        string withErrorMessage;

        [GenerateProperty]
        [MinLength(5, ErrorMessage = MinLengthAttributeErrorMessage),
        Required(ErrorMessage = RequiredAttributeErrorMessage)]
        string withDoubleErrorMessage;
    }

    [TestFixture]
    public class DataErrorInfoTests {
        [Test]
        public void ErrorMessage() {
            var generated = new ClassWithDataErrorInfo();

            var errorMessage = ((IDataErrorInfo)generated)[nameof(generated.WithoutErrorInfo)];
            var expectedErrorMessage = string.Empty;
            Assert.AreEqual(expectedErrorMessage, errorMessage);

            generated.WithErrorMessage = "1";
            errorMessage = ((IDataErrorInfo)generated)[nameof(generated.WithErrorMessage)];
            expectedErrorMessage = string.Empty;
            Assert.AreEqual(expectedErrorMessage, errorMessage);

            generated.WithErrorMessage = "123456";
            errorMessage = ((IDataErrorInfo)generated)[nameof(generated.WithErrorMessage)];
            expectedErrorMessage = ClassWithDataErrorInfo.MaxLengthAttributeErrorMessage;
            Assert.AreEqual(expectedErrorMessage, errorMessage);

            generated.WithDoubleErrorMessage = "";
            errorMessage = ((IDataErrorInfo)generated)[nameof(generated.WithDoubleErrorMessage)];
            expectedErrorMessage = string.Join(" ", ClassWithDataErrorInfo.MinLengthAttributeErrorMessage, ClassWithDataErrorInfo.RequiredAttributeErrorMessage);
            Assert.AreEqual(expectedErrorMessage, errorMessage);

            generated.WithDoubleErrorMessage = "1";
            errorMessage = ((IDataErrorInfo)generated)[nameof(generated.WithDoubleErrorMessage)];
            expectedErrorMessage = ClassWithDataErrorInfo.MinLengthAttributeErrorMessage;
            Assert.AreEqual(expectedErrorMessage, errorMessage);
        }
    }
}
