using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
//tr
public class Program
{
    public static void Main()
    {
        // Get the base directory of the application (bin/Debug folder)
        var currentDirectory = Directory.GetCurrentDirectory();

        // Move up to the project directory where Program.cs and "stores" folder should be located
        string projectDirectory = Path.Combine(currentDirectory, "..", "..", "..");
        string storesFolderPath = Path.Combine(projectDirectory, "stores");

        // Check if the stores directory exists
        if (!Directory.Exists(storesFolderPath))
        {
            Console.WriteLine("The 'stores' folder does not exist at path: " + storesFolderPath);
            return;
        }

        // Call the FindFiles method
        var salesFiles = FindFiles(storesFolderPath);

        // Create the sales total directory
        var salesTotalDir = Path.Combine(projectDirectory, "salesTotalDir");
        Directory.CreateDirectory(salesTotalDir);

        // Calculate the sales total and write it to a file
        double totalSales = CalculateSalesTotal(salesFiles, out Dictionary<string, (string FolderName, double Total)> salesDetails);
        File.WriteAllText(Path.Combine(salesTotalDir, "totals.txt"), $"{totalSales}{Environment.NewLine}");

        Console.WriteLine($"Sales total calculated: {totalSales:F2}");

        //Generate Sales Summary report
        GenerateSalesSummaryReport(salesDetails, totalSales, Path.Combine(salesTotalDir, "SalesSummaryReport.txt"));
        Console.WriteLine($"Sales summary report generated at: {salesTotalDir}");
    }

    public static IEnumerable<string> FindFiles(string folderPath)
    {
        List<string> salesFiles = new List<string>();

        // Enumerate all files in the specified folder and its subdirectories
        var foundFiles = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories);

        foreach (var file in foundFiles)
        {
            // Only add files that end with "sales.json"
            if (file.EndsWith("sales.json", StringComparison.OrdinalIgnoreCase))
            {
                salesFiles.Add(file);
            }
        }

        return salesFiles;
    }

    public static double CalculateSalesTotal(IEnumerable<string> salesFiles, out Dictionary<string, (string folderName, double Total)> salesDetails)
    {
        double totalSales = 0;
        salesDetails = new Dictionary<string, (string folderName, double Total)>();

        // Read each file and accumulate the total
        foreach (var file in salesFiles)
        {
            try
            {
                // Read the contents of the file
                string salesJson = File.ReadAllText(file);

                // Parse the contents as JSON
                SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);

                // Extracting the folder name
                string folderName = Path.GetFileName(Path.GetDirectoryName(file)) ?? "Unknown Folder";

                // Add the amount found in the Total field to the salesTotal variable
                double fileSalesTotal = data?.Total ?? 0;
                salesDetails[file] = (folderName, fileSalesTotal);
                totalSales += fileSalesTotal;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {file}: {ex.Message}");
            }
        }

        return totalSales;
    }

    public static void GenerateSalesSummaryReport(Dictionary<string, (string FolderName, double Total)> salesDetails, double totalSales, string reportFilePath)
    {
        var reportBuilder = new StringBuilder();

        reportBuilder.AppendLine("Sales Summary\n-----------------------------");
        reportBuilder.AppendLine($"Total Sales: {totalSales:C}\nDetails:");

        foreach (var entry in salesDetails)
        {
            string  fileName = Path.GetFileName(entry.Key); //this is to just get the file name
            string folderName = entry.Value.FolderName;
            double total = entry.Value.Total;
            reportBuilder.AppendLine($" {folderName}/{fileName}: {total:C}");
        }

        //Finally I write the report to the file 
        File.WriteAllText(reportFilePath, reportBuilder.ToString());
    }


    // Record to represent the JSON structure
    public record SalesData(double Total);
}
   