namespace Core.Utilities.Security.Hashing
{
    public interface IHashingHelper
    {
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
    }
}