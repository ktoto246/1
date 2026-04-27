using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using WpfApp1.Models;

namespace WpfApp1.Pages
{
    public partial class DashboardPage : Page
    {
        private УчетСебестоимостиContext _context;

        // Свойства для привязки (Binding) к графикам в XAML
        public SeriesCollection BarSeries { get; set; }
        public string[] BarLabels { get; set; }
        public Func<double, string> Formatter { get; set; }
        public SeriesCollection PieSeries { get; set; }

        public DashboardPage()
        {
            InitializeComponent();

            // Инициализируем коллекции, чтобы WPF не выкинул NullReference
            BarSeries = new SeriesCollection();
            PieSeries = new SeriesCollection();
            Formatter = value => value.ToString("N2") + " ₽";

            // Говорим окну, что данные для биндинга лежат прямо в этом классе
            DataContext = this;

            LoadData();
        }

        private void LoadData()
        {
            _context = new УчетСебестоимостиContext();

            // Грузим продукцию для ComboBox и столбчатого графика
            var products = _context.Продукцияs.ToList();

            CmbProducts.ItemsSource = products;

            // --- 1. ЗАПОЛНЯЕМ СТОЛБЧАТЫЙ ГРАФИК ---
            BarSeries.Clear();
            BarLabels = products.Select(p => p.Название).ToArray();

            BarSeries.Add(new ColumnSeries
            {
                Title = "Себестоимость",
                Values = new ChartValues<decimal>(products.Select(p => p.Себестоимость))
            });

            // Выбираем первый товар в списке по умолчанию, чтобы нарисовать бублик
            if (products.Any())
            {
                CmbProducts.SelectedIndex = 0;
            }
        }

        // --- 2. ЗАПОЛНЯЕМ КРУГОВОЙ ГРАФИК ПРИ СМЕНЕ ТОВАРА ---
        private void CmbProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbProducts.SelectedItem is Продукция selectedProduct)
            {
                // Подтягиваем связанные данные для выбранного товара
                var productDetails = _context.Продукцияs
                    .Include(p => p.СоставПродукцииs)
                        .ThenInclude(c => c.IdМатериалаNavigation)
                    .Include(p => p.Трудозатратыs)
                    .FirstOrDefault(p => p.IdПродукта == selectedProduct.IdПродукта);

                if (productDetails == null) return;

                // Считаем суммы
                decimal materialCost = productDetails.СоставПродукцииs.Sum(c => c.Количество * c.IdМатериалаNavigation.ЦенаЗаЕдиницу);
                decimal laborCost = productDetails.Трудозатратыs.Sum(t => t.СтоимостьРаботы);
                decimal overhead = productDetails.НакладныеРасходы;

                // Чистим старый пирог и рисуем новый
                PieSeries.Clear();

                PieSeries.Add(new PieSeries
                {
                    Title = "Материалы",
                    Values = new ChartValues<decimal> { materialCost },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("{0:N2} ₽ ({1:P})", chartPoint.Y, chartPoint.Participation)
                });

                PieSeries.Add(new PieSeries
                {
                    Title = "Трудозатраты",
                    Values = new ChartValues<decimal> { laborCost },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("{0:N2} ₽ ({1:P})", chartPoint.Y, chartPoint.Participation)
                });

                PieSeries.Add(new PieSeries
                {
                    Title = "Накладные расходы",
                    Values = new ChartValues<decimal> { overhead },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("{0:N2} ₽ ({1:P})", chartPoint.Y, chartPoint.Participation)
                });
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            _context.Dispose();
            LoadData();
        }
    }
}
