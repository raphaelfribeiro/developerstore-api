namespace Ambev.DeveloperEvaluation.Common.Security
{
    /// <summary>
    /// Defines the contract for user representation in the system.
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// Gets the user's unique identifier.
        /// </summary>
        /// <returns>The user ID as a string.</returns>
        public string Id { get; }

        /// <summary>
        /// Gets the username.
        /// </summary>
        /// <returns>The username.</returns>
        public string Username { get; }

        /// <summary>
        /// Gets the user's role in the system.
        /// </summary>
        /// <returns>The user's role as a string.</returns>
        public string Role { get; }
    }
}
