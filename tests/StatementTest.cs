using Xunit;
using WPCalculator.Models;
using System.Collections.Generic;
using System.Linq;

namespace WPCalculator.Tests
{
    public class StatementTests
    {
        [Fact]
        public void Assignment_CalculateWP_SubstitutesVariable()
        {
            // Arrange
            var assignment = new Assignment { Variable = "x", Expression = "5" };
            var postCondition = new ComplexExpression("x > 0");

            // Act
            var (wp, steps) = assignment.CalculateWP(postCondition);

            // Assert
            Assert.Equal("(5) > 0", wp.ToString());
            Assert.Contains("Присваивание x := 5", steps);
            Assert.Contains("Заменяем x на 5 в условии", steps);
        }

        [Fact]
        public void Assignment_CalculateWP_WithDivision_AddsDefinitenessCondition()
        {
            // Arrange
            var assignment = new Assignment { Variable = "y", Expression = "10 / x" };
            var postCondition = new ComplexExpression("y > 0");

            // Act
            var (wp, steps) = assignment.CalculateWP(postCondition);

            // Assert
            Assert.Equal("(10 / x) > 0", wp.ToString());
            Assert.Contains("Добавляем условие определенности: знаменатель ≠ 0", steps);
        }

        [Fact]
        public void Sequence_CalculateWP_ReverseOrder()
        {
            // Arrange
            var sequence = new Sequence
            {
                Statements = new List<Statement>
                {
                    new Assignment { Variable = "x", Expression = "5" },
                    new Assignment { Variable = "y", Expression = "x + 1" }
                }
            };
            var postCondition = new ComplexExpression("y == 6");

            // Act
            var (wp, steps) = sequence.CalculateWP(postCondition);

            // Assert
            // Должно быть: сначала y := x + 1 с постусловием y == 6 → (x + 1) == 6
            // затем x := 5 с постусловием (x + 1) == 6 → (5 + 1) == 6
            Assert.Equal("((5) + 1) == 6", wp.ToString());
            Assert.Contains("Последовательность операторов", steps);
        }

        [Fact]
        public void IfStatement_CalculateWP_BothBranches()
        {
            // Arrange
            var ifStatement = new IfStatement
            {
                Condition = "x > 0",
                ThenBranch = new List<Statement>
                {
                    new Assignment { Variable = "y", Expression = "10" }
                },
                ElseBranch = new List<Statement>
                {
                    new Assignment { Variable = "y", Expression = "0" }
                }
            };
            var postCondition = new ComplexExpression("y >= 0");

            // Act
            var (wp, steps) = ifStatement.CalculateWP(postCondition);

            // Assert
            // Ожидаем: (x > 0 ∧ wp(y:=10, y>=0)) ∨ (!(x > 0) ∧ wp(y:=0, y>=0))
            // = (x > 0 ∧ 10>=0) ∨ (!(x > 0) ∧ 0>=0)
            Assert.Contains("(x > 0 ∧ (10) >= 0) ∨ (!(x > 0) ∧ (0) >= 0)", wp.ToString());
            Assert.Contains("Ветвление if (x > 0)", steps);
        }

        [Fact]
        public void IfStatement_CalculateWP_EmptyBranches()
        {
            // Arrange
            var ifStatement = new IfStatement
            {
                Condition = "x > 0",
                ThenBranch = new List<Statement>(),
                ElseBranch = new List<Statement>()
            };
            var postCondition = new ComplexExpression("x == 5");

            // Act
            var (wp, steps) = ifStatement.CalculateWP(postCondition);

            // Assert
            // Пустые ветки не меняют условие
            Assert.Contains($"(x > 0 ∧ {postCondition}) ∨ (!(x > 0) ∧ {postCondition})", wp.ToString());
        }

        [Fact]
        public void Incorrect_Sequence_CalculateWP_ReverseOrder()
        {
            // Arrange
            var sequence = new Sequence
            {
                Statements = new List<Statement>
                {
                    new Assignment { Variable = "x", Expression = "1" },
                    new Assignment { Variable = "y", Expression = "x + 2" }
                }
            };
            var postCondition = new ComplexExpression("y == 3");

            // Act
            var (wp, steps) = sequence.CalculateWP(postCondition);

            // Assert
            // Должно быть: сначала y := x + 1 с постусловием y == 3 → (x + 2) == 3
            // затем x := 4 с постусловием (x + 2) == 3 → (1 + 2) == 3
            Assert.Equal("((2) + 3) == 3", wp.ToString());
            Assert.Contains("Последовательность операторов", steps);
        }
    }
}