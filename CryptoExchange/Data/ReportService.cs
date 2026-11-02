using System;
using System.Data;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ClosedXML.Excel;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
namespace CryptoExchange.Data {
 public static class ReportService {
  public static DataTable GetUserReport(int userId) {
   return Db.Query(@"SELECT o.OrderID, a.Symbol AS FromAsset, b.Symbol AS ToAsset, o.FromAmount, o.ToAmount,
    o.Rate, o.Status, o.CreatedAt FROM ExchangeOrders o JOIN Assets a ON a.AssetID=o.FromAssetID
    JOIN Assets b ON b.AssetID=o.ToAssetID WHERE o.UserID=@u ORDER BY o.CreatedAt DESC", Db.P("@u",userId));
  }
  public static void ExportExcel(DataTable data,string path) {
   using(var wb=new XLWorkbook()) { var ws=wb.Worksheets.Add("Отчёт"); ws.Cell(1,1).InsertTable(data,"ExchangeReport",true); ws.Columns().AdjustToContents(); wb.SaveAs(path); }
  }
  public static void ExportWord(DataTable data,string path) {
   using(var doc=WordprocessingDocument.Create(path,WordprocessingDocumentType.Document)) {
    var main=doc.AddMainDocumentPart(); main.Document=new Document(new Body()); Body body=main.Document.Body;
    body.Append(new Paragraph(new Run(new Text("Отчёт по операциям CryptoExchange"))) { ParagraphProperties=new ParagraphProperties(new Justification{Val=JustificationValues.Center}) });
    body.Append(new Paragraph(new Run(new Text("Дата формирования: "+DateTime.Now.ToString("dd.MM.yyyy HH:mm")))));
    var table=new Table(); var props=new TableProperties(new TableBorders(
      new TopBorder{Val=BorderValues.Single,Size=6},new BottomBorder{Val=BorderValues.Single,Size=6},
      new LeftBorder{Val=BorderValues.Single,Size=6},new RightBorder{Val=BorderValues.Single,Size=6},
      new InsideHorizontalBorder{Val=BorderValues.Single,Size=4},new InsideVerticalBorder{Val=BorderValues.Single,Size=4}));
    table.AppendChild(props);
    var header=new TableRow(); foreach(DataColumn c in data.Columns) header.Append(new TableCell(new Paragraph(new Run(new Text(c.ColumnName))))); table.Append(header);
    foreach(DataRow row in data.Rows) { var tr=new TableRow(); foreach(var v in row.ItemArray) tr.Append(new TableCell(new Paragraph(new Run(new Text(Convert.ToString(v)))))); table.Append(tr); }
    body.Append(table); main.Document.Save();
   }
  }
  public static void ExportPdf(DataTable data,string path) {
   var doc=new PdfDocument(); var page=doc.AddPage(); var gfx=XGraphics.FromPdfPage(page);
   var title=new XFont("Arial",16,XFontStyle.Bold); var font=new XFont("Arial",8,XFontStyle.Regular);
   gfx.DrawString("CryptoExchange — отчёт по операциям",title,XBrushes.Black,new XRect(30,25,page.Width-60,25),XStringFormats.TopLeft);
   double y=65; string header=string.Join(" | ",System.Linq.Enumerable.Select(data.Columns.Cast<DataColumn>(),c=>c.ColumnName));
   gfx.DrawString(header,font,XBrushes.Black,new XRect(30,y,page.Width-60,30),XStringFormats.TopLeft); y+=22;
   foreach(DataRow row in data.Rows) { if(y>page.Height-40){page=doc.AddPage();gfx=XGraphics.FromPdfPage(page);y=35;}
    string line=string.Join(" | ",row.ItemArray); gfx.DrawString(line,font,XBrushes.Black,new XRect(30,y,page.Width-60,30),XStringFormats.TopLeft); y+=18;
   }
   doc.Save(path);
  }
 }
}
