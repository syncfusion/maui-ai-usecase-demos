using System.Collections.ObjectModel;

namespace AITaskPrioritization
{
    public class ViewModel
    {
        public ObservableCollection<CardDetails> Cards { get; set; }

        public ViewModel()
        {
            Cards = GetCardDetails();
        }

        private ObservableCollection<CardDetails> GetCardDetails()
        {
            var today = DateTime.Today;

            return new ObservableCollection<CardDetails>
            {
                // OPEN
                new CardDetails { Name = "Bella", Index=1, Title="Refactor Auth Service", Category="Open", Progress=0, Description="Refactor authentication service to support token refresh and improve modularity.", Image="bella.png", DueDate=today.AddDays(30) },
                new CardDetails { Name = "Olivia", Index=2, Title="Redesign Login UI", Category="Open", Progress=0, Description="Implement responsive login screen with validation and error handling.", Image="olivia.png", DueDate=today.AddDays(10) },
                new CardDetails { Name = "Nancy", Index=3, Title="Write API Docs", Category="Open", Progress=0, Description="Create OpenAPI/Swagger documentation for all public endpoints.", Image="nancy.png", DueDate=today.AddDays(15) },
                new CardDetails { Name = "Andrea", Index=4, Title="Build Onboarding Flow", Category="Open", Progress=0, Description="Develop onboarding screens with navigation and state persistence.", Image="andrea.png", DueDate=today.AddDays(20) },
                new CardDetails { Name = "Esther", Index=5, Title="Implement Dark Mode", Category="Open", Progress=0, Description="Add theme switching support and update styles across all screens.", Image="esther.png", DueDate=today.AddDays(18) },

                // IN PROGRESS
                new CardDetails { Name = "Zara", Index=6, Title="Integrate Payments", Category="In Progress", Progress=0.5, Description="Integrate Razorpay SDK and handle success/failure callbacks securely.", Image="zara.png", DueDate=today.AddDays(7) },
                new CardDetails { Name = "David", Index=7, Title="Setup Push Service", Category="In Progress", Progress=0.5, Description="Configure Firebase Cloud Messaging for push notifications.", Image="david.png", DueDate=today.AddDays(5) },
                new CardDetails { Name = "Katie", Index=8, Title="Implement Chat Module", Category="In Progress", Progress=0.4, Description="Develop real-time chat using WebSockets and message persistence.", Image="katie.png", DueDate=today.AddDays(5) },
                new CardDetails { Name = "Sherin", Index=9, Title="Build Analytics API", Category="In Progress", Progress=0.6, Description="Create backend APIs for analytics dashboard with aggregation logic.", Image="sherin.png", DueDate=today.AddDays(9) },
                new CardDetails { Name = "Janet", Index=10, Title="Enable File Upload", Category="In Progress", Progress=0.3, Description="Implement file upload with validation and cloud storage integration.", Image="janet.png", DueDate=today.AddDays(8) },

                // CODE REVIEW 
                new CardDetails { Name = "Edward", Index=11, Title="Test Kanban DragDrop", Category="Code Review", Progress=0.8, Description="Validate drag-and-drop functionality and state synchronization.", Image="edward.png", DueDate=today.AddDays(3) },
                new CardDetails { Name = "Janet", Index=12, Title="Optimize DB Queries", Category="Code Review", Progress=0.8, Description="Review and optimize SQL queries and indexing for performance.", Image="janet.png", DueDate=today.AddDays(4) },
                new CardDetails { Name = "Jenny", Index=13, Title="Review Security Issues", Category="Code Review", Progress=0.7, Description="Audit authentication flows and fix identified vulnerabilities.", Image="jenny.png", DueDate=today.AddDays(2) },
                new CardDetails { Name = "Zara", Index=14, Title="Validate API Contracts", Category="Code Review", Progress=0.75, Description="Ensure API requests/responses match contract and handle edge cases.", Image="zara.png", DueDate=today.AddDays(3) },
                new CardDetails { Name = "Liza", Index=15, Title="Fix UI Defects", Category="Code Review", Progress=0.85, Description="Resolve layout, responsiveness, and styling issues across screens.", Image="liza.png", DueDate=today.AddDays(1) },

                // DONE 
                new CardDetails { Name = "Esther", Index=16, Title="Complete Auth Module", Category="Done", Progress=1, Description="Implemented JWT authentication and role-based access control.", Image="esther.png", DueDate=today.AddDays(-2) },
                new CardDetails { Name = "Olivia", Index=17, Title="Finish Profile UI", Category="Done", Progress=1, Description="Built responsive profile page with editable fields and validation.", Image="olivia.png", DueDate=today.AddDays(-1) },
                new CardDetails { Name = "Rose", Index=18, Title="Implement Settings Page", Category="Done", Progress=1, Description="Added user settings including preferences and account controls.", Image="rose.png", DueDate=today.AddDays(-3) },
                new CardDetails { Name = "Andrea", Index=19, Title="Develop Search Feature", Category="Done", Progress=1, Description="Implemented search with filtering and pagination support.", Image="andrea.png", DueDate=today.AddDays(-4) },
                new CardDetails { Name = "Nancy", Index=20, Title="Setup Navigation", Category="Done", Progress=1, Description="Configured app routing and navigation flows across modules.", Image="nancy.png", DueDate=today.AddDays(-5) }
            };

        }
    }
}