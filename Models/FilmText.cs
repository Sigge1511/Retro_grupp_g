using System;
using System.Collections.Generic;

namespace Retro_grupp_g.Models;

public partial class FilmText
{
    public ushort FilmId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }
}
