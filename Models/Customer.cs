using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Retro_grupp_g.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    [Range(1, byte.MaxValue, ErrorMessage = "Välj en butik.")]
    public byte StoreId { get; set; }

    [Display(Name = "Förnamn")]
    [Required(ErrorMessage = "Förnamn är obligatoriskt.")]
    public string FirstName { get; set; } = null!;

    [Display(Name = "Efternamn")]
    [Required(ErrorMessage = "Efternamn är obligatoriskt.")]
    public string LastName { get; set; } = null!;

    [Display(Name = "E-post")]
    [Required(ErrorMessage = "E-post är obligatoriskt.")]
    [EmailAddress(ErrorMessage = "Ogiltig e-postadress.")]
    public string? Email { get; set; }

    [Display(Name = "Adress")]
    [Range(1, int.MaxValue, ErrorMessage = "Välj en adress.")]
    public int AddressId { get; set; }

    [Display(Name = "Aktiv")]
    public bool Active { get; set; }

    [Display(Name = "Skapad")]
    public DateTime CreateDate { get; set; }

    [Display(Name = "Senast uppdaterad")]
    public DateTime? LastUpdate { get; set; }

    [ValidateNever] public virtual Address Address { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();

    [ValidateNever] public virtual Store Store { get; set; } = null!;
}
