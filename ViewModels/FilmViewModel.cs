using System.Runtime.CompilerServices;
using Retro_grupp_g.Models;

namespace Retro_grupp_g.ViewModels
{
    public class FilmViewModel
    {
        public int FilmId { get; set; }

        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int? ReleaseYear { get; set; }
        public int? Length { get; set; }
        public string Rating { get; set; } = "";
        public string Language { get; set; } = "";
        public virtual List<string> Actors { get; set; } = new List<string>();//För att visa om en film har flera skådespelare
        public List<string> Genres { get; set; } = new(); // ⬅️ ändrat för att slippa castingfel

        public string ActorSummary { get; set; } = ""; //Egen sammanfattning? Men när sätts den isf?
        public string IsAvailable { get; set; }
    } }
   

