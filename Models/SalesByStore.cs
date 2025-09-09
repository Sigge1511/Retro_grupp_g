using System;
using System.Collections.Generic;

namespace Retro_grupp_g.Models;

public partial class SalesByStore
{
    public int StoreId { get; set; }

    public string Store { get; set; } = null!;

    public string Manager { get; set; } = null!;

    public decimal? TotalSales { get; set; }
}
