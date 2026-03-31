using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics;

namespace ArticleGenerationSample
{
    /// <summary>
    /// Service for Azure AI operations.
    /// Provides online (Semantic Kernel) and offline (seeded) responses.
    /// </summary>
    /// <summary>
    /// Service for Azure AI operations leveraging Semantic Kernel. Provides online responses
    /// and falls back to deterministic offline content when credentials are not configured.
    /// </summary>
    public class AzureAIService : AzureBaseService, IAzureAIService
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAIService"/> class and prepares the client.
        /// </summary>
        public AzureAIService()
        {
            InitializeClient();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reinitializes the chat history and primes the system message when credentials are valid.
        /// </summary>
        public void InitializeClient()
        {
            if (IsCredentialValid && ChatHistory != null)
            {
                ChatHistory.Clear();
                ChatHistory.AddSystemMessage("You are a helpful, intelligent and conversational assistant that can assit with a wide variety of topics.");
            }
        }

        /// <summary>
        /// Gets results from AI; falls back to offline seeded responses when the online call fails or is unavailable.
        /// </summary>
        /// <param name="userPrompt">Original user prompt.</param>
        /// <param name="userAIPrompt">System/user prompt used to guide the AI.</param>
        /// <returns>HTML string to render.</returns>
        public async Task<string> GetResultsFromAI(string userPrompt, string userAIPrompt)
        {
            if (ChatCompletions != null && ChatHistory != null)
            {
                try
                {
                    if (ChatHistory.Count > 5)
                    {
                        // Remove the message history to avoid exceeding the token limit
                        ChatHistory.RemoveRange(0, 2);
                    }

                    ChatHistory.AddUserMessage(userAIPrompt);

                    var response = await ChatCompletions.GetChatMessageContentAsync(chatHistory: ChatHistory, kernel: Kernel);
                    return response.ToString();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex);
                    var response = this.GetSolutionToPrompt(userPrompt);
                    return response;
                }
            }
            else
            {
                var response = this.GetSolutionToPrompt(userPrompt);
                return response;
            }
        }

        #region Offline Data generation

        /// <summary>
        /// Returns a seeded HTML response for known prompts when offline.
        /// </summary>
        internal string GetSolutionToPrompt(string prompt)
        {
            prompt = prompt.ToLower();
            for (int i = 0; i < promptRequest.Length; i++)
            {
                if (prompt.Equals(promptRequest[i]))
                {
                    return promptResponseHtml[i];
                }
            }
            return "Please connect to your preferred AI service for real-time queries.";
        }

        #endregion

        #endregion

        #region Prompts

        /// <summary>
        /// Known prompts used for offline fallback responses.
        /// </summary>
        private readonly string[] promptRequest =
        {
           // General Topics
           "how to improve team collaboration at work?",
           "why is time management important?",
           "give me tips for writing engaging blog posts",
           "help me draft article content for syncfusion maui listview",
        };

