using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;

// Vitaliy Trach - 11593957
namespace SpreadsheetEngine
{
    [Serializable]

    public class SpreadSheet
    {
        // Declaring a 2D array of cells
        private SpreadsheetCell[,] cells;

        // Event handler for the Cell change
        public event PropertyChangedEventHandler CellPropertyChanged;

        // Dictionary that holds all the cell values as variables
        Dictionary<string, double> variables = new Dictionary<string, double>();

        // Dictionary where the Key is the Cell, and it holds a list of Cells that it's dependent on
        Dictionary<SpreadsheetCell, List<SpreadsheetCell>> dependencies = new Dictionary<SpreadsheetCell, List<SpreadsheetCell>>();

        private const string version = "5.0";

        public SpreadSheet()
        { }

        // Constructor that builds all the cell objects
        public SpreadSheet(int rows, int columns)
        {
            // Builds the 2D array of Cells of size rows and columns
            cells = new SpreadsheetCell[rows, columns];

            // Loops that creates the cell object and gives it a PropertyChanged event
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    cells[i, j] = new SpreadsheetCell(i, j);
                    cells[i, j].PropertyChanged += SpreadSheetCell_PropertyChanged;
                }
            }
        }

        // Getter for the cell at index column and row
        public SpreadsheetCell GetCell(int row, int column)
        {
            return cells[row, column];
        }

        // Function to get the cell given a location in the form A1...Z50
        public SpreadsheetCell GetCell(string cellLocation)
        {
            int row = Convert.ToInt32(cellLocation.Substring(1)) - 1;
            int column = cellLocation[0] - 'A';

            return GetCell(row, column);
        }

        // PropertyChanged event for SpreadSheet.
        // This method is where all the meat is for when a cell gets an input by the user.
        // The purpose of it is to determine if a user entered an formula or a value.
        // If it's a formula it uses the ExpTree to calculate the value, and set the cells value to that.
        // If the user doesn't enter a formula, it sets that cell value and text to what the user entered.
        // -----------------------------------------------------------------------------
        // It's good to point out also how the there is a check to see if the input is valid,
        // if it's invalid the cell will display "#REF",
        // Then after the cell gets updated, all the dictionaries get updated accordingly.
        private void SpreadSheetCell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var currentCell = sender as SpreadsheetCell;
            ExpTree tree = new ExpTree(variables);
            List<string> multiDependencies = new List<string>();

            // Check if input is valid
            if (!IsValidInput(currentCell))
            {
                currentCell.SetValue("#REF");
                return;
            }

            // If the user entered a formula
            if (currentCell.Text != null && currentCell.Text[0] == '=')
            {
                tree.SetExpression(currentCell.Text.Substring(1));
                currentCell.SetExpression(currentCell.Text.Substring(1));
                currentCell.SetValue(tree.Eval().ToString());

                // Gets the new dependencies from the tree,
                // and in a loop updates the dependency dictionary
                multiDependencies = tree.GetDependencies();
                foreach (string s in multiDependencies)
                {
                    SpreadsheetCell myCell = GetCell(s);

                    if (!dependencies.ContainsKey(myCell))
                    {
                        dependencies.Add(myCell, new List<SpreadsheetCell>());
                    }
                    dependencies[myCell].Add(currentCell);
                }
            }
            // If user enters a value
            else
            {
                // Set value of the cell
                currentCell.SetValue(currentCell.Text);
                // Set the expression of that cell to null
                currentCell.SetExpression(string.Empty);

                RemoveDependencies(currentCell);
            }

            UpdateVariables(currentCell);

            if (CellPropertyChanged != null)
            {
                CellPropertyChanged(sender, e);
            }

            UpdateDependencies(currentCell, tree);
        }

        // This function updates the variables dictionary.
        // It does this by first checking if the variables dictionary
        // already has a key for that variable. Then it updates the value
        private void UpdateVariables(SpreadsheetCell currentCell)
        {
            string cellName = GetCellName(currentCell);

            if (!variables.ContainsKey(cellName))
            {
                variables.Add(cellName, Convert.ToDouble(currentCell.Value));
            }

            variables[cellName] = Convert.ToDouble(currentCell.Value);
        }

        // The purpose of this method is to update the dependencies in the spreadsheet
        // after a cell gets messed with. It iterates through a list of dependencies (from inside the dependencies dictionary)
        // and recursively updates all cells that are dependent on that specific cell
        public void UpdateDependencies(SpreadsheetCell currentCell, ExpTree tree)
        {
            // Check if that cell has dependencies
            if (dependencies.ContainsKey(currentCell))
            {
                // iterate through the list inside dependencies dictionary
                foreach (SpreadsheetCell c in dependencies[currentCell])
                {
                    // Updates the tree's expression
                    tree.SetExpression(c.GetExpression());
                    // Sets the new value to the new Eval from the tree
                    c.SetValue(tree.Eval().ToString());
                    CellPropertyChanged(c, new PropertyChangedEventArgs("Value"));

                    // Update variables dictionary
                    UpdateVariables(c);
                    UpdateDependencies(c, tree);
                }
            }
        }

        // Returns the name of the cell given a cell
        // E.g - A1, A2......
        private string GetCellName(SpreadsheetCell currentCell)
        {
            return Convert.ToChar((currentCell.ColumnIndex) + 'A').ToString() + (currentCell.RowIndex + 1).ToString();
        }

        // The function of IsValidInput is to check if the user
        // entered a valid value or formula into the Spreadsheet.
        // It uses a regular expression I grabbed from the ExpTree
        // to parse the inputted text. It returns false when:
        // - A cell is dependent on itself
        // - If the user enters something besides a formula or a value
        //              E.g. If user enters 24gfg123
        // - If a cell is dependent on a cell that doesn't have anytext       
        private bool IsValidInput(SpreadsheetCell currentCell)
        {
            // If the user's input begins with "="
            if (currentCell.Text != null && currentCell.Text[0] == '=')
            {
                // Used the regular expression from the ExpTree to split
                // the currentCell's text to see if it's a valid entry
                List<string> tokens = System.Text.RegularExpressions.Regex.Split(currentCell.Text.Substring(1),
                    @"([-/\+\*\(\)])|([A-Za-z]+\d*)|(\d+\.?\d+)").ToList();

                // Removes all "" from the list
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens.ElementAt(i) == "")
                    {
                        tokens.RemoveAt(i);
                    }
                }

                // Iterate through the List of strings and
                // check if the user entered a valid Cell (A1-Z50 counts as valid) 
                foreach (string s in tokens)
                {
                    if (Char.IsUpper(s[0]))
                    {
                        int output;
                        bool isNumeric = int.TryParse(s.Substring(1), out output);
                        if (isNumeric && output >= 1 && output <= 50)
                        {
                            if (currentCell == GetCell(s))
                            {
                                // Means current cell references itself
                                return false;
                            }
                            else if (GetCell(s).Text == null)
                            {
                                // Referencing a cell that doesn't have currentCell as a dependency
                                return false;
                            }
                        }
                    }
                }
            }
            // If input doesn't begin with an "="
            else if (currentCell.Text != null && currentCell.Text[0] != '=')
            {
                foreach (char c in currentCell.Text)
                {
                    if (!Char.IsDigit(c))
                    {
                        // If the value entered is a mix of digits and letters
                        return false;
                    }
                }
            }
            return true;
        }

        // The purpose of this method is too remove dependencies that are no longer
        // need. It only gets called when a cell that was dependent on other cells
        // gets changed to a value
        private void RemoveDependencies(SpreadsheetCell currentCell)
        {
            // Creating a dictionary that will keep track of which dependencies to delete
            Dictionary<SpreadsheetCell, List<SpreadsheetCell>> deleteTheseDependencies = new Dictionary<SpreadsheetCell, List<SpreadsheetCell>>();

            // Iterate through the dependencies dictionary and check if currentCell == a
            // cell in the dictionary. If it does then add it to the deleteTheseDependencies dictionary
            foreach (KeyValuePair<SpreadsheetCell, List<SpreadsheetCell>> kvp in dependencies)
            {
                foreach (SpreadsheetCell ssc in kvp.Value)
                {
                    if (!deleteTheseDependencies.ContainsKey(kvp.Key))
                    {
                        deleteTheseDependencies.Add(kvp.Key, new List<SpreadsheetCell>());
                    }

                    if (ssc == currentCell)
                    {
                        deleteTheseDependencies[kvp.Key].Add(ssc);
                    }
                }
            }

            // This loop just removes those dependencies from the dependencies dictionary
            foreach (KeyValuePair<SpreadsheetCell, List<SpreadsheetCell>> kvp in deleteTheseDependencies)
            {
                foreach (SpreadsheetCell ssc in kvp.Value)
                {
                    dependencies[kvp.Key].Remove(ssc);
                }
            }
        }

        // This method justs updates the currentCells text
        public void UpdateAfterTextboxChanged(SpreadsheetCell currentCell, string val)
        {
            // Checks if the value is null or empty
            if (val == null || val == "")
            {
                return;
            }

            // Update the cells Text
            currentCell.Text = val;
        }

        // This method writes all the Cells that are not empty 
        // to an Xml document using the stream that is passed in.
        // Using the XDocument we create cell elements with Text and
        // value elements inside of it.
        public void WriteToXml(Stream stream)
        {
            XDocument doc = new XDocument();
            XElement elem = new XElement("Spreadsheet");

            // List of Cells That are not empty
            var query = CellsWithData();

            // Iterate through the list of not empty cells
            // and add the need elements to elem
            foreach (SpreadsheetCell ssc in query)
            {
                //Format: (Just an example)
                // <Cell Name = "A1">
                //      <Text/>100<Text/>
                //      <Value>100<Value/>
                // <Cell/>

                elem.Add(new XElement("Cell", new XAttribute("Name", GetCellName(ssc)), 
                            new XElement("Text", ssc.Text),
                            new XElement("Value", ssc.Value)
                    ));
            }

            // Finally add elem to the XDocument
            // and save it
            doc.Add(elem);
            doc.Save(stream);
        }

        // This method is the Load method for
        // the Xml file that the user selected (In Form1.cs).
        // It first clears the existing spreadsheet
        // and then it iterates through all the Cell
        // XElements and all the XElements inside that cell
        // and it updates the cell that needs to be updated.
        public void LoadXml(StreamReader sr)
        {
            ClearArray();          

            XDocument doc = XDocument.Load(sr);

            SpreadsheetCell currentCell;

            // For every Element tagged as "Cell"
            foreach (XElement xe in doc.Descendants("Cell"))
            {
                // save currentCell as the that the reader is reading
                currentCell = GetCell(xe.Attribute("Name").Value);

                // For every element inside of "Cell"
                // find out if it's a text or a value,
                // and update accordingly
                foreach (XElement xe2 in xe.Descendants())
                {
                    if (xe2.Name == "Text")
                    {
                        // If the element value is null set cell text as error
                        currentCell.Text = xe2.Value != null ? xe2.Value : "Error!";
                    }
                    else if (xe2.Name == "Value")
                    {
                        // If the element value is null set cell value as error
                        currentCell.Value = xe2.Value != null ? xe2.Value : "Error!";
                    }
 
                }
            }
        }

        // Method that runs a query to find
        // all the cells that are have data in them,
        // and reset their attributes to null
        public void ClearArray()
        {
            var query = CellsWithData();

            dependencies.Clear();
            variables.Clear();

            foreach (SpreadsheetCell cell in query)
            {
                cell.Text = null;
                cell.Value = null;
                cell.SetExpression(string.Empty);
            }
        }

        // Returns a list of Cells that
        // are not empty
        public List<SpreadsheetCell> CellsWithData()
        {
            return (from SpreadsheetCell myCell in cells
                    where myCell.Text != null
                    select myCell).ToList();
        }

        // Getter for the Version string
        public static string GetVersion()
        {
            return version;
        }
    }
}
