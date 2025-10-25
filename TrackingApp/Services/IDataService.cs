using System.Threading.Tasks;
using TrackingApp.Models;

namespace TrackingApp.Services
{
    public interface IDataService
    {
        Task<MedicationDose?> GetLastConfirmedDoseAsync(int medicationId);
    }
}