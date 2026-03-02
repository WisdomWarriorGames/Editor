using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.SceneList.Helpers;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.SceneList.ViewModels;

public partial class SceneHierarchyViewModel : ObservableObject
{
    private readonly SceneTracker _sceneTracker;
    private readonly CurrentSceneManager _sceneManager;

    public ObservableCollection<SceneNodeViewModel> Scenes { get; } = new();

    private SceneNodeViewModel _activeSceneNode;

    public SceneHierarchyViewModel(CurrentSceneManager sceneManager)
    {
        _sceneManager = sceneManager;
        _sceneTracker = _sceneManager.Tracker;

        _sceneTracker.OnSceneModified += OnSceneModified;
        _sceneManager.CurrentSceneReady += OnSceneReady;
    }

    private void OnSceneReady()
    {
        _activeSceneNode = new SceneNodeViewModel(_sceneTracker);
        Scenes.Add(_activeSceneNode);
        Dispatcher.UIThread.Post(SyncRootEntities);
    }

    private void OnSceneModified()
    {
        Dispatcher.UIThread.Post(SyncRootEntities);
    }

    private void SyncRootEntities()
    {
        _activeSceneNode.SyncName();
        var currentTrackers = _sceneTracker.TrackedRoots;
        var rootList = _activeSceneNode.Children;

        for (int i = 0; i < currentTrackers.Count; i++)
        {
            var rootTracker = currentTrackers[i];
            var existingVm = rootList.FirstOrDefault(vm => vm.Tracker == rootTracker);

            if (existingVm != null)
            {
                int oldIndex = rootList.IndexOf(existingVm);
                if (oldIndex != i) rootList.Move(oldIndex, i);
            }
            else
            {
                rootList.Insert(i, new EntityViewModel(rootTracker, _sceneTracker));
            }
        }

        while (rootList.Count > currentTrackers.Count)
        {
            rootList.RemoveAt(rootList.Count - 1);
        }
    }

    public void ResetChanges()
    {
        foreach (var scene in Scenes)
        {
            scene.CommitEdit();
            scene.IsSelected = false;

            foreach (var entity in scene.Children)
            {
                ResetEntityRecursive(entity);
            }
        }
    }

    private void ResetEntityRecursive(EntityViewModel entity)
    {
        entity.CommitEdit();
        entity.IsSelected = false;

        foreach (var child in entity.Children)
        {
            ResetEntityRecursive(child);
        }
    }

    private bool CanAcceptDrop(object? droppedItem)
    {
        if (droppedItem.CanAccept<EntityViewModel>()) return true;

        return false;
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    public void AcceptDrop(object? droppedObject)
    {
        droppedObject.ProcessEntityDrop(this, _sceneTracker);
    }
}