using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace VS_DiffAllFiles.StructuresAndEnums
{
	public static class GitHelper
	{
		// Had to manually sign the LibGit2Sharp.dll myself as per http://www.codeproject.com/Tips/341645/Referenced-assembly-does-not-have-a-strong-name

		/// <summary>
		/// Returns true if the given path is in a Git repository; false if not.
		/// </summary>
		/// <param name="path">The full path to the file or directory.</param>
		/// <returns></returns>
		public static bool IsPathInGitRepository(string path)
		{
			return GetGitRepositoryPath(path) != null;
		}

		/// <summary>
		/// Gets the git repository path based on the path of the given file or directory inside of the repository.
		/// <para>Returns null if the given path is not inside of a Git repository.</para>
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static string GetGitRepositoryPath(string path)
		{
			// If an invalid path was given, just return null.
			if (!Directory.Exists(path) && !File.Exists(path))
				return null;

			// Return the path of the Git repository, if this path is actually within one.
			// https://github.com/libgit2/libgit2sharp/issues/818#issuecomment-54760613
			return Repository.Discover(Path.GetFullPath(path));
		}

		/// <summary>
		/// Copies the specific version of a file to the specified path.
		/// <para>Returns true if the version was found and the file copy performed; false if not.</para>
		/// </summary>
		/// <param name="filePathInRepository">The full file path of the file in the repository.</param>
		/// <param name="filePathToCopyFileTo">The file path to copy the specific version of the file to.</param>
		/// <param name="sha">The commit containing the file version to obtain. If this is null, the last commit of the file will be used.</param>
		/// <returns></returns>
		public static bool GetSpecificVersionOfFile(string filePathInRepository, string filePathToCopyFileTo, string sha = null)
		{
			// Get the path to the Git repository from the file path.
			var repositoryPath = GetGitRepositoryPath(filePathInRepository);
			if (repositoryPath == null)
				return false;

			// Remove the Git repository path from the file path to get the file's path relative to the Git repository.
			var repoRootDirectory = repositoryPath.Replace(@".git\", string.Empty);
			var relativeFilePath = filePathInRepository.Replace(repoRootDirectory, string.Empty);

			// Connect to the Git repository.
			using (var repository = new Repository(repositoryPath))
			{
				// Find the specific commit containing the version of the file to obtain.
				var commit = (sha == null) ? GetLastCommitOfFile(repository, relativeFilePath) : repository.Lookup<Commit>(sha);
				if (commit == null) return false;

				// Get the file from the commit.
				var treeEntry = commit[relativeFilePath];
				if (treeEntry == null) return false;

				var blob = treeEntry.Target as Blob;
				if (blob == null) return false;

				// Write a copy of the file to the specified file path.
				File.WriteAllText(filePathToCopyFileTo, blob.GetContentText());
			}

			return true;
		}

		private static Commit GetLastCommitOfFile(Repository repo, string filePathRelativeToRepository)
		{
			// Original algorithm taken from https://github.com/libgit2/libgit2sharp/issues/89

			var commit = repo.Head.Tip;
			var treeEntry = commit[filePathRelativeToRepository];
			if (treeEntry == null) return null;
			var gitObj = treeEntry.Target;

			var set = new HashSet<string>();
			var queue = new Queue<Commit>();
			queue.Enqueue(commit);
			set.Add(commit.Sha);

			while (queue.Count > 0)
			{
				commit = queue.Dequeue();
				var go = false;
				foreach (var parent in commit.Parents)
				{
					var tree = parent[filePathRelativeToRepository];
					if (tree == null)
						continue;
					var eq = tree.Target.Sha == gitObj.Sha;
					if (eq && set.Add(parent.Sha))
						queue.Enqueue(parent);
					go = go || eq;
				}
				if (!go)
					break;
			}

			return commit;
		}
	}
}
