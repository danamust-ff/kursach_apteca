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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kursach_apteca
{
    /// <summary>
    /// Логика взаимодействия для AuthSellersPage.xaml
    /// </summary>
    public partial class AuthSellersPage : Page
    {
        private int failedAtt = 0;
        public AuthSellersPage()
        {
            InitializeComponent();

        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            string login = TBoxlog.Text;
            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Sellers seller = PharmacyEntities.GetContext().Sellers.FirstOrDefault(s => s.Login == login);
            if (seller!=null)
            {
                TBoxlog.Text = "";
                Manager.MainFrame.Navigate(new Medicines_Page(seller));
            }
            else
            {
                failedAtt++;
                if (failedAtt>0)
                {
                    MessageBox.Show("Введены неверные данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TBoxlog.Text = "";
                }
            } 
                
        }

        private void TBoxlog_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
