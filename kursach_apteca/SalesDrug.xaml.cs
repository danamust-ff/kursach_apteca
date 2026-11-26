using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace kursach_apteca
{
    public partial class SalesDrug : Page
    {
        public SalesDrug()
        {
            InitializeComponent();

            var today = DateTime.Today;
            ReportStartDatePicker.SelectedDate = new DateTime(today.Year, today.Month, 1);
            ReportEndDatePicker.SelectedDate = today;

            LoadSales();

        }

        private void LoadSales()
        {
            var context = PharmacyEntities.GetContext();
            var drugs = context.Drugs.ToDictionary(d => d.DrugID, d => d.Name);
            var drugPrices = context.Drugs.ToDictionary(d => d.DrugID, d => d.Cost);
            var buyers = context.Buyers.ToDictionary(b => b.BuyerID, GetFullName);
            var sellers = context.Sellers.ToDictionary(s => s.SellerID, GetFullName);
            var recipes = context.Recipes.ToDictionary(r => r.RecipeID, r => r.DoctorName);

            var rows = context.Sales
                .ToList()
                .Select(s => new SaleRow
                {
                    SaleID = s.SaleID,
                    SaleDate = s.SaleDate,
                    Quantity = s.Quantity,
                    SaleAmount = s.Quantity * (drugPrices.ContainsKey(s.DrugID) ? drugPrices[s.DrugID] : 0m),
                    DrugName = drugs.ContainsKey(s.DrugID)
                        ? drugs[s.DrugID]
                        : "Не найдено",
                    BuyerName = s.BuyerID.HasValue && buyers.ContainsKey(s.BuyerID.Value)
                        ? buyers[s.BuyerID.Value]
                        : "—",
                    SellerName = sellers.ContainsKey(s.SellerID)
                        ? sellers[s.SellerID]
                        : "Не найдено",
                    RecipeInfo = s.RecipeID.HasValue && recipes.ContainsKey(s.RecipeID.Value)
                        ? "№" + s.RecipeID.Value + ", врач: " + (recipes[s.RecipeID.Value] ?? "—")
                        : "—",
                    PrescriptionText = s.ByPrescription ? "Да" : "Нет"
                })
                .OrderByDescending(s => s.SaleDate)
                .ToList();


            var saleDateFilter = TBSaleDate.Text.Trim();
            var drugFilter = TBNameDrug.Text.Trim();
            var buyerFilter = TBBuyerName.Text.Trim();
            var sellerFilter = TBSellerName.Text.Trim();

            rows = rows.Where(p =>
                (string.IsNullOrWhiteSpace(saleDateFilter) ||
                 p.SaleDate.ToString("dd.MM.yyyy").Contains(saleDateFilter)) &&
                (string.IsNullOrWhiteSpace(drugFilter) ||
                 ContainsIgnoreCase(p.DrugName, drugFilter)) &&
                (string.IsNullOrWhiteSpace(buyerFilter) ||
                 ContainsIgnoreCase(p.BuyerName, buyerFilter)) &&
                (string.IsNullOrWhiteSpace(sellerFilter) ||
                 ContainsIgnoreCase(p.SellerName, sellerFilter))
            ).ToList();


            SalesMedicinse.ItemsSource = rows;
        }

        private static bool ContainsIgnoreCase(string value, string searchText)
        {
            return !string.IsNullOrEmpty(value) &&
                   value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string LimitText(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, maxLength - 1) + "…";
        }
        


        private static string GetFullName(Buyers buyer)
        {
            return string.Join(" ", new[]
            {
                buyer.LastName,
                buyer.FirstName,
                buyer.Patronymic
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static string GetFullName(Sellers seller)
        {
            return string.Join(" ", new[]
            {
                seller.LastName,
                seller.FirstName,
                seller.Patronymic
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private void SalesMedicinse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TBSaleDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSales();
        }

        private void TBNameDrug_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSales();
        }

        private void TBBuyerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSales();
        }

        private void TBSellerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSales();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!ReportStartDatePicker.SelectedDate.HasValue ||
                !ReportEndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите начальную и конечную даты отчёта.", "Отчёт",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startDate = ReportStartDatePicker.SelectedDate.Value.Date;
            var endDate = ReportEndDatePicker.SelectedDate.Value.Date;

            if (startDate > endDate)
            {
                MessageBox.Show("Начальная дата не может быть позже конечной.", "Отчёт",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var context = PharmacyEntities.GetContext();
            var drugs = context.Drugs.ToDictionary(d => d.DrugID, d => d.Name);
            var drugPrices = context.Drugs.ToDictionary(d => d.DrugID, d => d.Cost);
            var buyers = context.Buyers.ToDictionary(b => b.BuyerID, GetFullName);
            var sellers = context.Sellers.ToDictionary(s => s.SellerID, GetFullName);

            var sales = context.Sales
                .ToList()
                .Where(s => s.SaleDate.Date >= startDate && s.SaleDate.Date <= endDate)
                .OrderBy(s => s.SaleDate)
                .ToList();

            if (sales.Count == 0)
            {
                MessageBox.Show("За выбранный период продаж не найдено.", "Отчёт",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "PDF-файл (*.pdf)|*.pdf",
                FileName = "Отчёт_о_продажах_" + startDate.ToString("dd.MM.yyyy") + "-" + endDate.ToString("dd.MM.yyyy") + ".pdf",
                AddExtension = true
            };

            if (saveDialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                CreateSalesPdf(saveDialog.FileName, startDate, endDate, sales, drugs, drugPrices, buyers, sellers);
                MessageBox.Show("Отчёт успешно сохранён.", "Отчёт",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Не удалось создать PDF-файл:\n" + exception.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void CreateSalesPdf(
            string fileName,
            DateTime startDate,
            DateTime endDate,
            IList<Sales> sales,
            IDictionary<int, string> drugs,
            IDictionary<int, decimal> drugPrices,
            IDictionary<int, string> buyers,
            IDictionary<int, string> sellers)
        {
            var document = new PdfDocument();
            document.Info.Title = "Отчёт о продажах";

            var regularFont = new XFont("Arial", 9, XFontStyle.Regular);
            var boldFont = new XFont("Arial", 11, XFontStyle.Bold);
            var titleFont = new XFont("Arial", 16, XFontStyle.Bold);
            var headerFont = new XFont("Arial", 9, XFontStyle.Bold);

            PdfPage page = null;
            XGraphics graphics = null;
            double y = 0;

            Action createPage = () =>
            {
                if (graphics != null)
                {
                    graphics.Dispose();
                    graphics = null;
                }

                page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                page.Orientation = PdfSharp.PageOrientation.Landscape;
                graphics = XGraphics.FromPdfPage(page);
                y = 35;
            };

            Action drawTableHeader = () =>
            {
                graphics.DrawString("№", headerFont, XBrushes.Black, new XRect(25, y, 35, 18), XStringFormats.TopLeft);
                graphics.DrawString("Дата", headerFont, XBrushes.Black, new XRect(60, y, 70, 18), XStringFormats.TopLeft);
                graphics.DrawString("Товар", headerFont, XBrushes.Black, new XRect(130, y, 210, 18), XStringFormats.TopLeft);
                graphics.DrawString("Кол-во", headerFont, XBrushes.Black, new XRect(340, y, 55, 18), XStringFormats.TopLeft);
                graphics.DrawString("Сумма", headerFont, XBrushes.Black, new XRect(395, y, 75, 18), XStringFormats.TopLeft);
                graphics.DrawString("Покупатель", headerFont, XBrushes.Black, new XRect(470, y, 130, 18), XStringFormats.TopLeft);
                graphics.DrawString("Продавец", headerFont, XBrushes.Black, new XRect(600, y, 135, 18), XStringFormats.TopLeft);
                graphics.DrawLine(XPens.Black, 25, y + 17, 735, y + 17);
                y += 24;
            };

            createPage();
            graphics.DrawString("ОТЧЁТ О ПРОДАЖАХ", titleFont, XBrushes.Black, new XPoint(25, y));
            y += 25;
            graphics.DrawString("Период: " + startDate.ToString("dd.MM.yyyy") + " — " + endDate.ToString("dd.MM.yyyy"), regularFont, XBrushes.Black, new XPoint(25, y));
            y += 18;
            graphics.DrawString("Количество операций: " + sales.Count, boldFont, XBrushes.Black, new XPoint(25, y));
            y += 28;
            var totalAmount = sales.Sum(s => s.Quantity *
                (drugPrices.ContainsKey(s.DrugID) ? drugPrices[s.DrugID] : 0m));
            graphics.DrawString("Общая сумма продаж: " + totalAmount.ToString("N2") + " руб.", boldFont, XBrushes.Black, new XPoint(25, y));
            y += 28;
            drawTableHeader();

            foreach (var sale in sales)
            {
                if (y > page.Height - 35)
                {
                    createPage();
                    drawTableHeader();
                }

                var drugName = drugs.ContainsKey(sale.DrugID) ? drugs[sale.DrugID] : "Не найдено";
                var buyerName = sale.BuyerID.HasValue && buyers.ContainsKey(sale.BuyerID.Value)
                    ? buyers[sale.BuyerID.Value]
                    : "—";
                var sellerName = sellers.ContainsKey(sale.SellerID) ? sellers[sale.SellerID] : "Не найдено";

                graphics.DrawString(sale.SaleID.ToString(), regularFont, XBrushes.Black, new XRect(25, y, 35, 18), XStringFormats.TopLeft);
                graphics.DrawString(sale.SaleDate.ToString("dd.MM.yyyy"), regularFont, XBrushes.Black, new XRect(60, y, 70, 18), XStringFormats.TopLeft);
                graphics.DrawString(drugName, regularFont, XBrushes.Black, new XRect(130, y, 210, 18), XStringFormats.TopLeft);
                graphics.DrawString(sale.Quantity.ToString(), regularFont, XBrushes.Black, new XRect(340, y, 55, 18), XStringFormats.TopLeft);
                var saleAmount = sale.Quantity * (drugPrices.ContainsKey(sale.DrugID) ? drugPrices[sale.DrugID] : 0m);
                graphics.DrawString(saleAmount.ToString("N2") + " руб.", regularFont, XBrushes.Black, new XRect(395, y, 75, 18), XStringFormats.TopLeft);
                graphics.DrawString(LimitText(buyerName, 19), regularFont, XBrushes.Black, new XRect(470, y, 130, 18), XStringFormats.TopLeft);
                graphics.DrawString(LimitText(sellerName, 19), regularFont, XBrushes.Black, new XRect(600, y, 135, 18), XStringFormats.TopLeft);
                y += 18;
            }

            if (graphics != null)
            {
                graphics.Dispose();
                graphics = null;
            }

            foreach (var currentPage in document.Pages)
            {
                using (var pageGraphics = XGraphics.FromPdfPage(currentPage, XGraphicsPdfPageOptions.Append))
                {
                    pageGraphics.DrawString("Сформировано: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"), regularFont,
                        XBrushes.Gray, new XRect(25, currentPage.Height - 25, 300, 15), XStringFormats.TopLeft);
                }
            }

            document.Save(fileName);
        }
    }

    public class SaleRow
    {
        public int SaleID { get; set; }
        public DateTime SaleDate { get; set; }
        public int Quantity { get; set; }
        public decimal SaleAmount { get; set; }
        public string DrugName { get; set; }
        public string BuyerName { get; set; }
        public string SellerName { get; set; }
        public string RecipeInfo { get; set; }
        public string PrescriptionText { get; set; }
    }
}
