using System;
using System.Collections.Generic;

namespace Retro_grupp_g.Models;

public partial class ActorInfo
{
    public int ActorId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? FilmInfo { get; set; }
}
