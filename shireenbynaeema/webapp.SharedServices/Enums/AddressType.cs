namespace SharedServices
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Specifies the type of address, such as a physical location or a remittance address.
    /// </summary>
    public enum AddressType
    {
        /// <summary>
        /// Represents a physical location address.
        /// </summary>
        [Display(Name = "Location")]
        Location = 1,

        /// <summary>
        /// Represents an address used for remittance or billing purposes.
        /// </summary>
        [Display(Name = "Remit To")]
        RemitTo = 2,
    }
}