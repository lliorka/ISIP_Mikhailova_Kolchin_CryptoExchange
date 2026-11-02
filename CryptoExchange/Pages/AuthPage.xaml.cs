using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CryptoExchange.Data;
namespace CryptoExchange.Pages {
 public partial class AuthPage : Page {
  private int failed=0; private string captcha="";
  private readonly Random random=new Random();
  public AuthPage(){InitializeComponent();}
  private void Page_Loaded(object s,RoutedEventArgs e){(Application.Current.MainWindow as MainWindow).SetTitle("Авторизация");}
  private void Login_Click(object s,RoutedEventArgs e) {
   if(string.IsNullOrWhiteSpace(LoginBox.Text)||string.IsNullOrWhiteSpace(PasswordBox.Password)){MessageBox.Show("Заполните логин и пароль.");return;}
   if(failed>=3 && !string.Equals(CaptchaBox.Text,captcha,StringComparison.OrdinalIgnoreCase)){MessageBox.Show("Введите правильную капчу.");NewCaptcha();return;}
   try {
    var dt=Db.Query("SELECT UserID,Login,PasswordHash,Role,FullName,Email FROM dbo.Users WHERE Login=@l AND IsActive=1",Db.P("@l",LoginBox.Text.Trim()));
    if(dt.Rows.Count==0 || !PasswordHasher.Verify(PasswordBox.Password,dt.Rows[0]["PasswordHash"].ToString())) {
     failed++; MessageBox.Show("Неверный логин или пароль."); if(failed>=3){CaptchaText.Visibility=Visibility.Visible;CaptchaBox.Visibility=Visibility.Visible;NewCaptcha();} return;
    }
    var r=dt.Rows[0]; MainWindow.CurrentUser=new UserAccount{UserID=(int)r["UserID"],Login=r["Login"].ToString(),Role=r["Role"].ToString(),FullName=r["FullName"].ToString(),Email=r["Email"].ToString()};
    Db.Execute("UPDATE dbo.Users SET LastLoginAt=SYSDATETIME() WHERE UserID=@u",Db.P("@u",MainWindow.CurrentUser.UserID));
    failed=0; var w=(MainWindow)Application.Current.MainWindow;
    if(MainWindow.CurrentUser.Role=="Admin") w.Navigate(new AdminPage(),"Администратор — управление данными");
    else w.Navigate(new UserPage(),"Личный кабинет пользователя");
   } catch(Exception ex){MessageBox.Show("Не удалось подключиться к БД. Проверьте строку подключения и выполните db/schema.sql.\n"+ex.Message,"Ошибка БД");}
  }
  private void NewCaptcha(){const string chars="ABCDEFGHJKLMNPQRSTUVWXYZ23456789";captcha=new string(Enumerable.Range(0,6).Select(i=>chars[random.Next(chars.Length)]).ToArray());CaptchaText.Text="Код: "+captcha;CaptchaBox.Clear();}
  private void Register_Click(object s,RoutedEventArgs e){(Application.Current.MainWindow as MainWindow).Navigate(new RegisterPage(),"Регистрация");}
  private void Change_Click(object s,RoutedEventArgs e){(Application.Current.MainWindow as MainWindow).Navigate(new ChangePasswordPage(),"Смена пароля");}
 }
}
