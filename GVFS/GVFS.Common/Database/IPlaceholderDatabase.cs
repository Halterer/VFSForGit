﻿using System.Collections.Generic;

namespace GVFS.Common.Database
{
    public interface IPlaceholderDatabase
    {
        int Count { get; }

        void GetAllEntries(out List<IPlaceholderData> filePlaceholders, out List<IPlaceholderData> folderPlaceholders);
        HashSet<string> GetAllFilePaths();

        void AddPartialFolder(string path);
        void AddExpandedFolder(string path);
        void AddPossibleTombstoneFolder(string path);

        void AddFile(string path, string sha);

        void Remove(string path);
    }
}
