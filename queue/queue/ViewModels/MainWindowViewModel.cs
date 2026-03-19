using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using queue.Models;

namespace queue.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly Queue<string> _queue;
        private string _newItem = string.Empty;
        private string _currentItem = string.Empty;
        private string _statusMessage = string.Empty;

        public MainWindowViewModel()
        {
            _queue = new Queue<string>();
            Items = new ObservableCollection<string>();

            AddCommand = new RelayCommand(AddItem, CanAddItem);
            RemoveCommand = new RelayCommand(RemoveItem, CanRemoveItem);
            ClearCommand = new RelayCommand(ClearQueue, CanClearQueue);
        }

        public ObservableCollection<string> Items { get; }

        public string NewItem
        {
            get => _newItem;
            set
            {
                _newItem = value ?? string.Empty;
                OnPropertyChanged();
                AddCommand.RaiseCanExecuteChanged();
            }
        }

        public string CurrentItem
        {
            get => _currentItem;
            private set
            {
                _currentItem = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public int Count => _queue.Count;
        public bool IsEmpty => _queue.IsEmpty;

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand RemoveCommand { get; }
        public RelayCommand ClearCommand { get; }

        private void AddItem(object? parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewItem))
                {
                    StatusMessage = "Введите текст для добавления";
                    return;
                }

                _queue.Enqueue(NewItem);
                UpdateItems();
                StatusMessage = $"Элемент '{NewItem}' добавлен в очередь";
                NewItem = string.Empty;

                // Обновляем состояние команд
                RemoveCommand.RaiseCanExecuteChanged();
                ClearCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
        }

        private bool CanAddItem(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(NewItem);
        }

        private void RemoveItem(object? parameter)
        {
            try
            {
                if (_queue.TryDequeue(out string? removedItem))
                {
                    UpdateItems();
                    StatusMessage = $"Элемент '{removedItem}' удален из очереди";
                }
                else
                {
                    StatusMessage = "Очередь пуста";
                }

                // Обновляем состояние команд
                RemoveCommand.RaiseCanExecuteChanged();
                ClearCommand.RaiseCanExecuteChanged();
                AddCommand.RaiseCanExecuteChanged(); // На случай если NewItem изменился
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
        }

        private bool CanRemoveItem(object? parameter)
        {
            return !_queue.IsEmpty;
        }

        private void ClearQueue(object? parameter)
        {
            try
            {
                _queue.Clear();
                UpdateItems();
                StatusMessage = "Очередь очищена";

                // Обновляем состояние команд
                RemoveCommand.RaiseCanExecuteChanged();
                ClearCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
        }

        private bool CanClearQueue(object? parameter)
        {
            return !_queue.IsEmpty;
        }

        private void UpdateItems()
        {
            Items.Clear();
            foreach (var item in _queue.GetAllItems())
            {
                Items.Add(item);
            }

            // Безопасное получение текущего элемента
            if (_queue.TryGetCurrentItem(out string? current))
            {
                CurrentItem = current;
            }
            else
            {
                CurrentItem = string.Empty;
            }

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(IsEmpty));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}