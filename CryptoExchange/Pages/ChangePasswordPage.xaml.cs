using System;
using System.Windows;
using System.Windows.Controls;
using CryptoExchange.Data;
namespace CryptoExchange.Pages {
 public partial class ChangePasswordPage : Page {
  public ChangePasswordPage(){InitializeComponent();}
  private void Save_Click(object s,RoutedEventArgs e){
   if(string.IsNullOrWhiteSpace(LoginBox.Text)||string.IsNullOrEmpty(OldBox.Password)||string.IsNullOrEmpty(NewBox.Password)||string.IsNullOrEmpty(ConfirmBox.Password)){MessageBox.Show("Все поля обязательны к заполнению!");return;}
   if(NewBox.Password!=ConfirmBox.Password){MessageBox.Show("Новые пароли не совпадают.");return;}
   string err;if(!PasswordHasher.IsValid(NewBox.Password,out err)){MessageBox.Show(err);return;}
   try{
    var dt=Db.Query("SELECT UserID,PasswordHash FROM dbo.Users WHERE Login=@l AND IsActive=1",Db.P("@l",LoginBox.Text.Trim()));
    if(dt.Rows.Count==0||!PasswordHasher.Verify(OldBox.Password,dt.Rows[0]["PasswordHash"].ToString())){MessageBox.Show("Текущий логин или пароль неверный.");return;}
    Db.Execute("UPDATE dbo.Users SET PasswordHash=@p,UpdatedAt=SYSDATETIME() WHERE UserID=@u",Db.P("@p",PasswordHasher.Hash(NewBox.Password)),Db.P("@u",dt.Rows[0]["UserID"]));
    MessageBox.Show("Пароль успешно изменён."); (Application.Current.MainWindow as MainWindow).Navigate(new AuthPage(),"Авторизация");
   }catch(Exception ex){MessageBox.Show(ex.Message);}
  }
  private void Back_Click(object s,RoutedEventArgs e){(Application.Current.MainWindow as MainWindow).Navigate(new AuthPage(),"Авторизация");}
 }
}
