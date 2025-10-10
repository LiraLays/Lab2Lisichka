namespace WPCalculator.Models
{
    
    public class CalculationStep
    {
        public string StepDescription { get; set; }  // Что делали на этом шаге
        public string Condition { get; set; }        // Какое условие получилось

        public CalculationStep(string description, string condition)
        {
            StepDescription = description;
            Condition = condition;
        }
    }
}