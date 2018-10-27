using System;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using TNCApp_New.Utilities;
namespace TNCApp_NewTest
{
    [TestClass]
    public class WpfUnitTest
    {
        [TestMethod]
        public void XlsToCsvTest()
        {
            OpenFileDialog confirmdlg = new OpenFileDialog();
            confirmdlg.DefaultExt = ".csv";
            confirmdlg.ShowDialog();
            SaveFileDialog savedlg = new SaveFileDialog();
            savedlg.ShowDialog();
            Application excel = new Application();
            Workbook wb = excel.Workbooks.Open(confirmdlg.FileName);
            wb.SaveAs(savedlg.FileName, XlFileFormat.xlCSV);
        }
    }
}
