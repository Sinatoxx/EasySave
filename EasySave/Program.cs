using EasyLog;
using EasySave.Services;
using EasySave.ViewModels;
using EasySave.Views;

BackupManagerViewModel viewModel = ComposeDependencies();
List<int> ids = ParseArguments(args);

if (ids.Count > 0)
{
    viewModel.ExecuteJobs(ids);
    return;
}

ConsoleView view = new ConsoleView(viewModel);
view.Run();

static BackupManagerViewModel ComposeDependencies()
{
    Logger logger = new Logger();
    StateService stateService = new StateService();
    ConfigService configService = new ConfigService();
    LanguageService langService = new LanguageService();
    BackupService backupService = new BackupService(logger);
    backupService.AddObserver(stateService);
    return new BackupManagerViewModel(backupService, configService, stateService, langService);
}

static List<int> ParseArguments(string[] args)
{
    List<int> ids = new();
    if (args.Length == 0) return ids;
    string input = args[0];

    if (input.Contains('-'))
    {
        string[] parts = input.Split('-');
        if (int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
            for (int i = start; i <= end; i++)
                ids.Add(i);
    }
    else
    {
        foreach (string part in input.Split(';'))
            if (int.TryParse(part.Trim(), out int id))
                ids.Add(id);
    }
    return ids;
}