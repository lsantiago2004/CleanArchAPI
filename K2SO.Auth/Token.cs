using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace K2SO.Auth
{
    public class Token
    {
        private static string _Secret = "VmFuY2VzSURzIEFyZSBBbGwgR1VJRHMhIEtpbmRyYSBpcyBraWxsaW5nIGl0ISBKZW5uIHRoZSBidWcgc21hc2hlciE=";

        /// <summary>
        /// Generates a Token
        /// </summary>
        /// <param name="user"></param>
        /// <param name="company"></param>
        /// <param name="permissions"></param>
        /// <param name="expireMinutes"></param>
        /// <returns></returns>
        //public static string GenerateToken(Ghost.Data.Models.GH_User user, Executor.Data.Models.DV_Company company, Dictionary<string, string> permissions, int? expireMinutes = 90)
        //{
        //    var claims = new List<Claim>()
        //    {
        //                new Claim(JwtRegisteredClaimNames.Sub, user.FirstName),
        //                new Claim(JwtRegisteredClaimNames.Email, user.EmailAddress),
        //                new Claim("CompanyId", company.Id.ToString()),
        //                new Claim("Company", company.Name)
        //            };

        //    foreach (var permission in permissions)
        //    {
        //        claims.Add(new Claim(permission.Key, permission.Value.ToString()));
        //    }

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Secret));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken("iPaaS",
        //      "iPaaS",
        //      claims,
        //      expires: DateTime.Now.AddMinutes((double)expireMinutes),
        //      signingCredentials: creds);

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        /// <summary>
        /// Generates a Token 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="company"></param>
        /// <param name="permissions"></param>
        /// <param name="expireMinutes"></param>
        /// <returns></returns>
        //public static string GenerateToken(Ghost.Data.Models.GH_User user, Executor.Data.Models.DV_Company company, List<K2SO.Data.Models.PermissionResponse> permissions, double? expireMinutes = 90)
        //{
        //    var claims = new List<Claim>() {
        //        new Claim(JwtRegisteredClaimNames.Sub, user.FirstName),
        //        new Claim(JwtRegisteredClaimNames.Email, user.EmailAddress),
        //        new Claim("CompanyId", company.Id.ToString()),
        //        new Claim("Company", company.Name)
        //    };

        //    foreach (var permission in permissions)
        //    {
        //        claims.Add(new Claim(permission.Name, permission.AccessTypeId.ToString()));
        //    }

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Secret));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken("iPaaS",
        //      "iPaaS",
        //      claims,
        //      expires: DateTime.Now.AddMinutes((double)expireMinutes),
        //      signingCredentials: creds);

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        /// <summary>
        /// Verifies the token is valid 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static bool Validate(string token)
        {
            bool isValidToken = false;
            string email = null;
            string companyId = null;

            var simplePrinciple = GetPrincipal(token);
            var identity = simplePrinciple.Identity as ClaimsIdentity;

            if (identity == null)
                return isValidToken;

            if (!identity.IsAuthenticated)
                return isValidToken;

            var emailClaim = identity.FindFirst(ClaimTypes.Email);
            email = emailClaim?.Value;

            var companyIdClaim = identity.FindFirst("CompanyId");
            companyId = companyIdClaim?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(companyId))
                return isValidToken;

            isValidToken = true;

            return isValidToken;
        }

        /// <summary>
        /// Verifies the user has permission based off identity 
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static bool HasClaim(string token, string permission, string value)
        {
            var hasClaim = false;
            var simplePrinciple = new ClaimsPrincipal();

            // Check to prevent bad tokens
            try
            {
                simplePrinciple = GetPrincipal(token);
            }
            catch (Exception ex)
            {
                return hasClaim;
            }

            var identity = simplePrinciple.Identity as ClaimsIdentity;

            // Check for admin first
            var admin = identity.Claims.FirstOrDefault(x => x.Type == "Red Rook Admin");

            if (admin != null)
            {
                return true;
            }

            // If not admin then check if the user has permission
            var claims = identity.Claims.Where(x => x.Type == permission).ToList();
            int.TryParse(value, out int intValue);
            var cValue = (Constants.PermissionAccessType)Enum.ToObject(typeof(K2SO.Auth.Constants.PermissionAccessType), intValue);

            // Check if passed claim is set to None
            if (intValue == (int)Constants.PermissionAccessType.NONE)
            {
                return true;
            }

            if (claims != null)
            {
                foreach (var claim in claims)
                {
                    var accessTypes = BuildAccessTypes(claim.Value);

                    // Check to see if the claim is admin
                    if (accessTypes.HasFlag(Constants.PermissionAccessType.ADMIN))
                    {
                        return true;
                    }

                    // Check to see if the claim is view
                    if (accessTypes.HasFlag(Constants.PermissionAccessType.NONE) && Constants.PermissionAccessType.NONE == cValue)
                    {
                        hasClaim = true;
                    }

                    // Check to see if the claim is view
                    if (accessTypes.HasFlag(Constants.PermissionAccessType.VIEW) && Constants.PermissionAccessType.VIEW == cValue)
                    {
                        hasClaim = true;
                    }

                    // Check to see if the claim is create
                    if (accessTypes.HasFlag(Constants.PermissionAccessType.CREATE) && Constants.PermissionAccessType.CREATE == cValue)
                    {
                        hasClaim = true;
                    }

                    // Check to see if the claim is edit
                    if (accessTypes.HasFlag(Constants.PermissionAccessType.EDIT) && Constants.PermissionAccessType.EDIT == cValue)
                    {
                        hasClaim = true;
                    }

                    // Check to see if the claim is delete
                    if (accessTypes.HasFlag(Constants.PermissionAccessType.DELETE) && Constants.PermissionAccessType.DELETE == cValue)
                    {
                        hasClaim = true;
                    }
                }
            }

            return hasClaim;
        }

        public static ClaimsPrincipal GetPrincipal(string token)
        {
            ClaimsPrincipal principal;

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = new JwtSecurityToken();

            try
            {
                jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (jwtToken == null)
                return null;

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Secret));

            var validationParameters = new TokenValidationParameters()
            {
                RequireExpirationTime = false,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = symmetricSecurityKey
            };

            SecurityToken securityToken;
            principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

            return principal;

        }

        private static Constants.PermissionAccessType BuildAccessTypes(string value)
        {
            // This build the catchall 
            var accessTypes = Constants.PermissionAccessType.NONE;

            if (!string.IsNullOrEmpty(value))
            {
                int.TryParse(value, out int intValue);
                accessTypes = (Constants.PermissionAccessType)Enum.ToObject(typeof(Constants.PermissionAccessType), intValue);
            }

            return accessTypes;
        }
    }
}
