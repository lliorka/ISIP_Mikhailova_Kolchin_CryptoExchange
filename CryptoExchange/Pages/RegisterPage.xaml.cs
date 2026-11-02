using System;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using CryptoExchange.Data;
namespace CryptoExchange.Pages {
 public partial class RegisterPage : Page {
  public RegisterPage(){InitializeComponent();}
  private void Save_Click(object s,RoutedEventArgs e){
   string login=LoginBox.Text.Trim(), email=EmailBox.Text.Trim(), fio=FioBox.Text.Trim(), pass=PassBox.Password;
   if(string.IsNullOrWhiteSpace(login)||string.IsNullOrWhiteSpace(email)||string.IsNullOrWhiteSpace(fio)||string.IsNullOrWhiteSpace(pass)||string.IsNullOrWhiteSpace(ConfirmBox.Password)){MessageBox.Show("Заполните все поля.");return;}
   try{var addr=new MailAddress(email);if(addr.Address!=email)throw new Exception();}catch{MessageBox.Show("Введите корректный адрес электронной почты.");return;}
   if(pass!=ConfirmBox.Password){MessageBox.Show("Пароли не совпадают.");return;}
   string err;if(!PasswordHasher.IsValid(pass,out err)){MessageBox.Show(err);return;}
   try{
    if(Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM dbo.Users WHERE Login=@l",Db.P("@l",login)))>0){MessageBox.Show("Такой логин уже зарегистрирован.");return;}
    string role=(RoleBox.SelectedItem as ComboBoxItem).Content.ToString();
    Db.Register(login,PasswordHasher.Hash(pass),role,fio,email);
    MessageBox.Show("Учётная запись создана. Теперь выполните вход.");
    (Application.Current.MainWindow as MainWindow).Navigate(new AuthPage(),"Авторизация");
   }catch(Exception ex){MessageBox.Show("Ошибка регистрации: "+ex.Message);}
  }
  private void Back_Click(object s,RoutedEventArgs e){(Application.Current.MainWindow as MainWindow).Navigate(new AuthPage(),"Авторизация");}
 }
}
