# Committing Large Files to Git

This guide explains how to commit large files (like `rag_data_backup.tar.gz`) to Git using Git LFS.

## What is Git LFS?

Git LFS (Large File Storage) stores large files outside the main Git repository, keeping your repo size manageable while still tracking file versions.

## Setup (Already Done ✅)

1. ✅ Git LFS installed
2. ✅ Git LFS initialized in repository
3. ✅ Archive file tracked: `rag_data_backup.tar.gz`
4. ✅ `.gitignore` updated to allow the archive

## Committing the Large File

### Step 1: Add the file

```bash
git add rag_data_backup.tar.gz
```

This will upload the file to Git LFS (if configured) or prepare it for commit.

### Step 2: Commit normally

```bash
git commit -m "Add RAG data backup archive (162MB)"
```

### Step 3: Push to remote

```bash
git push origin main
# or
git push origin master
```

**Note:** The first push with Git LFS may take longer as it uploads the large file.

## Verify Git LFS is Working

Check if the file is tracked by LFS:

```bash
git lfs ls-files
```

Should show:
```
rag_data_backup.tar.gz
```

## For Your Partner (Pulling Large Files)

When your partner clones/pulls the repository:

1. **Make sure Git LFS is installed:**
   ```bash
   git lfs install
   ```

2. **Clone or pull normally:**
   ```bash
   git clone <repo-url>
   # or
   git pull
   ```

3. **Git LFS will automatically download the large file** when you checkout the branch.

## Alternative: Without Git LFS

If you don't want to use Git LFS, you have these options:

### Option 1: Use GitHub Releases
- Upload `rag_data_backup.tar.gz` as a GitHub Release asset
- Share the release URL with your partner
- File size limit: 2GB per file

### Option 2: External Storage
- Upload to cloud storage (Google Drive, Dropbox, etc.)
- Share the link
- Keep the link in a README or config file

### Option 3: Git Submodules
- Create a separate repository for large files
- Add it as a Git submodule
- More complex but keeps main repo clean

## Current Status

✅ **Git LFS is set up and ready to use**

You can now commit `rag_data_backup.tar.gz` normally - Git LFS will handle it automatically.

## Troubleshooting

### "File too large" error

If you get this error, make sure:
1. Git LFS is installed: `git lfs install`
2. File is tracked: `git lfs track "rag_data_backup.tar.gz"`
3. `.gitattributes` exists and contains the file

### Check Git LFS status

```bash
git lfs status
```

### Verify file is in LFS

```bash
git lfs ls-files
```

