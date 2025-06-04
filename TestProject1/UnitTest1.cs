using SYP.Models;

namespace TestProject1
{
    public class EmployeeValidatorTests
    {
        [Theory]
        [InlineData("Иванов Иван Иванович", true)]
        [InlineData("Иван", false)]
        [InlineData("", false)]
        public void ValidateFullName_Test(string input, bool expected)
        {
            var result = EmployeeValidator.ValidateFullName(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("invalid-email", false)]
        public void ValidateEmail_Test(string input, bool expected)
        {
            var result = EmployeeValidator.ValidateEmail(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("+79998887766", true)]
        [InlineData("89998887766", false)]
        public void ValidatePhone_Test(string input, bool expected)
        {
            var result = EmployeeValidator.ValidatePhone(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ValidateDates_Test()
        {
            var birth = new DateTime(1990, 1, 1);
            var hire = new DateTime(2020, 1, 1);
            var invalid = new DateTime(1980, 1, 1);

            Assert.True(EmployeeValidator.ValidateDates(birth, hire));
            Assert.False(EmployeeValidator.ValidateDates(hire, birth));
            Assert.False(EmployeeValidator.ValidateDates(null, hire));
        }

        [Fact]
        public void ValidateSelection_Test()
        {
            Assert.True(EmployeeValidator.ValidateSelection("Менеджер", "Отдел продаж", "Активный"));
            Assert.False(EmployeeValidator.ValidateSelection(null, "Отдел", "Активный"));
        }
    }
}
