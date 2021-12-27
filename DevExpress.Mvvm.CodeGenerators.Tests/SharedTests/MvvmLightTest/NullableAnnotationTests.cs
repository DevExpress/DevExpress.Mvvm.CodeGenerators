using System.Threading.Tasks;
using DevExpress.Mvvm.CodeGenerators.MvvmLight;

namespace MvvmLight.Mvvm.Tests {
    [GenerateViewModel(ImplementINotifyPropertyChanging = true,
    ImplementICleanup = true)]
    public partial class NullableAnnotation {
#nullable enable
        public NullableAnnotation() {
            string1 = "123";
        }
        [GenerateProperty]
        int int1;
        [GenerateProperty]
        int? nullableInt1;
        [GenerateProperty]
        string string1;
        [GenerateProperty]
        string? nullableString1;

        [GenerateCommand]
        void NullableParameter1(string? str) { }
        [GenerateCommand]
        void NonNullableParameter1(string str) { }
        [GenerateCommand]
        Task NullableParameterAsync1(string? str) => Task.CompletedTask;
        [GenerateCommand]
        Task NonNullableParameterAsync1(string str) => Task.CompletedTask;

#nullable disable
        [GenerateProperty]
        int int2;
        [GenerateProperty]
        int? nullableInt2;
        [GenerateProperty]
        string string2;
        [GenerateProperty]
        string? nullableString2;

        [GenerateCommand]
        void NullableParameter2(string? str) { }
        [GenerateCommand]
        void NonNullableParameter2(string str) { }
        [GenerateCommand]
        Task NullableParameterAsync2(string? str) => Task.CompletedTask;
        [GenerateCommand]
        Task NonNullableParameterAsync2(string str) => Task.CompletedTask;

        void OnString1Changed(string str) { }
    }
}
