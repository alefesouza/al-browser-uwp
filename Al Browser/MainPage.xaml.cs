using Al_Browser.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Al_Browser
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static SQLiteConnection objConn = new SQLiteConnection("AlBrowser.db");
        ObservableCollection<string> Items = new ObservableCollection<string>();
        ResourceLoader loader = new ResourceLoader();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void CommandBar_Opening(object sender, object e)
        {
            CBBack.Label = loader.GetString("Back");
            CBForward.Label = loader.GetString("Forward");
            CBRefresh.Label = loader.GetString("Refresh");
        }

        private void CommandBar_Closing(object sender, object e)
        {
            CBBack.Label = "";
            CBForward.Label = "";
            CBRefresh.Label = "";
        }

        private void AddressBar_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.QueryText.Split(' ').Length > 1)
            {
                WView.Source = new Uri("https://www.google.com/search?q=" + args.QueryText);
            }
            else
            {
                if (!args.QueryText.StartsWith("http"))
                {
                    AddressBar.Text = "http://" + args.QueryText;
                }
                WView.Source = new Uri(AddressBar.Text);
            }
        }

        private void AddressBar_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            Items.Clear();

            string favoritesSQL = "SELECT * FROM favorites ORDER BY id DESC";
            var favorites = objConn.Prepare(favoritesSQL);

            string historySQL = "SELECT * FROM history ORDER BY times";
            var history = objConn.Prepare(historySQL);

            while (favorites.Step() == SQLiteResult.ROW)
            {
                Items.Add(favorites[2].ToString());
            }

            while (history.Step() == SQLiteResult.ROW)
            {
                Items.Add(history[2].ToString());
            }

            var filtered = Items.Where(p => p.Contains(sender.Text)).ToArray();
            sender.ItemsSource = filtered;
        }

        private void WView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            UpdateCB();
            AddressBar.Text = WView.Source.ToString();
            Favicon.Visibility = Visibility.Collapsed;
            Loading.Visibility = Visibility.Visible;
        }

        private void WView_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            UpdateCB();
            AddressBar.Text = WView.Source.ToString();
        }

        private void WView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            UpdateCB();

            string historySQL = "SELECT * FROM history";
            var history = objConn.Prepare(historySQL);

            Favicon.Source = new BitmapImage(new Uri("http://google.com/s2/favicons?domain=" + WView.Source.ToString()));
            Favicon.Visibility = Visibility.Visible;
            Loading.Visibility = Visibility.Collapsed;

            while (history.Step() == SQLiteResult.ROW)
            {
                if(history[2].ToString().Equals(WView.Source.ToString()))
                {
                    objConn.Prepare("UPDATE history SET times=" + (int.Parse(history[3].ToString()) + 1) + " WHERE url='" + WView.Source.ToString() + "';").Step();
                    return;
                }
            }

            objConn.Prepare("INSERT INTO history (name, url, times) VALUES ('" + WView.DocumentTitle + "', '" + WView.Source.ToString() + "', 1);").Step();
        }

        private void CBBack_Click(object sender, RoutedEventArgs e)
        {
            if (WView.CanGoBack)
            {
                WView.GoBack();
            }
        }

        private void CBForward_Click(object sender, RoutedEventArgs e)
        {
            if (WView.CanGoForward)
            {
                WView.GoForward();
            }
        }

        private void CBRefresh_Click(object sender, RoutedEventArgs e)
        {
            WView.Refresh();
        }

        private void CBFavorite_Click(object sender, RoutedEventArgs e)
        {
            AddFName.Text = WView.DocumentTitle;
            AddFURL.Text = WView.Source.ToString();
        }

        private void AddFavorite_Click(object sender, RoutedEventArgs e)
        {
            objConn.Prepare("INSERT INTO favorites (name, url) VALUES ('" + AddFName.Text + "', '" + AddFURL.Text + "');").Step();
            ChangeFavButton(true);
        }

        private void CBUnFavorite_Click(object sender, RoutedEventArgs e)
        {
            objConn.Prepare("DELETE FROM favorites WHERE url='" + WView.Source.ToString() + "';").Step();
            ChangeFavButton(false);
        }

        private void OpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            openBrowser(WView.Source);
        }

        public async void openBrowser(Uri url)
        {
            await Launcher.LaunchUriAsync(url);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage(loader.GetString("AppDeveloped") + " Alefe Souza");
        }

        public void ChangeFavButton(bool isFav)
        {
            if (isFav)
            {
                CBFavorite.Visibility = Visibility.Collapsed;
                CBUnFavorite.Visibility = Visibility.Visible;
            }
            else
            {
                CBFavorite.Visibility = Visibility.Visible;
                CBUnFavorite.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateCB()
        {
            string QuestionPhrase = @"SELECT * FROM favorites";
            var favorites = objConn.Prepare(QuestionPhrase);

            bool isFav = false;

            while (favorites.Step() == SQLiteResult.ROW)
            {
                if (WView.Source.ToString().Equals(favorites[2]))
                {
                    isFav = true;
                    break;
                }
            }

            ChangeFavButton(isFav);

            if (WView.DocumentTitle.Equals(""))
            {
                Title.Text = WView.Source.ToString().Split('/')[2];
            }
            else
            {
                Title.Text = WView.DocumentTitle;
            }

            CBBack.IsEnabled = WView.CanGoBack;
            CBForward.IsEnabled = WView.CanGoForward;

            CBBack2.IsEnabled = WView.CanGoBack;
            CBForward2.IsEnabled = WView.CanGoForward;
        }

        public async void ShowMessage(string message)
        {
            MessageDialog md = new MessageDialog(message);
            md.Commands.Add(new UICommand("GitHub", new UICommandInvokedHandler(CommandHandlers)) { Id = 0 });
            md.Commands.Add(new UICommand(loader.GetString("Close")) { Id = 1 });

            md.DefaultCommandIndex = 0;
            md.CancelCommandIndex = 1;
            await md.ShowAsync();
        }

        public void CommandHandlers(IUICommand commandLabel)
        {
            var Actions = commandLabel.Label;
            switch (Actions)
            {
                case "GitHub":
                    WView.Source = new Uri("http://github.com/alefesouza");
                    break;
            }
        }

        private void FavoritesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (FavoritesList.SelectedIndex != -1)
            {
                Favorite f = e.AddedItems[0] as Favorite;

                WView.Source = new Uri(f.Url);

                MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;

                FavoritesList.SelectedIndex = -1;
            }
        }

        private void FavoriteListCB_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
            UpdateLeft("favorites", loader.GetString("Favorites"));
        }

        private void History_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
            UpdateLeft("history", loader.GetString("History"));
        }

        public void UpdateLeft(string what, string title)
        {
            Clear.Visibility = what.Equals("favorites") ? Visibility.Collapsed : Visibility.Visible;

            LeftTitle.Text = title;

            string favoritesSQL = "SELECT * FROM " + what + " ORDER BY id DESC";
            var favorites = objConn.Prepare(favoritesSQL);

            ObservableCollection<Favorite> Items = new ObservableCollection<Favorite>();

            while (favorites.Step() == SQLiteResult.ROW)
            {
                Items.Add(new Favorite(favorites[1].ToString(), favorites[2].ToString()));
            }

            Binding myBinding = new Binding();
            myBinding.Source = Items;
            FavoritesList.SetBinding(ItemsControl.ItemsSourceProperty, myBinding);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            string historySQL = "DELETE FROM history";
            objConn.Prepare(historySQL).Step();
            UpdateLeft("history", loader.GetString("History"));
        }
    }
}