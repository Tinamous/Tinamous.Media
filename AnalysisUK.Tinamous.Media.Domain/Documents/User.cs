using System;

namespace AnalysisUK.Tinamous.Media.Domain.Documents
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string FullUserName { get; set; }
        public Guid AccountId { get; set; }
    }
}