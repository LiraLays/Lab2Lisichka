using System.Collections.Generic;
using System.Linq;

namespace WPCalculator.Models
{
    public abstract class Statement
    {
        // Возвращает WP и шаги расчета ( можно вернуть 2 значения)
        public abstract (Expression wp, List<string> steps) CalculateWP(Expression postCondition, string stepPrefix = "");
        public abstract string ToString();
    }

    // Оператор присваивания: x := 5
    public class Assignment : Statement
    {
        public string Variable { get; set; }     // Левая часть (x)
        public string Expression { get; set; }   // Правая часть (5)

        public override (Expression wp, List<string> steps) CalculateWP(Expression postCondition, string stepPrefix = "")
        {
            var steps = new List<string>();

            // Правило WP: wp(x := E, R) = R[E/x] (заменяем x на E в R)
            var newCondition = postCondition.Substitute(Variable, Expression);

            steps.Add($"{stepPrefix}Присваивание {Variable} := {Expression}");
            steps.Add($"{stepPrefix}Заменяем {Variable} на {Expression} в условии");
            steps.Add($"{stepPrefix}Получаем: {newCondition}");

            // Добавляем проверки определенности (деление на ноль и т.д.)
            var definiteness = newCondition.GetDefinitenessConditions();
            foreach (var condition in definiteness)
            {
                steps.Add($"{stepPrefix}Добавляем условие определенности: {condition}");
            }

            return (newCondition, steps);
        }

        public override string ToString() => $"{Variable} := {Expression}";
    }

    // Оператор if: if (условие) типо  { ... } else { ... }
    public class IfStatement : Statement
    {
        public string Condition { get; set; }                    // Условие if
        public List<Statement> ThenBranch { get; set; } = new List<Statement>(); // Then ветка
        public List<Statement> ElseBranch { get; set; } = new List<Statement>(); // Else ветка

        public override (Expression wp, List<string> steps) CalculateWP(Expression postCondition, string stepPrefix = "")
        {
            var steps = new List<string>();
            steps.Add($"{stepPrefix}Ветвление if ({Condition})");

            // Правило WP: (B ∧ wp(S1,R)) ∨ (¬B ∧ wp(S2,R))
            // Вычисляем WP для обеих веток
            var thenWP = CalculateSequenceWP(ThenBranch, postCondition, stepPrefix + "  ");
            var elseWP = CalculateSequenceWP(ElseBranch, postCondition, stepPrefix + "  ");

            // Добавляем шаги расчета
            steps.AddRange(thenWP.steps);
            steps.AddRange(elseWP.steps);

            // Объединяем результаты: (условие AND thenWP) OR (NOT условие AND elseWP)
            var result = new ComplexExpression($"({Condition} ∧ {thenWP.wp}) ∨ (!({Condition}) ∧ {elseWP.wp})");

            steps.Add($"{stepPrefix}Объединяем ветки: {result}");

            return (result, steps);
        }

        // Вспомогательный метод для расчета WP последовательности
        private (Expression wp, List<string> steps) CalculateSequenceWP(List<Statement> statements, Expression postCondition, string stepPrefix)
        {
            if (!statements.Any())
                return (postCondition, new List<string> { $"{stepPrefix}Пустая ветка - условие не меняется" });

            var currentCondition = postCondition;
            var allSteps = new List<string>();

            // Идем с КОНЦА последовательности (правило WP для последовательности)
            for (int i = statements.Count - 1; i >= 0; i--)
            {
                var (newCondition, steps) = statements[i].CalculateWP(currentCondition, stepPrefix);
                currentCondition = newCondition;
                allSteps.AddRange(steps);

                // Добавляем разделитель между операторами
                if (i > 0)
                {
                    allSteps.Add($"{stepPrefix}---");
                }
            }

            return (currentCondition, allSteps);
        }

        public override string ToString() => $"if ({Condition}) {{ ... }} else {{ ... }}";
    }

    // Последовательность операторов: S1; S2; S3
    public class Sequence : Statement
    {
        public List<Statement> Statements { get; set; } = new List<Statement>();

        public override (Expression wp, List<string> steps) CalculateWP(Expression postCondition, string stepPrefix = "")
        {
            var steps = new List<string>();
            steps.Add($"{stepPrefix}Последовательность операторов");

            var currentCondition = postCondition;

            // Правило WP: wp(S1; S2, R) = wp(S1, wp(S2, R))
            // Идем с КОНЦА к началу
            for (int i = Statements.Count - 1; i >= 0; i--)
            {
                var (newCondition, stepList) = Statements[i].CalculateWP(currentCondition, stepPrefix + "  ");
                currentCondition = newCondition;
                steps.AddRange(stepList);
            }

            steps.Add($"{stepPrefix}Итог последовательности: {currentCondition}");

            return (currentCondition, steps);
        }

        public override string ToString() => string.Join("; ", Statements.Select(s => s.ToString()));
    }
}