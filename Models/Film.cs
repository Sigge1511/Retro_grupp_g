using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace Retro_grupp_g.Models;

public partial class Film
{
    [Column("film_id")]

    public ushort FilmId { get; set; }
    [Column("title")]

    public string Title { get; set; } = null!;
    [Column("description")]

    public string? Description { get; set; }
    [Column("release_year")]

    public short? ReleaseYear { get; set; }
    [Column("language_id")]

    public byte LanguageId { get; set; }
    [Column("original_language_id")]

    public byte? OriginalLanguageId { get; set; }
    [Column("rental_duration")]

    public byte RentalDuration { get; set; }
    [Column("rental_rate")]

    public decimal RentalRate { get; set; }
    [Column("length")]

    public ushort? Length { get; set; }
    [Column("replacement_cost")]

    public decimal ReplacementCost { get; set; }
    [Column("rating")]

    public string? Rating { get; set; }
    [Column("special_features")]

    public string? SpecialFeatures { get; set; }

    [Column("last_update")]
    public DateTime LastUpdate { get; set; }

    public virtual ICollection<FilmActor> FilmActors { get; set; } = new List<FilmActor>();

    public virtual ICollection<FilmCategory> FilmCategories { get; set; } = new List<FilmCategory>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual Language Language { get; set; } = null!;

    public virtual Language? OriginalLanguage { get; set; }
}
