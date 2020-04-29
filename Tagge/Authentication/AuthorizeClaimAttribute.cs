using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Tagge.Authentication
{
    /// <summary>
    /// This houses the Authorize Claim Attribute Model!
    /// </summary>
    public class AuthorizeClaimAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Constructor for AuthorizeClaimAttribute
        /// </summary>
        /// <param name="claimType"></param>
        /// <param name="claimValue"></param>
        public AuthorizeClaimAttribute(string claimType, K2SO.Auth.Constants.PermissionAccessType claimValue) : base(typeof(ClaimRequirementFilter))
        {
            int cValue = (int)claimValue;

            Arguments = new object[] { new Claim(claimType, cValue.ToString()) };
        }
    }

    /// <summary>
    /// Claim Requirement Filter is a Filter model of course!
    /// </summary>
    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        /// <summary>
        /// Claim
        /// </summary>
        readonly Claim _claim;

        /// <summary>
        /// Constructor for Claim Requirement Filter
        /// </summary>
        /// <param name="claim"></param>
        public ClaimRequirementFilter(Claim claim)
        {
            _claim = claim;
        }

        /// <summary>
        /// Checks to see if the user making a call has permission to this endpoint. To see how the token is broken down and verified check K2SO.Auth
        /// </summary>
        /// <param name="context"></param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var bearerToken = request.Headers["Authorization"].ToString();
            var token = bearerToken.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                context.Result = new UnauthorizedResult();
            else
            {
                var hasClaim = K2SO.Auth.Token.HasClaim(token, _claim.Type, _claim.Value);

                if (!hasClaim)
                    context.Result = new UnauthorizedResult();
            }
        }
    }
}
