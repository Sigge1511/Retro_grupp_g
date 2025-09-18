using Retro_grupp_g.Models;

namespace Retro_grupp_g.Repositories
{
    public interface IRentalRepository
    {
        Task<bool> RentAsync(int customerId, int filmId, int? staffId = null);

        Task<(IReadOnlyList<(int InventoryId, int FilmId, string Title)> Films,
                   IReadOnlyList<(int CustomerId, string FullName, string Email)> Customers)> OnGetReturnAsync();
        Task ReturnNormalAsync(int rentalId);

        Task ReturnLateAsync(int rentalId);

        Task ReturnDamagedAsync(int rentalId);

        Task<List<Rental>> GetOpenRentalsByCustomerAsync(int customerId);
    }
}