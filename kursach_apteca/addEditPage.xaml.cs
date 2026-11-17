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
    /// Логика взаимодействия для addEditPage.xaml
    /// </summary>
    public partial class addEditPage : Page
    {
        private Drugs _currentDrug = new Drugs();
        private Drugs selectedDrug;

        public addEditPage()
        {
            InitializeComponent();
            _currentDrug = new Drugs(); 
            TxtTitle.Text = "Добавление лекарства";
        }


        public addEditPage(Drugs selectedDrug)
        {
            InitializeComponent();
            _currentDrug = selectedDrug;
            TxtTitle.Text = "Редактирование лекарства";

            // Заполняем поля текущими данными
            TxtName.Text = _currentDrug.Name;
            TxtViewN.Text = _currentDrug.ViewN;
            TxtCost.Text = _currentDrug.Cost.ToString();

            // Выбираем нужный тип в ComboBox (если типы совпадают)
            foreach (ComboBoxItem item in CmbType.Items)
            {
                if (item.Content.ToString() == _currentDrug.Type)
                {
                    CmbType.SelectedItem = item;
                    break;
                }
            }
        }





        // Обработчик кнопки "Назад"
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Возвращаемся на предыдущую страницу (Medicines_Page)
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
        // Обработчик кнопки "Сохранить"
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Простая валидация
            if (string.IsNullOrWhiteSpace(TxtName.Text) || string.IsNullOrWhiteSpace(TxtCost.Text))
            {
                MessageBox.Show("Заполните хотя бы название и цену!", "Ошибка");
                return;
            }

            // 2. Переносим данные из TextBox'ов в объект
            _currentDrug.Name = TxtName.Text;
            _currentDrug.ViewN = TxtViewN.Text;

            // 3. Парсим цену
            if (decimal.TryParse(TxtCost.Text, out decimal cost))
            {
                _currentDrug.Cost = cost;
            }
            else
            {
                MessageBox.Show("Цена должна быть числом!", "Ошибка");
                return;
            }

            if (CmbType.SelectedItem != null)
            {
                _currentDrug.Type = ((ComboBoxItem)CmbType.SelectedItem).Content.ToString();
            }

           
            try
            {
                // Получаем контекст базы данных (как вы делали на странице со списком)
                var db = PharmacyEntities.GetContext();

              
                if (_currentDrug.DrugID == 0) 
                {
                    db.Drugs.Add(_currentDrug);
                }
              
                db.SaveChanges();

                MessageBox.Show("Успешно сохранено!");

                // Возвращаемся назад к списку
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                // Если база данных выдаст ошибку (например, нет связи), мы ее увидим
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка");
            }
        }
    }
}

