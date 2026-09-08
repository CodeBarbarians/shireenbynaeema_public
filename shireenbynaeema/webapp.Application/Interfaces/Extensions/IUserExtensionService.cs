namespace Application
{
    using SharedServices;

    /// <summary>
    /// Interface for user extension services, providing methods to manage user-related operations such as sending emails, verifying invite tokens, activating or deactivating users,
    /// and handling user registration processes.
    /// </summary>
    public interface IUserExtensionService
    {
        /// <summary>
        /// Sends an email to the specified email address. This method is typically used for sending user-related notifications, such as account activation emails,
        /// password reset emails, and other relevant communications.
        /// </summary>
        /// <param name="email">The email address to which the notification will be sent.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IResponse indicating the success or failure of the email sending operation.</returns>
        Task<IResponse> SendEmail(string email);

        /// <summary>
        /// Verifies the user invite token provided in the request. This method checks the validity of the token, ensuring that it is associated with a valid
        /// user invitation and has not expired.
        /// </summary>
        /// <param name="request">A TokenVerificationRequest object containing the token to be verified.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IResponse indicating whether the token is valid and any associated token details.</returns>
        Task<IResponse> VerifyUserInviteToken(TokenVerificationRequest request);

        /// <summary>
        /// Activates or deactivates a user based on the provided request. This method allows administrators
        /// to manage individual user access efficiently.
        /// </summary>
        /// <param name="request">An ActiveInActiveUserRequest object containing the user information and the desired activation status.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IResponse indicating the success or failure of the activation or deactivation operation.</returns>
        Task<IResponse> ActiveInActiveUser(ActiveInActiveUserRequest request);

        /// <summary>
        /// Activates or deactivates multiple users in bulk based on the provided request. This method is designed to handle large-scale user management operations, allowing administrators
        /// to efficiently manage user access and permissions.
        /// </summary>
        /// <param name="request">An ActiveInActiveUserRequest object containing the list of users and the desired activation status.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IResponse indicating the success or failure of the bulk activation or deactivation operation.</returns>
        Task<IResponse> BulkActiveInActiveUser(ActiveInActiveUserRequest request);

        /// <summary>
        /// Sends bulk user invitations based on the provided request. This method allows administrators to invite multiple users at once,
        /// streamlining the onboarding process and ensuring that new users receive their invitations in a timely manner.
        /// </summary>
        /// <param name="request">A BulkUserInviteRequest object containing the list of users to be invited and invitation details.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IResponse indicating the success or failure of the bulk invitation operation.</returns>
        Task<IResponse> BulkInviteUsers(BulkUserInviteRequest request);

        /// <summary>
        /// Registers a new user based on the provided registration request. This method handles the user registration process, including validating the input data,
        /// creating the user account, and sending any necessary confirmation emails.
        /// </summary>
        /// <param name="request">A UserRegisterRequest object containing the user registration information.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IResponse indicating the success or failure of the registration operation.</returns>
        Task<IResponse> RegisterUser(UserRegisterRequest request);

        /// <summary>
        /// Completes the user registration process based on the provided registration request. This method is typically called after the initial registration step,
        /// allowing users to finalize their account setup, including setting up any additional profile information or preferences.
        /// </summary>
        /// <param name="request">A UserRegisterRequest object containing the completion registration information.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IResponse indicating the success or failure of the registration completion operation.</returns>
        Task<IResponse> CompleteUserRegistration(UserRegisterRequest request);
    }
}