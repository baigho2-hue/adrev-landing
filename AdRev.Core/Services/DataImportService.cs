using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using AdRev.Domain.Variables;
using AdRev.Domain.Enums;

namespace AdRev.Core.Services
{
    public class DataImportService
    {
        public List<Dictionary<string, object>> ImportFromCsv(string filePath)
        {
            var data = new List<Dictionary<string, object>>();
            if (!File.Exists(filePath)) return data;

            var lines = File.ReadAllLines(filePath);
                if (lines.Length < 2) return data;

                // Detect delimiter (, or ;)
                char delimiter = lines[0].Contains(';') ? ';' : ',';
                var headers = lines[0].Split(delimiter).Select(h => h.Trim('\"', ' ')).ToArray();

                // Quality Check: Empty Headers
                for (int i = 0; i < headers.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(headers[i]))
                    {
                        throw new InvalidDataException($"La colonne {i + 1} n'a pas d'en-tête (Ligne 1 vide).");
                    }
                }

                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    var values = ParseCsvLine(lines[i], delimiter);
                    var row = new Dictionary<string, object>();
                    for (int j = 0; j < headers.Length; j++)
                    {
                        string header = headers[j];
                        if (j < values.Count)
                            row[header] = values[j];
                        else
                            row[header] = string.Empty;
                    }
                    data.Add(row);
                }


