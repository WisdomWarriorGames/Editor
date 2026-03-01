using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.Core.ShadowTree;

namespace WisdomWarrior.Editor.SceneList.ViewModels;

public partial class SceneHierarchyViewModel : ObservableObject
{
    private readonly SceneTracker _sceneTracker;

    public ObservableCollection<EntityViewModel> RootEntities { get; } = new();

    public SceneHierarchyViewModel(SceneTracker sceneTracker)
    {
        _sceneTracker = sceneTracker;
        _sceneTracker.OnSceneModified += OnSceneModified;

        SyncRootEntities();
    }

    private void OnSceneModified()
    {
        Dispatcher.UIThread.Post(SyncRootEntities);
    }

    private void SyncRootEntities()
    {
        var currentTrackers = _sceneTracker.TrackedRoots;

        for (int i = 0; i < currentTrackers.Count; i++)
        {
            var rootTracker = currentTrackers[i];
            var existingVm = RootEntities.FirstOrDefault(vm => vm.Tracker == rootTracker);

            if (existingVm != null)
            {
                int oldIndex = RootEntities.IndexOf(existingVm);
                if (oldIndex != i)
                {
                    RootEntities.Move(oldIndex, i);
                }
            }
            else
            {
                RootEntities.Insert(i, new EntityViewModel(rootTracker));
            }
        }

        while (RootEntities.Count > currentTrackers.Count)
        {
            RootEntities.RemoveAt(RootEntities.Count - 1);
        }
    }
}