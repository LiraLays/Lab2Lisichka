using Xunit;
using WPCalculator.Models;
using System.Linq.Expressions;

namespace WPCalculator.Tests
{
    public class ExpressionTests
    {
        [Fact]
        public void VariableExpression_Substitute_ReplacesVariable()
        {
            // Arrange
            var variable = new VariableExpression("x");

            // Act
            var result = variable.Substitute("x", "5");
            // Assert
            Assert.IsType<Models.ConstantExpression>(result);
            Assert.Equal("5", result.ToString());
        }

        [Fact]
        public void VariableExpression_Substitute_DifferentVariable_NoChange()
        {
            // Arrange
            var variable = new VariableExpression("y");

            // Act
            var result = variable.Substitute("x", "5");

            // Assert
            Assert.IsType<VariableExpression>(result);
            Assert.Equal("y", result.ToString());
        }

        [Fact]
        public void ConstantExpression_Substitute_ReturnsSame()
        {
            // Arrange
            var constant = new Models.ConstantExpression("10");

            // Act
            var result = constant.Substitute("x", "5");

            // Assert
            Assert.Same(constant, result);
            Assert.Equal("10", result.ToString());
        }

        [Fact]
        public void ComplexExpression_Substitute_ReplacesVariable()
        {
            // Arrange
            var complex = new ComplexExpression("x + y > 5");

            // Act
            var result = complex.Substitute("x", "z + 1");

            // Assert
            Assert.Equal("(z + 1) + y > 5", result.ToString());
        }

        [Fact]
        public void ComplexExpression_ToHumanReadable_ConvertsOperators()
        {
            // Arrange
            var complex = new ComplexExpression("x > 5 && y <= 10");

            // Act
            var result = complex.ToHumanReadable();

            // Assert
            Assert.Equal("x больше 5 и y меньше или равно 10", result);
        }

        [Fact]
        public void IncorrectComplexExpression_ToHumanReadable_ConvertsOperators()
        {
            // Arrange
            var complex = new ComplexExpression("x < 5 && y > 10");

            // Act
            var result = complex.ToHumanReadable();

            // Assert
            Assert.Equal("x больше 5 и y меньше или равно 10", result);
        }

        [Fact]
        public void ComplexExpression_GetDefinitenessConditions_DetectsDivision()
        {
            // Arrange
            var complex = new ComplexExpression("x / y + 5");

            // Act
            var conditions = complex.GetDefinitenessConditions();

            // Assert
            Assert.Single(conditions);
            Assert.Contains("знаменатель ≠ 0", conditions);
        }

        [Theory]
        [InlineData("x + 5", 0)] // нет деления
        [InlineData("x / y", 1)] // одно деление
        [InlineData("a / (b / c)", 1)] // вложенные деления (текущая логика считает как 1)
        public void ComplexExpression_DefinitenessConditions_VariousCases(string expression, int expectedCount)
        {
            // Arrange
            var complex = new ComplexExpression(expression);

            // Act
            var conditions = complex.GetDefinitenessConditions();

            // Assert
            Assert.Equal(expectedCount, conditions.Count);
        }
    }
}