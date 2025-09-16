using System;
using System.Collections.Generic;

namespace Retro_grupp_g.Models;

public partial class NicerButSlowerFilmList
{
    public int Fid { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public decimal Price { get; set; }

    public ushort? Length { get; set; }

    public string? Rating { get; set; }

    public string? Actors { get; set; }
}
