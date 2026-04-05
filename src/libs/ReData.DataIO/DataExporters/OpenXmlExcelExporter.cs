using System.Data.Common;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ReData.DataIO.DataExporters;

// public sealed class OpenXmlExcelExporter : IDataExporter
// {
//     public async Task ExportAsync(DbDataReader reader, Stream outputStream, CancellationToken ct)
//     {
//         // Открываем Excel файл для записи
//         using (var spreadsheetDocument = SpreadsheetDocument.Create(outputStream, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
//         {
//             // Создаем книгу (Workbook) и лист (Worksheet)
//             var workbookPart = spreadsheetDocument.AddWorkbookPart();
//             workbookPart.Workbook = new Workbook();
//             
//             var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
//             worksheetPart.Worksheet = new Worksheet(new SheetData());
//             
//             // Добавляем лист в книгу
//             var sheets = workbookPart.Workbook.AppendChild(new Sheets());
//             sheets.AppendChild(new Sheet()
//             {
//                 Id = workbookPart.GetIdOfPart(worksheetPart),
//                 SheetId = 1,
//                 Name = "sheetName"
//             });
//
//             // Получаем доступ к SheetData для записи
//             var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
//
//             // Получаем столбцы из DbDataReader
//             var columnCount = reader.FieldCount;
//
//             // Записываем заголовки
//             var headerRow = new Row();
//             for (int i = 0; i < columnCount; i++)
//             {
//                 var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(reader.GetName(i)) };
//                 headerRow.AppendChild(cell);
//             }
//             sheetData.AppendChild(headerRow);
//
//             // Записываем строки данных
//             while (await reader.ReadAsync(ct))
//             {
//                 // Выход, если токен отмены активирован
//                 if (ct.IsCancellationRequested)
//                 {
//                     break;
//                 }
//
//                 var dataRow = new Row();
//                 for (int i = 0; i < columnCount; i++)
//                 {
//                     var cell = new Cell() { DataType = CellValues.String, CellValue = new CellValue(reader[i]?.ToString() ?? string.Empty) };
//                     dataRow.AppendChild(cell);
//                 }
//                 sheetData.AppendChild(dataRow);
//             }
//
//             // Закрываем все части документа
//             workbookPart.Workbook.Save();
//         }
//     }
// }