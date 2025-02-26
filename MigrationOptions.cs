namespace Svn2GitConsole
{
    public class MigrationOptions
    {
        public string Authors { get; set; }

        public List<string> Branches { get; set; } = new List<string>();

        public string EmailDomain { get; set; } = "example.com";

        public List<string> Exclude { get; set; } = new List<string>();

        public string GetSvnRepoName
        {
            get
            {
                return ExtractRepoName(SvnRepoUrl);
            }
        }

        public bool Metadata { get; set; }

        public bool NoMinimizeUrl { get; set; }

        public string? OutputDirectory { get; set; }

        public string? RebaseBranch { get; set; }

        //public string Password { get; set; }

        public string Revision { get; set; }

        public bool RootIsTrunk { get; set; }

        public string SvnRepoUrl { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public string Trunk { get; set; }

        public string Username { get; set; }

        public bool Verbose { get; set; }

        private string ExtractRepoName(string svnRepoUrl)
        {
            if (string.IsNullOrEmpty(svnRepoUrl))
            {
                return string.Empty;
            }

            // 1. Using Uri   Handles various URL formats
            if (Uri.TryCreate(svnRepoUrl, UriKind.Absolute, out Uri uri))
            {
                string path = uri.AbsolutePath;

                // Remove leading/trailing slashes and split the path
                string[] segments = path.Trim('/').Split('/');

                // Check if there are any segments
                if (segments.Length > 0)
                {
                    return segments[segments.Length - 1]; // Return the last segment
                }
            }

            return string.Empty;
        }
    }
}
