using EasySave.Models;
using EasySave.ViewModels;

namespace EasySave.Views
{
    public class ConsoleView
    {
        private readonly BackupManagerViewModel _viewModel;

        public ConsoleView(BackupManagerViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Run()
        {
            while (true)
            {
                ShowMenu();
                string input = Console.ReadLine() ?? string.Empty;
                if (input.Trim() == "0") break;
                HandleUserInput(input);
            }
        }

        public void ShowMessage(string msg) => Console.WriteLine(msg);

        public void ShowJobs(List<BackupJob> jobs)
        {
            if (jobs.Count == 0) { ShowMessage(_viewModel.Translate("no.jobs")); return; }
            foreach (BackupJob job in jobs)
                Console.WriteLine($"[{job.Id}] {job.Name} | {job.Type} | {job.SourcePath} -> {job.TargetPath}");
        }

        public void ShowStates(List<BackupState> states)
        {
            if (states.Count == 0) { ShowMessage(_viewModel.Translate("no.states")); return; }
            foreach (BackupState state in states)
                Console.WriteLine($"{state.JobName} | {state.Status} | {state.Progress}% | {state.RemainingFiles} files remaining");
        }

        public void ShowError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {msg}");
            Console.ResetColor();
        }

        private void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine(_viewModel.Translate("menu.title"));
            Console.WriteLine($"1. {_viewModel.Translate("menu.list")}");
            Console.WriteLine($"2. {_viewModel.Translate("menu.add")}");
            Console.WriteLine($"3. {_viewModel.Translate("menu.remove")}");
            Console.WriteLine($"4. {_viewModel.Translate("menu.execute.one")}");
            Console.WriteLine($"5. {_viewModel.Translate("menu.execute.all")}");
            Console.WriteLine($"6. {_viewModel.Translate("menu.states")}");
            Console.WriteLine($"7. {_viewModel.Translate("menu.language")}");
            Console.WriteLine($"8. {_viewModel.Translate("menu.log.format")} [{_viewModel.GetCurrentLogFormat()}]");
            Console.WriteLine($"0. {_viewModel.Translate("menu.exit")}");
            Console.Write(_viewModel.Translate("menu.choice") + " ");
        }

        private void HandleUserInput(string input)
        {
            switch (input.Trim())
            {
                case "1":
                    ShowJobs(_viewModel.Jobs);
                    break;

                case "2":
                    PromptNewJob();
                    break;

                case "3":
                    Console.Write(_viewModel.Translate("job.enter.id") + " ");
                    if (int.TryParse(Console.ReadLine(), out int removeId))
                        _viewModel.RemoveJob(removeId);
                    break;

                case "4":
                    Console.Write(_viewModel.Translate("job.enter.id") + " ");
                    string execInput = Console.ReadLine() ?? string.Empty;
                    List<int> ids = ParseIds(execInput);
                    if (ids.Count == 1)
                        _viewModel.ExecuteJob(ids[0]);
                    else
                        _viewModel.ExecuteJobs(ids);
                    ShowMessage(_viewModel.Translate("job.done"));
                    break;

                case "5":
                    _viewModel.ExecuteAll();
                    ShowMessage(_viewModel.Translate("job.done"));
                    break;

                case "6":
                    ShowStates(_viewModel.GetStates());
                    break;

                case "7":
                    Console.Write(_viewModel.Translate("language.choose") + " (fr/en): ");
                    string? lang = Console.ReadLine();
                    if (lang == "fr" || lang == "en")
                    {
                        _viewModel.SetLanguage(lang);
                        ShowMessage(_viewModel.Translate("language.changed"));
                    }
                    break;

                case "8":
                    PromptLogFormat();
                    break;

                default:
                    ShowError(_viewModel.Translate("invalid.input"));
                    break;
            }
        }

        private void PromptLogFormat()
        {
            Console.WriteLine(_viewModel.Translate("log.format.choose"));
            Console.WriteLine("1. JSON");
            Console.WriteLine("2. XML");
            Console.Write(_viewModel.Translate("menu.choice") + " ");
            string? choice = Console.ReadLine();

            LogFormat format = choice == "2" ? LogFormat.Xml : LogFormat.Json;
            _viewModel.SetLogFormat(format);
            ShowMessage($"{_viewModel.Translate("log.format.changed")} {format}");
        }

        private void PromptNewJob()
        {
            try
            {
                Console.Write(_viewModel.Translate("job.name") + " ");
                string? name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name)) return;

                Console.Write(_viewModel.Translate("job.source") + " ");
                string? source = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(source)) return;

                Console.Write(_viewModel.Translate("job.target") + " ");
                string? target = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(target)) return;

                Console.Write(_viewModel.Translate("job.type") + " (1=Full, 2=Differential): ");
                string? typeInput = Console.ReadLine();
                BackupType type = typeInput == "2" ? BackupType.Differential : BackupType.Full;

                bool added = _viewModel.AddJob(name, source, target, type);
                ShowMessage(added
                    ? _viewModel.Translate("job.added")
                    : _viewModel.Translate("job.max.reached"));
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private List<int> ParseIds(string input)
        {
            List<int> ids = new();
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
    }
}