        /// <summary>
        /// HTML responses aligned by index with <see cref="promptRequest"/>.
        /// </summary>
        private readonly string[] promptResponseHtml =
        {
            // How to improve team collaboration at work?
            "<h2>Improve Team Collaboration at Work</h2>" +
            "<p>Strong collaboration turns individual effort into collective impact. High‑performing teams share context openly, align on outcomes, and use lightweight rituals to stay coordinated. The result is faster delivery, fewer handoffs, and a culture where people feel trusted and informed.</p>" +
            "<h3>Foundation: Shared Context and Clear Ownership</h3>" +
            "<ul>" +
            "<li><b>North Star:</b> Document the team’s mission, current goals, and success metrics in a visible place (wiki, dashboard, or project brief).</li>" +
            "<li><b>Ownership Map:</b> Keep a simple RACI (Responsible, Accountable, Consulted, Informed) for recurring work so decisions don’t stall.</li>" +
            "<li><b>Working Agreements:</b> Define response times, meeting etiquette, and how to raise risks to reduce friction.</li>" +
            "</ul>" +
            "<h3>Communication that Scales</h3>" +
            "<ul>" +
            "<li><b>Asynchronous first:</b> Prefer written updates and decision logs. Reserve meetings for alignment or decisions.</li>" +
            "<li><b>Rituals:</b> Weekly planning, short daily checkpoints, and monthly retros keep momentum without overload.</li>" +
            "<li><b>Transparent tooling:</b> Use shared boards (Azure Boards/Jira), chat channels (Teams/Slack), and a single source of truth for docs.</li>" +
            "</ul>" +
            "<h3>Collaborative Delivery</h3>" +
            "<ul>" +
            "<li><b>Small batches:</b> Break work into thin slices to reduce dependencies and enable quicker reviews.</li>" +
            "<li><b>Pairing/Mobbing:</b> Use focused co‑working for complex tasks to transfer knowledge and reduce rework.</li>" +
            "<li><b>Feedback loops:</b> Demo early, test with users, and capture learnings in a short retro.</li>" +
            "</ul>" +
            "<h3>Example Weekly Rhythm</h3>" +
            "<ol>" +
            "<li>Mon: 30‑min planning; update goals and priorities.</li>" +
            "<li>Tue–Thu: Async status in chat; ad‑hoc pairing sessions.</li>" +
            "<li>Fri: 20‑min demo + 20‑min retro; log decisions and improvements.</li>" +
            "</ol>" +
            "<h3>Quick Wins</h3>" +
            "<ul>" +
            "<li>Create a single team home (README/wiki) with goals, roles, and active work.</li>" +
            "<li>Adopt a 24‑hour SLA for message responses and a 2‑day SLA for PR reviews.</li>" +
            "<li>Celebrate small wins weekly to reinforce positive behaviors.</li>" +
            "</ul>" +
            "<p><i>Outcome:</i> Clear priorities, fewer blockers, and a team that ships value predictably.</p>",

            // Why is time management important?
            "<h2>Why Time Management Matters</h2>" +
            "<p>Time is a fixed resource; how you allocate it determines output quality and personal wellbeing. Effective time management reduces context switching, increases focus on high‑leverage work, and creates space for rest and learning.</p>" +
            "<h3>Core Benefits</h3>" +
            "<ul>" +
            "<li><b>Higher throughput:</b> Prioritizing the vital few tasks (80/20) yields disproportionate results.</li>" +
            "<li><b>Lower stress:</b> Planned work and predictable routines reduce last‑minute crunch.</li>" +
            "<li><b>Better decisions:</b> Clear schedules create mental bandwidth for strategic thinking.</li>" +
            "<li><b>Work‑life balance:</b> Boundaries protect time for health, relationships, and recovery.</li>" +
            "</ul>" +
            "<h3>Practical System (45‑minute focus blocks)</h3>" +
            "<ol>" +
            "<li>Define 3 outcomes for the day. If everything slips, these still move the needle.</li>" +
            "<li>Work in 45‑minute focus blocks with a 10‑minute break; stack 3 blocks per theme.</li>" +
            "<li>Batch shallow work (email, approvals) into 1–2 windows to avoid constant interruption.</li>" +
            "<li>Review at day’s end: What worked? What will I change tomorrow?</li>" +
            "</ol>" +
            "<h3>Tools and Tips</h3>" +
            "<ul>" +
            "<li>Use a kanban board for visibility and WIP (work‑in‑progress) limits.</li>" +
            "<li>Schedule deep work on your calendar; treat it like an unmovable meeting.</li>" +
            "<li>Turn off non‑critical notifications during focus blocks.</li>" +
            "<li>Keep a ‘parking lot’ note for ideas and tasks that appear mid‑focus.</li>" +
            "</ul>" +
            "<p><i>Bottom line:</i> Manage attention, not just time. A simple, repeatable system compounds into meaningful results.</p>",

            // Tips for writing engaging blog posts
            "<h2>Tips for Writing Engaging Blog Posts</h2>" +
            "<p>Great posts are clear, useful, and memorable. They help a specific reader solve a real problem and make the next step obvious. Use the structure below to increase readability and engagement.</p>" +
            "<h3>1) Start with a Compelling Hook</h3>" +
            "<ul>" +
            "<li>Open with a surprising stat, short story, or strong promise that addresses the reader’s pain.</li>" +
            "<li>In the intro, state who the post is for and what they will take away.</li>" +
            "</ul>" +
            "<h3>2) Organize for Skimmability</h3>" +
            "<ul>" +
            "<li>Use H2/H3 headings, short paragraphs (2–4 lines), and bulleted lists.</li>" +
            "<li>Add callouts for tips, warnings, and examples to guide the eye.</li>" +
            "</ul>" +
            "<h3>3) Make It Practical</h3>" +
            "<ul>" +
            "<li>Provide step‑by‑step instructions, checklists, or templates readers can copy.</li>" +
            "<li>Include simple code or screenshots when explaining technical steps.</li>" +
            "</ul>" +
            "<h3>4) Build Trust</h3>" +
            "<ul>" +
            "<li>Cite credible sources and link to deeper references.</li>" +
            "<li>Share small real‑world examples or lessons learned to add authenticity.</li>" +
            "</ul>" +
            "<h3>Simple Outline Template</h3>" +
            "<ol>" +
            "<li>Hook and promise (what you’ll learn)</li>" +
            "<li>Context (why this matters now)</li>" +
            "<li>Steps or tactics (3–7 sections)</li>" +
            "<li>Common mistakes and fixes</li>" +
            "<li>Conclusion with a clear CTA</li>" +
            "</ol>" +
            "<p><i>Pro tip:</i> End with a single action—subscribe, try a checklist, or explore a related post—to convert interest into momentum.</p>",

            // Syncfusion MAUI ListView
            "<b>Syncfusion MAUI ListView</b><br>The Syncfusion MAUI ListView is a powerful and versatile control designed for .NET MAUI (Multi-platform App UI) applications. It allows developers to display and manage collections of data in a highly customizable and efficient manner. This control is particularly useful for creating mobile and desktop applications that require a rich user interface.<br><br><b>Key Features</b><br><ol><li><b>Data Binding:</b> The ListView supports data binding, allowing developers to easily connect the control to various data sources, such as collections, databases, or web services. This feature simplifies the process of displaying dynamic data.</li><li><b>Customizable Item Templates:</b> Developers can define custom item templates to control the appearance of each item in the ListView. This flexibility enables the creation of visually appealing and user-friendly interfaces.</li><li><b>Selection Modes:</b> The ListView supports multiple selection modes, including single selection, multiple selection, and none. This feature is essential for applications that require user interaction with list items.</li><li><b>Sorting and Grouping:</b> The ListView allows for sorting and grouping of data, making it easier for users to navigate through large datasets. Developers can implement custom sorting logic and group items based on specific criteria.</li><li><b>Virtualization:</b> To enhance performance, the ListView employs virtualization techniques. This means that only the visible items are rendered, reducing memory usage and improving scrolling performance, especially with large datasets.</li><li><b>Built-in Features:</b> The control comes with several built-in features, such as:<ul><li>Swipe Actions: Users can swipe list items to reveal additional actions.</li><li>Pull-to-Refresh: A common feature in mobile applications that allows users to refresh the data by pulling down on the list.</li><li>Load More: A feature that enables loading additional data as the user scrolls down the list.</li></ul></li></ol>",
        };

        #endregion
    }
}
