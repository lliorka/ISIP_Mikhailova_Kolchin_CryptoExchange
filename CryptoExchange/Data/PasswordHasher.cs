using System;
using System.Security.Cryptography;
namespace CryptoExchange.Data {
 public static class PasswordHasher {
  private const int Iterations = 120000;
  public static string Hash(string password) {
   byte[] salt = new byte[16]; using (var rng=RandomNumberGenerator.Create()) rng.GetBytes(salt);
   using (var pbkdf = new Rfc2898DeriveBytes(password, salt, Iterations)) {
    byte[] key=pbkdf.GetBytes(32); return "PBKDF2$"+Iterations+"$"+Convert.ToBase64String(salt)+"$"+Convert.ToBase64String(key);
   }
  }
  public static bool Verify(string password, string stored) {
   try { var p=stored.Split('$'); if(p.Length!=4 || p[0]!="PBKDF2") return false;
    int rounds=int.Parse(p[1]); byte[] salt=Convert.FromBase64String(p[2]); byte[] expected=Convert.FromBase64String(p[3]);
    using(var pbkdf=new Rfc2898DeriveBytes(password,salt,rounds)) {
     byte[] actual=pbkdf.GetBytes(expected.Length); int diff=0; for(int i=0;i<actual.Length;i++) diff|=actual[i]^expected[i]; return diff==0;
    }
   } catch { return false; }
  }
  public static bool IsValid(string value, out string error) {
   error=null; if(string.IsNullOrWhiteSpace(value) || value.Length<8) { error="Пароль должен содержать не менее 8 символов."; return false; }
   bool latin=true,digit=false,upper=false;
   foreach(char c in value) { if(c>127) latin=false; if(char.IsDigit(c)) digit=true; if(char.IsUpper(c)) upper=true; }
   if(!latin) error="Используйте только латинские символы.";
   else if(!digit) error="Добавьте хотя бы одну цифру.";
   else if(!upper) error="Добавьте хотя бы одну заглавную букву.";
   return error==null;
  }
 }
}
