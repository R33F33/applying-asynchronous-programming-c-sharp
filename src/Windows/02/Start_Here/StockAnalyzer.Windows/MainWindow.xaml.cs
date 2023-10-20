using Newtonsoft.Json;
using StockAnalyzer.Core;
using StockAnalyzer.Core.Domain;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace StockAnalyzer.Windows
{
    public partial class MainWindow : Window
    {
        private static string API_URL = "https://ps-async.fekberg.com/api/stocks";
        private Stopwatch stopwatch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
        }



        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            BeforeLoadingStockData();

            await GetStocksLocal();

            AfterLoadingStockData();
        }

        // Never use async void, async Task is the way to go
        // if you get errors in async void - app crash :) 
        // with async Task you can do a try/catch 
        // with await you can catch what happens in the async methods
        private async Task GetStocks()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync($"{API_URL}/{StockIdentifier.Text}");

                    // GOOD - does not block
                    var content = await response.Content.ReadAsStringAsync();

                    // BAD - blocks the thread
                    // var content = response.Content.ReadAsStringAsync().Result;

                    var data = JsonConvert.DeserializeObject<IEnumerable<StockPrice>>(content);

                    Stocks.ItemsSource = data;
                }
            }
            catch (System.Exception e)
            {
                // Notes is WPF UI element
                Notes.Text = "Kur " + e;
                throw;
            }
            
        }

        private async Task GetStocksLocal()
        {
            try
            {
                var store = new DataStore();

                var response = store.GetStockPrices(StockIdentifier.Text); // StockIdentifier is WPF UI element

                Stocks.ItemsSource = await response;
            }
            catch (System.Exception e)
            {
                Notes.Text = "Nope - " + e.Message;
            }

        }






        private void BeforeLoadingStockData()
        {
            stopwatch.Restart();
            StockProgress.Visibility = Visibility.Visible;
            StockProgress.IsIndeterminate = true;
        }

        private void AfterLoadingStockData()
        {
            StocksStatus.Text = $"Loaded stocks for {StockIdentifier.Text} in {stopwatch.ElapsedMilliseconds}ms";
            StockProgress.Visibility = Visibility.Hidden;
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));

            e.Handled = true;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
