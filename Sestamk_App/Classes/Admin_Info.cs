namespace Sestamk.Classes
{
    /// <summary>
    /// REPLACED: Hardcoded admin credentials removed.
    /// The system now uses database-based admin accounts only.
    /// First-run setup creates the admin user in the DB.
    ///
    /// Migration guide for Login.cs:
    ///   BEFORE: if (username == Admin_Info.usernameadmin && password == Admin_Info.passwordadmin)
    ///   AFTER:  Remove the entire if-block. All authentication goes through the DB path.
    ///           Run Migration 004 to ensure a default admin exists in the Users table.
    /// </summary>
    [System.Obsolete("Hardcoded admin removed for security. Use DB-based admin. See migration 004.")]
    public static class Admin_Info
    {
        // Compilation will warn anywhere this is still referenced
        public static string usernameadmin => throw new System.InvalidOperationException(
            "Hardcoded admin credentials have been removed. Use database authentication.");
        public static string passwordadmin => throw new System.InvalidOperationException(
            "Hardcoded admin credentials have been removed. Use database authentication.");
    }
}
