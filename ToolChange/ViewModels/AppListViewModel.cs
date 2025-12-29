using Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using ToolChange.Models;
using ToolChange.Services;

namespace ToolChange.ViewModels
{
    public class AppListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AppItem> ListA { get; set; } = new ObservableCollection<AppItem>();
        public ObservableCollection<AppItem> ListB { get; set; } = new ObservableCollection<AppItem>();


        public ICommand MoveToBCommand { get; }
        public ICommand MoveToACommand { get; }
        public ICommand OKCommand { get; }
        public ICommand CancelCommand { get; }

        public Action<bool> CloseAction { get; set; }
        public AppListViewModel(Models.DeviceModel device)
        {

            OKCommand = new RelayCommandCD(o =>
            {
                TempMemoryStorage.CachedListB = ListB.ToList();
                CloseAction?.Invoke(true);
            });

            CancelCommand = new RelayCommandCD(o => CloseAction?.Invoke(false));
            MoveToBCommand = new RelayCommand<AppItem>(MoveToB);
            MoveToACommand = new RelayCommand<AppItem>(MoveToA);

            Task.Run(() => LoadPackage(device));
        }
        private async Task LoadPackage(Models.DeviceModel device)
        {
            var packages = await ADBService.GetUserInstalledAppsAsync(device.DeviceId);

            Debug.WriteLine($"Loaded packages: {packages.Count}");

            foreach (var (packageName, appLabel) in packages)
            {
                if (TempMemoryStorage.CachedListB.Any(x => x.Package == packageName))
                    continue;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ListA.Add(new AppItem
                    {
                        Package = packageName,
                        Name = appLabel
                    });
                });
            }

            foreach (var item in TempMemoryStorage.CachedListB)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ListB.Add(item);
                });
            }
        }

        private void MoveToB(AppItem item)
        {
            if (item != null && ListA.Contains(item))
            {
                ListA.Remove(item);
                ListB.Add(item);
            }
        }

        private void MoveToA(AppItem item)
        {
            if (item != null && ListB.Contains(item))
            {
                ListB.Remove(item);
                ListA.Add(item);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
