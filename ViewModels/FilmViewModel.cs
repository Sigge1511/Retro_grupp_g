namespace Retro_grupp_g.ViewModels
{
    public class FilmViewModel
    {
        public int FilmId { get; set; }

        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string>Genres { get; set; } = new(); //För att visa om en film ligger i flera genrer
        public int? ReleaseYear { get; set; } 
        public int? Length { get; set; } 
        public string Rating { get; set; } = "";
        public string Language { get; set; } = "";
        public string ActorSummary { get; set; } = "";


    }
}
