using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpreadsheetEngine;
using System.Xml.Serialization;
using System.IO;

namespace Spreadsheet
{
    // Vitaliy Trach - 11593957
    public partial class form1 : Form
    {
        public form1()
        {
            InitializeComponent();
            spreadsheet.CellPropertyChanged += Spreadsheet_CellPropertyChanged;
            textBox1.Hide();
        }
     
        private SpreadSheet spreadsheet = new SpreadSheet(50, 26);

        // Form1 constructor, creates columns A-Z
        private void Form1_Load(object sender, EventArgs e)
        {
            // 65 is A on the ascii table,
            // and 90 is Z
            for (int i = 65; i <= 90; i++)
            {
                dataGridView1.Columns.Add(((char)i).ToString(), ((char)i).ToString());
            }

            // Adds 50 rows and numbers them 1-50
            for (int i = 0; i < 50; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();                             
            }

            dataGridView1.ClearSelection();
        }

        // This method decidess if the the datagridview cell should display the spreadsheet cell value or text
        private void Spreadsheet_CellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var cell = sender as Cell;

            if (e.PropertyName == "Value")
            {
                dataGridView1[cell.ColumnIndex, cell.RowIndex].Value = cell.Value;
            }
            else if (e.PropertyName == "Text")
            {
                dataGridView1[cell.ColumnIndex, cell.RowIndex].Value = cell.Text;
            }
        }

        // Event for when stuff in the datagridview cell is being changed
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // Sets the text of the datagridview cell to the text of the spreadsheetcell
            dataGridView1[e.ColumnIndex, e.RowIndex].Value = spreadsheet.GetCell(e.RowIndex, e.ColumnIndex).Text;
        }

        // Event for when the user finishes editing a cell
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string textboxText = "";

            // If datagridview cell is not empty then set value to the cell text
            if (dataGridView1[e.ColumnIndex, e.RowIndex].Value != null)
            {
                textboxText = dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();
            }
              
            // If the value is not empty
            if (textboxText != "")
            {
                // Sets the text of that Spreadsheetcell to what the textbox has
                spreadsheet.GetCell(e.RowIndex, e.ColumnIndex).Text = textboxText.ToString();
                // Setting the datagridview cell to the spreadsheetcell value
                dataGridView1[e.ColumnIndex, e.RowIndex].Value = spreadsheet.GetCell(e.RowIndex, e.ColumnIndex).Value;
            }
        }

        // Keydown event for the textbox that checks if the user pressed enter or not
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // If keydown is enter
            if (e.KeyCode == Keys.Return)
            {
                // Update the spreadsheet with the contents inside the textbox
                spreadsheet.UpdateAfterTextboxChanged(spreadsheet.GetCell(
                    dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex)
                    , textBox1.Text);

                // update datagridview with the spreadsheet cell value
                dataGridView1.SelectedCells[0].Value = spreadsheet.GetCell(
                    dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex)
                    .Value;

                // Hide textbox
                textBox1.Hide();
                textBox1.Text = String.Empty;
            }
        }           
        
        // Click event for the datagridview cells that pops up a textbox in
        // the middle of the screen so the user can edit the cells contents from
        // inside the textbox
        private void dataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            // Get row and column of the selected cell in datagridview
            int row = dataGridView1.CurrentCell.RowIndex;
            int column = dataGridView1.CurrentCell.ColumnIndex;

            textBox1.Location = new Point(GetWidth(), GetHeight());

            // If textbox is already open then clear it
            if (textBox1.Visible)
            {
                textBox1.Clear();
            }
            // else if it's hidden, show it
            else
            {
                textBox1.Show();
            }        

            // If the spreadsheet cell's text is not null then update the textbox
            // to the text of the spreadsheetcell
            if (spreadsheet.GetCell(row, column).Text != null)
            {
                textBox1.Text = spreadsheet.GetCell(row, column).Text.ToString();
            }

            // Selects all the text in the textbox
            textBox1.SelectAll();
            textBox1.Focus();
        }

        // Event for resizes the form1 application, the whole
        // purpose is to move the textbox to the new horizontal center
        private void form1_Resize(object sender, EventArgs e)
        {           
            textBox1.Location = new Point(GetWidth(), GetHeight());
        }

        // Returns the height location the textbox should move to
        private int GetHeight()
        {
            // return (this.Height / 2) - (textBox1.Height / 2);
            return 0;
        }

        // Returns the width location the textbox should move to
        private int GetWidth()
        {
            return (this.Width / 2) - (textBox1.Width / 2);
        }

        // Event for when the user presses the save button and finds
        // a file location to save the spreadsheet.
        // It then calls the spreadsheets WriteToXml method
        // that uses a stream passed to it and a XDocument to write
        // in Xml format
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "Xml File|*.Xml";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;

            // Check if the dialog opens, and send the filestream to the WriteToXml method
            // in Spreadsheet
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create))
                {
                    spreadsheet.WriteToXml(fs);
                }
            }
        }

        // Event for when the user presses load file and chooses an Xml file to load.
        // It passes a streamreader to the LoadXml method in spreadsheet that loads
        // and parses Xml file and fills the cells.
        // Lastely the dataGridView cells in winforms get updated
        private void loadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "Xml File|*.Xml";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;

            // Check if load dialog opens, if it does it passes
            // a streamreader to the LoadXml method
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(ofd.FileName))
                {
                    spreadsheet.LoadXml(sr);
                }
            }

            // List of cells that need to be updated in the GUI
            var cells = spreadsheet.CellsWithData();

            // Iterate through the list and update the GUI cells
            // with information from the Spreadsheet engine
            foreach (var cell in cells)
            {
                dataGridView1[cell.ColumnIndex, cell.RowIndex].Value = spreadsheet.GetCell(cell.RowIndex, cell.ColumnIndex).Value;
            }
        }

        // When the File menu is click, hide the textbox1 and clear all
        // selections of cells. (Looks cleaner this way)
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (textBox1.Visible)
            {
                textBox1.Hide();
            }
            dataGridView1.ClearSelection();
        }

        // When the user presses the "New" menu option, we clear
        // the spreadsheet cells array, and the datagridview also
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Clear spreadsheet
            spreadsheet.ClearArray();
            var cells = spreadsheet.CellsWithData();

            // Clear GUI cells
            foreach (var cell in cells)
            {
                dataGridView1[cell.ColumnIndex, cell.RowIndex].Value = null;
            }
        }

        // Event for when the user selects the About menu option, it creates
        // an instance of the AboutBox1 and displays it.
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox = new AboutBox1();
            aboutBox.Show();
        }
    }
}
