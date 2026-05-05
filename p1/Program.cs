using System.CommandLine;

// Define the root command for the CLI
var rootCommand = new RootCommand("Root command for file bundler CLI");

// Define the 'bundle' command which handles the file collection
var bundleCommand = new Command("bundle", "Bundle code files to a single file");

// Option for the output file path (Required)
var bundleOutput = new Option<FileInfo>("--output", "Enter file name") { IsRequired = true };
bundleOutput.AddAlias("-o");

// Option to filter by programming languages (e.g., 'cs', 'py', or 'all')
var bundleLanguage = new Option<string>("--language", "Enter language you want") { IsRequired = true };
bundleLanguage.AddAlias("-l");

// Option to include the source file path as a comment in the output
var bundleNote = new Option<bool>("--note", "Include source file path as a comment");
bundleNote.AddAlias("-n");

// Option to sort files by 'name' or 'type'
var bundleSort = new Option<string>("--sort", "Sort files by 'name' or 'type'");
bundleSort.AddAlias("-s");

// Option to remove empty lines from the source code
var bundleRemoveEmptyLines = new Option<bool>("--remove-empty-lines", "Remove empty lines from the source code");
bundleRemoveEmptyLines.AddAlias("-r");

// Add options to the bundle command
bundleCommand.AddOption(bundleRemoveEmptyLines);
bundleCommand.AddOption(bundleNote);
bundleCommand.AddOption(bundleLanguage);
bundleCommand.AddOption(bundleOutput);
bundleCommand.AddOption(bundleSort);

// Define the 'create-rsp' command to generate a response file
var createRspCommand = new Command("create-rsp", "Create a response file for the bundle command");

// Handler for the create-rsp command - prompts the user for input
createRspCommand.SetHandler(() =>
{
    Console.WriteLine("--- Welcome to the Response File Generator ---");

    string output;
    do
    {
        Console.Write("Enter output file name: ");
        output = Console.ReadLine();
    } while (string.IsNullOrWhiteSpace(output));

    string language;
    do
    {
        Console.Write("Enter languages to bundle (or 'all'): ");
        language = Console.ReadLine();
    } while (string.IsNullOrWhiteSpace(language));

    Console.Write("Include source file path as a comment? (y/n): ");
    var noteInput = Console.ReadLine()?.ToLower() == "y";

    Console.Write("Sort by name or type? (name/type): ");
    var sortInput = Console.ReadLine()?.ToLower();
    var sortValue = (sortInput == "type") ? "type" : "name";

    Console.Write("Remove empty lines? (y/n): ");
    var removeInput = Console.ReadLine()?.ToLower() == "y";

    // Build the content for the response file
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("bundle");
    sb.AppendLine("-o");
    sb.AppendLine(output);
    sb.AppendLine("-l");
    sb.AppendLine(language);
    if (noteInput) sb.AppendLine("-n");
    sb.AppendLine("-s");
    sb.AppendLine(sortValue);
    if (removeInput) sb.AppendLine("-r");

    try
    {
        Console.Write("Enter the name for the response file: ");
        string fileName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(fileName)) fileName = "response";

        File.WriteAllText($"{fileName}.rsp", sb.ToString());
        Console.WriteLine($"File '{fileName}.rsp' created successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating response file: {ex.Message}");
    }
});

// Handler for the bundle command - performs the actual bundling
bundleCommand.SetHandler((output, language, note, sort, removeEmptyLines) =>
{
    // Step 1: Get the list of files based on user criteria
    string[] allFiles = HandleFiles(output, language, sort);

    try
    {
        // Step 2: Write the content of gathered files into the output file
        using (StreamWriter writer = new StreamWriter(output.FullName))
        {
            foreach (var file in allFiles)
            {
                if (note) writer.WriteLine($"// Source: {file}");

                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    if (removeEmptyLines && string.IsNullOrWhiteSpace(line)) continue;
                    writer.WriteLine(line);
                }
                writer.WriteLine(); // Add a newline between files
            }
        }
        Console.WriteLine("Bundle created successfully!");
    }
    catch (Exception)
    {
        Console.WriteLine("Error: Could not save the file. Check the path and permissions.");
    }

}, bundleOutput, bundleLanguage, bundleNote, bundleSort, bundleRemoveEmptyLines);

// Add commands to root and invoke the CLI
rootCommand.AddCommand(createRspCommand);
rootCommand.AddCommand(bundleCommand);
await rootCommand.InvokeAsync(args);

// Helper function to find and filter files
static string[] HandleFiles(FileInfo output, string language, string sort)
{
    try
    {
        // 1. Define the Whitelist of allowed extensions
        var allowedExtensions = new HashSet<string> { ".cs", ".txt", ".js", ".py", ".java", ".cpp", ".h", ".html", ".css", ".json", ".ts" };

        string dirPath = Directory.GetCurrentDirectory();

        var allFiles = new List<FileInfo>();

        HashSet<string> targets;

        if (language.ToLower() == "all")
        { 
            targets = allowedExtensions;
        }
        else
        {

            targets = new HashSet<string> { "." + language.ToLower().TrimStart('.') };
        }
        allFiles = Directory.GetFiles(dirPath, "*.*", new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true
        })
            .Where(file =>
                !file.Split(Path.DirectorySeparatorChar).Any(part => part.ToLower() == "bin" || part.ToLower() == "obj") &&
                !file.EndsWith(output.Name) &&
                targets.Contains(Path.GetExtension(file).ToLower())
            )
            .Select(file => new FileInfo(file)) 
            .ToList();
        // Sort files by 'type' or 'name'
        var result = (sort?.ToLower() == "type")
            ? allFiles.OrderBy(f => f.Extension).ThenBy(f => f.Name)
                .Select(f => f.FullName)
                .ToArray()
            : allFiles.OrderBy(f => f.Name)
                .Select(f => f.FullName) 
                .ToArray();

        return result;
    }
    catch (Exception)
    {
        Console.WriteLine("Error accessing directories.");
        return Array.Empty<string>();
    }
}