using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
namespace CryptoExchange.Data {
 public class UserAccount { public int UserID {get;set;} public string Login {get;set;} public string Role {get;set;} public string FullName {get;set;} public string Email {get;set;} }
 public static class Db {
  public static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["CryptoDb"].ConnectionString; } }
  public static SqlConnection Open() { var c=new SqlConnection(ConnectionString); c.Open(); return c; }
  public static DataTable Query(string sql, params SqlParameter[] args) {
   using(var c=Open()) using(var cmd=new SqlCommand(sql,c)) { if(args!=null) cmd.Parameters.AddRange(args); using(var da=new SqlDataAdapter(cmd)) { var t=new DataTable(); da.Fill(t); return t; } }
  }
  public static int Execute(string sql, params SqlParameter[] args) {
   using(var c=Open()) using(var cmd=new SqlCommand(sql,c)) { if(args!=null) cmd.Parameters.AddRange(args); return cmd.ExecuteNonQuery(); }
  }
  public static object Scalar(string sql, params SqlParameter[] args) {
   using(var c=Open()) using(var cmd=new SqlCommand(sql,c)) { if(args!=null) cmd.Parameters.AddRange(args); return cmd.ExecuteScalar(); }
  }
  public static SqlParameter P(string name, object value) { return new SqlParameter(name,value??DBNull.Value); }
  public static UserAccount Authenticate(string login,string hash) {
   var t=Query("SELECT UserID,Login,PasswordHash,Role,FullName,Email FROM dbo.Users WHERE Login=@l AND IsActive=1",P("@l",login));
   if(t.Rows.Count==0 || !PasswordHasher.Verify(hash,t.Rows[0]["PasswordHash"].ToString())) return null;
   var r=t.Rows[0]; return new UserAccount{UserID=(int)r["UserID"],Login=r["Login"].ToString(),Role=r["Role"].ToString(),FullName=r["FullName"].ToString(),Email=r["Email"].ToString()};
  }
  public static bool Register(string login,string passwordHash,string role,string fio,string email) {
   return Execute("INSERT dbo.Users(Login,PasswordHash,Role,FullName,Email) VALUES(@l,@p,@r,@f,@e)",
    P("@l",login),P("@p",passwordHash),P("@r",role),P("@f",fio),P("@e",email))>0;
  }
  public static readonly Dictionary<string,string> AdminTables = new Dictionary<string,string> {
   {"Пользователи","Users"},{"Активы","Assets"},{"Курсы обмена","ExchangeRates"},{"Кошельки","Wallets"},
   {"Заявки обмена","ExchangeOrders"},{"Операции","Transactions"},{"Движения кошельков","WalletEntries"},{"Журнал аудита","AuditLog"}
  };
 }
}
