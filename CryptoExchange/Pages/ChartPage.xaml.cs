using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CryptoExchange.Data;
namespace CryptoExchange.Pages {
 public partial class ChartPage : Page {
  public ChartPage(){InitializeComponent();}
  private void Page_Loaded(object s,RoutedEventArgs e){Draw();}
  private void Refresh_Click(object s,RoutedEventArgs e){Draw();}
  private void Back_Click(object s,RoutedEventArgs e){(Application.Current.MainWindow as MainWindow).Navigate(new AdminPage(),"Администратор — управление данными");}
  private void Draw(){
   try{
    ChartCanvas.Children.Clear(); var dt=Db.Query("SELECT Status,COUNT(*) AS Total FROM dbo.ExchangeOrders GROUP BY Status ORDER BY Status");
    double max=dt.Rows.Count==0?1:Math.Max(1,dt.AsEnumerable().Max(r=>Convert.ToDouble(r["Total"])));
    double h=Math.Max(250,ChartCanvas.ActualHeight); double w=Math.Max(500,ChartCanvas.ActualWidth); double barW=Math.Max(35,(w-100)/Math.Max(1,dt.Rows.Count*2));
    for(int i=0;i<dt.Rows.Count;i++){
     string label=dt.Rows[i]["Status"].ToString(); double val=Convert.ToDouble(dt.Rows[i]["Total"]); double bh=(h-100)*val/max;
     var rect=new Rectangle{Width=barW,Height=bh,Fill=new SolidColorBrush(Color.FromRgb(40,199,165))};
     Canvas.SetLeft(rect,50+i*barW*2);Canvas.SetTop(rect,h-bh-55);ChartCanvas.Children.Add(rect);
     var tx=new TextBlock{Text=val.ToString(),Foreground=Brushes.White,FontWeight=FontWeights.Bold};Canvas.SetLeft(tx,50+i*barW*2);Canvas.SetTop(tx,h-bh-78);ChartCanvas.Children.Add(tx);
     var lab=new TextBlock{Text=label,Foreground=Brushes.White,Width=barW+35,TextWrapping=TextWrapping.Wrap};Canvas.SetLeft(lab,45+i*barW*2);Canvas.SetTop(lab,h-48);ChartCanvas.Children.Add(lab);
    }
   }catch(Exception ex){MessageBox.Show("Не удалось построить диаграмму: "+ex.Message);}
  }
 }
}
