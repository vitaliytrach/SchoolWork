using System;

namespace TestExpressionTree
{
    // Vitaliy Trach - 11593957
    class Program
    {
        // Initialize new expression tree
        private static SpreadsheetEngine.ExpTree tree = new SpreadsheetEngine.ExpTree();

        static void Main(string[] args)
        {
            // Loop for the menu
            while (true)
            { 
                SwitchCases(PrintOptions());
            }
        }

        // This method asks the user which option they
        // would like to chose, and returns that key character
        public static char PrintOptions()
        {
            Console.WriteLine("Menu: ");
            Console.WriteLine("  1 = Add new expression");
            Console.WriteLine("  2 = Create new variable");
            Console.WriteLine("  3 = Evaluate tree");
            Console.WriteLine("  4 = Quit");

            return Console.ReadKey().KeyChar;
        }

        // Switch case for the menu interface
        public static void SwitchCases(char option)
        {
            switch (option)
            {
                case '1':
                    Console.WriteLine();
                    Console.WriteLine("What is your expression?");
                    tree.SetExpression(Console.ReadLine().ToString());

                    break;
                case '2':
                    Console.WriteLine();
                    Console.WriteLine("What is your variable name? ");
                    string varName = Console.ReadLine();

                    Console.WriteLine();
                    Console.WriteLine("What is the value of " + varName + " ?");
                    string varValue = Console.ReadLine();

                    tree.SetVar(varName, Convert.ToDouble(varValue));

                    break;
                case '3':
                    Console.WriteLine();
                    Console.WriteLine(tree.Eval().ToString());
                    break;
                case '4':
                    Environment.Exit(0);
                    break;
            }
        }
    }
}
