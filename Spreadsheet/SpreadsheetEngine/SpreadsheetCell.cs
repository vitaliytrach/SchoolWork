using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Vitaliy Trach - 11593957
namespace SpreadsheetEngine
{
    [Serializable]
    public class SpreadsheetCell : Cell
    {
        public SpreadsheetCell() 
        {

        }

        // Non abstract cell's constructor
        public SpreadsheetCell(int row, int column) : base(row, column)
        {
            
        }

        // Sets the value to the text
        public void SetValue(string text)
        {           
            mValue = text;
        }
    }
}
