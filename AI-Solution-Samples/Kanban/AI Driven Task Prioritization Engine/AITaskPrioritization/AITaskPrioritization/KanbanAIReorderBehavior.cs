using System.Text.Json;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Kanban;

namespace AITaskPrioritization
{
    public class KanbanAIReorderBehavior : Behavior<ContentPage>
    {
        private SfButton? button;
        private Label? infoLabel;
        private Border? infoContainer;
        private SfKanban? kanban;
        private ViewModel? viewModel;
        private ContentPage? attachedPage;
        private readonly AzureOpenAIBaseService aiService = new();
        private const string DoneCategory = "Done";
        private const int AiTimeoutMs = 3000;

        protected override void OnAttachedTo(ContentPage bindable)
        {
            base.OnAttachedTo(bindable);
            attachedPage = bindable;

            bindable.BindingContextChanged += OnBindingContextChanged;
            viewModel = bindable.BindingContext as ViewModel;

            button = bindable.FindByName<SfButton>("aiSortButton");
            infoLabel = bindable.FindByName<Label>("infoLabel");
            infoContainer = bindable.FindByName<Border>("infoContainer");
            kanban = bindable.FindByName<SfKanban>("kanbanBoard");

            if (button != null)
            {
                button.Clicked += OnAISortClicked;
            }
        }

        private void OnBindingContextChanged(object? sender, EventArgs e)
        {
            viewModel = (sender as ContentPage)?.BindingContext as ViewModel;
        }

        private async void OnAISortClicked(object? sender, EventArgs e)
        {
            if (viewModel == null || button == null || infoLabel == null) return;

            button.IsEnabled = false;
            if (infoContainer != null) infoContainer.IsVisible = false;

            await ShowInfo("AI analyzing tasks...");

            if (!aiService.IsCredentialValid)
            {
                await StopUI("Invalid AI credentials");
                return;
            }

            if (kanban != null) await kanban.FadeToAsync(0.3, 150);

            var activeTasks = viewModel.Cards.Where(c => c.Category != DoneCategory).ToList();
            var doneTasks = viewModel.Cards.Where(c => c.Category == DoneCategory)
                                            .OrderBy(c => c.Index).ToList();

            string prompt = BuildPrompt(activeTasks);
            string result = await GetAIResponseWithTimeout(prompt);

            var finalList = ApplyAIResult(activeTasks, result);
            finalList.AddRange(doneTasks);

            viewModel.Cards.Clear();
            int index = 1;
            foreach (var card in finalList)
            {
                card.Index = index++;
                viewModel.Cards.Add(card);
            }

            var topTasks = viewModel.Cards
                .Where(c => c.Category != DoneCategory)
                .GroupBy(c => c.Category)
                .Select(g => g.First())
                .ToList();

            foreach (var task in topTasks)
            {
                _ = ApplyGlowEffect(task);
            }

            if (kanban != null)
            {
                kanban.TranslationY = 30;
                await Task.WhenAll(
                    kanban.FadeToAsync(1, 300),
                    kanban.TranslateToAsync(0, 0, 300, Easing.CubicIn));
            }

            await StopUI("✅ AI prioritization completed");
        }

        private async Task<string> GetAIResponseWithTimeout(string prompt)
        {
            try
            {
                var aiTask = aiService.GetAIResponse(prompt);
                var winner = await Task.WhenAny(aiTask, Task.Delay(AiTimeoutMs));
                return winner == aiTask ? await aiTask : string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AI request failed: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task ShowInfo(string message)
        {
            if (infoLabel == null) return;

            infoLabel.Text = message;
            if (infoContainer != null) infoContainer.IsVisible = true;

            infoLabel.Opacity = 0;
            await infoLabel.FadeToAsync(1, 150);
        }

        private string BuildPrompt(List<CardDetails> tasks)
        {
            string taskText = string.Join("\n", tasks.Select(t =>
                $"Title: {t.Title}, Description: {t.Description}, DueDate: {t.DueDate:yyyy-MM-dd}"));

            return $@"You are an AI task prioritization engine.
                Analyze the following tasks and determine priority.
                RULES:
                1. Tasks with nearer DueDate = higher priority
                2. If tasks have SAME DueDate:
                    - prioritize based on IMPACT (critical, backend, integrations)
                    - prioritize tasks affecting other tasks (DEPENDENCIES)
                3. Payment, API, security, core system = HIGH priority
                4. UI, cosmetic changes = LOW priority
                Return ONLY JSON array of Titles in correct order.
                Tasks:{taskText}";
        }

        private static List<CardDetails> ApplyAIResult(List<CardDetails> tasks, string aiResult)
        {
            if (string.IsNullOrWhiteSpace(aiResult))
                return tasks.OrderBy(t => t.DueDate).ToList();

            try
            {
                int start = aiResult.IndexOf('[');
                int end = aiResult.LastIndexOf(']');
                if (start < 0 || end < 0 || end <= start)
                    return tasks.OrderBy(t => t.DueDate).ToList();

                string json = aiResult.Substring(start, end - start + 1);
                var titles = JsonSerializer.Deserialize<List<string>>(json);
                return tasks.OrderBy(t => titles?.IndexOf(t.Title!) ?? int.MaxValue).ToList();
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AI JSON parse failed: {ex.Message}");
                return tasks.OrderBy(t => t.DueDate).ToList();
            }
        }

        private async Task StopUI(string message)
        {
            if (infoLabel != null)
            {
                infoLabel.Text = message;
                if (infoContainer != null) infoContainer.IsVisible = true;

                infoLabel.Opacity = 1;
                await Task.Delay(2000);
                await infoLabel.FadeToAsync(0, 300);
                if (infoContainer != null) infoContainer.IsVisible = false;
            }

            if (button != null) button.IsEnabled = true;
        }

        private async Task ApplyGlowEffect(CardDetails card)
        {
            try
            {
                card.UrgencyStrokeColor = Colors.PapayaWhip;
                card.StrokeThickness = 3;

                for (int i = 0; i < 3; i++)
                {
                    card.GlowOpacity = 0.6;
                    await Task.Delay(400);
                    card.GlowOpacity = 1;
                    await Task.Delay(200);
                }

                await Task.Delay(1200);
                card.StrokeThickness = 0;
                card.UrgencyStrokeColor = Colors.Gold;
                card.GlowOpacity = 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Glow effect failed: {ex.Message}");
            }
        }

        protected override void OnDetachingFrom(ContentPage bindable)
        {
            if (attachedPage != null)
            {
                attachedPage.BindingContextChanged -= OnBindingContextChanged;
                attachedPage = null;
            }

            if (button != null)
            {
                button.Clicked -= OnAISortClicked;
                button = null;
            }

            base.OnDetachingFrom(bindable);
        }
    }
}