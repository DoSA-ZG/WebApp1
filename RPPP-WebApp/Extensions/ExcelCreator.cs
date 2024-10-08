﻿using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using RPPP_WebApp.Util;

namespace RPPP_WebApp.Extensions {
  /// <summary>
  /// Extension class for creating Excel files from IEnumerable data.
  /// </summary>
  public static class ExcelCreator {
    /// <summary>
    /// Creates an ExcelPackage containing data from an IEnumerable collection.
    /// </summary>
    /// <typeparam name="T">Type of elements in the collection.</typeparam>
    /// <param name="data">IEnumerable collection containing the data.</param>
    /// <param name="worksheetName">Name of the worksheet to be created.</param>
    /// <returns>An ExcelPackage containing the data.</returns>
    public static ExcelPackage CreateExcel<T>(this IEnumerable<T> data, string worksheetName) {
      ExcelPackage excel = new ExcelPackage();
      var worksheet = excel.Workbook.Worksheets.Add(worksheetName);
      int row = 1;
      int col = 1;

      PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
      foreach (var prop in props) {
        if (prop.PropertyType is IEnumerable)
          continue;
        string name = prop.Name;
        if (prop.IsDefined(typeof(DisplayAttribute))) {
          name = prop.GetCustomAttribute<DisplayAttribute>().Name;
        }
        worksheet.Cells[row, col++].Value = name;
      }


      foreach (T t in data) {
        ++row;
        col = 1;
        foreach (var prop in props) {
          if (prop.PropertyType is IEnumerable)
            continue;

          object value = prop.GetValue(t);
          worksheet.Cells[row, col].Value = value;
          if (prop.IsDefined(typeof(ExcelFormatAttribute))) {
            string format = prop.GetCustomAttribute<ExcelFormatAttribute>().ExcelFormat;
            if (!string.IsNullOrWhiteSpace(format)) {
              worksheet.Cells[row, col].Style.Numberformat.Format = format;
            }
          }
          ++col;
        }
      }

      worksheet.Cells[1, 1, row - 1, col - 1].AutoFitColumns();
      return excel;
    }
  }
}
