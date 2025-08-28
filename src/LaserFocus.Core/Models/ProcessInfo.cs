using System.ComponentModel;

namespace LaserFocus.Core.Models
{
    /// <summary>
    /// Represents information about a running process with change notification support
    /// </summary>
    public class ProcessInfo : INotifyPropertyChanged
    {
        private string _processName = string.Empty;
        private int _id;
        private string _status = string.Empty;
        private string _statusColor = "Black";
        private DateTime _lastSeen;

        /// <summary>
        /// Name of the process
        /// </summary>
        public string ProcessName
        {
            get => _processName;
            set
            {
                if (_processName != value)
                {
                    _processName = value;
                    OnPropertyChanged(nameof(ProcessName));
                }
            }
        }

        /// <summary>
        /// Process ID
        /// </summary>
        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        /// <summary>
        /// Current status of the process (Allowed/Blocked)
        /// </summary>
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                    
                    // Update status color based on status
                    StatusColor = value.ToLower() switch
                    {
                        "allowed" => "Green",
                        "blocked" => "Red",
                        _ => "Black"
                    };
                }
            }
        }

        /// <summary>
        /// Color name to display the status (Green for allowed, Red for blocked)
        /// </summary>
        public string StatusColor
        {
            get => _statusColor;
            set
            {
                if (_statusColor != value)
                {
                    _statusColor = value;
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }

        /// <summary>
        /// Timestamp when the process was last seen
        /// </summary>
        public DateTime LastSeen
        {
            get => _lastSeen;
            set
            {
                if (_lastSeen != value)
                {
                    _lastSeen = value;
                    OnPropertyChanged(nameof(LastSeen));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}