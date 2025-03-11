
namespace Backend.Infrastructure.Interface
{
    public interface IRovTranslationLayer
    {
        List<object> Translate(Dictionary<string, object> rovCommand); // Translate Generic command data to ROV command data.
    }
}
