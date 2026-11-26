using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для BestBeforDatePage.xaml
    /// </summary>
    public partial class BestBeforDatePage : Page
    {
        public BestBeforDatePage()
        {
            InitializeComponent();
            LoadExpiryDates();
        }

        private void LoadExpiryDates()
        {
            var today = DateTime.Today;
            var rows = PharmacyEntities.GetContext().Drugs
                .ToList()
                .Where(drug => drug.Sell_by.Date <= today.AddDays(90))
                .OrderBy(drug => drug.Sell_by)
                .Select(drug => new ExpiryDrugRow(drug, today))
                .ToList();

            BestBeforDate.ItemsSource = rows;
        }

        private void BestBeforDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class ExpiryDrugRow
    {
        public string DrugName { get; private set; }
        public int Quantity { get; private set; }
        public DateTime SellBy { get; private set; }
        public string Status { get; private set; }
        public string StatusBackground { get; private set; }

        public ExpiryDrugRow(Drugs drug, DateTime today)
        {
            DrugName = drug.Name;
            Quantity = drug.Stock;
            SellBy = drug.Sell_by;

            var daysLeft = (drug.Sell_by.Date - today).Days;

            if (daysLeft < 0)
            {
                Status = "Срок истёк";
                StatusBackground = "#FFFFCDD2";
            }
            else if (daysLeft <= 30)
            {
                Status = "Заканчивается в течение 30 дней";
                StatusBackground = "#FFFFCDD2";
            }
            else if (daysLeft <= 60)
            {
                Status = "Заканчивается в течение 60 дней";
                StatusBackground = "#FFFFF9C4";
            }
            else if (daysLeft <= 90)
            {
                Status = "Заканчивается в течение 90 дней";
                StatusBackground = "#FFC8E6C9";
            }
            else
            {
                Status = "Более 90 дней";
                StatusBackground = "#FFF5F5F5";
            }
        }
    }
}
