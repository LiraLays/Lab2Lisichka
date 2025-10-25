using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using WPCalculator.Models;
using WPCalculator.Services;

namespace WPCalculator.ViewModels
{
    // ViewModel - связывает интерфейс (XAML) с логикой (Models)
    // INotifyPropertyChanged - чтобы интерфейс обновлялся при изменении данных
    public class WPCalculatorViewModel : INotifyPropertyChanged
    {
        // Приватные поля для хранения значений
        private string _finalPrecondition = "";
        private ObservableCollection<CalculationStep> _calculationSteps;
        private string _humanReadablePost = "";
        private string _hoareTriple = "";
        private string _errorMessage = "";

        // Свойства, которые связаны с интерфейсом через Binding
        public string PostConditionText { get; set; } = "max > 100";
        public string ProgramCode { get; set; } = "if (x1 >= x2)\n    max := x1;\nelse\n    max := x2;";

        // Свойство для отображения ошибок
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        // ObservableCollection автоматически обновляет интерфейс при изменении
        public ObservableCollection<CalculationStep> CalculationSteps
        {
            get => _calculationSteps;
            set
            {
                _calculationSteps = value;
                OnPropertyChanged(nameof(CalculationSteps)); // Уведомляем интерфейс
            }
        }

        // Свойство с уведомлением об изменении
        public string FinalPrecondition
        {
            get => _finalPrecondition;
            set
            {
                _finalPrecondition = value;
                OnPropertyChanged(nameof(FinalPrecondition));
                UpdateHoareTriple(); // При изменении WP обновляем триаду
            }
        }

        //  описание постусловия
        public string HumanReadablePost
        {
            get => _humanReadablePost;
            set
            {
                _humanReadablePost = value;
                OnPropertyChanged(nameof(HumanReadablePost));
            }
        }

        // Триада Хоара: {Pre} программа {Post}
        public string HoareTriple
        {
            get => _hoareTriple;
            set
            {
                _hoareTriple = value;
                OnPropertyChanged(nameof(HoareTriple));
            }
        }

        // Готовые примеры для тестирования они не обезательны можно удолить сделать самому
        public List<Preset> Presets { get; set; }
        public Preset SelectedPreset { get; set; }

        // Команды - связывают кнопки с методами
        public ICommand CalculateWPCommand { get; }
        public ICommand ClearStepsCommand { get; }
        public ICommand ClearResultCommand { get; }
        public ICommand ShowHoareTripleCommand { get; }
        public ICommand ClearErrorCommand { get; }

        public WPCalculatorViewModel()
        {
            // Инициализация коллекции
            _calculationSteps = new ObservableCollection<CalculationStep>();

            // Связываем команды с методами
            CalculateWPCommand = new RelayCommand(CalculateWP);
            ClearStepsCommand = new RelayCommand(ClearSteps);
            ClearResultCommand = new RelayCommand(ClearResult);
            ShowHoareTripleCommand = new RelayCommand(ShowHoareTriple);
            ClearErrorCommand = new RelayCommand(ClearError);

            // Загружаем примеры
            InitializePresets();
            UpdateHumanReadablePost();
        }

