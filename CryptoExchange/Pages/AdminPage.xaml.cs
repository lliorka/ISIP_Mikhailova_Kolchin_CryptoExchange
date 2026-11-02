using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using CryptoExchange.Data;
namespace CryptoExchange.Pages {
 public partial class AdminPage : Page {
  private DataTable table; private string tableName="Users"; private SqlDataAdapter adapter;
  public AdminPage(){InitializeComponent();}
  private void Page_Loaded(object s,RoutedEventArgs e){(Application.Current.MainWindow as MainWindow).SetTitle("Администратор — управление данными");LoadTable();}
  private string SelectedTable(){var item=TableBox.SelectedItem as ComboBoxItem; string key=item==null?"Пользователи":item.Content.ToString();return Db.AdminTables[key];}
  private void Table_Changed(object s,SelectionChangedEventArgs e){if(IsLoaded)LoadTable();}
  private void LoadTable(){
   try{tableName=SelectedTable(); using(var c=Db.Open()){adapter=new SqlDataAdapter("SELECT * FROM dbo.["+tableName+"]",c);var builder=new SqlCommandBuilder(adapter);table=new DataTable();adapter.Fill(table);AdminGrid.ItemsSource=table.DefaultView;}}
   catch(Exception ex){MessageBox.Show("Не удалось загрузить таблицу: "+ex.Message);}
  }
  private void Search_Changed(object s,TextChangedEventArgs e){
   if(table==null||!IsLoaded)return; string q=SearchBox.Text.Replace("'","''");
   try{string col=table.Columns.Cast<DataColumn>().FirstOrDefault(c=>c.DataType==typeof(string))?.ColumnName;
    table.DefaultView.RowFilter=string.IsNullOrWhiteSpace(q)||col==null?"":"Convert(["+col+"], 'System.String') LIKE '%"+q+"%'";}catch{table.DefaultView.RowFilter="";}
  }
  private void Refresh_Click(object s,RoutedEventArgs e){LoadTable();}
  private void Save_Click(object s,RoutedEventArgs e){
   try{AdminGrid.CommitEdit(DataGridEditingUnit.Cell,true);AdminGrid.CommitEdit(DataGridEditingUnit.Row,true);adapter.Update(table);MessageBox.Show("Изменения сохранены.");LoadTable();}
   catch(Exception ex){MessageBox.Show("Не удалось сохранить данные. Проверьте обязательные поля и внешние ключи.\n"+ex.Message);}
  }
  private void Delete_Click(object s,RoutedEventArgs e){
   if(AdminGrid.SelectedItems.Count==0){MessageBox.Show("Выберите строки для удаления.");return;}
   if(MessageBox.Show("Удалить выбранные записи? Связанные записи могут удалиться каскадно.","Подтверждение",MessageBoxButton.YesNo,MessageBoxImage.Warning)!=MessageBoxResult.Yes)return;
   try{foreach(var item in AdminGrid.SelectedItems.Cast<DataRowView>().ToList())item.Row.Delete();adapter.Update(table);MessageBox.Show("Записи удалены.");LoadTable();}
   catch(Exception ex){MessageBox.Show("Удаление не выполнено: "+ex.Message);}
  }
  private void Charts_Click(object s,RoutedEventArgs e){(Application.Current.MainWindow as MainWindow).Navigate(new ChartPage(),"Диаграммы и аналитика");}
 }
}
