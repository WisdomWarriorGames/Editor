using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.Models;
using WisdomWarrior.Engine.Core.Assets;
using WisdomWarrior.Engine.Core.DataTypes;

namespace WisdomWarrior.Editor.Inspector.Models;

public partial class ImageAssetViewModel : ObservableObject
{
    private readonly Action<ImageAsset> _onChanged;
    private ImageAsset _value;

    public ImageAssetViewModel(ImageAsset value, Action<ImageAsset> onChanged)
    {
        _value = value;
        _onChanged = onChanged;
    }

    public string Name => _value.Name;

    public string Path
    {
        get => _value.AssetPath;
        set
        {
            if (_value.AssetPath == value) return;

            var newAsset = AssetHelpers.CreateImageAsset(value);
            _value = newAsset;

            OnPropertyChanged();
            OnPropertyChanged(nameof(Name));
            _onChanged.Invoke(_value);
        }
    }

    public void UpdateFromEngine(ImageAsset newValue)
    {
        if (_value == newValue) return;

        _value = newValue;
        OnPropertyChanged(nameof(Path));
        OnPropertyChanged(nameof(Name));
    }

    private bool CanAcceptDrop(object? droppedItem)
    {
        return droppedItem
            .GetPayloadItems<IAssetDropData>()
            .Any(IsImageAssetDrop);
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    private Task AcceptDrop(object? droppedItem)
    {
        var droppedAsset = droppedItem
            .GetPayloadItems<IAssetDropData>()
            .FirstOrDefault(IsImageAssetDrop);

        if (droppedAsset == null)
        {
            return Task.CompletedTask;
        }

        Path = droppedAsset.FullPath;

        return Task.CompletedTask;
    }

    private static bool IsImageAssetDrop(IAssetDropData assetDrop)
    {
        if (assetDrop.IsFolder)
        {
            return false;
        }

        return AssetHelpers.IsImage(assetDrop.Extension);
    }
}
