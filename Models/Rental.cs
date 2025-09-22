using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
    
    [NotMapped] //Lägger den som notmapped då det inte finns någon kolumn i tabellen
    public int FilmId { get; internal set; }
}
