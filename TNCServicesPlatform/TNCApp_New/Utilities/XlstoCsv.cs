using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

namespace TNCApp_New.Utilities
{
    public class XlstoCsv
    {
        public static void convertExcelToCSV(string sourceFile, string targetFile)
        {
            Application excel = new Application();
            excel.DisplayAlerts = false;

            Workbook wb = excel.Workbooks.Open(sourceFile);

            wb.SaveAs(targetFile, XlFileFormat.xlCSV);
            wb.Close();
        }
    }
}
