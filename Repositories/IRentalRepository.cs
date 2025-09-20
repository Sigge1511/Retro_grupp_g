using Retro_grupp_g.Models;

namespace Retro_grupp_g.Repositories
{
    public interface IRentalRepository
    {
        Task<bool> RentAsync(int customerId, int filmId, int? staffId = null);

//********************** KOLLAR INNAN RETUR **********************************************************
        Task<(IReadOnlyList<(int InventoryId, int FilmId, string Title)> Films,
                   IReadOnlyList<(int CustomerId, string FullName, string Email)> Customers)> OnGetReturnAsync();


        Task<(bool Found, string FilmTitle, DateOnly RentalDate, 
                   DateOnly DueDate, bool IsLate, int DaysLate, int RentalDurationDays,
                   int ActualCustomerId, string ActualCustomerName, 
                   bool CustomerMatches)>OnGetReturnPreviewAsync(int inventoryId, int customerId);



        //********************** RETURER **********************************************************
        // Gör endast riktig retur om kund+film matchar
        Task<(bool Ok, string Message)> ReturnNormalRealAsync(int inventoryId, int customerId);

        Task ReturnLateAsync(int rentalId);

        Task ReturnDamagedAsync(int rentalId);

        Task<List<Rental>> GetOpenRentalsByCustomerAsync(int customerId);
    }
}