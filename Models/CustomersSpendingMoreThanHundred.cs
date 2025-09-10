using System;
using System.Collections.Generic;

namespace Retro_grupp_g.Models;

public partial class CustomersSpendingMoreThanHundred
{
    public int? AntalBetalningar { get; set; }

    public decimal? MaxBetalning { get; set; }

    public decimal? MinBetalning { get; set; }

    public decimal? MedelBetalning { get; set; }

    public decimal? Betalt { get; set; }

    public int? UnikaKunder { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Email { get; set; }
}
