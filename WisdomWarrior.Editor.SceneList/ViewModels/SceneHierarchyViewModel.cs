using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Toasts;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.SceneList.Helpers;

namespace WisdomWarrior.Editor.SceneList.ViewModels;

public partial class SceneHierarchyViewModel : ObservableObject
{
    private readonly SceneTracker _sceneTracker;
    private readonly CurrentSceneManager _sceneManager;
    private readonly SelectionManager _selectionManager;
    private readonly ScenePersistenceService _scenePersistenceService;

    public ObservableCollection<SceneNodeViewModel> Scenes { get; } = new();

    private SceneNodeViewModel? _activeSceneNode;
    private SceneNodeViewModel? _selectedSceneNode;
    private EntityViewModel? _selectedEntityNode;

    public SceneHierarchyViewModel(
        CurrentSceneManager sceneManager,
        SelectionManager selectionManager,
        ScenePersistenceService scenePersistenceService)
    {
        _sceneManager = sceneManager;
        _selectionManager = selectionManager;
        _scenePersistenceService = scenePersistenceService;
        _sceneTracker = _sceneManager.Tracker;

        _sceneTracker.OnSceneModified += OnSceneModified;
        _sceneManager.CurrentSceneReady += OnSceneReady;
        _selectionManager.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(object? obj)
    {
        if (obj is EntityTracker entityTracker)
        {
            Dispatcher.UIThread.Post(() => ApplyEntitySelection(entityTracker));
            return;
        }

        if (obj is SceneTracker sceneTracker && ReferenceEquals(sceneTracker, _sceneTracker))
        {
            Dispatcher.UIThread.Post(ApplySceneSelection);
            return;
        }

        Dispatcher.UIThread.Post(ResetChanges);
    }

    private void OnSceneReady()
    {
        ClearCurrentSelectionState();
        Scenes.Clear();

        _activeSceneNode = new SceneNodeViewModel(_sceneTracker, _selectionManager, SaveActiveScene);
        Scenes.Add(_activeSceneNode);
        Dispatcher.UIThread.Post(SyncRootEntities);
    }

    private void SaveActiveScene()
    {
        var saved = _scenePersistenceService.TrySaveSceneToPreferredDirectory();
        if (saved)
            return;

        EditorUI.ToastManager.CreateToast()
            .WithTitle("Unable to save scene in the selected folder.")
            .Queue();
    }

    private void OnSceneModified()
    {
        Dispatcher.UIThread.Post(SyncRootEntities);
    }

    private void SyncRootEntities()
    {
        if (_activeSceneNode == null)
        {
            return;
        }

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
                rootList.Insert(i, new EntityViewModel(rootTracker, _sceneTracker, _selectionManager));
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

        _selectedSceneNode = null;
        _selectedEntityNode = null;
    }

    public void ClearSelection()
    {
        _selectionManager.Clear();
    }

    private static void ResetEntityRecursive(EntityViewModel entity)
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

    private void ApplyEntitySelection(EntityTracker targetTracker)
    {
        if (_activeSceneNode == null)
        {
            return;
        }

        ClearCurrentSelectionState();
        _activeSceneNode.IsExpanded = true;

        var selectedEntity = TrySelectEntityRecursive(_activeSceneNode.Children, targetTracker);
        if (selectedEntity == null)
        {
            return;
        }

        _selectedEntityNode = selectedEntity;
    }

    private void ApplySceneSelection()
    {
        if (_activeSceneNode == null)
        {
            return;
        }

        ClearCurrentSelectionState();
        _activeSceneNode.IsSelected = true;
        _activeSceneNode.IsExpanded = true;
        _selectedSceneNode = _activeSceneNode;
    }

    private void ClearCurrentSelectionState()
    {
        if (_selectedSceneNode != null)
        {
            _selectedSceneNode.IsSelected = false;
            _selectedSceneNode = null;
        }

        if (_selectedEntityNode != null)
        {
            _selectedEntityNode.IsSelected = false;
            _selectedEntityNode = null;
        }
    }

    private static EntityViewModel? TrySelectEntityRecursive(
        IEnumerable<EntityViewModel> entities,
        EntityTracker targetTracker)
    {
        foreach (var entity in entities)
        {
            if (ReferenceEquals(entity.Tracker, targetTracker))
            {
                entity.IsSelected = true;
                return entity;
            }

            var selectedChild = TrySelectEntityRecursive(entity.Children, targetTracker);
            if (selectedChild != null)
            {
                entity.IsExpanded = true;
                return selectedChild;
            }
        }

        return null;
    }
}
