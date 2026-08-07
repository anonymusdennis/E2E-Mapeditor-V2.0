using System;
using System.Collections.Generic;

namespace E2EApi.Features
{
    /// <summary>
    /// Unified editor undo/redo covering both vanilla block edits and mod tool
    /// actions (tile stamps, connected brushes, fences, trigger links, ...).
    ///
    /// Vanilla edits are tracked by polling the game's
    /// <c>BuildingInstructionManager</c> user-instruction counters each frame;
    /// each new vanilla edit pushes a marker entry so interleaved histories
    /// stay ordered. Undoing a marker calls the game's own
    /// <c>LevelEditor_Controller.UndoLast()</c>; mod entries carry explicit
    /// undo/redo closures supplied by the tool that recorded them.
    /// </summary>
    public static class UndoHistory
    {
        /// <summary>One reversible action in the unified history.</summary>
        public class Entry
        {
            public string Label;
            /// <summary>True for vanilla-edit markers (undo via the game).</summary>
            public bool Vanilla;
            public Action Undo;
            public Action Redo;
        }

        private const int MaxEntries = 200;

        private static readonly List<Entry> _undo = new List<Entry>();
        private static readonly List<Entry> _redo = new List<Entry>();
        private static bool _initialised;

        // Vanilla instruction counters as of the previous poll.
        private static int _lastTotal = -1;
        private static int _lastTotalValid = -1;

        // Set by UndoTrackerPatch when the game's own UndoLast/RedoLast run
        // (toolbar buttons); lets the poll distinguish "redo" from "new edit"
        // (both grow m_iTotal). Not counted while we drive vanilla ourselves.
        private static int _noteUndoCalls;
        private static int _noteRedoCalls;
        private static bool _driving;

        /// <summary>Bumped on every history change (for UI refresh).</summary>
        public static int Version { get; private set; }

        /// <summary>Called by the tracker patch after LevelEditor_Controller.UndoLast.</summary>
        internal static void NoteVanillaUndoCall()
        {
            if (!_driving) _noteUndoCalls++;
        }

        /// <summary>Called by the tracker patch after LevelEditor_Controller.RedoLast.</summary>
        internal static void NoteVanillaRedoCall()
        {
            if (!_driving) _noteRedoCalls++;
        }

        public static bool CanUndo => _undo.Count > 0;
        public static bool CanRedo => _redo.Count > 0;
        public static int UndoCount => _undo.Count;
        public static int RedoCount => _redo.Count;

        public static void Initialise()
        {
            if (_initialised)
            {
                return;
            }
            _initialised = true;
            Editor.Patches.UndoTrackerPatchGroup.EnsurePatched();
            Events.GameEvents.EditorExited += Clear;
            Persistence.ModExtras.EnsurePatched();
            Persistence.ModExtras.Loaded += OnLoaded;
        }

        private static void OnLoaded(Persistence.ModExtras extras)
        {
            Clear();
        }

        /// <summary>Label of the next action Undo would revert, or null.</summary>
        public static string PeekUndoLabel =>
            _undo.Count > 0 ? _undo[_undo.Count - 1].Label : null;

        /// <summary>Label of the next action Redo would replay, or null.</summary>
        public static string PeekRedoLabel =>
            _redo.Count > 0 ? _redo[_redo.Count - 1].Label : null;

        /// <summary>
        /// Record a mod action that has ALREADY been applied. Clears the redo
        /// stack (a new action invalidates redo, matching vanilla behaviour).
        /// </summary>
        public static void Push(string label, Action undo, Action redo)
        {
            if (undo == null || redo == null)
            {
                return;
            }
            _undo.Add(new Entry { Label = label, Undo = undo, Redo = redo });
            _redo.Clear();
            TrimToCap();
            Version++;
        }

        /// <summary>Undo the most recent action (vanilla or mod). False if none.</summary>
        public static bool Undo()
        {
            // Absorb any vanilla edits made since the last poll first, so the
            // ordering is accurate even when called before Tick this frame.
            PollVanilla();
            if (_undo.Count == 0)
            {
                return false;
            }
            var entry = _undo[_undo.Count - 1];
            _undo.RemoveAt(_undo.Count - 1);
            if (entry.Vanilla)
            {
                if (!VanillaUndo())
                {
                    // Vanilla says it can't undo (stack diverged) — drop marker.
                    Version++;
                    return false;
                }
            }
            else
            {
                SafeInvoke(entry.Undo, entry.Label, "undo");
            }
            _redo.Add(entry);
            Version++;
            return true;
        }

