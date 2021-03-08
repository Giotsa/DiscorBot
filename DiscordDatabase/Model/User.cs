using System;

namespace DiscordDatabase
{
    public class User
    {
        public ulong Id { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public string AvatarUrl { get; set; }
        public string BackgroundUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset JoinedAt { get; set; }
    }
}