        // Главный метод расчета WP
        private void CalculateWP()
        {
            // Очистка предыдущих ошибок
            ClearError();
            CalculationSteps.Clear();

            try
            {
                // Проверка введенных данных
                if (!ValidateInput())
                    return;

                // Парсим программу из текста
                var parser = new ProgramParser();
                var program = parser.ParseProgram(ProgramCode);

                // Проверяем, что удалось распарсить хотя бы один оператор
                if (program.Count == 0)
                {
                    ShowError("Не удалось распознать программу. Проверьте синтаксис.");
                    return;
                }

                // Создаем объект постусловия
                var postCondition = new ComplexExpression(PostConditionText);
                UpdateHumanReadablePost();

                // Создаем последовательность и вычисляем WP
                var sequence = new Sequence { Statements = program };
                var (wp, steps) = sequence.CalculateWP(postCondition);

                // Добавляем шаги в коллекцию для отображения
                foreach (var step in steps)
                {
                    CalculationSteps.Add(new CalculationStep("", step));
                }

                // Устанавливаем итоговый результат
                FinalPrecondition = wp.ToString();
            }
            catch (FormatException ex)
            {
                ShowError($"Ошибка формата: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                ShowError($"Ошибка выполнения: {ex.Message}");
            }
            catch (Exception ex)
            {
                ShowError($"Неожиданная ошибка: {ex.Message}");
            }
        }

        // Проверка корректности введенных данных
        private bool ValidateInput()
        {
            // Проверка постусловия
            if (string.IsNullOrWhiteSpace(PostConditionText))
            {
                ShowError("Постусловие не может быть пустым.");
                return false;
            }

            if (PostConditionText.Length > 500)
            {
                ShowError("Постусловие слишком длинное. Максимум 500 символов.");
                return false;
            }

            // Проверка программы
            if (string.IsNullOrWhiteSpace(ProgramCode))
            {
                ShowError("Программа не может быть пустой.");
                return false;
            }

            if (ProgramCode.Length > 2000)
            {
                ShowError("Программа слишком длинная. Максимум 2000 символов.");
                return false;
            }

            // Проверка на наличие недопустимых символов в постусловии
            var invalidChars = PostConditionText.Where(c =>
                !char.IsLetterOrDigit(c) &&
                !char.IsWhiteSpace(c) &&
                !IsAllowedSymbol(c)).ToArray();

            if (invalidChars.Any())
            {
                ShowError($"Недопустимые символы в постусловии: {string.Join(", ", invalidChars.Distinct())}");
                return false;
            }

            // Проверка на наличие недопустимых символов в программе
            invalidChars = ProgramCode.Where(c =>
                !char.IsLetterOrDigit(c) &&
                !char.IsWhiteSpace(c) &&
                !IsAllowedSymbol(c) &&
                c != ':' && c != '=' && c != ';' && c != '(' && c != ')' && c != '\n' && c != '\r').ToArray();

            if (invalidChars.Any())
            {
                ShowError($"Недопустимые символы в программе: {string.Join(", ", invalidChars.Distinct())}");
                return false;
            }

            return true;
        }

        // Проверка допустимости символов для математических выражений
        private bool IsAllowedSymbol(char c)
        {
            var allowedSymbols = new[] { '>', '<', '=', '!', '&', '|', '+', '-', '*', '/', '(', ')', '[', ']', '{', '}', ',', '.' };
            return allowedSymbols.Contains(c);
        }

        // Показ ошибки
        private void ShowError(string message)
        {
            ErrorMessage = message;
            CalculationSteps.Add(new CalculationStep("ОШИБКА", message));
            FinalPrecondition = "Ошибка расчета";
        }

        // Очистка ошибки
        private void ClearError()
        {
            ErrorMessage = "";
        }


        // Обновляет человекочитаемое описание
        private void UpdateHumanReadablePost()
        {
            try
            {
                var expression = new ComplexExpression(PostConditionText);
                HumanReadablePost = expression.ToHumanReadable();
            }
            catch (Exception ex)
            {
                HumanReadablePost = $"Ошибка преобразования: {ex.Message}";
            }
        }

        // Формирует триаду Хоара
        private void UpdateHoareTriple()
        {
            if (!string.IsNullOrEmpty(FinalPrecondition) && !HasError)
            {
                try
                {
                    var pre = new ComplexExpression(FinalPrecondition).ToHumanReadable();
                    var post = new ComplexExpression(PostConditionText).ToHumanReadable();
                    HoareTriple = $"{{ {pre} }}\nпрограмма\n{{ {post} }}";
                }
                catch (Exception ex)
                {
                    HoareTriple = $"Ошибка формирования триады: {ex.Message}";
                }
            }
        }

        private void ShowHoareTriple()
        {
            UpdateHoareTriple();
        }

        private void ClearSteps()
        {
            CalculationSteps.Clear();
            ClearError();
        }

        private void ClearResult()
        {
            FinalPrecondition = "";
            HoareTriple = "";
            ClearError();
        }

        // Загружает готовые примеры тоже не обезательный момент можно удолить нашел на ютубе добавил чтоб пример готовый был для просмотра 
        private void InitializePresets()
        {
            Presets = new List<Preset>
            {
                new Preset
                {
                    Name = "Max из двух",
                    ProgramCode = "if (x1 >= x2)\n    max := x1;\nelse\n    max := x2;",
                    PostCondition = "max > 100"
                },
                new Preset
                {
                    Name = "Квадратное уравнение",
                    ProgramCode = "D := b*b - 4*a*c;\nif (D >= 0)\n    root := (-b + sqrt(D)) / (2*a);\nelse\n    root := -999;",
                    PostCondition = "(D >= 0 ∧ root*root*a + root*b + c == 0) ∨ (D < 0 ∧ root == -999)"
                },
                new Preset
                {
                    Name = "Последовательность присваиваний",
                    ProgramCode = "x := x + 10;\ny := x + 1;",
                    PostCondition = "y == x - 9 ∧ x > 15"
                },
                new Preset
                {
                    Name = "Простое условие",
                    ProgramCode = "if (x > 0)\n    y := 10;\nelse\n    y := 0;",
                    PostCondition = "y >= 0"
                }
            };

            SelectedPreset = Presets[0];
            ApplyPreset(SelectedPreset);
        }

        private void ApplyPreset(Preset preset)
        {
            ProgramCode = preset.ProgramCode;
            PostConditionText = preset.PostCondition;
            OnPropertyChanged(nameof(ProgramCode));
            OnPropertyChanged(nameof(PostConditionText));
            ClearError();
            UpdateHumanReadablePost();
        }

        // Реализация INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            //  - проверка на null (если есть )
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Класс для хранения примера
    public class Preset
    {
        public string Name { get; set; }
        public string ProgramCode { get; set; }
        public string PostCondition { get; set; }
    }

    // Простая реализация ICommand для привязки команд
    public class RelayCommand : ICommand
    {
        private Action _execute;

        public RelayCommand(Action execute) => _execute = execute;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged;
    }
}