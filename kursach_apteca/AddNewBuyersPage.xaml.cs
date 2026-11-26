using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для AddNewBuyersPage.xaml
    /// </summary>
    public partial class AddNewBuyersPage : Page
    {
        public AddNewBuyersPage()
        {
            InitializeComponent();
        }

        private void TBLastName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TBFirstName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TBPatronymicName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TBPhone_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TBAddress_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var lastName = TBLastName.Text.Trim();
            var firstName = TBFirstName.Text.Trim();
            var patronymic = TBPatronymicName.Text.Trim();
            var phone = TBPhone.Text.Trim();
            var address = TBAddress.Text.Trim();

            if (string.IsNullOrWhiteSpace(lastName))
            {
                ShowError("Введите фамилию клиента.", TBLastName);
                return;
            }

            if (string.IsNullOrWhiteSpace(firstName))
            {
                ShowError("Введите имя клиента.", TBFirstName);
                return;
            }

            if (!Regex.IsMatch(phone, @"^(?:\+7|8)\s*\(?\d{3}\)?[\s-]*\d{3}[\s-]*\d{2}[\s-]*\d{2}$"))
            {
                ShowError("Введите телефон в формате +7 (999) 123-45-67 или 89991234567.", TBPhone);
                return;
            }

            var buyer = new Buyers
            {
                LastName = lastName,
                FirstName = firstName,
                Patronymic = patronymic,
                Phone = phone,
                Address = address
            };

            try
            {
                var context = PharmacyEntities.GetContext();
                context.Buyers.Add(buyer);
                context.SaveChanges();

                MessageBox.Show("Клиент успешно добавлен.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);

                if (NavigationService != null && NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить клиента:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void ShowError(string message, Control control)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            control.Focus();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
