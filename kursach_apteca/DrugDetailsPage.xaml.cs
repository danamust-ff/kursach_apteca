using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
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
    /// Логика взаимодействия для DrugDetailsPage.xaml
    /// </summary>
    public partial class DrugDetailsPage : Page
    {
        public Drugs Drug { get; }

        public DrugDetailsPage()
        {
            InitializeComponent();
        }

        public DrugDetailsPage(Drugs drug)
        {
            InitializeComponent();
            Drug = drug;
            DataContext = drug;

            TBPrice.Text = drug.Cost.ToString("0.00", CultureInfo.InvariantCulture);
            TBStock.Text = drug.Stock.ToString(CultureInfo.InvariantCulture);
            TBPlace.Text = drug.Place ?? string.Empty;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            decimal cost;
            int stock;

            var priceText = (TBPrice.Text ?? string.Empty).Trim().Replace(',', '.');
            var stockText = (TBStock.Text ?? string.Empty).Trim();
            var placeText = (TBPlace.Text ?? string.Empty).Trim();

            if (!decimal.TryParse(priceText, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out cost))
            {
                MessageBox.Show("Введите корректную цену.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TBPrice.Focus();
                return;
            }

            if (!int.TryParse(stockText, NumberStyles.Integer, CultureInfo.InvariantCulture, out stock) || stock < 0)
            {
                MessageBox.Show("Введите корректное количество на складе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TBStock.Focus();
                return;
            }

            if (cost <= 0)
            {
                MessageBox.Show("Цена не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TBPrice.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(placeText))
            {
                MessageBox.Show("Введите место хранения товара!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TBPlace.Focus();
                return;
            }

            Drug.Cost = cost;
            Drug.Stock = stock;
            Drug.Place = placeText;

            PharmacyEntities.GetContext().SaveChanges();
            MessageBox.Show("Данные товара сохранены.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

       
    }
}
