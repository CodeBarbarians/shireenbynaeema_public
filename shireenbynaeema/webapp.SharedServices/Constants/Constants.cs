namespace SharedServices
{
    using System.Diagnostics.CodeAnalysis;

    public static class Constants
    {
        public const string LoginSuccess = "Logged-In successfully.";
        public const string LoginFailed = "Invalid email or password.";
        public const string SaveSuccess = "{0} created successfully.";
        public const string SaveFailed = "{0} creation failed.";
        public const string AddSuccess = "{0} added successfully";
        public const string AddFailed = "{0} added failed ";
        public const string DeleteSuccess = "{0} deleted successfully.";
        public const string DeleteFailed = "{0} deletion failed.";
        public const string UpdateSuccess = "{0} updated successfully.";
        public const string UpdateFailed = "{0} updating failed.";
        public const string ListSuccess = "{0} listed successfully.";
        public const string ListFailed = "{0} listing failed.";
        public const string RetrieveSuccess = "{0} retrieved successfully.";
        public const string RetrieveFailed = "{0} retrieving failed.";
        public const string VerifiedSuccess = "{0} verified successfully.";
        public const string VerifiedFailed = "{0} verification failed.";
        public const string OtpGeneratedSuccess = "OTP sent successfully.";
        public const string OtpGeneratedFailure = "OTP sending failed.";
        public const string RegisterSuccess = "{0} registration completed successfully.";
        public const string UserLocked = "Your account has been locked due to failed login attempts. Either reset your password or contact admin.";
        public const string UserInactive = "User is In-active. Contact admin.";
        public const string NotFound = "{0} not found";
        public const string InvalidResetLink = "Invalid password reset link.";
        public const string InvalidUserInviteLink = "Invalid User invite link.";
        public const string AlreadyExists = "{0} already exists";
        public const string SubTableValueAlreadyExists = "{0} for {1} already exists.";
        public const string ValueRequired = "{0} Needs atleast 1 {1}.";
        public const string PasswordAndConfirmPasswordNotSame = "Both passwords are not the same. Please check both of them again.";
        public const string PasswordNullError = "Password is null or has whitespaces.";
        public const string PasswordNotLongEnough = "Password must be at least 8 characters long.";
        public const string PasswordNotWithLowercaseCharacter = "Password must contain at least one lowercase letter.";
        public const string PasswordNotWithSpecialCharacter = "Password must contain at least one special character.";
        public const string ListingEmpty = "-";
        public const string RetrieveEmpty = "N/A";
        public const string RoleNotAssigned = "The role {0} cannot be assigned to this User {1}.";
        public const string BulkUserInviteSuccess = "All user/s have been invited successfully.";
        public const string BulkUserInviteFailure = "Bulk Invite completed. Some invitations failed or user/s are already active.";
        public const string InValidRequest = "Invalid request.";
        public const bool ResponseSuccess = true;
        public const bool ResponseFailure = false;

        public static class TimeZones
        {
            public const string Eastern = "Eastern Standard Time";
            public const string Pacific = "Pacific Standard Time";
            public const string Karachi = "Asia/Karachi";
        }

        public static class FromEmailKeys
        {
            public const string PwdReset = "PwdReset";
            public const string Invoices = "Invoices";
            public const string Delivery = "Delivery";
            public const string Notifications = "Notifications";
        }

        public static class Token
        {
            public const string TokenExpiredSuccess = "{0} Tokens Expired Successfully";
            public const string Provided = "Token must be provided.";

            public static class Captcha
            {
                public const string VerificationFailed = "Captcha verification failed";
                public const string VerficationSuccess = "Captcha verified successfully";
            }
        }

        public static class User
        {
            public const string NotFound = "User not found.";
            public const string ProfileRetrieveSuccess = "Profile details retrieved successfully.";
        }

        public static class Notes
        {
            public const string NoNewNotes = "No new notes after mapping";
        }

        public static class History
        {
            public const string Success = "History added successfully";
        }

        public static class Notification
        {
            public static class BaseNotification
            {
                public const string PublicNoteAdded = "Public Message Added";
                public const string NewNoteAdded = "New Message added to order {0}";
            }
        }

        public static class Email
        {
            public const string ResetPasswordEmailSubject = "Reset Your Password";
            public const string OTPEmailSubject = "User Verification";
            public const string ResetPasswordEmailPlainText = "Hi {0},\n\nClick the link below to reset your password:\n{1}";
            public const string ResetPasswordEmailSent = "A reset password link has been sent to your email.";
            public const string OTPVerified = "OTP verified successfully";
            public const string OTPSentSuccess = "OTP sent. Please verify.";
            public const string SentSuccess = "Email sent successfully.";
            public const string InValidEmail = "The listed email/s {0} is in-valid. Please check you email address.";
            public const string DuplicateEmail = "Duplicate emails found in request {0}";
            public const string NewUserInvitation = "New User Invitation ";
            public const string InvitationBy = "A new user has been invited by {0}.";
            public const string InvoiceEmailSubject = "Your Shireen by Naeema Invoice - {0}";
            public const string DeliveryEmailSubject = "Your Order Has Shipped! - {0}";
            public const string OrderConfirmationSubject = "Order Confirmed - {0}";

            public static class ContentFiles
            {
                public const string SendForgetPassword = "SendForgetPassword";
                public const string EmailNotificationTemplate = "EmailNotificationTemplate";
                public const string EmailNotificationTemplateFacilityTeam = "EmailNotificationTemplateFacilityTeam";
                public const string SendOtp = "SendOtp";
                public const string InvitationEmailTemplate = "InvitationEmailTemplate";
                public const string DispatchEmailTemplate = "DispatchEmailTemplate";
                public const string SendEmailToSystemAdmin = "SendEmailToSystemAdmin";
                public const string InvoiceEmailTemplate = "InvoiceEmailTemplate";
                public const string DeliveryEmailTemplate = "DeliveryEmailTemplate";
                public const string OrderConfirmationEmailTemplate = "OrderConfirmationEmailTemplate";
            }
        }

        public static class BulkInviteUsers
        {
            public const string InvitedToJoin = "You've been Invited to Join Shireen by Naeema";
        }

        public static class Role
        {
            public const string SuperAdmin = "SuperAdmin";
            public const string SystemAdministrator = "SystemAdministrator";
            public const string Admin = "Admin";
            public const string Customer = "Customer";
        }

        public static class Seed
        {
            public const int CreatedOnUnix = 1752570729;
            public const string DefaultPasswordHash = "$2a$11$5l1I877fmxebiAabu.w8/eqjdZ2By0IJzP.bVDqJk4xZuR9quYFBO";
            public const string AdminEmail = "admin@shireenbynaeema.com";
            public const string AdminFirstName = "System";
            public const string AdminLastName = "Admin";
            public const string AdminDisplayName = "System Admin";
            public const string SystemAdminRoleName = "SystemAdministrator";
            public const string UserRoleName = "User";
            public const string SuperAdminRoleName = "SuperAdmin";
            public static readonly Guid SuperAdminRoleId = Guid.Parse("b4a3a7f3-6d28-4e18-bd67-1a9b66e3d0a1");
            public static readonly Guid SystemAdminRoleId = Guid.Parse("e17c2e8d-1e7d-4af4-b86f-cd59b37ae567");
            public static readonly Guid UserRoleId = Guid.Parse("a06b7c58-3d1f-492a-8e3d-f2fc43a1dc34");
            public static readonly Guid AdminUserId = Guid.Parse("c5903b3e-9a64-4ed1-8c11-2b1a63f17eaf");
            public static readonly Guid SystemUserId = Guid.Empty;
        }

        public static class DateTime
        {
            [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
            public const string ListingFormat = "MMM dd, yyyy, hh:mm tt";

            [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
            public const string ListingFormatDate = "MMM dd, yyyy";

            [StringSyntax(StringSyntaxAttribute.TimeOnlyFormat)]
            public const string ListingFormatTime = "hh:mm tt";

            [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
            public const string SubTableListingFormat = "MMM dd, yyyy";

            [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
            public const string DashboardDateFormat = "MMM ''yy";
        }

        public static class ActivityLogActions
        {
            public const string Login = "Login";
            public const string LoginFailed = "LoginFailed";
            public const string PasswordChanged = "PasswordChanged";
            public const string RoleUpdated = "RoleUpdated";
            public const string UserAdded = "UserAdded";
            public const string UserModified = "UserModified";
            public const string UserDeleted = "UserDeleted";
            public const string Modified = "Modified";
            public const string Added = "Added";
            public const string Deleted = "Deleted";
        }

        public static class ActivityLogModules
        {
            public const string Authentication = "Authentication";
            public const string Security = "Security";
            public const string RoleManagement = "RoleManagement";
            public const string UserManagement = "UserManagement";
            public const string ProductManagement = "ProductManagement";
            public const string OrderManagement = "OrderManagement";
        }

        public static class ActivityLogStatuses
        {
            public const string Success = "Success";
            public const string Failed = "Failed";
            public const string Update = "Update";
            public const string Add = "Add";
        }
    }
}