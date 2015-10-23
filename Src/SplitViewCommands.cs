// Copyright (c) 2015 'Abdelkarim Sellamna' (abdelkarim.se [at] gmail [dot] com)
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows.Input;

namespace WpfGhost
{
    /// <summary>
    /// Provides a standard set of <see cref="SplitView"/> related commands.
    /// </summary>
    public static class SplitViewCommands
    {
        #region "Fields"

        private static RoutedUICommand _expandCollapsePaneCommand;
        private static RoutedUICommand _splitVerticalCommand;
        private static RoutedUICommand _splitHorizontalCommand;
        private static RoutedUICommand _swapPanesCommand;
        private static RoutedUICommand _activateViewCommand;

        #endregion

        /// <summary>
        /// Gets a value that represents the <b>Activate View</b> command.
        /// </summary>
        public static RoutedUICommand ActivateViewCommand
        {
            get
            {
                if (_activateViewCommand == null)
                {
                    _activateViewCommand = new RoutedUICommand("Activate View", "ActivateView", typeof(SplitViewCommands));
                }

                return _activateViewCommand;
            }
        }

        /// <summary>
        /// Gets a value that represents the <b>Expand Collapse</b> command.
        /// </summary>
        public static RoutedUICommand ExpandCollapsePaneCommand
        {
            get
            {
                if (_expandCollapsePaneCommand == null)
                {
                    _expandCollapsePaneCommand = new RoutedUICommand("Expand/Collapse Pane", "ExpandCollapsePaneCommand", typeof(SplitViewCommands));
                }
                return _expandCollapsePaneCommand;
            }
        }

        /// <summary>
        /// Gets a value that represents the <b>Split Vertical</b> command.
        /// </summary>
        public static RoutedUICommand SplitVerticalCommand
        {
            get
            {
                if (_splitVerticalCommand == null)
                {
                    _splitVerticalCommand = new RoutedUICommand("Split Vertical", "SplitVerticalCommand", typeof(SplitViewCommands));
                }
                return _splitVerticalCommand;
            }
        }

        /// <summary>
        /// Gets a value that represents the <b>Split Horizontal</b> command.
        /// </summary>
        public static RoutedUICommand SplitHorizontalCommand
        {
            get
            {
                if (_splitHorizontalCommand == null)
                {
                    _splitHorizontalCommand = new RoutedUICommand("Split Horizontal", "SplitHorizontalCommand", typeof(SplitViewCommands));
                }
                return _splitHorizontalCommand;
            }
        }

        /// <summary>
        /// Gets a value that represents the <b>Swap Panes</b> command.
        /// </summary>
        public static RoutedUICommand SwapPanesCommand
        {
            get
            {
                if (_swapPanesCommand == null)
                {
                    _swapPanesCommand = new RoutedUICommand("Swap Panes", "SwapPanesCommand", typeof(SplitViewCommands));
                }
                return _swapPanesCommand;
            }
        }
    }
}