        /// <summary>Redo the most recently undone action. False if none.</summary>
        public static bool Redo()
        {
            PollVanilla();
            if (_redo.Count == 0)
            {
                return false;
            }
            var entry = _redo[_redo.Count - 1];
            _redo.RemoveAt(_redo.Count - 1);
            if (entry.Vanilla)
            {
                if (!VanillaRedo())
                {
                    Version++;
                    return false;
                }
            }
            else
            {
                SafeInvoke(entry.Redo, entry.Label, "redo");
            }
            _undo.Add(entry);
            Version++;
            return true;
        }

        /// <summary>Forget everything (level load / editor exit).</summary>
        public static void Clear()
        {
            _undo.Clear();
            _redo.Clear();
            _lastTotal = -1;
            _lastTotalValid = -1;
            Version++;
        }

        /// <summary>
        /// Poll vanilla edit counters once per editor frame. Call from the mod's
        /// editor Update loop.
        /// </summary>
        public static void Tick()
        {
            if (!Events.GameEvents.IsEditorActive)
            {
                return;
            }
            PollVanilla();
        }

        /// <summary>
        /// Detect vanilla edits/undos/redos performed since the previous poll
        /// (via toolbar buttons or the game's own hotkeys) and mirror them into
        /// the unified stacks so ordering stays consistent.
        ///
        /// One marker = one undo GROUP (a user gesture): a single UndoLast()
        /// call reverts everything between a Start_Undo/End_Undo command pair,
        /// or exactly one instruction outside such a pair.
        /// </summary>
        private static void PollVanilla()
        {
            var manager = BuildingInstructionManager.GetInstance();
            if (manager == null || manager.m_LevelInstructions == null ||
                manager.m_LevelInstructions.m_UserInstructions == null)
            {
                return;
            }
            var list = manager.m_LevelInstructions.m_UserInstructions;
            int total = list.m_iTotal;
            int totalValid = list.m_iTotalValid;

            if (_lastTotal < 0)
            {
                // First poll of this session: adopt counters silently.
                _lastTotal = total;
                _lastTotalValid = totalValid;
                return;
            }
            if (total == _lastTotal && totalValid == _lastTotalValid)
            {
                _noteUndoCalls = 0;
                _noteRedoCalls = 0;
                return;
            }

            if (_noteUndoCalls > 0 && total < _lastTotal)
            {
                // Vanilla undo via the game's own button: one marker per call.
                for (int i = 0; i < _noteUndoCalls; i++)
                {
                    MoveTopVanillaMarker(_undo, _redo);
                }
                Version++;
            }
            else if (_noteRedoCalls > 0 && total > _lastTotal)
            {
                // Vanilla redo via the game's own button: one marker per call.
                for (int i = 0; i < _noteRedoCalls; i++)
                {
                    MoveTopVanillaMarker(_redo, _undo);
                }
                Version++;
            }
            else if (total > _lastTotal && totalValid == total)
            {
                // New vanilla edits (AddToCurrentList truncates redo, so
                // totalValid == total after a genuine new edit). Count complete
                // gesture groups; an unfinished drag (open Start_Undo) defers
                // until its End_Undo arrives on a later frame.
                int groups;
                int consumedUpTo;
                CountGroups(manager, _lastTotal, total, out groups, out consumedUpTo);
                for (int i = 0; i < groups; i++)
                {
                    _undo.Add(new Entry { Label = "map edit", Vanilla = true });
                }
                if (groups > 0)
                {
                    // Vanilla wiped its own redo tail; mirror by dropping vanilla
                    // markers from our redo stack (mod redo entries stay valid).
                    for (int i = _redo.Count - 1; i >= 0; i--)
                    {
                        if (_redo[i].Vanilla)
                        {
                            _redo.RemoveAt(i);
                        }
                    }
                    TrimToCap();
                    Version++;
                }
                _noteUndoCalls = 0;
                _noteRedoCalls = 0;
                _lastTotal = consumedUpTo;
                _lastTotalValid = totalValid;
                return;
            }
            else if (total < _lastTotal)
            {
                // Untracked shrink (history reset / level reload): recount the
                // vanilla ground truth and drop excess markers.
                int applied, dummy;
                CountGroups(manager, 0, total, out applied, out dummy);
                RemoveExcessVanillaMarkers(_undo, applied);
                Version++;
            }

            _noteUndoCalls = 0;
            _noteRedoCalls = 0;
            _lastTotal = total;
            _lastTotalValid = totalValid;
        }