            return data;
        }

        private List<string> ParseCsvLine(string line, char delimiter)
        {
            var result = new List<string>();
            bool inQuotes = false;
            string currentField = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(currentField.Trim('\"', ' '));
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }
            result.Add(currentField.Trim('\"', ' '));
            return result;
        }

        public List<Dictionary<string, object>> ImportFromExcel(string filePath)
        {
            var data = new List<Dictionary<string, object>>();
            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(filePath, false))
                {
                    WorkbookPart? workbookPart = doc.WorkbookPart;
                    if (workbookPart == null) return data;

                    if (workbookPart?.Workbook?.Sheets == null) return data;

                    Sheet? sheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();
                    if (sheet == null || sheet.Id == null) return data;

                    WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
                    if (worksheetPart?.Worksheet == null) return data;
                    
                    SheetData? sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    if (sheetData == null) return data;

                    SharedStringTablePart? sharedStringPart = workbookPart.SharedStringTablePart;

                    var rows = sheetData.Elements<Row>().ToList();
                    if (rows.Count < 2) return data;

                    var headerRow = rows[0];
                    var headers = new List<string>();
                    int colIndex = 0;
                    foreach (Cell cell in headerRow.Elements<Cell>())
                    {
                        string val = GetCellValue(cell, sharedStringPart);
                        if (string.IsNullOrWhiteSpace(val))
                        {
                            throw new InvalidDataException($"La colonne {colIndex + 1} (Excel) n'a pas d'en-tête.");
                        }
                        headers.Add(val);
                        colIndex++;
                    }

                    for (int i = 1; i < rows.Count; i++)
                    {
                        var rowData = new Dictionary<string, object>();
                        var cells = rows[i].Elements<Cell>().ToList();
                        
                        // We need to match cells to their column headers based on cell reference (A1, B1 etc)
                        // But for a simple scientific import, we assume columns are contiguous.
                        for (int j = 0; j < headers.Count; j++)
                        {
                            string header = headers[j];
                            if (string.IsNullOrEmpty(header)) continue;

                            // OpenXml rows don't always contain empty cells. 
                            // We need to find the cell that corresponds to the column index j.
                            var cell = GetCellAtColumn(rows[i], j);
                            rowData[header] = cell != null ? GetCellValue(cell, sharedStringPart) : string.Empty;
                        }
                        data.Add(rowData);
                    }
                }

            return data;
        }

        private Cell? GetCellAtColumn(Row row, int columnIndex)
        {
            // Simple mapping: A=0, B=1, etc.
            string columnLetter = GetColumnLetter(columnIndex);
            return row.Elements<Cell>().FirstOrDefault(c => c.CellReference?.Value?.StartsWith(columnLetter) == true);
        }

        private string GetColumnLetter(int index)
        {
            int dividend = index + 1;
            string columnLetter = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnLetter = Convert.ToChar(65 + modulo).ToString() + columnLetter;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnLetter;
        }

        private string GetCellValue(Cell cell, SharedStringTablePart? sharedStringPart)
        {
            if (cell.CellValue == null) return string.Empty;
            string value = cell.CellValue.InnerText;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString && sharedStringPart?.SharedStringTable != null)
            {
                var element = sharedStringPart.SharedStringTable.ElementAt(int.Parse(value));
                return element?.InnerText ?? string.Empty;
            }
            return value;
        }

        public string ExportToCsv(List<Dictionary<string, object>> data, List<StudyVariable> variables, string filePath, bool anonymize)
        {
            var headers = variables.Select(v => v.Name).ToList();
            var lines = new List<string>();
            lines.Add(string.Join(";", headers));

            foreach (var row in data)
            {
                var values = new List<string>();
                foreach (var v in variables)
                {
                    string val = row.ContainsKey(v.Name) ? row[v.Name]?.ToString() ?? "" : "";
                    if (anonymize && v.IsSensitive)
                    {
                        val = "[ANONYMISÉ]";
                    }
                    // Escape semicolon
                    if (val.Contains(";")) val = "\"" + val + "\"";
                    values.Add(val);
                }
                lines.Add(string.Join(";", values));
            }

            File.WriteAllLines(filePath, lines, System.Text.Encoding.UTF8);
            return filePath;
        }

        public string ExportToExcel(List<Dictionary<string, object>> data, List<StudyVariable> variables, string filePath, bool anonymize)
        {
            using (SpreadsheetDocument doc = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = doc.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                Sheets sheets = doc.WorkbookPart!.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet() { Id = doc.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Données AdRev" };
                sheets.Append(sheet);

                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;

                // Headers
                Row headerRow = new Row();
                foreach (var v in variables)
                {
                    headerRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue(v.Name) });
                }
                sheetData.Append(headerRow);

                // Data
                foreach (var row in data)
                {
                    Row dataRow = new Row();
                    foreach (var v in variables)
                    {
                        string val = row.ContainsKey(v.Name) ? row[v.Name]?.ToString() ?? "" : "";
                        if (anonymize && v.IsSensitive)
                        {
                            val = "[ANONYMISÉ]";
                        }
                        dataRow.Append(new Cell { DataType = CellValues.String, CellValue = new CellValue(val) });
                    }
                    sheetData.Append(dataRow);
                }

                workbookPart.Workbook.Save();
            }
            return filePath;
        }

        public List<StudyVariable> InferVariables(List<Dictionary<string, object>> data)
        {
            var variables = new List<StudyVariable>();
            if (data == null || data.Count == 0) return variables;

            foreach (var key in data[0].Keys)
            {
                if (string.IsNullOrWhiteSpace(key)) continue;

                var variable = new StudyVariable
                {
                    Name = key,
                    Prompt = key
                };

                // Type detection logic (scan all rows)
                var nonNullValues = data.Select(r => r.ContainsKey(key) ? r[key]?.ToString() : null)
                                      .Where(s => !string.IsNullOrWhiteSpace(s))
                                      .ToList();

                if (nonNullValues.Count == 0)
                {
                    variable.Type = VariableType.Text;
                }
                else if (nonNullValues.All(s => double.TryParse(s, out _)))
                {
                    // Check if all are integers
                    if (nonNullValues.All(s => int.TryParse(s, out _)))
                        variable.Type = VariableType.QuantitativeDiscrete;
                    else
                        variable.Type = VariableType.QuantitativeContinuous;
                }
                else if (nonNullValues.All(s => DateTime.TryParse(s, out _)))
                {
                    variable.Type = VariableType.QuantitativeTemporal;
                }
                else if (nonNullValues.All(s => s != null && (s.Equals("oui", StringComparison.OrdinalIgnoreCase) || 
                                              s.Equals("non", StringComparison.OrdinalIgnoreCase) ||
                                              s.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                              s.Equals("false", StringComparison.OrdinalIgnoreCase))))
                {
                    variable.Type = VariableType.QualitativeBinary;
                }
                else
                {
                    // If few unique values, could be nominal
                    int uniqueCount = nonNullValues.Distinct().Count();
                    if (uniqueCount > 1 && uniqueCount <= 10)
                    {
                        variable.Type = VariableType.QualitativeNominal;
                        variable.ChoiceOptions = string.Join(",", nonNullValues.Distinct());
                    }
                    else
                    {
                        variable.Type = VariableType.Text;
                    }
                }

                variables.Add(variable);
            }
            return variables;
        }
    }
}
