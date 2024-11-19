using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        var salesTotal = CalculateSalesTotal(salesFiles);
        File.WriteAllText(Path.Combine(salesTotalDir, "totals.txt"), $"{salesTotal}{Environment.NewLine}");

        Console.WriteLine($"Sales total calculated: {salesTotal:F2}");
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

    public static double CalculateSalesTotal(IEnumerable<string> salesFiles)
    {
        double salesTotal = 0;

        // Read each file and accumulate the total
        foreach (var file in salesFiles)
        {
            try
            {
                // Read the contents of the file
                string salesJson = File.ReadAllText(file);

                // Parse the contents as JSON
                SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);

                // Add the amount found in the Total field to the salesTotal variable
                salesTotal += data?.Total ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {file}: {ex.Message}");
            }
        }

        return salesTotal;
    }

    // Record to represent the JSON structure
    public record SalesData(double Total);
}
   