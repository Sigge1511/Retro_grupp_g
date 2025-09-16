using System;
using System.Collections.Generic;

namespace Retro_grupp_g.Models;

public partial class Rental
{
    public int RentalId { get; set; }

    public DateTime RentalDate { get; set; }

    public uint InventoryId { get; set; }

    public int CustomerId { get; set; }

    public DateTime? ReturnDate { get; set; }

    public byte StaffId { get; set; }

    public DateTime LastUpdate { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Inventory Inventory { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Staff Staff { get; set; } = null!;
    public int FilmId { get; internal set; }
}
