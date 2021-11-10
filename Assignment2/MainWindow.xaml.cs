using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
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
using System.Xml.Linq;

namespace Assignment2
{
    public partial class MainWindow : Window
    {
        private Thickness spacing = new Thickness(5);
        private HttpClient http = new HttpClient();
        // We will need these as instance variables to access in event handlers.
        private TextBox addFeedTextBox;
        private Button addFeedButton;
        private ComboBox selectFeedComboBox;
        private Button loadArticlesButton;
        private StackPanel articlePanel;

        private List<XDocument> sites = new List<XDocument>();

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }



        private void Start()
        {
            //================= Premade Ui =========================
            #region
            // Window options
            Title = "Feed Reader";
            Width = 800;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            

            // Scrolling
            var root = new ScrollViewer();
            root.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Content = root;

            // Main grid
            var grid = new Grid();
            root.Content = grid;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });


            var addFeedLabel = new Label
            {
                Content = "Feed URL:",
                Margin = spacing
            };
            grid.Children.Add(addFeedLabel);

            addFeedTextBox = new TextBox
            {
                Margin = spacing,
                Padding = spacing
            };
            grid.Children.Add(addFeedTextBox);
            Grid.SetColumn(addFeedTextBox, 1);

            addFeedButton = new Button
            {
                Content = "Add Feed",
                Margin = spacing,
                Padding = spacing
            };
            grid.Children.Add(addFeedButton);
            Grid.SetColumn(addFeedButton, 2);
            addFeedButton.Click += AddFeedButton_Click;

            var selectFeedLabel = new Label
            {
                Content = "Select Feed:",
                Margin = spacing
            };
            grid.Children.Add(selectFeedLabel);
            Grid.SetRow(selectFeedLabel, 1);

            selectFeedComboBox = new ComboBox
            {
                Margin = spacing,
                Padding = spacing,
                IsEditable = false
            };
            selectFeedComboBox.Items.Add("-- All feeds --");
            selectFeedComboBox.SelectedIndex = 0;
            grid.Children.Add(selectFeedComboBox);
            Grid.SetRow(selectFeedComboBox, 1);
            Grid.SetColumn(selectFeedComboBox, 1);

            loadArticlesButton = new Button
            {
                Content = "Load Articles",
                Margin = spacing,
                Padding = spacing
            };
            grid.Children.Add(loadArticlesButton);
            Grid.SetRow(loadArticlesButton, 1);
            Grid.SetColumn(loadArticlesButton, 2);
            loadArticlesButton.Click += LoadArticlesButton_Click;

            articlePanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = spacing
            };
            grid.Children.Add(articlePanel);
            Grid.SetRow(articlePanel, 2);
            Grid.SetColumnSpan(articlePanel, 3);
            #endregion
            //==================================================

        }


        private async void LoadArticlesButton_Click(object sender, RoutedEventArgs e)
        {
            loadArticlesButton.IsEnabled = false;
            articlePanel.Children.Clear();
            if (selectFeedComboBox.SelectedIndex == 0)
            {
                ShowFiveFeedsFromAllSites();
            }
            else
            {
                int index = --selectFeedComboBox.SelectedIndex;
                var site = sites.ElementAt(index);
                for (int i = 0; i < 5; i++)
                {
                    GetFeed(site,i);
                }
            }

            await Task.Delay(1000);
            loadArticlesButton.IsEnabled = true;
        }



        private void ShowFiveFeedsFromAllSites()
        {
            foreach (XDocument item in sites)
            {
                for (int i = 0; i < 5; i++)
                {
                    GetFeed(item, i);
                }
            }
        }



        private void GetFeed(XDocument item, int i)
        {
            string title = item.Descendants("item").Descendants("title").Skip(i).First().Value;
            var pubDate = item.Descendants("item").Descendants("pubDate").Skip(i).First().Value;
            var newb = DateTime.ParseExact(pubDate.Substring(0, 25), "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            var artivleHolder = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = spacing
            };
            articlePanel.Children.Add(artivleHolder);

            var articleTitle = new TextBlock
            {
                Text = $"{newb} - {title} # {i + 1} ",
                FontWeight = FontWeights.Bold,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            artivleHolder.Children.Add(articleTitle);
            var articleWebsite = new TextBlock
            {
                Text = $"{item.Descendants("title").First().Value}"
            };
            artivleHolder.Children.Add(articleWebsite);
        }



        private async void AddFeedButton_Click(object sender, RoutedEventArgs e)
        {
            /*Test Rss Feeds
             * https://feeds.fireside.fm/bibleinayear/rss
             * https://www.cinemablend.com/rss/topic/news/movies
             * https://www.comingsoon.net/feed
             * https://screencrush.com/feed/ */

            addFeedButton.IsEnabled = false;
            XDocument doc = await LoadDocumentAsync(addFeedTextBox.Text);
            string title = doc.Descendants("title").First().Value;
            selectFeedComboBox.Items.Add(title);
            sites.Add(doc);
            addFeedTextBox.Clear();
            await Task.Delay(1000);
            addFeedButton.IsEnabled = true;
        }




        private async Task<XDocument> LoadDocumentAsync(string url)
        {
            // This is just to simulate a slow/large data transfer and make testing easier.
            // Remove it if you want to.
            await Task.Delay(1000);
            var response = await http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var feed = XDocument.Load(stream);
            return feed;
        }
    }
}
