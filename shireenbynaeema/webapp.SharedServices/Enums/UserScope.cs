namespace SharedServices
{
    using System.ComponentModel.DataAnnotations;

    public enum UserScope
    {
        [Display(Name = "Organization")]
        Organization = 1,

        [Display(Name = "Customer")]
        Customer = 2,
    }

    public enum RoleScope
    {
        [Display(Name ="Organization")]
        Organization = 1,

        [Display(Name = "Customer")]
        Customer = 2,
    }
}