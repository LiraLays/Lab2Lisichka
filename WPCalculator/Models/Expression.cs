using System.Collections.Generic;

namespace WPCalculator.Models
{
    public abstract class Expression
    {
        // Заменяет переменную на выражение 
        public abstract Expression Substitute(string variable, string expression);

        // Для красивого вывода на русском 
        public abstract string ToHumanReadable();

        // Проверки типа "деление на ноль"
        public virtual List<string> GetDefinitenessConditions() => new List<string>();
    }

    // Просто переменная (x, y, max)
    public class VariableExpression : Expression
    {
        public string Name { get; set; }

        public VariableExpression(string name) => Name = name;

        public override Expression Substitute(string variable, string expression)
        {
            // Если это та переменная, которую нужно заменить
            if (Name == variable)
                return new ComplexExpression(expression); // Заменяем
            return this; // Иначе оставляем как есть
        }

        public override string ToHumanReadable() => $"переменная {Name}";
        public override string ToString() => Name;
    }

    // Постоянное значение (5, 100, 0)
    public class ConstantExpression : Expression
    {
        public string Value { get; set; }

        public ConstantExpression(string value) => Value = value;

        public override Expression Substitute(string variable, string expression) => this;
        public override string ToHumanReadable() => $"значение {Value}";
        public override string ToString() => Value;
    }

    // Сложное выражение (x > 5, a + b, x1 >= x2)
    public class ComplexExpression : Expression
    {
        public string ExpressionText { get; set; }

        public ComplexExpression(string expression) => ExpressionText = expression;

        public override Expression Substitute(string variable, string expression)
        {
            // Простая замена: x на (x+1) в "x > 5" → "(x+1) > 5"
            var result = ExpressionText.Replace(variable, $"({expression})");
            return new ComplexExpression(result);
        }

        public override string ToHumanReadable()
        {
            // Заменяем мат. символы на русские слова
            return ExpressionText
                .Replace(">", " больше ")
                .Replace("<", " меньше ")
                .Replace(">=", " больше или равно ")
                .Replace("<=", " меньше или равно ")
                .Replace("==", " равно ")
                .Replace("&&", " и ")
                .Replace("||", " или ");
        }

        public override List<string> GetDefinitenessConditions()
        {
            var conditions = new List<string>();

            // Проверяем деление на ноль
            if (ExpressionText.Contains("/"))
            {
                // Простая проверка - если есть /, добавляем условие
                conditions.Add("знаменатель ≠ 0");
            }

            return conditions;
        }

        public override string ToString() => ExpressionText;
    }
}