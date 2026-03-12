using System.Text.Json;
using WisdomWarrior.Engine.Core.Converters;

namespace WisdomWarrior.Editor.Core.Tests.TestInfrastructure;

public static class TestJsonOptions
{
    public static JsonSerializerOptions Create(bool writeIndented = false)
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = writeIndented
        };

        options.Converters.Add(new ComponentConverter());
        options.Converters.Add(new SystemDrawingColorJsonConverter());

        return options;
    }
}
