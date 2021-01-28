namespace YAILSharp
{
    public class UserIdentity
    {
        public string Nickname { get; private set; }
        public string Username { get; private set; }
        public string Realname { get; private set; }

        public UserIdentity(string nickname, string username, string realname)
        {
            Nickname = nickname;
            Username = username;
            Realname = realname;
        }
    }
}