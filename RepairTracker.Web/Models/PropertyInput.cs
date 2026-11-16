using System.ComponentModel.DataAnnotations;
using RepairTracker.Domain;

namespace RepairTracker.Web.Models;

// What the add/edit form is allowed to send. No Id or LandlordId on purpose -
// the server sets those, so nobody can post a property into someone else's account.
public class PropertyInput
{
    [Required(ErrorMessage = "Enter the first line of the address.")]
    [StringLength(200)]
    [Display(Name = "Address line 1")]
    public string AddressLine1 { get; set; } = "";

    [StringLength(200)]
    [Display(Name = "Address line 2 (optional)")]
    public string? AddressLine2 { get; set; }

    [Required(ErrorMessage = "Enter the town or city.")]
    [StringLength(100)]
    [Display(Name = "Town or city")]
    public string Town { get; set; } = "";

    [Required(ErrorMessage = "Enter the postcode.")]
    [RegularExpression(@"^\s*[A-Za-z]{1,2}\d[A-Za-z\d]?\s*\d[A-Za-z]{2}\s*$",
        ErrorMessage = "Enter a UK postcode, like EH1 1YZ.")]
    public string Postcode { get; set; } = "";

    [StringLength(80)]
    [Display(Name = "Nickname (optional)")]
    public string? Nickname { get; set; }

    public static PropertyInput From(Property p) => new()
    {
        AddressLine1 = p.AddressLine1,
        AddressLine2 = p.AddressLine2,
        Town = p.Town,
        Postcode = p.Postcode,
        Nickname = p.Nickname
    };

    // Copies the form onto an entity and tidies the text on the way
    public void ApplyTo(Property p)
    {
        p.AddressLine1 = AddressLine1.Trim();
        p.AddressLine2 = string.IsNullOrWhiteSpace(AddressLine2) ? null : AddressLine2.Trim();
        p.Town = Town.Trim();
        p.Postcode = Postcode.Trim().ToUpperInvariant();
        p.Nickname = string.IsNullOrWhiteSpace(Nickname) ? null : Nickname.Trim();
    }
}