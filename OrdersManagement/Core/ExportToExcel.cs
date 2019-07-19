using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OrdersManagement.Core
{
    class ExportToExcel
    {
        public static void ExportDsToExcelSheet(DataSet ds, string fileName)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage pck = new ExcelPackage())
            {
                foreach (DataTable dt in ds.Tables)
                {
                    ExcelWorksheet EWS = pck.Workbook.Worksheets.Add(dt.TableName);

                    EWS.Cells["A1"].LoadFromDataTable(dt, true);
                    EWS.Cells.AutoFitColumns();
                    EWS.View.FreezePanes(2, 1);

                }
                pck.SaveAs(ms);
            }
            ms.WriteTo(HttpContext.Current.Response.OutputStream);
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".xlsx");
            HttpContext.Current.Response.ContentType = "application/ms-excel";
            HttpContext.Current.Response.BinaryWrite(ms.ToArray());
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.SuppressContent = true;
        }
    }
}
