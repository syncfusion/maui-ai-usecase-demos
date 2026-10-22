using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace SmartDailyDecisionCompanion
{
    /// <summary>
    /// MainViewModel serves as the central ViewModel for the Smart Daily Decision Companion app.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Index of the Today tab in the mobile and desktop tab views.</summary>
        public const int TodayTabIndex = 0;

        /// <summary>Index of the Insights tab in the mobile and desktop tab views.</summary>
        public const int InsightsTabIndex = 1;

        private readonly IAzureAIService _aiService;
        private List<TaskItem> _aiOrdered = new List<TaskItem>();
        private int _aiCurrentIndex = 0;
        private bool _isDesktop;
        private bool _isMobile;
        private string _pressureLabel = "Medium Pressure";
        private string _tips = string.Empty;
        private TaskItem? _selectedTask;
        private string _mentorshipDetail = "This report is a Keystone Task. Completing it today reduces your end-of-week cognitive load by 40%.";
        private string _urgencyText = "Due in 48 hours. Missing today loses the buffer for review.";
        private string _importanceText = "Directly impacts Q3 budget approval and team morale.";
        private string _positiveMomentumText = "Completing this unlocks downstream approvals and reduces rework.";
        private string _delayRiskText = "Postponing may shift effort to next week and increase context switching.";
        private string _focusLabel = "2H FOCUS";
        private string _newTaskTopic = string.Empty;
        private string _newTaskDescription = string.Empty;
        private bool _isAddTaskPopupVisible;
        private DateTime _newTaskDeadline = DateTime.Today;
        private string _newTaskProjectType = string.Empty;
        private string _newTaskPriority = string.Empty;
        private string _recommendationTitle = "—";
        private int _selectedTaskAnalysisVersion;
        private int _selectedTabIndex;
        private bool _credentialsValidated;

        /// <summary>
        /// It initializes the MainViewModel, resolving the AI service through dependency injection,
        /// detecting the platform for layout purposes, and seeding and categorizing the initial tasks.
        /// </summary>
        public MainViewModel(IAzureAIService aiService)
        {
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));

            // Detect the platform once so XAML can switch layouts reliably.
            IsDesktop = DeviceInfo.Platform == DevicePlatform.WinUI || DeviceInfo.Platform == DevicePlatform.MacCatalyst;
            IsMobile = !IsDesktop;

            SeedTasks();
            CategorizeTasks();
            AcceptRecommendationCommand = new RelayCommand(ExecuteAcceptRecommendation);
            PostponeRecommendationCommand = new RelayCommand(ExecutePostponeRecommendation);

            // Add-task popup commands
            ShowAddTaskCommand = new RelayCommand(_ => { IsAddTaskPopupVisible = true; });
            HideAddTaskCommand = new RelayCommand(_ => { IsAddTaskPopupVisible = false; });
            ConfirmAddTaskCommand = new RelayCommand(ExecuteConfirmAddTask);
        }

        /// <summary>
        /// Validates the Azure OpenAI credentials and surfaces a user-facing alert when they are
        /// missing or rejected, then kicks off the initial AI analyses. The credential check is
        /// awaited here rather than started from the AI service constructor so the outcome is
        /// deterministic and this failure path stays visible to the caller.
        /// </summary>
        public async Task InitializeAsync()
        {
            await _aiService.ValidateCredentialAsync();

            if (!_aiService.IsCredentialValid)
            {
                await NotifyCredentialsInvalidAsync();
                return;
            }

            _credentialsValidated = true;
            RefreshDailyAnalysis();
        }

        /// <summary>
        /// Starts the analyses that depend on the current Do Today list. Callers must confirm the
        /// credentials first, so no AI request is issued while they are known to be invalid.
        /// This is the single place those analyses are kicked off, so each one runs once per
        /// categorization instead of once per caller.
        /// </summary>
        private void RefreshDailyAnalysis()
        {
            _ = AnalyzePressureAsync();
            _ = AnalyzeRecommendationAsync();
            _ = AnalyzeFocusAsync();
        }

        /// <summary>
        /// Shows the invalid-credentials alert. This runs after the awaited validation call, so
        /// the continuation is already back on the UI thread that started InitializeAsync.
        /// </summary>
        private static async Task NotifyCredentialsInvalidAsync()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null)
                return;

            await page.DisplayAlert(
                "Alert",
                "The Azure API key or endpoint is missing or incorrect. Please verify your credentials in Services/AzureBaseService.cs.",
                "OK");
        }

        /// <summary>
        /// Gets a dynamic greeting based on the current time of day.
        /// </summary>
        public string Greeting
        {
            get
            {
                var hour = DateTime.Now.Hour;
                if (hour < 12) return "Good morning, Mentor";
                if (hour < 18) return "Good afternoon, Mentor";
                return "Good evening, Mentor";
            }
        }

        /// <summary>
        /// Gets or sets the selected tab index for both the mobile and desktop tab views.
        /// Selecting a task opens the Insights tab (index 1); accepting or postponing a
        /// recommendation returns to the Today tab (index 0).
        /// </summary>
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex == value) return;
                _selectedTabIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTabIndex)));
            }
        }

        /// <summary>
        /// Gets the collection of all tasks. This is the master list that contains every task regardless of categorization.
        /// </summary>
        public ObservableCollection<TaskItem> AllTasks { get; } = new ObservableCollection<TaskItem>();

        /// <summary>
        /// Gets the collection of tasks that are categorized as "Do Today".
        /// </summary>
        public ObservableCollection<TaskItem> DoToday { get; } = new ObservableCollection<TaskItem>();

        /// <summary>
        /// Gets the collection of tasks that are categorized as "Postpone".
        /// </summary>
        public ObservableCollection<TaskItem> Postpone { get; } = new ObservableCollection<TaskItem>();

        /// <summary>
        /// Gets the collection of tasks that are categorized as "Watch".
        /// </summary>
        public ObservableCollection<TaskItem> Watch { get; } = new ObservableCollection<TaskItem>();

        /// <summary>
        /// Gets the count of tasks in each category.
        /// </summary>
        public int DoTodayCount => DoToday.Count;

        /// <summary>
        /// Gets the count of tasks in the Postpone category.
        /// </summary>
        public int PostponeCount => Postpone.Count;

        /// <summary>
        /// Gets the count of tasks in the Watch category.
        /// </summary>
        public int WatchCount => Watch.Count;

        /// <summary>
        /// Gets a label for the number of meetings in today's tasks, formatted as "1 Meeting" or "N Meetings".
        /// </summary>
        public string MeetingsCountLabel =>
            (DoToday?.Count(x => string.Equals(x.ProjectType, "Meeting", StringComparison.OrdinalIgnoreCase)) ?? 0) switch
            {
                1 => "1 Meeting",
                var n => $"{n} Meetings"
            };

        /// <summary>
        /// Gets a label for today's date in the format "DAY, MONTH DATE" (e.g., "MONDAY, JANUARY 1").
        /// </summary>
        public string DateLabel { get; } = DateTime.Today.ToString("dddd, MMMM d").ToUpperInvariant();

        /// <summary>
        /// Gets or sets a boolean indicating whether the app is running on a desktop platform.
        /// </summary>
        public bool IsDesktop
        {
            get => _isDesktop;
            set
            {
                if (_isDesktop == value) return;
                _isDesktop = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDesktop)));
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the app is running on a mobile platform.
        /// </summary>
        public bool IsMobile
        {
            get => _isMobile;
            set
            {
                if (_isMobile == value) return;
                _isMobile = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMobile)));
            }
        }

        /// <summary>
        /// Gets or sets the pressure label, which is determined by AI analysis of today's tasks.
        /// </summary>
        public string PressureLabel
        {
            get => _pressureLabel;
            set
            {
                if (_pressureLabel != value)
                {
                    _pressureLabel = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PressureLabel)));
                }
            }
        }

        /// <summary>
        /// Gets or sets the tips text, which is determined by AI analysis of today's tasks.
        /// </summary>
        public string Tips
        {
            get => _tips;
            set
            {
                if (_tips == value) return;
                _tips = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tips)));
            }
        }

        /// <summary>
        /// Gets or sets the currently selected task in the UI.
        /// </summary>
        public TaskItem? SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (_selectedTask == value) return;
                _selectedTask = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTask)));

                // When a user selects a task, update the recommendation title and request mentorship analysis
                if (_selectedTask != null)
                {
                    var analysisVersion = ++_selectedTaskAnalysisVersion;
                    RecommendationTitle = _selectedTask.Topic;
                    MentorshipDetail = "Analyzing...";
                    UrgencyText = "Analyzing urgency...";
                    ImportanceText = "Analyzing importance...";
                    PositiveMomentumText = "Analyzing positive momentum...";
                    DelayRiskText = "Analyzing delay risk...";
                    SelectedTabIndex = InsightsTabIndex;
                    _ = AnalyzeMentorshipAsync(_selectedTask, analysisVersion);
                    _ = AnalyzeUrgencyAsync(_selectedTask, analysisVersion);
                    _ = AnalyzePositiveMomentumAsync(_selectedTask, analysisVersion);
                    _ = AnalyzeDelayRiskAsync(_selectedTask, analysisVersion);
                    _ = AnalyzeImportanceAsync(_selectedTask, analysisVersion);
                }
            }
        }

        /// <summary>
        /// Gets or sets the mentorship detail text, which is determined by AI analysis of the selected task and today's tasks.
        /// </summary>
        public string MentorshipDetail
        {
            get => _mentorshipDetail;
            set
            {
                if (_mentorshipDetail == value) return;
                _mentorshipDetail = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MentorshipDetail)));
            }
        }

        /// <summary>
        /// Gets or sets the urgency analysis text for the selected task.
        /// </summary>
        public string UrgencyText
        {
            get => _urgencyText;
            set
            {
                if (_urgencyText == value) return;
                _urgencyText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UrgencyText)));
            }
        }

        /// <summary>
        /// Gets or sets the importance analysis text for the selected task.
        /// </summary>
        public string ImportanceText
        {
            get => _importanceText;
            set
            {
                if (_importanceText == value) return;
                _importanceText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImportanceText)));
            }
        }

        /// <summary>
        /// Gets or sets the positive momentum analysis text for the selected task, which is determined by AI analysis of the benefits and downstream effects of completing the task.
        /// </summary>
        public string PositiveMomentumText
        {
            get => _positiveMomentumText;
            set
            {
                if (_positiveMomentumText == value) return;
                _positiveMomentumText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PositiveMomentumText)));
            }
        }

        /// <summary>
        /// Gets or sets the delay risk analysis text for the selected task.
        /// </summary>
        public string DelayRiskText
        {
            get => _delayRiskText;
            set
            {
                if (_delayRiskText == value) return;
                _delayRiskText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DelayRiskText)));
            }
        }

        /// <summary>
        /// Gets or sets the focus label, which is determined by AI analysis of today's tasks and the user's calendar to suggest an optimal focus block duration (e.g., "2H FOCUS").
        /// </summary>
        public string FocusLabel
        {
            get => _focusLabel;
            set
            {
                if (_focusLabel == value) return;
                _focusLabel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FocusLabel)));
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the add-task popup is currently visible. 
        /// </summary>
        public bool IsAddTaskPopupVisible
        {
            get => _isAddTaskPopupVisible;
            set
            {
                if (_isAddTaskPopupVisible == value) return;
                _isAddTaskPopupVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAddTaskPopupVisible)));
            }
        }

        /// <summary>
        /// Gets or sets the topic input for the add-task popup. 
        /// </summary>
        public string NewTaskTopic
        {
            get => _newTaskTopic;
            set
            {
                if (_newTaskTopic == value) return;
                _newTaskTopic = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewTaskTopic)));
            }
        }

        /// <summary>
        /// Gets or sets the description input for the add-task popup. 
        /// </summary>
        public string NewTaskDescription
        {
            get => _newTaskDescription;
            set
            {
                if (_newTaskDescription == value) return;
                _newTaskDescription = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewTaskDescription)));
            }
        }

        /// <summary>
        /// Gets or sets the deadline input for the add-task popup. 
        /// </summary>
        public DateTime NewTaskDeadline
        {
            get => _newTaskDeadline;
            set
            {
                if (_newTaskDeadline == value) return;
                _newTaskDeadline = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewTaskDeadline)));
            }
        }

        /// <summary>
        /// Gets or sets the project type input for the add-task popup.
        /// </summary>
        public string NewTaskProjectType
        {
            get => _newTaskProjectType;
            set
            {
                if (_newTaskProjectType == value) return;
                _newTaskProjectType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewTaskProjectType)));
            }
        }

        /// <summary>
        /// Gets or sets the priority input for the add-task popup.
        /// </summary>
        public string NewTaskPriority
        {
            get => _newTaskPriority;
            set
            {
                if (_newTaskPriority == value) return;
                _newTaskPriority = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewTaskPriority)));
            }
        }

        /// <summary>
        /// Gets or sets the recommendation title, which is typically set to the topic of the currently selected task or the top AI-recommended task.
        /// </summary>
        public string RecommendationTitle
        {
            get => _recommendationTitle;
            set
            {
                if (_recommendationTitle == value) return;
                _recommendationTitle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecommendationTitle)));
            }
        }

        /// <summary>
        /// Command to show the add-task popup.
        /// </summary>
        public ICommand ShowAddTaskCommand { get; private set; }

        /// <summary>
        /// Command to hide the add-task popup.
        /// </summary>
        public ICommand HideAddTaskCommand { get; private set; }

        /// <summary>
        /// Command to confirm adding a new task based on the inputs in the add-task popup.
        /// </summary>
        public ICommand ConfirmAddTaskCommand { get; private set; }

        /// <summary>
        /// Command to accept the AI recommendation for task prioritization.
        /// </summary>
        public ICommand AcceptRecommendationCommand { get; private set; }

        /// <summary>
        /// Command to postpone the AI recommendation for task prioritization.
        /// </summary>
        public ICommand PostponeRecommendationCommand { get; private set; }

        /// <summary>
        /// Method to seed initial tasks into the AllTasks collection.
        /// </summary>
        private void SeedTasks()
        {
            var today = DateTime.Today;
            AllTasks.Add(new TaskItem { Topic = "Style Guide Update", Description = "Can wait until Friday focus block", Deadline = today, ProjectType = "Design", Priority = "Low" });
            AllTasks.Add(new TaskItem { Topic = "Team Check-in", Description = "Crucial alignment for the upcoming sprint", Deadline = today, ProjectType = "Meeting", Priority = "High" });
            AllTasks.Add(new TaskItem { Topic = "Reply to Client Alpha", Description = "Expected response window closing soon", Deadline = today, ProjectType = "Email", Priority = "Medium" });
            AllTasks.Add(new TaskItem { Topic = "Design Audit", Description = "Collect examples and notes", Deadline = today, ProjectType = "Audit", Priority = "Medium" });
            AllTasks.Add(new TaskItem { Topic = "Contract Renewal", Description = "Check terms and pricing", Deadline = today, ProjectType = "Contract", Priority = "Medium" });

            AllTasks.Add(new TaskItem { Topic = "Quarterly Report", Description = "High impact on team goals", Deadline = today.AddDays(3), ProjectType = "Report", Priority = "Low" });
            AllTasks.Add(new TaskItem { Topic = "Inbox Cleanup", Description = "Capacity limit reached · Save your energy", Deadline = today.AddDays(4), ProjectType = "Cleanup", Priority = "High" });
            AllTasks.Add(new TaskItem { Topic = "Project X Feedback", Description = "Awaiting stakeholder input before next action.", Deadline = today.AddDays(6), ProjectType = "Feedback", Priority = "Medium" });

            AllTasks.Add(new TaskItem { Topic = "Onboarding Docs", Description = "Prepare for new hire next month", Deadline = today.AddDays(10), ProjectType = "Docs", Priority = "Low" });
            AllTasks.Add(new TaskItem { Topic = "Roadmap Review", Description = "Quarter planning workshop", Deadline = today.AddDays(14), ProjectType = "Review", Priority = "High" });      
        }

        /// <summary>
        /// Method to categorize tasks from the AllTasks collection into DoToday, Postpone, and Watch collections based on their deadlines.
        /// </summary>
        private void CategorizeTasks()
        {
            DoToday.Clear();
            Postpone.Clear();
            Watch.Clear();

            var today = DateTime.Today;

            foreach (var t in AllTasks.OrderBy(x => x.Deadline == DateTime.MaxValue ? DateTime.MaxValue : x.Deadline))
            {
                if (t.Deadline == DateTime.MaxValue)
                {
                    Watch.Add(t);
                }
                else if (t.Deadline.Date <= today)
                {
                    DoToday.Add(t);
                }
                else if (t.Deadline.Date <= today.AddDays(7))
                {
                    Postpone.Add(t);
                }
                else
                {
                    Watch.Add(t);
                }
            }

            // Collections notify their own changes; only the derived counters need a manual raise.
            RaiseCategoryChanged();
        }

        /// <summary>
        /// Raises change notification for the count and label properties that are derived from
        /// the Do Today, Postpone, and Watch collections.
        /// </summary>
        private void RaiseCategoryChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DoTodayCount)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PostponeCount)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WatchCount)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MeetingsCountLabel)));
        }

        /// <summary>
        /// Method executed when the user confirms adding a new task through the add-task popup.
        /// </summary>
        private void ExecuteConfirmAddTask(object? _)
        {
            var topic = (NewTaskTopic ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(topic))
            {
                // ignore empty topic
                return;
            }

            var newTask = new TaskItem
            {
                Topic = topic,
                Description = NewTaskDescription ?? string.Empty,
                Deadline = NewTaskDeadline,
                ProjectType = NewTaskProjectType ?? string.Empty,
                Priority = string.IsNullOrWhiteSpace(NewTaskPriority) ? "Medium" : NewTaskPriority
            };

            AllTasks.Add(newTask);
            CategorizeTasks();

            // The Do Today list changed, so pressure, recommendation, and focus are stale.
            // Only refresh when the credentials were confirmed valid during startup.
            if (_credentialsValidated)
                RefreshDailyAnalysis();

            // clear inputs and close
            NewTaskTopic = string.Empty;
            NewTaskDescription = string.Empty;
            NewTaskProjectType = string.Empty;
            NewTaskPriority = string.Empty;
            NewTaskDeadline = DateTime.Today;
            IsAddTaskPopupVisible = false;

        }

        /// <summary>
        /// Method to analyze the pressure of today's tasks using the AI service.
        /// </summary>
        /// <returns>The task.</returns>
        public async Task AnalyzePressureAsync()
        {
            try
            {
                var aiService = _aiService;
                // Build prompt with today's tasks
                var sb = new StringBuilder();
                sb.AppendLine("You are given a list of tasks. Analyze the complexity of each task and the overall pressure.");
                sb.AppendLine("Return ONLY valid JSON with this exact schema:");
                sb.AppendLine("{\"tasks\": [ { \"topic\": string, \"complexity\": \"Low|Medium|High\" } ], \"pressure\": \"Low Pressure|Medium Pressure|High Pressure\", \"tip\": string }");
                sb.AppendLine("Tasks:");
                foreach (var t in DoToday)
                {
                    sb.AppendLine($"- {t.Topic}: {t.Description}");
                }

                var prompt = sb.ToString();
                var result = await aiService.GetResultsFromAI(prompt);
                if (string.IsNullOrWhiteSpace(result))
                {
                    PressureLabel = "Medium Pressure"; // fallback
                    return;
                }

                // Try parse JSON from response
                try
                {
                    using var doc = JsonDocument.Parse(result);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("pressure", out var pressureEl))
                    {
                        var p = pressureEl.GetString();
                        if (!string.IsNullOrWhiteSpace(p)) PressureLabel = p;
                    }

                    if (root.TryGetProperty("tip", out var tipEl))
                    {
                        var tipText = tipEl.GetString() ?? string.Empty;
                        // ensure up to two lines
                        var lines = tipText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(l => l.Trim())
                                           .Where(l => !string.IsNullOrWhiteSpace(l))
                                           .Take(2)
                                           .ToArray();
                        Tips = string.Join("\n", lines);
                    }

                    if (root.TryGetProperty("tasks", out var tasksEl) && tasksEl.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in tasksEl.EnumerateArray())
                        {
                            if (item.TryGetProperty("topic", out var topicEl) && item.TryGetProperty("complexity", out var compEl))
                            {
                                var topic = topicEl.GetString();
                                var complexity = compEl.GetString();
                                if (!string.IsNullOrWhiteSpace(topic))
                                {
                                    var match = DoToday.FirstOrDefault(x => string.Equals(x.Topic, topic, StringComparison.OrdinalIgnoreCase));
                                    if (match != null)
                                    {
                                        match.Complexity = complexity ?? "Medium";
                                    }
                                }
                            }
                        }
                    }
                    return;
                }
                catch
                {
                    // fallback to keyword parsing
                    var lower = result.ToLowerInvariant();
                    if (lower.Contains("high pressure") || lower.Contains("high"))
                        PressureLabel = "High Pressure";
                    else if (lower.Contains("low pressure") || lower.Contains("low"))
                        PressureLabel = "Low Pressure";
                    else
                        PressureLabel = "Medium Pressure";
                }
            }
            catch
            {
                PressureLabel = "Medium Pressure";
            }
        }

        /// <summary>
        /// Method to analyze today's tasks and provide a recommendation for which task should be done first using the AI service.
        /// </summary>
        /// <returns>The task.</returns>
        public async Task AnalyzeRecommendationAsync()
        {
            try
            {
                var aiService = _aiService;
                if (DoToday == null || DoToday.Count == 0)
                {
                    RecommendationTitle = "—";
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine("You are given a list of tasks for today. For each task, analyze its complexity and timeline, then choose the single task that should be done first today.");
                sb.AppendLine("Return ONLY the task TOPIC (exactly as in the list) that should be done first today. Do not include any extra text, JSON, or explanation.");
                sb.AppendLine("Tasks:");
                foreach (var t in DoToday)
                {
                    sb.AppendLine($"- Topic: {t.Topic}");
                    sb.AppendLine($"  Description: {t.Description}");
                    sb.AppendLine($"  Deadline: {(t.Deadline == DateTime.MaxValue ? "No deadline" : t.Deadline.ToString("yyyy-MM-dd"))}");
                }

                var prompt = sb.ToString();
                var result = await aiService.GetResultsFromAI(prompt);
                if (string.IsNullOrWhiteSpace(result))
                {
                    RecommendationTitle = DoToday.FirstOrDefault()?.Topic ?? "—";
                    return;
                }

                // Expect AI to return a priority-ordered list of task topics, one per line (highest priority first)
                var cleaned = result.Trim();
                var lines = cleaned.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(l => l.Trim())
                                   .Where(l => !string.IsNullOrWhiteSpace(l))
                                   .Select(l =>
                                   {
                                       // remove leading bullets/numbers like "1. ", "- ", "• "
                                       var s = l;
                                       int i = 0;
                                       while (i < s.Length && !char.IsLetterOrDigit(s[i])) i++;
                                       if (i > 0 && i < s.Length) s = s.Substring(i).Trim();
                                       // also remove leading numbering like "1) " or "1."
                                       if (s.Length > 2 && char.IsDigit(s[0]) && (s[1] == '.' || s[1] == ')'))
                                           s = s.Substring(2).Trim();
                                       return s.Trim('"', '\'', ' ');
                                   })
                                   .ToList();

                // Map lines to TaskItem instances in DoToday preserving AI order
                var ordered = new List<TaskItem>();
                foreach (var line in lines)
                {
                    var match = DoToday.FirstOrDefault(x => string.Equals(x.Topic, line, StringComparison.OrdinalIgnoreCase))
                                ?? DoToday.FirstOrDefault(x => x.Topic.IndexOf(line, StringComparison.OrdinalIgnoreCase) >= 0)
                                ?? AllTasks.FirstOrDefault(x => string.Equals(x.Topic, line, StringComparison.OrdinalIgnoreCase));
                    if (match != null && !ordered.Contains(match)) ordered.Add(match);
                }

                // Append any remaining DoToday tasks that AI didn't list, preserving original order
                foreach (var t in DoToday)
                {
                    if (!ordered.Contains(t)) ordered.Add(t);
                }

                // Save AI ordered list but do NOT reorder the DoToday collection until user accepts
                _aiOrdered = ordered;
                _aiCurrentIndex = 0;

                // Set recommendation title as first in AI ordered list
                RecommendationTitle = _aiOrdered.FirstOrDefault()?.Topic ?? "—";
            }
            catch
            {
                RecommendationTitle = DoToday.FirstOrDefault()?.Topic ?? "—";
            }
        }

        /// <summary>
        /// Method to analyze the selected task in the context of today's tasks and provide mentorship advice using the AI service.
        /// </summary>
        /// <param name="task"></param>
        /// <returns>The task.</returns>
        public async Task AnalyzeMentorshipAsync(TaskItem task, int analysisVersion)
        {
            try
            {
                if (analysisVersion == _selectedTaskAnalysisVersion)
                {
                    MentorshipDetail = "Analyzing...";
                }
                var aiService = _aiService;
                if (task == null) { MentorshipDetail = string.Empty; return; }

                // Prepare the Do Today snapshot and ask for plain-text mentorship guidance.

                var sb = new StringBuilder();

                sb.AppendLine("You are a task management assistant.");
                sb.AppendLine("Analyze the selected task against the current DoToday list.");
                sb.AppendLine("If the task is already in DoToday, state: 'This task is already included in your DoToday list.'");
                sb.AppendLine("Compare its priority, deadline urgency, complexity, and dependencies with other DoToday tasks.");
                sb.AppendLine("Assume an 8-hour workday.");
                sb.AppendLine("For tasks from Postpone or Watch lists, recommend adding them to DoToday only if today's workload is within 8 hours and the task has sufficient priority or deadline urgency.");
                sb.AppendLine("Do not recommend adding the task if the existing DoToday workload already exceeds 8 hours.");
                sb.AppendLine("Return only plain text in 1-4 concise lines. Do not use JSON, bullets, brackets, quotes, or special formatting.");
                sb.AppendLine();
                sb.AppendLine("SelectedTask:");
                sb.AppendLine($"topic: {task.Topic}");
                sb.AppendLine($"description: {task.Description}");
                sb.AppendLine($"deadline: {(task.Deadline == DateTime.MaxValue ? "No deadline" : task.Deadline.ToString("yyyy-MM-dd"))}");
                sb.AppendLine($"complexity: {task.Complexity}");
                sb.AppendLine($"priority: {task.Priority}");
                sb.AppendLine();
                sb.AppendLine("DoToday:");
                foreach (var t in DoToday)
                {
                    sb.AppendLine($"- {t.Topic} | complexity: {t.Complexity} | priority: {t.Priority}");
                }

                var prompt = sb.ToString();
                var result = await aiService.GetResultsFromAI(prompt);

                if (analysisVersion != _selectedTaskAnalysisVersion)
                    return;

                if (string.IsNullOrWhiteSpace(result))
                {
                    MentorshipDetail = "No mentorship data available.";
                    return;
                }

                // The prompt requests plain text, so the whole response is the mentorship guidance.
                // Urgency and Importance are produced by their own analyzers and are not touched here.
                MentorshipDetail = CleanInsightText(result);
            }
            catch
            {
                if (analysisVersion != _selectedTaskAnalysisVersion)
                    return;

                MentorshipDetail = "Unable to get mentorship analysis.";
            }
        }

        /// <summary>
        /// Strips the decoration a chat model may add around a requested plain-text answer
        /// (markdown emphasis, surrounding quotes, or a fenced code block) and collapses it
        /// to at most <paramref name="maxLines"/> non-empty lines.
        /// </summary>
        private static string CleanInsightText(string result, int maxLines = 4)
        {
            var cleaned = result.Trim();

            if (cleaned.StartsWith("```", StringComparison.Ordinal))
            {
                var firstNewLine = cleaned.IndexOf('\n');
                var closingFence = cleaned.LastIndexOf("```", StringComparison.Ordinal);
                if (firstNewLine >= 0 && closingFence > firstNewLine)
                {
                    cleaned = cleaned.Substring(firstNewLine + 1, closingFence - firstNewLine - 1);
                }
                else
                {
                    cleaned = cleaned.Trim('`');
                }
            }

            cleaned = cleaned.Trim().Trim('"', '\'', ' ', '*');

            var lines = cleaned
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Take(maxLines);

            return string.Join("\n", lines).Trim();
        }

        /// <summary>
        /// Method to analyze the urgency of the selected task in the context of today's tasks and set the UrgencyText property with a 1-2 line summary of the urgency.
        /// </summary>
        /// <param name="task"></param>
        /// <returns>The task.</returns>
        public async Task AnalyzeUrgencyAsync(TaskItem task, int analysisVersion)
        {
            try
            {
                UrgencyText = "Analyzing urgency...";
                if (task == null) { UrgencyText = string.Empty; return; }
                var aiService = _aiService;

                var sb = new StringBuilder();
                sb.AppendLine("You are an assistant that summarizes the URGENCY of a single task in 1-2 short lines.");
                sb.AppendLine("Working hours per day = 8 hours. Consider today's DoToday tasks when deciding urgency (deadline proximity, priority, estimated effort).");
                sb.AppendLine("Return plain text only (1-2 short lines) describing urgency, do not include extra explanation.");
                sb.AppendLine("Do not return JSON, brackets, quotes, semicolons, or bullet points.");
                sb.AppendLine();
                sb.AppendLine($"Topic: {task.Topic}");
                sb.AppendLine($"Deadline: {(task.Deadline == DateTime.MaxValue ? "No deadline" : task.Deadline.ToString("yyyy-MM-dd"))}");
                sb.AppendLine($"Priority: {task.Priority}");
                sb.AppendLine($"Complexity: {task.Complexity}");
                sb.AppendLine();
                sb.AppendLine("DoToday summary:");
                foreach (var t in DoToday)
                {
                    sb.AppendLine($"- {t.Topic} | complexity: {t.Complexity} | priority: {t.Priority}");
                }

                var prompt = sb.ToString();
                var result = await aiService.GetResultsFromAI(prompt);

                if (analysisVersion != _selectedTaskAnalysisVersion)
                    return;

                if (string.IsNullOrWhiteSpace(result))
                {
                    UrgencyText = "";
                    return;
                }

                UrgencyText = CleanInsightText(result, maxLines: 2);
            }
            catch
            {
                if (analysisVersion != _selectedTaskAnalysisVersion)
                    return;

                UrgencyText = "Unable to get urgency details.";
            }
        }

        /// <summary>
        /// Method to analyze the importance of the selected task in the context of today's tasks and set the ImportanceText property with a 1-2 line summary of the importance.
        /// </summary>
        /// <param name="task"></param>
        /// <returns>The task.</returns>
        public async Task AnalyzeImportanceAsync(TaskItem task, int analysisVersion)
        {
            try
            {
                ImportanceText = "Analyzing importance...";
                if (task == null) { ImportanceText = string.Empty; return; }
                var aiService = _aiService;

                var sb = new StringBuilder();
                sb.AppendLine("You are an assistant that summarizes the IMPORTANCE of a single task in 1-2 short lines.");
                sb.AppendLine("Consider business impact, stakeholders, and how completing the task benefits the project or team.");
                sb.AppendLine("Return plain text only (1-2 short lines) describing importance, do not include extra explanation.");
                sb.AppendLine("Do not return JSON, brackets, quotes, semicolons, or bullet points.");
                sb.AppendLine();
                sb.AppendLine($"Topic: {task.Topic}");
                sb.AppendLine($"Description: {task.Description}");
                sb.AppendLine($"Priority: {task.Priority}");
                sb.AppendLine();
                sb.AppendLine("DoToday summary:");
                foreach (var t in DoToday)
                {
                    sb.AppendLine($"- {t.Topic} | complexity: {t.Complexity} | priority: {t.Priority}");
                }

                var prompt = sb.ToString();
                var result = await aiService.GetResultsFromAI(prompt);

                if (analysisVersion != _selectedTaskAnalysisVersion)
                    return;

                if (string.IsNullOrWhiteSpace(result))
                {
                    ImportanceText = "";
                    return;
                }

                ImportanceText = CleanInsightText(result, maxLines: 2);
            }
            catch
            {
                if (analysisVersion != _selectedTaskAnalysisVersion)
                    return;

                ImportanceText = "Unable to get importance details";
            } 
        }

        /// <summary>
        /// Method to analyze the positive momentum created by completing the selected task in the context of today's tasks and set the PositiveMomentumText property with a 1-2 line summary of the positive momentum.
        /// </summary>
        /// <param name="task"></param>
        /// <returns>The task.</returns>
        public async Task AnalyzePositiveMomentumAsync(TaskItem task, int analysisVersion)
        {
            try
            {
                PositiveMomentumText = "Analyzing positive momentum...";
                if (task == null) { PositiveMomentumText = string.Empty; return; }
                var aiService = _aiService;

                var sb = new StringBuilder();
                sb.AppendLine("You are an assistant that summarizes the POSITIVE MOMENTUM created by completing a single task in 1-2 short lines.");
                sb.AppendLine("Consider momentum, downstream unblocks, confidence, progress, and measurable benefits.");
                sb.AppendLine("Return plain text only (1-2 short lines) describing positive momentum, do not include extra explanation.");
                sb.AppendLine("Do not return JSON, brackets, quotes, semicolons, or bullet points.");
                sb.AppendLine();
                sb.AppendLine($"Topic: {task.Topic}");
                sb.AppendLine($"Description: {task.Description}");
                sb.AppendLine($"Priority: {task.Priority}");
                sb.AppendLine($"Complexity: {task.Complexity}");
                sb.AppendLine();
                sb.AppendLine("DoToday summary:");
                foreach (var t in DoToday)
                {
                    sb.AppendLine($"- {t.Topic} | complexity: {t.Complexity} | priority: {t.Priority}");
                }

                var prompt = sb.ToString();
                var result = await aiService.GetResultsFromAI(prompt);

                if (analysisVersion != _selectedTaskAnalysisVersion)
                    return;

                if (string.IsNullOrWhiteSpace(result))
                {
                    PositiveMomentumText = "";
                    return;
                }

                PositiveMomentumText = CleanInsightText(result, maxLines: 2);
            }
            catch
            {
                if (analysisVersion != _selectedTaskAnalysisVersion)
                    return;

                PositiveMomentumText = "Unable to get positive momentum details.";
            }
        }

        /// <summary>
        /// Method to analyze the delay risk of postponing the selected task in the context of today's tasks and set the DelayRiskText property with a 1-2 line summary of the delay risk.
        /// </summary>
        /// <param name="task"></param>
        /// <returns>The task.</returns>
        public async Task AnalyzeDelayRiskAsync(TaskItem task, int analysisVersion)
        {
            try
            {
                DelayRiskText = "Analyzing delay risk...";
                if (task == null) { DelayRiskText = string.Empty; return; }
                var aiService = _aiService;

                var sb = new StringBuilder();
                sb.AppendLine("You are an assistant that summarizes the DELAY RISK of postponing a single task in 1-2 short lines.");
                sb.AppendLine("Consider deadline slip, missed dependencies, context switching, and accumulated effort.");
                sb.AppendLine("Return plain text only (1-2 short lines) describing delay risk, do not include extra explanation.");
                sb.AppendLine("Do not return JSON, brackets, quotes, semicolons, or bullet points.");
                sb.AppendLine();
                sb.AppendLine($"Topic: {task.Topic}");
                sb.AppendLine($"Description: {task.Description}");
                sb.AppendLine($"Priority: {task.Priority}");
                sb.AppendLine($"Complexity: {task.Complexity}");
                sb.AppendLine();
                sb.AppendLine("DoToday summary:");
                foreach (var t in DoToday)
                {
                    sb.AppendLine($"- {t.Topic} | complexity: {t.Complexity} | priority: {t.Priority}");
                }

                var prompt = sb.ToString();
                var result = await aiService.GetResultsFromAI(prompt);

                if (analysisVersion != _selectedTaskAnalysisVersion)
                    return;

                if (string.IsNullOrWhiteSpace(result))
                {
                    DelayRiskText = "";
                    return;
                }

                DelayRiskText = CleanInsightText(result, maxLines: 2);
            }
            catch
            {
                if (analysisVersion != _selectedTaskAnalysisVersion)
                    return;

                DelayRiskText = "";
            }
        }

        /// <summary>
        /// Method to analyze the total focus time required to complete today's tasks using the AI service. 
        /// </summary>
        /// <returns>The task.</returns>
        public async Task AnalyzeFocusAsync()
        {
            try
            {
                if (DoToday == null || DoToday.Count == 0)
                {
                    FocusLabel = "0H FOCUS";
                    return;
                }

                FocusLabel = "Calculating...";
                var aiService = _aiService;

                var sb = new StringBuilder();
                sb.AppendLine("You are an assistant that estimates the total time required to complete the DoToday tasks. The total time should be calculated based on each task's complexity.");
                sb.AppendLine("Return a calculated time in hours and minutes (H/M)");
                sb.AppendLine();
                sb.AppendLine("Tasks:");
                foreach (var t in DoToday)
                {
                    sb.AppendLine($"- {t.Topic} | description: {t.Description}");
                }

                var prompt = sb.ToString();
                var result = await aiService.GetResultsFromAI(prompt);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    var cleaned = result.Trim();
                    // try to extract something like '2H' or '30m' from response
                    var m = Regex.Match(cleaned, @"\d+(?:\.\d+)?\s*(?:h|H)\b");
                    if (m.Success)
                    {
                        FocusLabel = m.Value.ToUpperInvariant().Trim() + " FOCUS";
                        return;
                    }
                    var m2 = Regex.Match(cleaned, @"\d+\s*(?:m|M)\b");
                    if (m2.Success)
                    {
                        FocusLabel = m2.Value.ToUpperInvariant().Trim() + " FOCUS";
                        return;
                    }

                    // If AI returned something like '2 hours' or 'two hours'
                    var hoursMatch = Regex.Match(cleaned, @"(\d+(?:\.\d+)?)\s*hours?", RegexOptions.IgnoreCase);
                    if (hoursMatch.Success)
                    {
                        if (double.TryParse(hoursMatch.Groups[1].Value, out var hrs))
                        {
                            var label = Math.Round(hrs * 2) / 2; // round to 0.5
                            FocusLabel = (label < 1 ? (label * 60).ToString("0") + "m FOCUS" : label.ToString("0.#") + "H FOCUS");
                            return;
                        }
                    }

                    // fallback: use whole AI response as label if short
                    if (cleaned.Length <= 12)
                    {
                        FocusLabel = cleaned;
                        return;
                    }
                }

                // Local heuristic fallback: estimate from complexities
                double totalMinutes = 0;
                foreach (var t in DoToday)
                {
                    var c = (t.Complexity ?? string.Empty).ToLowerInvariant();
                    if (c.Contains("low")) totalMinutes += 30;
                    else if (c.Contains("high")) totalMinutes += 120;
                    else totalMinutes += 60; // medium or unknown
                }

                // round to nearest 30 minutes
                var halfHours = Math.Round(totalMinutes / 30.0);
                var minutesRounded = halfHours * 30;
                if (minutesRounded < 60)
                {
                    FocusLabel = ((int)minutesRounded).ToString() + "m FOCUS";
                }
                else
                {
                    var hrs = minutesRounded / 60;
                    FocusLabel = ((int)hrs).ToString() + "H FOCUS";
                }
            }
            catch
            {
                FocusLabel = "2H FOCUS";
            }
        }

        /// <summary>
        /// Resolves the task the recommendation actions apply to: the user's current
        /// selection when present, otherwise the next task in the AI-ordered queue,
        /// otherwise a match on the current recommendation title.
        /// </summary>
        private TaskItem? ResolveRecommendationTarget()
        {
            if (SelectedTask != null)
                return SelectedTask;

            if (_aiOrdered.Count > _aiCurrentIndex)
                return _aiOrdered[_aiCurrentIndex];

            var topic = RecommendationTitle;
            if (string.IsNullOrWhiteSpace(topic))
                return null;

            return DoToday.FirstOrDefault(x => string.Equals(x.Topic, topic, StringComparison.OrdinalIgnoreCase))
                   ?? AllTasks.FirstOrDefault(x => string.Equals(x.Topic, topic, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Advances the AI-ordered queue past a handled task and republishes the next
        /// recommendation title, or clears it once the queue is exhausted.
        /// </summary>
        private void AdvanceToNextRecommendation(TaskItem item)
        {
            if (_aiCurrentIndex < _aiOrdered.Count && _aiOrdered[_aiCurrentIndex] == item)
                _aiCurrentIndex++;

            RecommendationTitle = _aiCurrentIndex < _aiOrdered.Count
                ? _aiOrdered[_aiCurrentIndex].Topic
                : "—";
        }

        /// <summary>
        /// Method to execute when user postpones the AI recommendation. The task moves out
        /// of Do Today (or Watch) to the top of Postpone and the app returns to the Today tab.
        /// </summary>
        private void ExecutePostponeRecommendation(object? _)
        {
            try
            {
                var item = ResolveRecommendationTarget();
                if (item == null) return;

                // Remove the task from whichever list currently holds it.
                if (DoToday.Remove(item))
                {
                    RaiseCategoryChanged();
                }
                else if (Watch.Remove(item))
                {
                    RaiseCategoryChanged();
                }

                // Move (rather than duplicate) into Postpone at the top.
                Postpone.Remove(item);
                Postpone.Insert(0, item);
                RaiseCategoryChanged();

                // Do Today may have shrunk, so the focus estimate is stale.
                _ = AnalyzeFocusAsync();

                AdvanceToNextRecommendation(item);
            }
            finally
            {
                // The decision has been recorded; show the user the updated Today tab.
                SelectedTabIndex = TodayTabIndex;
            }
        }

        /// <summary>
        /// Method to execute when user accepts the AI recommendation. The task moves to the top
        /// of Do Today (or is added to it when it came from Postpone or Watch) and the app
        /// returns to the Today tab.
        /// </summary>
        private void ExecuteAcceptRecommendation(object? _)
        {
            try
            {
                var item = ResolveRecommendationTarget();
                if (item == null) return;

                var movedFromOtherList = Postpone.Remove(item) || Watch.Remove(item);

                // Accepting promotes the task to the front of Do Today.
                DoToday.Remove(item);
                DoToday.Insert(0, item);

                RaiseCategoryChanged();

                // The focus estimate covers Do Today, so it is only stale when membership changed.
                if (movedFromOtherList)
                    _ = AnalyzeFocusAsync();

                AdvanceToNextRecommendation(item);
            }
            finally
            {
                // The decision has been recorded; show the user the updated Today tab.
                SelectedTabIndex = TodayTabIndex;
            }
        }

        /// <summary>
        /// This class implements the ICommand interface to create a relay command that can be used for binding UI actions to methods in the MainViewModel.
        /// </summary>
        private class RelayCommand : ICommand
        {
            /// <summary>
            /// Field to hold the action to be executed when the command is invoked.
            /// </summary>
            private readonly Action<object?> _execute;

            /// <summary>
            /// Field to hold the function that determines whether the command can be executed.
            /// </summary>
            private readonly Func<object?, bool>? _canExecute;

            /// <summary>
            /// Method to initialize a new instance of the RelayCommand class with the specified execute action and an optional canExecute function. 
            /// </summary>
            /// <param name="execute"></param>
            /// <param name="canExecute"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            /// <summary>
            /// Event that is triggered when the ability of the command to execute changes.
            /// </summary>
            public event EventHandler? CanExecuteChanged;

            /// <summary>
            /// Method to determine whether the command can be executed with the given parameter. 
            /// </summary>
            /// <param name="parameter"></param>
            /// <returns></returns>
            public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

            /// <summary>
            /// Method to execute the command's action with the given parameter. 
            /// </summary>
            /// <param name="parameter"></param>
            public void Execute(object? parameter) => _execute(parameter);

            /// <summary>
            /// Method to raise the CanExecuteChanged event, indicating that the ability of the command to execute may have changed. 
            /// </summary>
            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}