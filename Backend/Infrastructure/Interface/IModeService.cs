
namespace Backend.Infrastructure.Interface
{
    public interface IModeService
    {
        void SetModeToManual();
        void SetModeToAutonomous();
        bool IsManual();
    }
}