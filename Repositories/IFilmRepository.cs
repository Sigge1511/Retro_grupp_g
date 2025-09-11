using Retro_grupp_g.Models;

namespace Retro_grupp_g.Repositories
{
    public interface IFilmRepository
    {
        Task<List<Film>> GetAllAsync();
        Task<Film?> GetByIdAsync(int id);
        Task AddAsync (Film film);
        Task UpdateAsync (Film film);
        Task DeleteAsync (int id);
        Task SaveAsync ();

    }
}
