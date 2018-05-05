using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// Vitaliy Trach - 11593957
namespace SpreadsheetEngine
{
    public class ExpTree
    {
        // String to hold the inputted expression
        private string expression;

        // List of all the dependencies in an expression
        private List<string> dependencies = new List<string>();

        // Constructor
        public ExpTree(Dictionary<string, double> variables)
        {
            lookUp = variables;
        }

        public ExpTree()
        {
        }

        // Setter for the expression variable
        public void SetExpression(string exp)
        {
            expression = exp;
            ToReversePolishNotation(exp);
        }

        // Dictionary that holds all the variables and their values
        public Dictionary<string, double> lookUp;
        
        // Array of characters for the operators handled
        private char[] operators = new char[] { '+', '-', '*', '/'};

        // Initalizing root as null
        Node root = null;

        // Setter that adds the variables and their values to the dictionary
        public void SetVar(string varName, double varValue)
        {         
            lookUp.Add(varName, varValue);
        }

        // The public Eval function the calls the private one and gives it the root node
        public double Eval()
        {
            double result = Eval(root);


            return result;
        }

        // Evaluates the expression tree,
        // takes in a Node as a parameter where
        // it finds which node it is (ValNode, OpNode or VarNode)
        // and then recursively finds it's value
        private double Eval(Node current)
        {
            
            // Case 1: If current node is a value node, it returns the nodes value
            if (current is ValNode)
            {
                var temp = current as ValNode;
                return temp.GetValue();
            }

            // Case 2: If current node is a VarNode,
            // it searchs the dictionary for the value of the variable 
            // and returns it
            if (current is VarNode)
            {
                var temp = current as VarNode;

                // Add VarNode name as a dependency to a list
                dependencies.Add(temp.GetVariableName());

                // Returns the value of the variable from the dictionary
                return lookUp[temp.GetVariableName()];
            }

            // Case 3, Current is an OpNode, so it uses
            // a switch case to apply the operation
            var op = current as OpNode;

            switch (op.GetOp())
            {
                case '*':
                    return Eval(op.leftChild) * Eval(op.rightChild);
                case '/':
                    return Eval(op.leftChild) / Eval(op.rightChild);
                case '+':
                    return Eval(op.leftChild) + Eval(op.rightChild);
                case '-':
                    return Eval(op.leftChild) - Eval(op.rightChild);
            }

            return -1;
        }

        // This function first parses the expression, uses a regular expression function, and
        // then uses Dijkstras Shunting Yard Algorithm to get a queue that has everything in
        // reverse polish notation
        //
        // Video I watched and followed to make the algorithm
        // https://www.youtube.com/watch?v=QzVVjboyb0s
        public void ToReversePolishNotation(string expression)
        {
            // Uses a queue for the output of the RPN, and a stack for the operators
            Stack<string> operators = new Stack<string>();
            Queue<string> output = new Queue<string>();

            // Parses expression and saves as list
            List<string> tokens = Regex.Split(expression, @"([-/\+\*\(\)])|([A-Za-z]+\d*)|(\d+\.?\d+)").ToList();

            // Removes all "" from the list
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens.ElementAt(i) == "")
                {
                    tokens.RemoveAt(i);
                }
            }

            foreach (string s in tokens)
            {
                Decimal num;

                // Check is string is a number or a variable
                // if it is add to queue
                if (s.All(c => Char.IsDigit(c)) || Decimal.TryParse(s, out num) || Char.IsLetter(s[0]))
                {
                    output.Enqueue(s);
                }

                // If s is an operator
                if (s.All(c => IsOperator(c)))
                {               
                    if (operators.Count != 0)
                    {
                        // while theres an operator on the stack with greater precedence
                        while (operators.Count != 0 && Precedence(operators.Peek()) > Precedence(s))
                        {
                            // pop current operator from stack into queue
                            output.Enqueue(operators.Pop());
                        }
                    }
                    // Push current operator onto stack
                    operators.Push(s);
                }

                // If left parenthesis, push onto stack
                if (s == "(")
                {
                    operators.Push(s);
                }
                else if (s == ")")
                {
                    // While theres no left bracket at the top of the stack
                    while (operators.Peek() != "(")
                    {
                        output.Enqueue(operators.Pop());
                    }
                    operators.Pop();
                }
            }

            // While stack has items, pop into queue
            while (operators.Count != 0)
            {
                output.Enqueue(operators.Pop());
            }

            // Build the tree using the output queue
            BuildTree(output);
        }

        // Builds the tree by pushing or popping on a stack
        // depending on which Node we are looking at
        private void BuildTree(Queue<string> output)
        {
            Stack<Node> nodes = new Stack<Node>();
            Decimal num;

            // For every string on the output queue
            foreach (string s in output)
            {
                // Check if it is a number
                if (s.All(c => Char.IsDigit(c)) || Decimal.TryParse(s, out num))
                {
                    // Create new value node
                    nodes.Push(new ValNode(Convert.ToDouble(s)));
                }
                // Check if it is a variable
                else if (Char.IsLetter(s[0]))
                {
                    // Create new variable node
                    nodes.Push(new VarNode(s));
                    
                }
                else
                {
                    // Create new operator node, and give it the last 2 things
                    // as it's children
                    OpNode temp = new OpNode(Convert.ToChar(s));
                    temp.rightChild = nodes.Pop();
                    temp.leftChild = nodes.Pop();
                    nodes.Push(temp);
                }
            }
            // Pop the top of the stack and set it to root
            root = nodes.Pop();
        }

        // Simple boolean method that checks if
        // a char is an Operator or not
        private bool IsOperator(char c)
        {
            if (operators.Contains(c))
                return true;

            return false;
        }

        // This method determines the precedences of an operator
        private int Precedence(string s)
        {
            if (s == "(" || s == ")")
            {
                return 0;
            }
            else if (s == "+" || s == "-")
            {
                return 1;
            }
            else if (s == "*" || s == "/")
            {
                return 2;
            }
            else
            {
                // If S is something else which wouldn't make sense
                return -1;
            }
        }

        public List<String> GetDependencies()
        {
            return dependencies;
        }

        public void SetLookUp(Dictionary<String, double> newLookUp)
        {
            lookUp = newLookUp;
        }
        
        // **************************************************************
        // The following section is all of the Node classes that are needed

        // Base class Node for all the nodes
        private abstract class Node
        {

        }

        // Node class for my variable node
        private class VarNode : Node
        {
            private string variableName;

            // Constructor
            public VarNode(string varName)
            {
                variableName = varName;
            }

            // Getter for the variableName
            public string GetVariableName()
            {
                
                return variableName;
            }
        }

        // Node class for the operator node
        private class OpNode : Node
        {
            private char op;
            public Node leftChild, rightChild;

            // Constructor
            public OpNode(char oper)
            {
                op = oper;
            }

            // Getter for the Operator character
            public char GetOp()
            {
                return op;
            }
        }

        // Node class for the value node
        private class ValNode : Node
        {
            private double constValue;

            public ValNode(double val)
            {
                constValue = val;
            }

            // Getter for the value
            public double GetValue()
            {
                return constValue;
            }
        }
    }


}
