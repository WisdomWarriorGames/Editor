using Avalonia.Controls;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.Inspector.Helpers;
using WisdomWarrior.Engine.Core.Interfaces;
using WisdomWarrior.Engine.Core.Rendering;
using WisdomWarrior.Engine.Core.Systems;

namespace WisdomWarrior.Editor.Core.Tests.Inspector;

public class InspectorPropertyExpansionTests
{
    [Fact]
    public void ExpandVisibleProperties_FlattensBatchSettingsIntoTopLevelRows()
    {
        var rootProperties = CreateRootProperties(new TestRenderSystem());

        try
        {
            var visibleProperties = InspectorPropertyExpander.ExpandVisibleProperties(rootProperties);
            var propertyNames = visibleProperties.Select(p => p.Property.Name).ToArray();

            Assert.DoesNotContain(nameof(RenderSystem.BatchSettings), propertyNames);
            Assert.Contains(nameof(RenderBatchSettings.SortMode), propertyNames);
            Assert.Contains(nameof(RenderBatchSettings.BlendMode), propertyNames);
            Assert.Contains(nameof(RenderBatchSettings.SamplerMode), propertyNames);
            Assert.Contains(nameof(RenderBatchSettings.DepthStencilMode), propertyNames);
            Assert.Contains(nameof(RenderBatchSettings.RasterizerMode), propertyNames);
            Assert.Contains(nameof(RenderBatchSettings.UseTransformMatrix), propertyNames);
            Assert.DoesNotContain(nameof(RenderBatchSettings.TransformMatrix), propertyNames);

            DisposeVisibleProperties(visibleProperties);
        }
        finally
        {
            DisposeProperties(rootProperties);
        }
    }

    [Fact]
    public void FlattenedEnumRows_WriteBackToBatchSettings()
    {
        var system = new TestRenderSystem();
        var rootProperties = CreateRootProperties(system);

        try
        {
            var visibleProperties = InspectorPropertyExpander.ExpandVisibleProperties(rootProperties);

            visibleProperties.Single(p => p.Property.Name == nameof(RenderBatchSettings.SortMode))
                .Property.SetValue(RenderBatchSortMode.BackToFront);
            visibleProperties.Single(p => p.Property.Name == nameof(RenderBatchSettings.BlendMode))
                .Property.SetValue(RenderBlendMode.Additive);

            Assert.Equal(RenderBatchSortMode.BackToFront, system.BatchSettings.SortMode);
            Assert.Equal(RenderBlendMode.Additive, system.BatchSettings.BlendMode);

            DisposeVisibleProperties(visibleProperties);
        }
        finally
        {
            DisposeProperties(rootProperties);
        }
    }

    [Fact]
    public void FlattenedBoolRows_WriteBackToBatchSettings()
    {
        var system = new TestRenderSystem();
        var rootProperties = CreateRootProperties(system);

        try
        {
            var visibleProperties = InspectorPropertyExpander.ExpandVisibleProperties(rootProperties);

            visibleProperties.Single(p => p.Property.Name == nameof(RenderBatchSettings.UseTransformMatrix))
                .Property.SetValue(true);

            Assert.True(system.BatchSettings.UseTransformMatrix);

            DisposeVisibleProperties(visibleProperties);
        }
        finally
        {
            DisposeProperties(rootProperties);
        }
    }

    [Fact]
    public void TransformMatrix_IsSurfacedWhenEnabled_AndUsesUnsupportedEditor()
    {
        var system = new TestRenderSystem();
        var rootProperties = CreateRootProperties(system);

        try
        {
            var visibleProperties = InspectorPropertyExpander.ExpandVisibleProperties(rootProperties);
            visibleProperties.Single(p => p.Property.Name == nameof(RenderBatchSettings.UseTransformMatrix))
                .Property.SetValue(true);

            var updatedProperties = InspectorPropertyExpander.ExpandVisibleProperties(rootProperties);
            var transformMatrixProperty = updatedProperties.Single(p => p.Property.Name == nameof(RenderBatchSettings.TransformMatrix)).Property;
            var editor = new UserControl().CreateEditorControl(transformMatrixProperty);

            var unsupportedEditor = Assert.IsType<TextBlock>(editor);
            Assert.Equal("Unsupported Type", unsupportedEditor.Text);

            DisposeVisibleProperties(updatedProperties);
        }
        finally
        {
            DisposeProperties(rootProperties);
        }
    }

    [Fact]
    public void ReflectionCache_DoesNotTreatIndexerPropertiesAsTrackable()
    {
        var properties = WisdomWarrior.Editor.Core.Helpers.ReflectionCache.GetTrackableProperties(typeof(System.Numerics.Matrix4x4));

        Assert.Empty(properties);
    }

    private static List<TrackerInspectorProperty> CreateRootProperties(TestRenderSystem system)
    {
        var tracker = new SystemTracker(system);
        return tracker.Properties
            .Select(property => new TrackerInspectorProperty(property))
            .ToList();
    }

    private static void DisposeProperties(IEnumerable<TrackerInspectorProperty> properties)
    {
        foreach (var property in properties)
        {
            property.Dispose();
        }
    }

    private static void DisposeVisibleProperties(IEnumerable<VisibleInspectorProperty> properties)
    {
        foreach (var property in properties.Where(property => property.DisposeWhenDetached)
                     .Select(property => property.Property)
                     .OfType<IDisposable>())
        {
            property.Dispose();
        }
    }

    private sealed class TestRenderSystem : RenderSystem
    {
        public override void LoadContent()
        {
        }

        public override void Render(IRenderService renderService)
        {
        }
    }
}
