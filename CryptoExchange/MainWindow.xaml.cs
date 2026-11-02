using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Resources;
using System.IO;
using System.Linq;
using CryptoExchange.Pages;
using System.Windows.Controls;
namespace CryptoExchange {
 public partial class MainWindow : Window {
  public static Data.UserAccount CurrentUser { get; set; }
  private DispatcherTimer timer;
  public MainWindow() { InitializeComponent(); MainFrame.Navigate(new AuthPage()); }
  private void Window_Loaded(object sender, RoutedEventArgs e) {
   timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
   timer.Tick += (s,a) => DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
   timer.Start();
   var theme = Properties.Settings.Default.ThemeName;
   ApplyTheme(theme == "Light" ? "DictionaryLight.xaml" : "Dictionary.xaml");
  }
  public void SetTitle(string title) { PageTitle.Text = title; }
  public void Navigate(Page page, string title) { SetTitle(title); MainFrame.Navigate(page); }
  private void Back_Click(object sender, RoutedEventArgs e) { if (MainFrame.CanGoBack) MainFrame.GoBack(); else MainFrame.Navigate(new AuthPage()); }
  private void Logout_Click(object sender, RoutedEventArgs e) {
   if (CurrentUser == null) return;
   if (MessageBox.Show("Завершить текущий сеанс?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
   CurrentUser = null; MainFrame.Navigate(new AuthPage()); SetTitle("Авторизация");
  }
  private void LightTheme_Click(object sender, RoutedEventArgs e) { ApplyTheme("DictionaryLight.xaml"); Properties.Settings.Default.ThemeName="Light"; Properties.Settings.Default.Save(); }
  private void DarkTheme_Click(object sender, RoutedEventArgs e) { ApplyTheme("Dictionary.xaml"); Properties.Settings.Default.ThemeName="Dark"; Properties.Settings.Default.Save(); }
  private void ApplyTheme(string file) {
   try { var d = new ResourceDictionary { Source = new Uri(file, UriKind.Relative) }; Application.Current.Resources.MergedDictionaries.Clear(); Application.Current.Resources.MergedDictionaries.Add(d); }
   catch { }
  }
  private void Window_Closing(object sender, CancelEventArgs e) {
   if (MessageBox.Show("Вы действительно хотите выйти из приложения?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) e.Cancel=true;
  }
 }
}
