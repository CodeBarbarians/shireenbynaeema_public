namespace Server
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Options;
    using SharedServices;

    /// <summary>
    /// Provides authorization policies based on permission requirements, supporting custom permission-based policies as
    /// well as standard fallback policies.
    /// </summary>
    /// <remarks>This provider enables dynamic creation of authorization policies by interpreting policy names
    /// that use a specific permission prefix. Policies with the prefix are mapped to permission requirements, while all
    /// other policy requests are delegated to the default fallback provider. This allows for flexible, attribute-driven
    /// permission checks in ASP.NET Core applications.</remarks>
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider fallbackPolicyProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionPolicyProvider"/> class,.
        /// which is responsible for providing authorization policies based on permission requirements.
        /// </summary>
        /// <param name="options">
        /// The options used to configure authorization policies, passed to the default fallback policy provider.
        /// </param>
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            this.fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        /// <summary>
        /// Retrieves an authorization policy based on the provided policy name.
        /// If the policy name starts with the defined permission prefix, a custom policy
        /// requiring the specified permissions is created; otherwise, the fallback provider is used.
        /// </summary>
        /// <param name="policyName">
        /// The name of the policy to retrieve, potentially including the permission prefix and CSV of permissions.
        /// </param>
        /// <returns>
        /// A Task returning the corresponding AuthorizationPolicy if found or created; otherwise, null.
        /// </returns>
        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(PermissionAttribute.POLICYPREFIX + ":"))
            {
                var permissionsCsv = policyName[(PermissionAttribute.POLICYPREFIX.Length + 1)..];
                var permissions = permissionsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries);

                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(permissions))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return this.fallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        /// <summary>
        /// Retrieves the default authorization policy, which is typically applied
        /// when no specific policy is specified for a request.
        /// </summary>
        /// <returns>
        /// A Task returning the default AuthorizationPolicy from the fallback policy provider.
        /// </returns>
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
            this.fallbackPolicyProvider.GetDefaultPolicyAsync();

        /// <summary>
        /// Retrieves the fallback authorization policy, which is used when no specific policy matches.
        /// </summary>
        /// <returns>
        /// A Task returning the fallback AuthorizationPolicy from the fallback policy provider.
        /// </returns>
        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
            this.fallbackPolicyProvider.GetFallbackPolicyAsync();
    }
}