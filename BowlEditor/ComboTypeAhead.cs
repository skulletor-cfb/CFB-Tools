namespace BowlEditor
{
    /// <summary>
    /// Makes a grid's dropdowns pick on Enter (or a click) rather than on every key. A closed WinForms dropdown changes
    /// its value as you type, so the first key opens the list instead; typing then only moves the highlight (T, T again
    /// for the next T team), Enter or a click picks and commits, and closing the list any other way (Escape, clicking
    /// away) puts the old value back.
    /// </summary>
    internal static class ComboTypeAhead
    {
        public static void Attach(DataGridView grid)
        {
            var hooked = new HashSet<ComboBox>();
            int indexWhenOpened = -1;
            bool picked = false;

            grid.EditingControlShowing += (s, e) =>
            {
                // the grid reuses one editing control per column type; hook it once
                if (e.Control is not ComboBox combo || !hooked.Add(combo))
                {
                    return;
                }

                combo.KeyDown += (_, k) =>
                {
                    bool navigates = char.IsLetterOrDigit((char)k.KeyValue) || k.KeyCode is Keys.Up or Keys.Down;
                    if (!combo.DroppedDown && navigates && !k.Alt && !k.Control)
                    {
                        combo.DroppedDown = true;
                    }
                };
                combo.DropDown += (_, _) =>
                {
                    indexWhenOpened = combo.SelectedIndex;
                    picked = false;
                };
                combo.SelectionChangeCommitted += (_, _) =>
                {
                    picked = true;
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                };
                combo.DropDownClosed += (_, _) =>
                {
                    if (!picked && combo.SelectedIndex != indexWhenOpened)
                    {
                        combo.SelectedIndex = indexWhenOpened;
                    }
                };
            };
        }
    }
}
