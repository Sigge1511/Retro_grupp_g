using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Retro_grupp_g.Models;

public partial class Address
{
    public int AddressId { get; set; }

    [Display(Name = "Adress")]
    [Required(ErrorMessage = "Adress är obligatoriskt.")]
    public string Address1 { get; set; } = null!;

    public string? Address2 { get; set; }

    [Display(Name = "Stadsdel")]
    [Required(ErrorMessage = "Stadsdel är obligatoriskt.")]
    public string District { get; set; } = null!;

    [Display(Name = "Stad")]
    [Range(1, int.MaxValue, ErrorMessage = "Välj en stad.")]
    public int CityId { get; set; }

    [Display(Name = "Postnummer")]
    [Required(ErrorMessage = "Postnummer är obligatoriskt.")]
    [RegularExpression(@"^\d{3}\s?\d{2}$", ErrorMessage = "Postnumret måste vara fem siffror.")]
    public string? PostalCode { get; set; }

    [Display(Name = "Telefon")]
    [Required(ErrorMessage = "Telefonnummer är obligatoriskt.")]
    [RegularExpression(@"^\d{6,15}$", ErrorMessage = "Telefonnumret får bara innehålla siffror.")]
    public string Phone { get; set; } = null!;

    [Display(Name = "Senast uppdaterad")]
    public DateTime LastUpdate { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();

    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
}
