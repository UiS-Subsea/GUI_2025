
namespace Backend.Infrastructure.Interface
{
    public interface IGUITranslationLayer
    {
        List<object> DecodeAndTranslatePackets(byte[] tcpData);  // Translate ROVData (byte[]) to RenderableData For the GUI.
    }
}
