using Retro_grupp_g.Models;

namespace Retro_grupp_g.Repositories
{
    public interface IRentalRepository
    {
        Task<bool> RentAsync(int customerId, int filmId, int? staffId = null);
        Task<List<Rental>> GetOpenRentalsByCustomerAsync(int customerId);

//********************** KOLLAR INNAN RETUR **********************************************************
        Task<(IReadOnlyList<(int InventoryId, int FilmId, string Title)> Films,
                   IReadOnlyList<(int CustomerId, string FullName, string Email)> Customers)> OnGetReturnAsync();
        Task<(bool Found, string FilmTitle, DateOnly RentalDate, 
                   DateOnly DueDate, bool IsLate, int DaysLate, int RentalDurationDays,
                   int ActualCustomerId, string ActualCustomerName, 
                   bool CustomerMatches)>OnGetReturnPreviewAsync(int inventoryId, int customerId);
//              NORMAL RETURER *********    Endast riktig retur om kund+film matchar
        Task<(bool Ok, string Message)> ReturnNormalRealAsync(int inventoryId, int customerId);

        //********************** SENA RETURER **********************************************************       
        // LATE preview
        Task<(bool Found, int RentalId, int CustomerId, 
            string CustomerName, string FilmTitle, DateOnly RentalDate, DateOnly DueDate, 
            int DaysLate, ushort FeeAmount)>GetLateFeePreviewByInventoryAsync(int inventoryId);
        Task<(bool Found, int RentalId, int CustomerId, int InventoryId,
           string CustomerName, string FilmTitle, DateOnly RentalDate,
           DateOnly DueDate, int DaysLate, ushort FeeAmount)> GetLateFeePreviewByRentalIdAsync(int rentalId);
        // LATE post
        Task<bool> ReturnLateRealAsync(int inventoryId, int customerId, int staffId, int storeId);



        //********************** SKADAD RETUR/BORTTAPPAD **********************************************************
        Task ReturnDamagedAsync(int rentalId);


//*****************   Hjälpmetoder
        Task<Rental?> GetOpenRentalByInventoryAsync(int inventoryId);
        Task<Rental?> GetOpenRentalByIdAsync(int rentalId);

    }
}