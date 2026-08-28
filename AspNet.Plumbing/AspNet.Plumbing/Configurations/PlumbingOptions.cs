namespace AspNet.Plumbing.Configurations
{
    /// <summary>
    /// Represents configuration options for the plumbing infrastructure in an ASP.NET Core application.
    /// </summary>
    public sealed class PlumbingOptions
    {
        /// <summary>
        /// Gets or sets the API scope used for authentication and authorization.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the API scope.
        /// Defaults to an empty string.
        /// </value>
        public string ApiScope { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the policy used for API scope authorization.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the name of the API scope policy.
        /// The default value is "ApiScope".
        /// </value>
        public string ApiScopePolicyName { get; set; } = "ApiScope";

        /// <summary>
        /// Gets or initializes the URL of the authority used for authentication and authorization.
        /// </summary>
        /// <remarks>This property is typically used to specify the base URL of the identity provider or authorization server.</remarks>
        public string AuthorityUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the CORS policy to be used in the application.
        /// </summary>
        /// <value>A <see cref="string"/> representing the name of the CORS policy. The default value is "DefaultCors".</value>
        public string CorsPolicyName { get; set; } = "DefaultCors";

        /// <summary>
        /// Gets or sets the list of allowed origins for CORS (Cross-Origin Resource Sharing).
        /// </summary>
        /// <remarks>This property specifies the origins that are permitted to access resources in the application. It is used to configure CORS policies.</remarks>
        public string[] CorsAllowedOrigins { get; set; } = [];
    }
}