        /// <summary>
        /// Counts complete undo groups in user instructions [from, to). A group
        /// is a Start_Undo..End_Undo command pair (one user gesture, reverted by
        /// a single UndoLast) or one standalone instruction at depth 0.
        /// <paramref name="consumedUpTo"/> stops before an unterminated group so
        /// it can be counted once complete.
        /// </summary>
        private static void CountGroups(BuildingInstructionManager manager,
            int from, int to, out int groups, out int consumedUpTo)
        {
            groups = 0;
            consumedUpTo = from;
            var instructions = manager.m_LevelInstructions.m_UserInstructions.m_Instructions;
            var commands = manager.m_LevelInstructions.m_CommandList;
            int depth = 0;
            for (int i = from; i < to && i < instructions.Count; i++)
            {
                var element = instructions[i];
                bool isStart = false, isEnd = false;
                if (element.m_Type == BaseBuildInstruction.InstructionTypeEnum.Command &&
                    commands != null && element.m_Index >= 0 &&
                    element.m_Index < commands.m_Instructions.Count)
                {
                    var cmd = commands.m_Instructions[element.m_Index].m_Commmand;
                    isStart = cmd == BuildingInstructionManager.CommandsEnum.Start_Undo;
                    isEnd = cmd == BuildingInstructionManager.CommandsEnum.End_Undo;
                }

                if (isStart)
                {
                    depth++;
                }
                else if (isEnd)
                {
                    if (depth > 0) depth--;
                    if (depth == 0)
                    {
                        groups++;
                        consumedUpTo = i + 1;
                    }
                }
                else if (depth == 0)
                {
                    groups++;
                    consumedUpTo = i + 1;
                }
            }
        }

        private static int CountVanillaMarkers(List<Entry> stack)
        {
            int count = 0;
            foreach (var entry in stack)
            {
                if (entry.Vanilla) count++;
            }
            return count;
        }

        /// <summary>Drop newest vanilla markers until at most <paramref name="keep"/> remain.</summary>
        private static void RemoveExcessVanillaMarkers(List<Entry> stack, int keep)
        {
            int have = CountVanillaMarkers(stack);
            for (int i = stack.Count - 1; i >= 0 && have > keep; i--)
            {
                if (stack[i].Vanilla)
                {
                    stack.RemoveAt(i);
                    have--;
                }
            }
        }

        /// <summary>
        /// Move the newest vanilla marker from one stack to the other, mirroring
        /// an out-of-band vanilla undo/redo. Mod entries above it stay put — the
        /// vanilla button only touched vanilla state.
        /// </summary>
        private static void MoveTopVanillaMarker(List<Entry> from, List<Entry> to)
        {
            for (int i = from.Count - 1; i >= 0; i--)
            {
                if (from[i].Vanilla)
                {
                    var entry = from[i];
                    from.RemoveAt(i);
                    to.Add(entry);
                    return;
                }
            }
        }

        private static bool VanillaUndo()
        {
            var editor = LevelEditor_Controller.GetInstance();
            var manager = BuildingInstructionManager.GetInstance();
            if (editor == null || manager == null || !manager.CanUndo())
            {
                return false;
            }
            _driving = true;
            try
            {
                editor.UndoLast();
            }
            finally
            {
                _driving = false;
            }
            RefreshCounters();
            return true;
        }

        private static bool VanillaRedo()
        {
            var editor = LevelEditor_Controller.GetInstance();
            var manager = BuildingInstructionManager.GetInstance();
            if (editor == null || manager == null || !manager.CanRedo())
            {
                return false;
            }
            _driving = true;
            try
            {
                editor.RedoLast();
            }
            finally
            {
                _driving = false;
            }
            RefreshCounters();
            return true;
        }

        /// <summary>Adopt current counters (used after we drive vanilla ourselves).</summary>
        private static void RefreshCounters()
        {
            var manager = BuildingInstructionManager.GetInstance();
            if (manager == null || manager.m_LevelInstructions == null ||
                manager.m_LevelInstructions.m_UserInstructions == null)
            {
                return;
            }
            _lastTotal = manager.m_LevelInstructions.m_UserInstructions.m_iTotal;
            _lastTotalValid = manager.m_LevelInstructions.m_UserInstructions.m_iTotalValid;
        }

        private static void TrimToCap()
        {
            while (_undo.Count > MaxEntries)
            {
                _undo.RemoveAt(0);
            }
        }

        private static void SafeInvoke(Action action, string label, string what)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Log.Error($"undo history: {what} \"{label}\" failed: {ex.Message}");
            }
        }
    }
}
