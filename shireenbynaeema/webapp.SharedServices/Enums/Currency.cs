namespace SharedServices
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Specifies supported currency codes for monetary values.
    /// </summary>
    /// <remarks>Use this enumeration to indicate the currency associated with financial amounts, such as prices or
    /// balances. The values correspond to standard ISO 4217 currency codes.</remarks>
    public enum Currency
    {
        /// <summary>
        /// Canadian Dollar.
        /// </summary>
        [Display(Name = "Canadian Dollar")]
        CAD = 1,

        /// <summary>
        /// US Dollar.
        /// </summary>
        [Display(Name = "US Dollar")]
        USD = 2,
    }
}