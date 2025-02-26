namespace Svn2GitConsole.Interfaces
{
    public interface IGitService
    {
        void Checkout(string branch, bool force);

        void CheckoutAndCreateBranch(string branch);

        void CheckoutAndCreateBranchFromSvn(string branch);

        void CherryPick(string commitHash);

        void CherryPickMerge(string commit, int parentNumber = 1);

        void CreateBareRepo();

        void CreateTag(string tagName, string revision, string message);

        void CreateTagFromSvnRemote(string tagName, string message);

        void CreateTagWithCommit(string tagName, string commit, string message);

        void DeleteBranch(string branch, bool remote);

        void Fetch();

        string FindEquivalentCommitOnMaster(string commit);

        string FindMergeBase(string commit1, string commit2);

        void GarbageCollect();

        string GetBranchTrackInfo(string branch);

        string GetCommitAuthorEmail(string revision);

        string GetCommitAuthorName(string revision);

        string GetCommitDate(string revision);

        string GetCommitSubject(string revision);

        string GetConfigValue(string key);

        string GetGitConfigCommand();

        List<string> GetLocalBranches();

        List<string> GetRemoteBranches();

        List<string> GetTagsListWithHashes(string tagsPath);

        string GetUserEmail();

        string GetUserName();

        string GetWorkingDirectoryStatus();

        void RebaseBranch(string remoteBranch);

        string RunCommand(string command, bool exitOnError = true, bool printoutOutput = false);

        void SetAuthorsFile(string authorsFile);

        void SetConfigValue(string key, string value);

        void SetUserEmail(string userEmail);

        void SetUserName(string userName);

        void UnsetConfigValue(string key);

        bool VerifyTag(string tagName, string expectedCommit);
    }
}
