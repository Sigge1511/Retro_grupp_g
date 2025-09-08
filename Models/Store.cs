using System;
using System.Collections.Generic;

namespace Retro_grupp_g.Models;

public partial class Store
{
    public byte StoreId { get; set; }

    public byte ManagerStaffId { get; set; }

    public ushort AddressId { get; set; }

    public DateTime LastUpdate { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual Staff ManagerStaff { get; set; } = null!;

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
