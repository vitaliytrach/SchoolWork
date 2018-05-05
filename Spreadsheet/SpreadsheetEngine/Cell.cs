using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

// Vitaliy Trach - 11593957
namespace SpreadsheetEngine
{
    [Serializable]

    // The class SpreadsheetCell represents one cell in the worksheet
    public abstract class Cell : INotifyPropertyChanged
    {
        private int rowIndex;
        private int columnIndex;
        protected string text;
        protected string mValue;

        // Each Cell keeps track of the expression the user entered.
        // Default is Empty string, and if a user enters a value it's also empty
        protected string expression;

        // Declaring a change event
        public event PropertyChangedEventHandler PropertyChanged;

        public Cell()
        {
        }

        public Cell(int row, int column)
        {
            ColumnIndex = column;
            RowIndex = row;
        }

        // Getter and setter for the rowIndex variable
        public int RowIndex
        {
            get { return rowIndex; }
            set { rowIndex = value; }
        }

        // Getter and setter for the columnIndex variable
        public int ColumnIndex
        {
            get { return columnIndex; }
            set { columnIndex = value; }
        }

        // Getter and setter for the text member variable
        public string Text
        {
            get { return text; }
            set
            {
                // Check if the text in the cell exists already
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        // Getter and setter for the value member variable
        public string Value
        {
            get { return mValue; }
            set { }
        }

        // Method that raises the event
        protected void OnPropertyChanged(string text)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(text));
            }
        }

        // Setter for the expression
        public void SetExpression(string exp)
        {
            expression = exp;
        }

        // Getter for the expression
        public string GetExpression()
        {
            return expression;
        }




    }
}
