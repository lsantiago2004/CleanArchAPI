using System;
using System.Security.Claims;

namespace K2SO.Auth
{
    public class Security
    {
        #region Properties
        private Guid CompanyId { get; set; }
        private string EmailAddress { get; set; }
        private string AuthToken { get; set; }
        private bool RedRookAdmin { get; set; }
        private bool UserAdmin { get; set; }
        #endregion

        #region Constructor(s)
        public Security(string bearerToken)
        {
            var token = bearerToken.Replace("Bearer ", "");
            ClaimsPrincipal simplePrinciple;

            simplePrinciple = Token.GetPrincipal(token);

            var identity = simplePrinciple.Identity as ClaimsIdentity;

            var emailClaim = identity.FindFirst(ClaimTypes.Email);
            EmailAddress = emailClaim?.Value;

            var companyIdClaim = identity.FindFirst("CompanyId");
            CompanyId = Guid.Parse(companyIdClaim?.Value);

            AuthToken = bearerToken;

            var claimValue = (int)Constants.PermissionAccessType.ADMIN;
            if (identity.HasClaim("Red Rook Admin", claimValue.ToString()))
                RedRookAdmin = true;

            if (identity.HasClaim("User", claimValue.ToString()))
                UserAdmin = true;
        }
        #endregion

        public Guid GetCompanyId()
        {
            //Get the current claims principal
            //var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            //return Guid.Parse(identity.Claims.FirstOrDefault(x => x.Type == "CompanyId").Value);
            return CompanyId;
        }

        /// <summary>
        /// Gets Email for current user 
        /// </summary>
        /// <returns></returns>
        public string GetEmail()
        {
            return EmailAddress;
        }

        /// <summary>
        /// Gets Auth Token for current user 
        /// </summary>
        /// <returns></returns>
        public string GetAuthToken()
        {
            return AuthToken;
        }

        /// <summary>
        /// Checks if user is a Red Rook Admin 
        /// </summary>
        /// <returns></returns>
        public bool IsRedRookAdmin()
        {
            //bool isAdmin = false;
            ////Get the current claims principal
            //var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            //var claimValue = (int)Constants.PermissionAccessType.ADMIN;
            //if (identity.HasClaim("Red Rook Admin", claimValue.ToString()))
            //    isAdmin = true;

            //return isAdmin;

            return RedRookAdmin;
        }

        /// <summary>
        /// Checks if user is an Admin 
        /// </summary>
        /// <returns></returns>
        public bool IsUserAdmin()
        {
            //bool isAdmin = false;
            ////Get the current claims principal
            //var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            //var claimValue = (int)Constants.PermissionAccessType.ADMIN;
            //if (identity.HasClaim("User", claimValue.ToString()))
            //    isAdmin = true;

            //return isAdmin;
            return UserAdmin;
        }

        /// <summary>
        /// Gets the template company id
        /// </summary>
        /// <returns></returns>
        public Guid GetTemplateCompanyId()
        {
            return Guid.Parse("C4E6854C-602D-4197-BF6A-BFA5DF543914");
        }
    }
}
