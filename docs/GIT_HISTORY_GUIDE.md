# Git history and collaboration notes

The uploaded Activity 3 ZIP did not contain a `.git` directory, so this Activity 4 source archive cannot include or reconstruct the original commit history. To preserve your real course repository history, apply the updated source files to your existing local Git working copy rather than replacing its `.git` directory.

The rubric does not require a fixed commit count. It asks for meaningful development history. A reasonable sequence, adapted to the changes you actually make, is:

1. `Switch persistence provider from SQLite to PostgreSQL/Npgsql`
2. `Add PostgreSQL schema migration and integrity constraints`
3. `Implement transactional single-item and multi-item checkout/return`
4. `Add student registration, updates and eligibility management`
5. `Add equipment registration and descriptive updates`
6. `Add borrowing history and search/filter/sort queries`
7. `Connect Activity 4 workflows to Avalonia MVVM screens`
8. `Document setup, acceptance evidence and Activity 4 architecture`

Example workflow:

```powershell
git status
git add src\EquipmentBorrowing.Infrastructure
git commit -m "Switch persistence provider to PostgreSQL"
```

Stage and commit the actual logical changes separately. Do not create commits that claim a test passed unless you ran that test. Before submitting, export a concise history for the archive if your instructor needs it:

```powershell
git log --oneline --decorate --graph -30 > docs\git-history.txt
```

Both partners should understand the changes and identify both full names and student IDs in the private submission comment requested by the brief.
