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
    /// Interaction logic for AccountConfirm.xaml
    /// </summary>
    public partial class AccountConfirm : Window
    {
        public AccountConfirm()
        {
            InitializeComponent();

        }

        public void ConfirmFailed()
        {
            Background = Brushes.IndianRed;
            ConfirmTextBox.Background = Brushes.DarkRed;
            ConfirmTextBox.Foreground = Brushes.NavajoWhite;
            ConfirmTextBox.Text = "Account Creation Failed";
            ConfirmTextBox.Height += 25;
            this.Height = this.Height + 25;
        }

        public void ConfirmSuccess()
        {
            ConfirmTextBox.Background = Brushes.ForestGreen;
            ConfirmTextBox.Foreground = Brushes.NavajoWhite;
            ConfirmTextBox.Text = "Account Created";      
        }

        public void LoginFailed()
        {
            ConfirmTextBox.Background = Brushes.DarkRed;
            ConfirmTextBox.Foreground = Brushes.NavajoWhite;
            ConfirmTextBox.Text = "Login Failed";
        }
    }
}
