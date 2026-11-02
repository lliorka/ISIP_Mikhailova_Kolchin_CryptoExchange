using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using CryptoExchange.Data;
namespace CryptoExchange.Pages {
 public partial class UserPage : Page {
  private DataTable report;
  public UserPage(){InitializeComponent();}
  private void Page_Loaded(object s,RoutedEventArgs e){WelcomeText.Text="Здравствуйте, "+MainWindow.CurrentUser.FullName+"!";LoadData();}
  private void LoadData(){report=ReportService.GetUserReport(MainWindow.CurrentUser.UserID);OrdersGrid.ItemsSource=report.DefaultView;}
  private void Refresh_Click(object s,RoutedEventArgs e){LoadData();}
  private void Export(string ext,Action<DataTable,string> action){var d=new SaveFileDialog{Filter=ext.ToUpper()+" файл|*."+ext,FileName="CryptoExchange_Report_"+DateTime.Now.ToString("yyyyMMdd")+"."+ext};if(d.ShowDialog()==true){try{action(report,d.FileName);MessageBox.Show("Отчёт сохранён: "+d.FileName);}catch(Exception ex){MessageBox.Show("Ошибка экспорта: "+ex.Message);}}}
  private void Word_Click(object s,RoutedEventArgs e){Export("docx",ReportService.ExportWord);}
  private void Excel_Click(object s,RoutedEventArgs e){Export("xlsx",ReportService.ExportExcel);}
  private void Pdf_Click(object s,RoutedEventArgs e){Export("pdf",ReportService.ExportPdf);}
 }
}
