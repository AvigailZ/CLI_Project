using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

var languageOption = new Option<string>(new[] { "-l", "--language" }, "Code files for the bundle file");
var outputOption = new Option<FileInfo>(new[] { "-o", "--output" }, "File path and name");
var authorOption = new Option<string>(new[] { "-a", "--author" }, "The author of the file");
var removeEmptyLinesOption = new Option<bool>(new[] { "-r", "--remove-empty-lines" }, "Remove Empty Line before copy");
var sortOption = new Option<string>(new[] { "-s", "--sort" }, "Order of copying the code files");
var noteOption = new Option<bool>(new[] { "-n", "--note" }, "File-Source-Code name and path");

var bundleCommand = new Command("bundle", "Bundle code file for a single file");
var CreateRspCommand = new Command("create-rsp", "Create response file with ready answering");

bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(authorOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(noteOption);


bundleCommand.SetHandler((output,language,author,removeEmptyLine,sort,note) =>
{

    if (language != "")
    {
        var files = new List<string>();
        var searchOption = SearchOption.AllDirectories;

        if (language == "all") language = ".cs,.py,.js,.java,.rb, .c, .cpp, .go";
        List<string> languages = language.Split(",").ToList();
        foreach (var l in languages) {
        files.AddRange(Directory.GetFiles(Directory.GetCurrentDirectory(), "*" + l, searchOption));
        }
        files = files.Where(f => !f.Contains("bin") && !f.Contains("Debug")).ToList();

        if (sort == "code")
            files = files.OrderBy(f => Path.GetExtension(f)).ToList();
        if (sort == "AB" || sort == "")
            files = files.OrderBy(f => Path.GetFileName(f)).ToList();
        
            //File.Create(output.ToString());
            //Console.WriteLine("File was created");
            bool exists = File.Exists(output.ToString() + ".txt");
        if (!exists)
        {
            try
            {
                using (var bundleFile = new StreamWriter(output.ToString() + ".txt"))
                {
                    if (author != null)
                    {
                        bundleFile.WriteLine($"# Author: {author}");
                    }
                    foreach (var file in files)
                    {
                        if (note)
                        {
                            bundleFile.WriteLine($"# {file}");
                        }

                        var content = File.ReadAllLines(file);
                        if (removeEmptyLine)
                        {
                            content = content.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
                        }

                        foreach (var line in content)
                        {
                            bundleFile.WriteLine(line);
                        }
                        bundleFile.WriteLine("\n");
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Error : File path is Invalid");
            }
        }
        else
            Console.WriteLine("There is a same file_name in this path");
    }
    else { Console.WriteLine("Language is required"); }


},outputOption,languageOption,authorOption,removeEmptyLinesOption,sortOption,noteOption);



CreateRspCommand.SetHandler(() =>
{
    
    string rspFile = ""; 
    while (rspFile == "")
    {
        Console.WriteLine("Enter your FullName:");
        rspFile = Console.ReadLine();
        if (rspFile != "")
        {
            bool exists = File.Exists(rspFile.ToString() + ".rsp");
            if (exists)
            {
                Console.WriteLine("There is a same file_name in the path enter another");
                rspFile = "";
            }
        }
    }

    string language = "";
    while (language == "")
    {
        Console.WriteLine("Enter list of Languages you want to include with , between or Enter all");
        language = Console.ReadLine();
    }


    string output = "";
    try
    {
        while (output == "")
        {
            Console.WriteLine("Enter File path and name:");
            output = Console.ReadLine();
            if (output != "")
            {
                bool exists = File.Exists(output.ToString() + ".txt");
                if (exists)
                {
                    Console.WriteLine("There is a same file_name in the path enter another");
                    output = "";
                }
            }
        }
        
        
    }
    catch (DirectoryNotFoundException)
    {
        Console.WriteLine("Error : File path is Invalid");
    }
    

    Console.WriteLine("Enter The author of the file:");
    string author = Console.ReadLine();


    bool isRELSuccessful = false;
    bool removeEmptyLines = false;
    while (!isRELSuccessful)
    {
        try
        {
            Console.WriteLine("Remove Empty Line before copy? - Enter true or false:");
            removeEmptyLines = Convert.ToBoolean(Console.ReadLine());
            isRELSuccessful = true;
        }
        catch (FormatException)
        {
            Console.WriteLine("Expected true or false");
        }
    }


    string sort = "";
    while (!sort.Equals("AB") && !sort.Equals("code"))
    {
        Console.WriteLine("Order of copying the code files - Enter code or AB:");
        sort = Console.ReadLine();
    }


    bool isNoteSuccessful = false;
    bool note = false;
    while (!isNoteSuccessful)
    {
        try
        {
            Console.WriteLine("Write the File-Source-Code name and path? - Enter true or false");
            note = Convert.ToBoolean(Console.ReadLine());
            isNoteSuccessful = true;
        }
        catch (FormatException)
        {
            Console.WriteLine("Expected true or false");
        }
    }


    var rspCommand = $"bundle  --output \"{output}\" --language \"{language}\"";
    if (author != "")
    {
        rspCommand += $" --author \"{author}\"";
    } 
    if (removeEmptyLines)
    {
        rspCommand += " --remove-empty-lines";
    }
    if (sort != "")
    {
        rspCommand += $" --sort \"{sort}\"";
    }
    if (note)
    {
        rspCommand += " --note";
    }
   
    File.WriteAllText($"{rspFile}.rsp", rspCommand);
});

var rootCommand = new RootCommand("Root command for file Bunder CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(CreateRspCommand);

await rootCommand.InvokeAsync(args);