namespace Retro_grupp_g.ViewModels
{
    public class TopFilmsPerCategoryViewModel
    {
        public string CategoryName { get; set; } = "";
        public List<FilmRentalCountViewModel> TopFilms { get; set; } = new();
    }
}
