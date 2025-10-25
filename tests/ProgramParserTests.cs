using Xunit;
using WPCalculator.Services;
using WPCalculator.Models;
using System.Linq;

namespace WPCalculator.Tests
{
    public class ProgramParserTests
    {
        private readonly ProgramParser _parser;

        public ProgramParserTests()
        {
            _parser = new ProgramParser();
        }

        [Theory]
        [InlineData("x := 5", "x", "5")]
        [InlineData("variable := value + 1", "variable", "value + 1")]
        [InlineData("x := 5;", "x", "5")] // с точкой с запятой
        public void ParseAssignment_ValidInput_CreatesAssignment(string input, string expectedVar, string expectedExpr)
        {
            // Act
            var result = _parser.ParseAssignment(input);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedVar, result.Variable);
            Assert.Equal(expectedExpr, result.Expression);
        }

        [Theory]
        [InlineData("if (x > 5)", "x > 5")]
        [InlineData("if (a <= b && c == d)", "a <= b && c == d")]
        public void ParseIfStatement_ValidInput_CreatesIfStatement(string input, string expectedCondition)
        {
            // Act
            var result = _parser.ParseIfStatement(input);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCondition, result.Condition);
        }

        [Fact]
        public void ParseProgram_MixedStatements_ParsesCorrectly()
        {
            // Arrange
            var program = @"
                x := 5;
                if (x > 0)
                y := x + 1;
            ";

            // Act
            var result = _parser.ParseProgram(program);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.IsType<Assignment>(result[0]);
            Assert.IsType<IfStatement>(result[1]);
            Assert.IsType<Assignment>(result[2]);
        }

        [Fact]
        public void IncorrectParseProgram_MixedStatements_ParsesCorrectly()
        {
            // Arrange
            var program = @"
                x := 12;
                if (x > 0)
                y := x + 1;
            ";

            // Act
            var result = _parser.ParseProgram(program);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.IsType<Assignment>(result[2]);
            Assert.IsType<IfStatement>(result[0]);
            Assert.IsType<Assignment>(result[1]);
        }

        [Fact]
        public void ParseProgram_EmptyInput_ReturnsEmptyList()
        {
            // Arrange
            var program = "";

            // Act
            var result = _parser.ParseProgram(program);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ParseProgram_OnlyWhitespace_ReturnsEmptyList()
        {
            // Arrange
            var program = "   \n  \t  \n  ";

            // Act
            var result = _parser.ParseProgram(program);

            // Assert
            Assert.Empty(result);
        }
    }
}