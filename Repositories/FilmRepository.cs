using System.Collections.Generic;
using System.Linq;                   
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; 
using Retro_grupp_g.Data;            
using Retro_grupp_g.Models;          

namespace Retro_grupp_g.Repositories
{
    public class FilmRepository : IFilmRepository
    {
        private readonly SakilaDbContext _db;
        public FilmRepository(SakilaDbContext db) => _db = db;

        public Task<List<Film>> GetAllAsync() =>
            _db.Films.OrderBy(f => f.Title).ToListAsync();

        public Task<Film?> GetByIdAsync(int id) =>
            _db.Films.FindAsync(id).AsTask();

        public Task AddAsync(Film film) =>
            _db.Films.AddAsync(film).AsTask();

        public Task UpdateAsync(Film film)
        {
            _db.Films.Update(film);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var f = await GetByIdAsync(id);
            if (f != null)
            {
                _db.Films.Remove(f);
            }
        }

        public Task SaveAsync() => _db.SaveChangesAsync();

        public async Task<List<Film>> GetAllWithLanguagesAsync()
        {
            return await _db.Films
                .Include(f => f.Language)
                .Include(f => f.OriginalLanguage)
                .ToListAsync();
        }

    }
}
