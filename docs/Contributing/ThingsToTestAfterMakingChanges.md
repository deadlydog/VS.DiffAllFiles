# Things to test after making changes or new versions

## Settings pages

- Make sure you can open the Diff All Files Settings pages without any errors.
- Make sure the button in the Settings window to open the TF Diff Tool Configuration window still works (i.e. that the location of where we expect to find TF.exe hasn't changed).

## Diff functionality

- Test diffing both TFVC and Git changes to make sure they work as expected.
  - Make sure you can diff both Staged / Included and Unstaged / Excluded files without issue.
  - Ensure the changes that show in the diff are actually correct.
  It should only show real changes, not the entire file being changed, etc.
- Test diffing both with the Combined mode on and off.
- Ensure the correct configured diff tool gets used for doing the diffs.