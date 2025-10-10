using System.Collections.Generic;
using System.Linq;
using WPCalculator.Models;

namespace WPCalculator.Services
{
    // Парсер - преобразует текст программы в объекты Statement
    public class ProgramParser
    {
        public List<Statement> ParseProgram(string code)
        {
            var statements = new List<Statement>();

            // Разбиваем код на строки и убираем пустые
            var lines = code.Split('\n')
                           .Select(l => l.Trim())
                           .Where(l => !string.IsNullOrEmpty(l));

            foreach (var line in lines)
            {
                if (line.StartsWith("if"))
                {
                    statements.Add(ParseIfStatement(line));
                }
                else if (line.Contains(":="))
                {
                    statements.Add(ParseAssignment(line));
                }
            }

            return statements;
        }

        // Парсит присваивание: "x := 5" → Assignment { Variable = "x", Expression = "5" }
        private Assignment ParseAssignment(string line)
        {
            var parts = line.Split(new[] { ":=" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                return new Assignment
                {
                    Variable = parts[0].Trim(),
                    Expression = parts[1].Trim().TrimEnd(';') // Убираем точку с запятой если есть
                };
            }
            return null;
        }

        // Парсит if: "if (x > 5)" → IfStatement { Condition = "x > 5" }
        private IfStatement ParseIfStatement(string line)
        {
            // Ищем условие между скобками
            var conditionStart = line.IndexOf('(') + 1;
            var conditionEnd = line.IndexOf(')');
            var condition = line.Substring(conditionStart, conditionEnd - conditionStart);

            return new IfStatement { Condition = condition.Trim() };
        }
    }
}