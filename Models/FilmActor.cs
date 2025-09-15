using System;
using System.Collections.Generic;

namespace Retro_grupp_g.Models;

public partial class FilmActor
{
    public int ActorId { get; set; }

    public int FilmId { get; set; }

    public DateTime LastUpdate { get; set; }

    public virtual Actor Actor { get; set; } = null!;

    public virtual Film Film { get; set; } = null!;
}
