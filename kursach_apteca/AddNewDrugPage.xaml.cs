using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
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
    /// Логика взаимодействия для AddNewDrugPage.xaml
    /// </summary>
    public partial class AddNewDrugPage : Page
    {
        public AddNewDrugPage()
        {
            InitializeComponent();
        }

        private void CBNewTypeDrug_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void TBNewNameDrug_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void TBNewViewDrug_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void TBNewCostDrug_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var name = TBNewNameDrug.Text.Trim();
            var view = TBNewViewDrug.Text.Trim();
            var volume = TBNewVolumeDrug.Text.Trim();
            var place = TBNewPlaceDrug.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError("Введите название товара.", TBNewNameDrug);
                return;
            }

            if (CBNewTypeDrug.SelectedIndex <= 0)
            {
                ShowError("Выберите тип товара.", CBNewTypeDrug);
                return;
            }

            if (string.IsNullOrWhiteSpace(view))
            {
                ShowError("Введите вид товара.", TBNewViewDrug);
                return;
            }

            if (CBNewCategoryDrug.SelectedIndex <= 0)
            {
                ShowError("Выберите категорию товара.", CBNewCategoryDrug);
                return;
            }

            if (!TryParsePositiveDecimal(TBNewCostDrug.Text, out var cost))
            {
                ShowError("Введите корректную цену больше нуля.", TBNewCostDrug);
                return;
            }

            if (!int.TryParse(TBNewStockDrug.Text.Trim(), out var stock) || stock <= 0)
            {
                ShowError("Введите целое количество товара больше нуля.", TBNewStockDrug);
                return;
            }

            if (!Regex.IsMatch(volume, @"^\d+(?:[.,]\d+)?\s*(шт|мл|г|пак)$", RegexOptions.IgnoreCase))
            {
                ShowError("Введите объём в формате: 16 шт, 500 мл, 1 г или 2 пак.", TBNewVolumeDrug);
                return;
            }

            if (!TryGetDate(DPProductionDate, out var productionDate))
            {
                ShowError("Введите корректную дату производства в формате дд.ММ.гггг.", DPProductionDate);
                return;
            }

            if (!TryGetDate(DPSellByDate, out var sellByDate))
            {
                ShowError("Введите корректный срок годности в формате дд.ММ.гггг.", DPSellByDate);
                return;
            }

            if (productionDate.Date > DateTime.Today)
            {
                MessageBox.Show("Дата производства не может быть позже сегодняшней даты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                DPProductionDate.Focus();
                return;
            }

            if (sellByDate <= productionDate)
            {
                MessageBox.Show("Срок годности должен быть позже даты производства.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                DPSellByDate.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(place))
            {
                ShowError("Введите место хранения.", TBNewPlaceDrug);
                return;
            }

            if (CBNewRequireRecipe.SelectedIndex < 0)
            {
                ShowError("Укажите, требуется ли рецепт.", CBNewRequireRecipe);
                return;
            }

            var type = ((TextBlock)CBNewTypeDrug.SelectedItem).Text;
            var category = ((TextBlock)CBNewCategoryDrug.SelectedItem).Text;
            var isPrescription = CBNewRequireRecipe.SelectedIndex == 0;

            var drug = new Drugs
            {
                Name = name,
                Type = type,
                ViewN = view,
                Categoty = category == "Лекарства" ? "Лекарство" : category,
                Cost = cost,
                Stock = stock,
                Volume_per_package = volume,
                Production_date = productionDate,
                Sell_by = sellByDate,
                Place = place,
                IsPrescription = isPrescription
            };

            try
            {
                PharmacyEntities.GetContext().Drugs.Add(drug);
                PharmacyEntities.GetContext().SaveChanges();

                MessageBox.Show("Товар успешно добавлен.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);

                if (NavigationService != null && NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
                else
                {
                    // Без текущего продавца Medicines_Page создавать нельзя:
                    // конструктор страницы требует Sellers для проверки роли.
                    NavigationService?.Navigate(new AuthSellersPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить товар:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool TryParsePositiveDecimal(string text, out decimal value)
        {
            var normalizedText = (text ?? string.Empty).Trim().Replace(',', '.');
            return decimal.TryParse(
                       normalizedText,
                       NumberStyles.AllowDecimalPoint,
                       CultureInfo.InvariantCulture,
                       out value) && value > 0;
        }

        private static bool TryGetDate(DatePicker datePicker, out DateTime date)
        {
            if (datePicker.SelectedDate.HasValue)
            {
                date = datePicker.SelectedDate.Value.Date;
                return true;
            }

            return DateTime.TryParseExact(
                datePicker.Text.Trim(),
                new[] { "dd.MM.yyyy", "d.M.yyyy" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
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
