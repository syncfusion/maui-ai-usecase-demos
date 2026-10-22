using System;
using System.ComponentModel;

namespace SmartDailyDecisionCompanion
{
    /// <summary>
    /// This class represents a task item with various properties such as topic, description, deadline, project type, complexity, and priority.
    /// </summary>
    public class TaskItem : INotifyPropertyChanged
    {
        /// <summary>
        /// Event that is triggered whenever a property of the TaskItem changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Field to hold the unique identifier for the task.
        /// </summary>
        private string _topic = string.Empty;

        /// <summary>
        /// Field to hold the topic or title of the task.
        /// </summary>
        public string Topic { get => _topic; set { if (_topic == value) return; _topic = value; OnPropertyChanged(nameof(Topic)); } }

        /// <summary>
        /// Field to hold a more detailed description of the task.
        /// </summary>
        private string _description = string.Empty;

        /// <summary>
        /// Gets or sets the description of the task. 
        /// </summary>
        public string Description { get => _description; set { if (_description == value) return; _description = value; OnPropertyChanged(nameof(Description)); } }

        /// <summary>
        /// Get or set the deadline for the task. 
        /// </summary>
        private DateTime _deadline = DateTime.Now;

        /// <summary>
        /// Gets or sets the deadline for the task.
        /// </summary>
        public DateTime Deadline { get => _deadline; set { if (_deadline == value) return; _deadline = value; OnPropertyChanged(nameof(Deadline)); } }

        /// <summary>
        /// Get or set the project type for the task.
        /// </summary>
        private string _projectType = string.Empty;

        /// <summary>
        /// Gets or sets the project type for the task. 
        /// </summary>
        public string ProjectType { get => _projectType; set { if (_projectType == value) return; _projectType = value; OnPropertyChanged(nameof(ProjectType)); } }

        /// <summary>
        /// Get or set the complexity level for the task.
        /// </summary>
        private string _complexity = string.Empty;

        /// <summary>
        /// Gets or sets the complexity level for the task.
        /// </summary>
        public string Complexity { get => _complexity; set { if (_complexity == value) return; _complexity = value; OnPropertyChanged(nameof(Complexity)); } }

        /// <summary>
        /// Get or set the priority level for the task.
        /// </summary>
        private string _priority = string.Empty;

        /// <summary>
        /// Gets or sets the priority level for the task.
        /// </summary>
        public string Priority { get => _priority; set { if (_priority == value) return; _priority = value; OnPropertyChanged(nameof(Priority)); } }

        /// <summary>
        /// Protected method to raise the PropertyChanged event whenever a property value changes.
        /// </summary>
        /// <param name="name"></param>
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
