using Shared.Models.DealComponents;
using System.ComponentModel;

namespace Shared.Helpers
{
    public static class PermissionExtensions
    {
        public static string DealAdmin = Permissions.Deal.Admin.GetDescription();
        public static string DealWrite = Permissions.Deal.Write.GetDescription();
        public static string DealRead = Permissions.Deal.Read.GetDescription();
        public static string DealNone = Permissions.Deal.None.GetDescription();
        public static string PerformanceWrite = Permissions.Performance.Write.GetDescription();
        public static string PerformanceRead = Permissions.Performance.Read.GetDescription();
        public static string PerformanceNone = Permissions.Performance.None.GetDescription();
        public static string ExpendituresWrite = Permissions.Expenditures.Write.GetDescription();
        public static string ExpendituresRead = Permissions.Expenditures.Read.GetDescription();
        public static string ExpendituresNone = Permissions.Expenditures.None.GetDescription();
        public static List<string> DealPermissions = new List<string> { DealNone, DealRead, DealWrite };
        public static List<string> PerformancePermissions = new List<string> { PerformanceNone, PerformanceRead, PerformanceWrite };
        public static List<string> ExpendituresPermissions = new List<string> { ExpendituresNone, ExpendituresRead, ExpendituresWrite };

        public static bool IsPermissionsModalValuesValid(this PermissionsModal modal, List<string> currentUserPermissions)
        {
            var grantedPermissions = new List<string>() { modal.DealPermission, modal.PerformancePermission, modal.ExpenditurePermission };
            var canGrant = currentUserPermissions.CanGrantPermissions();
            if (modal.IsAdmin)
            {
                grantedPermissions = new List<string>() { DealAdmin };
            }

            if (DealPermissions.Contains(modal.DealPermission)
                && PerformancePermissions.Contains(modal.PerformancePermission)
                && ExpendituresPermissions.Contains(modal.ExpenditurePermission))
            {
                // If granting permissions you are not allowed to
                if (grantedPermissions.Except(canGrant).Count() > 0)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public static string GetPermissionDescription(this List<string> permissions, List<string> currentUserPermissions)
        {
            string permissionMessage = "Access Permissions: ";
            List<string> permissionMessages = new();

            var dealPermission = permissions.Where(x => !x.Contains("Deal.Admin")).FirstOrDefault(x => x.Contains("Deal."));
            var expenditurePermission = permissions.FirstOrDefault(x => x.Contains("Expenditures."));
            var performancePermission = permissions.FirstOrDefault(x => x.Contains("Performance."));
            var isDealAdmin = permissions.Exists(x => x.Contains("Deal.Admin"));

            if (dealPermission == DealNone && expenditurePermission == ExpendituresNone && performancePermission == PerformanceNone)
            {
                permissionMessages.Add("None");
            }

            // Deal
            if (isDealAdmin)
            {
                permissionMessages.Add("Deal Admin");
            }
            else if (dealPermission == DealWrite && currentUserPermissions.CanWriteDeal())
            {
                permissionMessages.Add("Edit Deal Information");
            }
            else if ((dealPermission == DealRead || dealPermission == DealWrite) && currentUserPermissions.CanReadDeal())
            {
                permissionMessages.Add("Read Deal Information");
            }

            // Performance
            if (performancePermission == PerformanceWrite && currentUserPermissions.CanWritePerformance())
            {
                permissionMessages.Add("Edit Performance Data");
            }
            else if ((performancePermission == PerformanceRead || performancePermission == PerformanceWrite) && currentUserPermissions.CanReadPerformance())
            {
                permissionMessages.Add("Read Performance Data");
            }

            // Expenditures
            if (expenditurePermission == ExpendituresWrite && currentUserPermissions.CanWriteExpenditures())
            {
                permissionMessages.Add("Edit Expenditures");
            }
            else if ((expenditurePermission == ExpendituresRead || expenditurePermission == ExpendituresWrite) && currentUserPermissions.CanReadExpenditures())
            {
                permissionMessages.Add("Read Expenditures");
            }

            if (!permissionMessages.Any())
            {
                return permissionMessage;
            }
            else
            {
                permissionMessage += permissionMessages.Aggregate((acc, next) => acc + ", " + next);
                permissionMessage += ".";
                return permissionMessage;
            }
        }

        public static List<string> CanGrantPermissions(this List<string> permissions)
        {
            // Based on users current permissions, what can they grant others
            List<string> canGrant = new List<string>();

            if (permissions == null || !permissions.Any())
            {
                return canGrant;
            }

            var dealPermission = permissions.Where(x => !x.Contains("Deal.Admin")).FirstOrDefault(x => x.Contains("Deal."));
            var expenditurePermission = permissions.FirstOrDefault(x => x.Contains("Expenditures."));
            var performancePermission = permissions.FirstOrDefault(x => x.Contains("Performance."));
            var isDealAdmin = permissions.Exists(x => x.Contains("Deal.Admin"));

            // Deal
            if (isDealAdmin)
            {
                canGrant.AddRange(DealPermissions.Concat(PerformancePermissions).Concat(ExpendituresPermissions).Concat(new List<string>() { DealAdmin }));
                return canGrant;
            }
            else if (dealPermission == DealWrite)
            {
                canGrant.AddRange(new List<string>() { DealWrite, DealRead, DealNone });
            }
            else if (dealPermission == DealRead)
            {
                canGrant.AddRange(new List<string>() { DealRead, DealNone });
            }

            // Only deal admins can grant Expenditures and Performance permissions
            // Performance
            if (isDealAdmin)
            {
                canGrant.AddRange(new List<string>() { PerformanceWrite, PerformanceRead, PerformanceNone });
            }
            else
            {
                canGrant.Add(PerformanceNone);
            }

            // Expenditures
            if (isDealAdmin)
            {
                canGrant.AddRange(new List<string>() { ExpendituresWrite, ExpendituresRead, ExpendituresNone });
            }
            else
            {
                canGrant.Add(ExpendituresNone);
            }

            return canGrant;

        }

        public static string? GetDescription(this Enum enumValue)
        {
            //Look for DescriptionAttributes on the enum field
            object[]? attr = enumValue?.GetType().GetField(enumValue?.ToString())?
                .GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attr.Length > 0) // a DescriptionAttribute exists; use it
                return ((DescriptionAttribute)attr[0]).Description;

            //The above code is all you need if you always use DescriptionAttributes;
            //If you don't, the below code will semi-intelligently 
            //"humanize" an UpperCamelCased Enum identifier
            string? result = enumValue?.ToString();

            return result;
        }

        public static bool CanReadDeal(this List<string> permissions)
        {
            if (permissions == null || !permissions.Any())
            {
                return false;
            }

            List<string> canRead = new List<string>() {
                DealAdmin,
                DealWrite,
                DealRead };
            return permissions.ContainsAny(canRead);
        }

        public static bool CanWriteDeal(this List<string> permissions)
        {
            if (permissions == null || !permissions.Any())
            {
                return false;
            }

            List<string> canRead = new List<string>() {
                DealAdmin,
                DealWrite };
            return permissions.ContainsAny(canRead);
        }

        public static bool CanReadExpenditures(this List<string> permissions)
        {
            if (permissions == null || !permissions.Any())
            {
                return false;
            }

            List<string> canRead = new List<string>() {
                DealAdmin,
                ExpendituresWrite,
                ExpendituresRead
            };
            return permissions.ContainsAny(canRead);
        }

        public static bool CanWriteExpenditures(this List<string> permissions)
        {
            if (permissions == null || !permissions.Any())
            {
                return false;
            }

            List<string> canRead = new List<string>() {
                DealAdmin,
                ExpendituresWrite };
            return permissions.ContainsAny(canRead);
        }

        public static bool CanReadPerformance(this List<string> permissions)
        {
            if (permissions == null || !permissions.Any())
            {
                return false;
            }

            List<string> canRead = new List<string>() {
                DealAdmin,
                PerformanceRead,
                PerformanceWrite };
            return permissions.ContainsAny(canRead);
        }

        public static bool CanWritePerformance(this List<string> permissions)
        {
            if (permissions == null || !permissions.Any())
            {
                return false;
            }

            List<string> canRead = new List<string>() {
                DealAdmin,
                PerformanceWrite };
            return permissions.ContainsAny(canRead);
        }

        public static bool IsOnlyDealAdmin(this List<string> permissions)
        {
            if (permissions == null || !permissions.Any())
            {
                return false;
            }
            return permissions.Contains(DealAdmin);
        }
        public static bool IsOnlyDealReader(this List<string> permissions)
        {
            if (permissions == null || !permissions.Any())
            {
                return false;
            }
            return permissions.Contains(DealRead);
        }
        public static bool IsOnlyDealWriter(this List<string> permissions)
        {
            if (permissions == null || !permissions.Any())
            {
                return false;
            }
            return permissions.Contains(DealWrite);
        }

        public static bool ContainsAny(this List<string> existingPermissions, List<string> desiredPermissions)
        {
            if (existingPermissions == null || !existingPermissions.Any())
            {
                return false;
            }
            return existingPermissions.Any(x => desiredPermissions.Any(y => y == x));
        }
    }
}