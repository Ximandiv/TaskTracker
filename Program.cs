using System.Text.Json;
using System.Text.RegularExpressions;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Welcome to Task Tracker.");
        Console.WriteLine("You can use the CLI by starting with the prefix 'ttracker'");
        Console.WriteLine("\nUse 'ttracker help' for available commands and 'ttracker task struct' to show how a task looks");
        Console.WriteLine("Use clear command to clear the terminal");
        Console.WriteLine("Use exit command to close the application");

        Console.WriteLine();

        string? inputCmd = string.Empty;

        string command = string.Empty;
        string firstArg = string.Empty;
        string secondArg = string.Empty;

        string pattern = @"^ttracker\s+(?<command>\w+)(?:\s+(?<arg1>.+))?$";

        while (string.IsNullOrEmpty(inputCmd))
        {
            inputCmd = Console.ReadLine();

            if(inputCmd == "clear")
            {
                Console.Clear();
                inputCmd = string.Empty;
                continue;
            }

            if(inputCmd == "exit")
            {
                break;
            }

            if (string.IsNullOrEmpty(inputCmd) || inputCmd.Length < 8)
            {
                Console.WriteLine("\nInvalid prefix command. Are you sure you\'re using ttracker prefix?\n");
                inputCmd = string.Empty;
                continue;
            }

            string cmdPrefix = inputCmd.Substring(0, 8);

            if (!Regex.IsMatch(cmdPrefix, @"^ttracker$"))
            {
                Console.WriteLine("\nInvalid prefix command. Are you sure you're using ttracker prefix?\n");
                inputCmd = string.Empty;
                continue;
            }

            inputCmd = inputCmd.ToLower();

            Match match = Regex.Match(inputCmd, pattern);

            command = match.Groups["command"].Value;
            firstArg = match.Groups["arg1"].Value;

            if (command == "task" && firstArg == "struct")
            {
                explainTaskStruct();
                inputCmd = string.Empty;
                continue;
            }

            if (command == "help")
            {
                availableCmds();
                inputCmd = string.Empty;
                continue;
            }

            if(command == "add")
            {
                if (string.IsNullOrEmpty(firstArg))
                {
                    Console.WriteLine();
                    Console.WriteLine("Invalid argument");
                    Console.WriteLine();
                    inputCmd = string.Empty;
                    continue;
                }

                string filePath = "tasks.json";

                List<TaskItem> jsonItems;

                if(File.Exists(filePath))
                {
                    string jsonFile = File.ReadAllText(filePath);
                    jsonItems = JsonSerializer.Deserialize<List<TaskItem>>(jsonFile) ?? new();
                }
                else
                    jsonItems = new List<TaskItem>();

                int newId = jsonItems.Count > 0 ? jsonItems[^1].Id + 1 : 1;
                string description = firstArg;

                TaskItem newTaskItem = new TaskItem
                {
                    Id = newId,
                    Description = description,
                    Status = "Todo",
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd hh:mm:sstt"),
                    UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd hh:mm:sstt")
                };

                jsonItems.Add(newTaskItem);

                string updatedJson = JsonSerializer.Serialize(jsonItems, 
                    new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(filePath, updatedJson);

                Console.WriteLine();

                Console.WriteLine($"Task added successfully with ID {newId}");

                Console.WriteLine();
                inputCmd = string.Empty;
                continue;
            }

            if(command == "list")
            {
                string status = firstArg;

                if(!string.IsNullOrEmpty(status))
                    status = char.ToUpper(status[0]) + status.Substring(1);

                if (status == "Todo" || status == "Inprogress" || status == "Done")
                {
                    string filePath = "tasks.json";

                    List<TaskItem> taskItems;

                    if (File.Exists(filePath))
                    {
                        string jsonFile = File.ReadAllText(filePath);
                        taskItems = JsonSerializer.Deserialize<List<TaskItem>>(jsonFile) ?? new();
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("No tasks found!");
                        Console.WriteLine();
                        inputCmd = string.Empty;
                        continue;
                    }

                    List<TaskItem> tasksByStatus = taskItems.Where(tItem => tItem.Status == status).ToList();

                    if(tasksByStatus.Count == 0)
                    {
                        Console.WriteLine($"\nNo tasks with status {status} were found\n");
                        inputCmd = string.Empty;
                        continue;
                    }

                    Console.WriteLine();

                    foreach (TaskItem taskItem in tasksByStatus)
                    {
                        Console.WriteLine($"{taskItem.Id} - {taskItem.Description}. Status: {taskItem.Status}, Created At: {taskItem.CreatedAt}, Updated At: {taskItem.UpdatedAt}");
                        Console.WriteLine();
                    }
                }
                else if(string.IsNullOrEmpty(status))
                {
                    string filePath = "tasks.json";

                    List<TaskItem> taskItems;

                    if (File.Exists(filePath))
                    {
                        string jsonFile = File.ReadAllText(filePath);
                        taskItems = JsonSerializer.Deserialize<List<TaskItem>>(jsonFile) ?? new();
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("No tasks found!");
                        Console.WriteLine();
                        inputCmd = string.Empty;
                        continue;
                    }

                    Console.WriteLine();

                    foreach (TaskItem taskItem in taskItems)
                    {
                        Console.WriteLine($"{taskItem.Id} - {taskItem.Description}. Status: {taskItem.Status}, Created At: {taskItem.CreatedAt}, Updated At: {taskItem.UpdatedAt}");
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine("Invalid status!");
                }

                inputCmd = string.Empty;
                continue;
            }

            if(command == "update")
            {
                if (string.IsNullOrEmpty(firstArg))
                {
                    Console.WriteLine();
                    Console.WriteLine("Invalid argument");
                    Console.WriteLine();
                    inputCmd = string.Empty;
                    continue;
                }

                string id = Regex.Replace(firstArg, @"\s.*", string.Empty);
                string description = Regex.Replace(firstArg, @"^.*?\s", string.Empty);
                bool isIdValid = Regex.IsMatch(id, @"^-?\d+$");

                if (isIdValid && int.TryParse(id, out int idNumber))
                {
                    string filePath = "tasks.json";

                    List<TaskItem> taskItems;

                    if (File.Exists(filePath))
                    {
                        string jsonFile = File.ReadAllText(filePath);
                        taskItems = JsonSerializer.Deserialize<List<TaskItem>>(jsonFile) ?? new();
                    }
                    else
                    {
                        Console.WriteLine("No tasks found!");
                        inputCmd = string.Empty;
                        continue;
                    }

                    TaskItem? taskToUpdate = taskItems.FirstOrDefault(tItem => tItem.Id == idNumber);

                    if(taskToUpdate == null)
                    {
                        Console.WriteLine("Task not found!");
                        inputCmd = string.Empty;
                        continue;
                    }

                    taskToUpdate.Description = description;
                    taskToUpdate.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd hh:mm:sstt");

                    string updatedJson = JsonSerializer.Serialize(taskItems,
                    new JsonSerializerOptions { WriteIndented = true });

                    File.WriteAllText(filePath, updatedJson);

                    Console.WriteLine();

                    Console.WriteLine($"Task updated with ID {idNumber}");

                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Invalid id! Must be a whole number");
                }

                inputCmd = string.Empty;
                continue;
            }

            if (command == "delete")
            {
                if (string.IsNullOrEmpty(firstArg))
                {
                    Console.WriteLine();
                    Console.WriteLine("Invalid argument");
                    Console.WriteLine();
                    inputCmd = string.Empty;
                    continue;
                }

                string id = Regex.Replace(firstArg, @"\s.*", string.Empty);
                string description = Regex.Replace(firstArg, @"^.*?\s", string.Empty);
                bool isIdValid = Regex.IsMatch(id, @"^-?\d+$");

                if (isIdValid && int.TryParse(id, out int idNumber))
                {
                    string filePath = "tasks.json";
                    List<TaskItem> taskItems;

                    if (File.Exists(filePath))
                    {
                        string jsonFile = File.ReadAllText(filePath);
                        taskItems = JsonSerializer.Deserialize<List<TaskItem>>(jsonFile) ?? new();
                    }
                    else
                    {
                        Console.WriteLine("No tasks found!");
                        inputCmd = string.Empty;
                        continue;
                    }

                    taskItems.RemoveAll(tItem => tItem.Id == idNumber);

                    string updatedJson = JsonSerializer.Serialize(taskItems,
                    new JsonSerializerOptions { WriteIndented = true });

                    File.WriteAllText(filePath, updatedJson);

                    Console.WriteLine();

                    Console.WriteLine($"Task deleted with ID {idNumber}");

                    Console.WriteLine();
                    inputCmd = string.Empty;
                    continue;
                }
            }

            if (command == "mark")
            {
                if (string.IsNullOrEmpty(firstArg))
                {
                    Console.WriteLine();
                    Console.WriteLine("Invalid argument");
                    Console.WriteLine();
                    inputCmd = string.Empty;
                    continue;
                }

                string status = Regex.Replace(firstArg, @"\s.*", string.Empty);
                status = char.ToUpper(status[0]) + status.Substring(1);

                string id = Regex.Replace(firstArg, @"^.*?\s", string.Empty);

                bool isIdValid = Regex.IsMatch(id, @"^-?\d+$");

                if (isIdValid && int.TryParse(id, out int idNumber))
                {
                    string filePath = "tasks.json";
                    List<TaskItem> taskItems;

                    if (File.Exists(filePath))
                    {
                        string jsonFile = File.ReadAllText(filePath);
                        taskItems = JsonSerializer.Deserialize<List<TaskItem>>(jsonFile) ?? new();
                    }
                    else
                    {
                        Console.WriteLine("No tasks found!");
                        inputCmd = string.Empty;
                        continue;
                    }

                    TaskItem? taskToUpdate = taskItems.FirstOrDefault(tItem => tItem.Id == idNumber);

                    if (taskToUpdate == null)
                    {
                        Console.WriteLine("Task not found!");
                        inputCmd = string.Empty;
                        continue;
                    }

                    taskToUpdate.Status = status;
                    taskToUpdate.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd hh:mm:sstt");

                    string updatedJson = JsonSerializer.Serialize(taskItems,
                    new JsonSerializerOptions { WriteIndented = true });

                    File.WriteAllText(filePath, updatedJson);

                    Console.WriteLine();

                    Console.WriteLine($"Task marked {status} with ID {taskToUpdate.Id}");

                    Console.WriteLine();
                    inputCmd = string.Empty;
                    continue;
                }
            }

            inputCmd = string.Empty;
            continue;
        }
    }

    private static void explainTaskStruct()
    {
        Console.WriteLine();

        Console.WriteLine("A task is composed of: ID, Description, Status and Created/Updated dates");

        Console.WriteLine();

        Console.WriteLine("Example:");

        Console.WriteLine("1 - Buy Milk on the Grocery Store. Status: In Progress, Created At: 2024-09-29 12:00:00AM, Updated At: 2024-09-30 12:00:00PM");

        Console.WriteLine();
    }

    private static void availableCmds()
    {
        Console.WriteLine();

        Console.WriteLine("Add, update and delete tasks:");
        Console.WriteLine();
        Console.WriteLine("ttracker add \"Task Description\"");
        Console.WriteLine("ttracker update (id) \"Task Description\"");
        Console.WriteLine("ttracker delete (id)");

        Console.WriteLine("-----------------------------------");

        Console.WriteLine("Mark task status:");
        Console.WriteLine();
        Console.WriteLine("ttracker mark todo (id)");
        Console.WriteLine("ttracker mark inprogress (id)");
        Console.WriteLine("ttracker mark done (id)");

        Console.WriteLine("-----------------------------------");

        Console.WriteLine("List all or filtered task list");
        Console.WriteLine();
        Console.WriteLine("ttracker list");
        Console.WriteLine("ttracker list in-progress");
        Console.WriteLine("ttracker list done");

        Console.WriteLine();
    }
}

public class TaskItem
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
}
