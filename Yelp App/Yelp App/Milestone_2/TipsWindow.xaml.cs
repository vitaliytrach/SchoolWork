using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Milestone_2
{
    /// <summary>
    /// Interaction logic for TipsWindow1.xaml
    /// </summary>
    public partial class TipsWindow : Window
    {
        // Window that displays tips for a certain business
        public TipsWindow()
        {
            InitializeComponent();

            AddColumnToGrid(tipsForBusinessDataGrid, "User Name", "name");
            AddColumnToGrid(tipsForBusinessDataGrid, "Date", "date");
            AddColumnToGrid(tipsForBusinessDataGrid, "Tip", "tip");
            AddColumnToGrid(tipsForBusinessDataGrid, "Likes", "likes");
        }

        private void AddColumnToGrid(DataGrid myGrid, string header, string binding)
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = header;
            col1.Binding = new Binding(binding);
            myGrid.Columns.Add(col1);
        }

        public void SetTitle(string title)
        {
            tipsWindow.Title = title;
        }

    }
}
