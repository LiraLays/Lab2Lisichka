using System.Linq;
using WPCalculator;
using WPCalculator.Services;
using Xunit;

namespace WPCalculator.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public void CompleteWorkflow_SimpleAssignment()
        {
            // Arrange
            var program = "x := 5";
            var parser = new ProgramParser();
            var postCondition = new Models.ComplexExpression("x == 5");

            // Act
            var statements = parser.ParseProgram(program);
            var (wp, steps) = statements[0].CalculateWP(postCondition);

            // Assert
            Assert.Single(statements);
            Assert.Equal("(5) == 5", wp.ToString());
            Assert.NotEmpty(steps);
        }
    }
}