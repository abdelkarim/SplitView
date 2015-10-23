// Copyright (c) 2015 'Abdelkarim Sellamna' (abdelkarim.se [at] gmail [dot] com)
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WpfGhost.Internals;

namespace WpfGhost.Primitives
{
    /// <summary>
    /// 
    /// </summary>
    public class SplitterGrip : Thumb
    {
        #region "Constructors"

        /// <summary>
        /// Initializes static members of the <see cref="SplitterGrip"/> class.
        /// </summary>
        static SplitterGrip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitterGrip), new FrameworkPropertyMetadata(typeof(SplitterGrip)));
        }

        #endregion

        #region "Properties"

        #region Orientation

        /// <summary>
        /// Orientation Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey OrientationPropertyKey = DependencyProperty.RegisterReadOnly(
            "Orientation",
            typeof(Orientation),
            typeof(SplitterGrip),
            new FrameworkPropertyMetadata(Orientation.Horizontal));

        /// <summary>
        /// Identifies the <see cref="Orientation"/> read-only dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = OrientationPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a value that indicates the orientation of the <see cref="SplitterGrip"/>.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            internal set { SetValue(OrientationPropertyKey, value); }
        }

        #endregion

        #region IsCollapsed

        private static readonly DependencyPropertyKey IsCollapsedPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsCollapsed",
            typeof(bool),
            typeof(SplitterGrip),
            new FrameworkPropertyMetadata(BooleanBoxes.False));

        /// <summary>
        /// Identifies the <see cref="IsCollapsed"/> read-only property.
        /// </summary>
        public static readonly DependencyProperty IsCollapsedProperty = IsCollapsedPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a value that indicates whether the <see cref="SplitterGrip"/> is collapsed or not.
        /// </summary>
        /// <value>
        /// <c>True</c> if the <see cref="SplitterGrip"/> is collapsed, otherwise <c>false</c>.
        /// </value>
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            internal set { SetValue(IsCollapsedPropertyKey, BooleanBoxes.Box(value)); }
        }

        #endregion

        #endregion

        #region "Internal Properties"

        internal Popup Popup { get; private set; }

        #endregion
        
        #region "Methods"

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Popup = GetTemplateChild("PART_Popup") as Popup;
        }

        #endregion
    }
}