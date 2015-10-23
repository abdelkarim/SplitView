// Copyright (c) 2015 'Abdelkarim Sellamna' (abdelkarim.se [at] gmail [dot] com)
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WpfGhost.Internals;
using WpfGhost.Primitives;

namespace WpfGhost
{
    /// <summary>
    /// 
    /// </summary>
    public class SplitView : FrameworkElement
    {
        #region "Constants"

        private Rect _emptyRect = new Rect();

        #endregion

        #region "Fields"

        private FrameworkElement _view1Container;
        private FrameworkElement _view2Container;
        private SplitterGrip _grip;
        private readonly UIElementCollection _visualChildren;
        private double _gripOffset = double.NaN; // in percent
        private double _previousGripOffset;
        private bool _isView1Collapsed;
        private bool _isView2Collapsed;

        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes static members of the <see cref="SplitView"/> class.
        /// </summary>
        static SplitView()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(SplitView), new FrameworkPropertyMetadata(BooleanBoxes.True));
            InitCommands();
            InitEventHandlers();
        }

        /// <summary>
        /// Initializes instance members of the <see cref="SplitView"/> class.
        /// </summary>
        public SplitView()
        {
            InitSplitterHandle();

            _visualChildren = new UIElementCollection(this, this)
            {
                _grip
            };
            InvalidateOrientationProperties();
        }

        #endregion

        #region "Events"

        #region ActiveViewChanged

        /// <summary>
        /// ActiveViewChanged Routed Event
        /// </summary>
        public static readonly RoutedEvent ActiveViewChangedEvent = EventManager.RegisterRoutedEvent("ActiveViewChanged",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitView));

        /// <summary>
        /// Occurs when ...
        /// </summary>
        public event RoutedEventHandler ActiveViewChanged
        {
            add { AddHandler(ActiveViewChangedEvent, value); }
            remove { RemoveHandler(ActiveViewChangedEvent, value); }
        }

        /// <summary>
        /// A helper method to raise the ActiveViewChanged event.
        /// </summary>
        protected RoutedEventArgs RaiseActiveViewChangedEvent()
        {
            return RaiseActiveViewChangedEvent(this);
        }

        /// <summary>
        /// A static helper method to raise the ActiveViewChanged event on a target element.
        /// </summary>
        /// <param name="target">UIElement or ContentElement on which to raise the event</param>
        internal static RoutedEventArgs RaiseActiveViewChangedEvent(SplitView target)
        {
            if (target == null) return null;

            var rags = new RoutedEventArgs { RoutedEvent = ActiveViewChangedEvent };
            target.RaiseEvent(rags);
            return rags;
        }

        #endregion

        #endregion

        #region "Internal & private Properties"

        private FrameworkElement InternalView1
        {
            get { return IsReversed ? View2 : View1; }
        }

        private FrameworkElement InternalView2
        {
            get { return IsReversed ? View1 : View2; }
        }

        internal double GripOffset
        {
            get { return _gripOffset; }
            set { _gripOffset = value; }
        }

        private double Density { get; set; }

        #endregion

        #region "Properties"

        #region StartupMode

        /// <summary>
        /// Identifies the <see cref="StartupMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StartupModeProperty = DependencyProperty.Register(
            "StartupMode",
            typeof(StartupMode),
            typeof(SplitView),
            new FrameworkPropertyMetadata(StartupMode.Dual, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the StartupMode property. This is a dependency property.
        /// </summary>
        /// <value>
        ///
        /// </value>
        [Bindable(true)]
        public StartupMode StartupMode
        {
            get { return (StartupMode)GetValue(StartupModeProperty); }
            set { SetValue(StartupModeProperty, value); }
        }

        #endregion

        #region Orientation

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(SplitView),
            new FrameworkPropertyMetadata(Orientation.Horizontal,
                FrameworkPropertyMetadataOptions.AffectsArrange,
                (o, e) =>
                {
                    var splitContainer = o as SplitView;
                    if (splitContainer == null || splitContainer._grip == null)
                        return;

                    splitContainer._grip.Orientation = (Orientation)e.NewValue;
                    splitContainer.InvalidateOrientationProperties();
                }),
            IsValidOrientation);

        private static bool IsValidOrientation(object value)
        {
            var orientation = (Orientation)value;
            return orientation == Orientation.Horizontal || orientation == Orientation.Vertical;
        }

        /// <summary>
        /// Gets or sets the Orientation property. This is a dependency property.
        /// </summary>
        /// <value>
        ///
        /// </value>
        [Bindable(true)]
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        #endregion

        #region IsReversed

        /// <summary>
        /// Identifies the <see cref="IsReversed"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsReversedProperty = DependencyProperty.Register(
            "IsReversed",
            typeof(bool),
            typeof(SplitView),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets the IsReversed property. This is a dependency property.
        /// </summary>
        /// <value>
        ///
        /// </value>
        [Bindable(true)]
        public bool IsReversed
        {
            get { return (bool)GetValue(IsReversedProperty); }
            set { SetValue(IsReversedProperty, value); }
        }

        #endregion

        #region IsView1Active

        /// <summary>
        /// IsView1Active Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey IsView1ActivePropertyKey
            = DependencyProperty.RegisterReadOnly("IsView1Active", typeof(bool), typeof(SplitView),
                new FrameworkPropertyMetadata(BooleanBoxes.False));

        public static readonly DependencyProperty IsView1ActiveProperty
            = IsView1ActivePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the IsView1Active property. This dependency property 
        /// indicates ....
        /// </summary>
        public bool IsView1Active
        {
            get { return (bool)GetValue(IsView1ActiveProperty); }
            private set { SetValue(IsView1ActivePropertyKey, BooleanBoxes.Box(value)); }
        }

        #endregion

        #region IsView2Active

        /// <summary>
        /// IsView2Active Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey IsView2ActivePropertyKey = DependencyProperty.RegisterReadOnly(
            "IsView2Active",
            typeof(bool),
            typeof(SplitView),
            new FrameworkPropertyMetadata(BooleanBoxes.False));

        public static readonly DependencyProperty IsView2ActiveProperty
            = IsView2ActivePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the IsView2Active property. This dependency property 
        /// indicates ....
        /// </summary>
        public bool IsView2Active
        {
            get { return (bool)GetValue(IsView2ActiveProperty); }
            private set { SetValue(IsView2ActivePropertyKey, BooleanBoxes.Box(value)); }
        }


        #endregion

        #region IsCollapsed

        private static readonly DependencyPropertyKey IsCollapsedPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsCollapsed",
            typeof(bool),
            typeof(SplitView),
            new FrameworkPropertyMetadata(
                false,
                (o, args) =>
                {
                    var splitContainer = o as SplitView;
                    if (splitContainer == null || splitContainer._grip == null)
                        return;

                    splitContainer._grip.IsCollapsed = splitContainer.IsCollapsed;

                    if (splitContainer.IsCollapsed)
                        splitContainer.ActivateView("View1");

                    splitContainer.InvalidateFocus();
                }));

        public static readonly DependencyProperty IsCollapsedProperty = IsCollapsedPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the IsExpanded property. This dependency property 
        /// indicates ....
        /// </summary>
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            internal set { SetValue(IsCollapsedPropertyKey, value); }
        }

        #endregion

        #region View1Header

        /// <summary>
        /// Identifies the <see cref="View1Header"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty View1HeaderProperty = DependencyProperty.Register(
            "View1Header",
            typeof(object),
            typeof(SplitView),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the View1Header property. This is a dependency property.
        /// </summary>
        /// <value>
        ///
        /// </value>
        [Bindable(true)]
        public object View1Header
        {
            get { return (object)GetValue(View1HeaderProperty); }
            set { SetValue(View1HeaderProperty, value); }
        }

        #endregion

        #region View2Header

        /// <summary>
        /// Identifies the <see cref="View2Header"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty View2HeaderProperty = DependencyProperty.Register(
            "View2Header",
            typeof(object),
            typeof(SplitView),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the View2Header property. This is a dependency property.
        /// </summary>
        /// <value>
        ///
        /// </value>
        [Bindable(true)]
        public object View2Header
        {
            get { return (object)GetValue(View2HeaderProperty); }
            set { SetValue(View2HeaderProperty, value); }
        }

        #endregion

        #region IsHorizontal

        /// <summary>
        /// IsHorizontal Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey IsHorizontalPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsHorizontal",
            typeof(bool),
            typeof(SplitView),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsHorizontalProperty
            = IsHorizontalPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the IsHorizontal property. This dependency property 
        /// indicates ....
        /// </summary>
        public bool IsHorizontal
        {
            get { return (bool)GetValue(IsHorizontalProperty); }
            private set { SetValue(IsHorizontalPropertyKey, value); }
        }

        #endregion

        #region IsVertical

        /// <summary>
        /// IsVertical Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey IsVerticalPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsVertical",
            typeof(bool),
            typeof(SplitView),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsVerticalProperty
            = IsVerticalPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the IsVertical property. This dependency property 
        /// indicates ....
        /// </summary>
        public bool IsVertical
        {
            get { return (bool)GetValue(IsVerticalProperty); }
            private set { SetValue(IsVerticalPropertyKey, value); }
        }

        #endregion

        public FrameworkElement View1
        {
            get { return _view1Container; }
            set
            {
                if (_view1Container == value)
                    throw new NotSupportedException("");

                UpdateView(_view1Container, value);
                _view1Container = value;
            }
        }

        public FrameworkElement View2
        {
            get { return _view2Container; }
            set
            {
                if (_view2Container == value)
                    throw new NotSupportedException("");

                UpdateView(_view2Container, value);
                _view2Container = value;
            }
        }

        protected override int VisualChildrenCount
        {
            get
            {
                if (_visualChildren == null)
                    return 0;

                return _visualChildren.Count;
            }
        }

        //private double _verticalOffset;
        //public double VerticalOffset
        //{
        //    get { return _verticalOffset; }
        //    set
        //    {
        //        _gripOffset = value;
        //    }
        //}

        #endregion

        #region "Static Methods"

        private static void InitEventHandlers()
        {
            EventManager.RegisterClassHandler(typeof(SplitView), Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocusHandler));
            EventManager.RegisterClassHandler(typeof(SplitView), Keyboard.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnLostKeyboardFocusHandler));
        }

        private static void OnGotKeyboardFocusHandler(object sender, KeyboardFocusChangedEventArgs e)
        {
            var focus = e.NewFocus as DependencyObject;
            if (focus == null)
                return;

            var container = (SplitView)sender;

            if (container.View1 != null && container.View1.IsAncestorOf(focus))
                container.IsView1Active = true;

            if (container.View2 != null && container.View2.IsAncestorOf(focus))
                container.IsView2Active = true;
        }

        private static void OnLostKeyboardFocusHandler(object sender, KeyboardFocusChangedEventArgs e)
        {
            var focus = e.NewFocus as DependencyObject;
            if (focus == null)
                return;

            var container = (SplitView)sender;

            if (container.View1 != null && container.View1.IsAncestorOf(focus))
                container.IsView1Active = false;

            if (container.View2 != null && container.View2.IsAncestorOf(focus))
                container.IsView2Active = false;
        }

        private static void InitCommands()
        {
            CommandManager.RegisterClassCommandBinding(typeof(SplitView),
                new CommandBinding(SplitViewCommands.ExpandCollapsePaneCommand, OnExpandCollapseExecuted, OnExpandCollapseCanExecute));

            CommandManager.RegisterClassCommandBinding(typeof(SplitView),
                new CommandBinding(SplitViewCommands.SplitHorizontalCommand, OnSplitHorizontalExecuted));

            CommandManager.RegisterClassCommandBinding(typeof(SplitView),
                new CommandBinding(SplitViewCommands.SplitVerticalCommand, OnSplitVerticalExecuted));

            CommandManager.RegisterClassCommandBinding(typeof(SplitView),
                new CommandBinding(SplitViewCommands.SwapPanesCommand, OnSwapPanesExecuted, OnSwapPanesCanExecute));

            CommandManager.RegisterClassCommandBinding(typeof(SplitView), new CommandBinding(SplitViewCommands.ActivateViewCommand, OnActivateView));
        }

        private static void OnActivateView(object sender, ExecutedRoutedEventArgs e)
        {
            var container = (SplitView)sender;
            string view = Convert.ToString(e.Parameter);

            if (string.IsNullOrWhiteSpace(view))
                return;

            container.ActivateView(view);
        }

        private void ActivateView(string view)
        {
            if (view == "View1")
            {
                if (IsCollapsed)
                    IsReversed = false;

                MoveFocusTo(View1);
                InvalidateActiveView();
            }
            else if (string.Equals(view, "View2", StringComparison.InvariantCultureIgnoreCase))
            {
                if (IsCollapsed)
                    IsReversed = true;

                MoveFocusTo(View2);
                InvalidateActiveView();
            }
        }

        private static void OnSwapPanesCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var splitContainer = sender as SplitView;
            if (splitContainer == null)
                return;

            e.CanExecute = !splitContainer.IsCollapsed;
        }

        private static void OnSplitHorizontalExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var splitContainer = sender as SplitView;
            if (splitContainer == null)
                return;

            splitContainer.Orientation = Orientation.Horizontal;
        }

        private static void OnSplitVerticalExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var splitContainer = sender as SplitView;
            if (splitContainer == null)
                return;

            splitContainer.Orientation = Orientation.Vertical;
        }

        private static void OnSwapPanesExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var splitContainer = sender as SplitView;
            if (splitContainer == null)
                return;

            splitContainer.OnSwapPanes();
        }

        private static void OnExpandCollapseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var splitContainer = sender as SplitView;
            if (splitContainer == null)
                return;

            splitContainer.OnExpandCollapse();
        }

        private static void OnExpandCollapseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var splitContainer = sender as SplitView;
            if (splitContainer == null)
                return;

            e.CanExecute = true;
        }

        #endregion

        #region "Virtual Methods"

        protected virtual void OnExpandCollapse()
        {
            if (!IsCollapsed)
            {
                _previousGripOffset = _gripOffset;
                _gripOffset = 100;
                IsCollapsed = true;
            }
            else
            {
                _gripOffset = _previousGripOffset;
                IsCollapsed = false;
            }
        }

        protected virtual void OnSwapPanes()
        {
            IsReversed = !IsReversed;
        }

        #endregion

        #region "Methods"

        private void InvalidateOrientationProperties()
        {
            IsHorizontal = Orientation == Orientation.Horizontal;
            IsVertical = Orientation == Orientation.Vertical;
        }

        private void InitSplitterHandle()
        {
            _grip = new SplitterGrip
            {
                Orientation = this.Orientation,
                IsCollapsed = this.IsCollapsed
            };
            _grip.DragDelta += OnGripDragDelta;
            _grip.DragCompleted += OnDragCompleted;
        }

        private void OnGripDragDelta(object sender, DragDeltaEventArgs e)
        {
            var isHorizontal = Orientation == Orientation.Horizontal;
            var change = isHorizontal ? e.VerticalChange : e.HorizontalChange;

            if (_grip.Popup != null)
            {
                InitializePopup(_grip.Popup);
                InvalidatePopupPlacement(_grip.Popup, change);
            }

            e.Handled = true;
        }

        public double InternalOffset
        {
            get
            {
                if (double.IsNaN(_gripOffset))
                {
                    if (StartupMode == StartupMode.Dual)
                    {
                        return 50;
                    }

                    // only a single view is visible.
                    var gripOffset = 6 * Density;
                    return -gripOffset;
                }

                return _gripOffset;
            }
        }

        private void InvalidatePopupPlacement(Popup popup, double change)
        {
            var rect = LayoutInformation.GetLayoutSlot(_grip);
            var isHorizontal = Orientation == Orientation.Horizontal;
            var offset = (isHorizontal ? rect.Top : rect.Left) + change;
            var maxOffset = isHorizontal ? (this.ActualHeight - 5) : (this.ActualWidth - 5);

            offset = offset + 5;
            offset = Math.Max(0, offset);
            offset = Math.Min(offset, maxOffset);

            if (isHorizontal)
                popup.VerticalOffset = offset;
            else
                popup.HorizontalOffset = offset;
        }

        private void InitializePopup(Popup popup)
        {
            if (popup == null || popup.IsOpen)
                return;

            popup.Placement = PlacementMode.Relative;
            popup.PlacementTarget = this;
            popup.IsOpen = true;
        }

        private void ComputeOffset(double change)
        {
            //double offset = double.IsNaN(_gripOffset) ? 50 : _gripOffset;
            double offset = InternalOffset;
            offset += Density * change;

            CoerceOffset(ref offset);

            _gripOffset = offset;
            InvalidateMeasure();
            InvalidateArrange();
        }

        private void CoerceOffset(ref double offset)
        {
            _isView2Collapsed = _isView1Collapsed = false;
            bool isCollapsed = false;
            bool isAdvanced = true;
            double min = 0;
            double max = 100;

            if (isAdvanced)
            {
                if (offset < 0)
                {
                    offset = 100;

                    _isView1Collapsed = true;
                    isCollapsed = true;
                }
                else if (offset > 100)
                {
                    offset = 100;
                    _isView1Collapsed = true;
                    isCollapsed = true;
                }
            }
            else
            {
                var gripOffset = Density * 6;

                min -= gripOffset;
                max += gripOffset;


                if (offset < min)
                {
                    offset = min;
                }
                else if (offset > max)
                {
                    offset = max;
                }

                if (offset < 0)
                {
                    isCollapsed = true;
                    _isView1Collapsed = true;
                    _isView2Collapsed = false;
                }
                else if (offset > 100)
                {
                    isCollapsed = true;
                    _isView1Collapsed = false;
                    _isView2Collapsed = true;
                }
            }

            IsCollapsed = isCollapsed;
        }

        private void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            var change = Orientation == Orientation.Horizontal ? e.VerticalChange : e.HorizontalChange;
            ComputeOffset(change);
            _grip.Popup.IsOpen = false;
        }

        private void UpdateView(FrameworkElement oldView, FrameworkElement newView)
        {
            if (oldView == newView)
                return;

            if (oldView != null)
                _visualChildren.Remove(oldView);

            if (newView != null)
                _visualChildren.Add(newView);

            InvalidateMeasure();
            InvalidateArrange();
        }

        protected override Visual GetVisualChild(int index)
        {
            if (_visualChildren == null || (index < 0 || index > (_visualChildren.Count - 1)))
                throw new ArgumentOutOfRangeException("index");

            return _visualChildren[index];
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            bool isHorizontal = Orientation == Orientation.Horizontal;

            if (isHorizontal && (double.IsPositiveInfinity(availableSize.Height) || double.IsInfinity(availableSize.Height)))
                return base.MeasureOverride(availableSize);

            if (!isHorizontal && (double.IsPositiveInfinity(availableSize.Width) || double.IsInfinity(availableSize.Width)))
                return base.MeasureOverride(availableSize);

            double startPos, handleSize, view1Size, view2Size;
            this.ComputePartsSizes(availableSize, isHorizontal, out view1Size, out view2Size, out handleSize, out startPos);

            var view1 = InternalView1;
            if (view1 != null)
                view1.Measure(isHorizontal
                    ? new Size(availableSize.Width, view1Size)
                    : new Size(view1Size, availableSize.Height));

            var view2 = InternalView2;
            if (view2 != null)
                view2.Measure(isHorizontal
                    ? new Size(availableSize.Width, view2Size)
                    : new Size(view2Size, availableSize.Height));

            _grip.Measure(availableSize);
            return _grip.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // initialization
            var isHorizontal = Orientation == Orientation.Horizontal;
            var isAdvanced = true;
            var view1 = InternalView1;
            var view2 = InternalView2;

            double view1Size, view2Size, gripSize, startPos;
            ComputePartsSizes(finalSize, isHorizontal, out view1Size, out view2Size, out gripSize, out startPos);


            Rect finalRect = isHorizontal
                ? new Rect
                {
                    Location = new Point(0, startPos),
                    Width = finalSize.Width
                }
                : new Rect
                {
                    Location = new Point(startPos, 0),
                    Height = finalSize.Height
                };


            if (IsCollapsed)
            {
                if (isAdvanced)
                {
                    // View1
                    if (isHorizontal)
                        finalRect.Height = view1Size;
                    else
                        finalRect.Width = view1Size;

                    if (view1 != null)
                    {
                        view1.Arrange(finalRect);
                    }

                    // View2
                    if (view2 != null)
                        view2.Arrange(_emptyRect);

                    // Grip
                    if (isHorizontal)
                    {
                        finalRect.Y += view1Size;
                        finalRect.Height = gripSize;
                    }
                    else
                    {
                        finalRect.X += view1Size;
                        finalRect.Width = gripSize;
                    }
                    _grip.Arrange(finalRect);

                    // if view1 is the one collapsed, we swap the views.
                    if (_isView1Collapsed)
                        SwapViews(false);
                }
                else
                {
                    if (_isView1Collapsed)
                    {
                        // View1
                        if (view1 != null)
                            view1.Arrange(_emptyRect);

                        // Grip
                        if (isHorizontal)
                        {
                            finalRect.Y = startPos;
                            finalRect.Height = gripSize;
                        }
                        else
                        {
                            finalRect.X = startPos;
                            finalRect.Width = gripSize;
                        }
                        _grip.Arrange(finalRect);

                        // view2
                        if (isHorizontal)
                        {
                            finalRect.Y += gripSize;
                            finalRect.Height = view2Size;
                        }
                        else
                        {
                            finalRect.X += gripSize;
                            finalRect.Width = view2Size;
                        }

                        if (view2 != null)
                            view2.Arrange(finalRect);
                    }
                }
            }
            else
            {
                // View1
                if (isHorizontal)
                    finalRect.Height = view1Size;
                else
                    finalRect.Width = view1Size;


                if (view1 != null)
                    view1.Arrange(finalRect);

                // grip
                if (isHorizontal)
                {
                    finalRect.Y += view1Size;
                    finalRect.Height = gripSize;
                }
                else
                {
                    finalRect.X += view1Size;
                    finalRect.Width = gripSize;
                }
                _grip.Arrange(finalRect);

                // View2
                if (isHorizontal)
                {
                    finalRect.Y += gripSize;
                    finalRect.Height = view2Size;
                }
                else
                {
                    finalRect.X += gripSize;
                    finalRect.Width = view2Size;
                }

                if (view2 != null)
                    view2.Arrange(finalRect);
            }

            return finalSize;
        }

        private void ComputePartsSizes(Size arrangeSize,
            bool isHorizontal,
            out double view1Size,
            out double view2Size,
            out double handleSize,
            out double startPos)
        {
            //double offset = double.IsNaN(_gripOffset) ? 50 : _gripOffset;

            double offset = InternalOffset;
            startPos = 0;

            double length;
            if (isHorizontal)
            {
                length = arrangeSize.Height;
                handleSize = _grip.DesiredSize.Height;
            }
            else
            {
                length = arrangeSize.Width;
                handleSize = _grip.DesiredSize.Width;
            }

            double remainingLength = length - handleSize;

            if (true)
            {
                // if either view is collapsed
                if (IsCollapsed)
                {
                    view1Size = view2Size = remainingLength;
                }
                else
                {
                    view1Size = remainingLength * (offset / 100);
                    view2Size = remainingLength - view1Size;
                }
            }
            else
            {

                if (offset >= 0 && offset <= 100)
                {
                    view1Size = remainingLength * (offset / 100);
                    view2Size = remainingLength - view1Size;
                }
                else if (offset < 0)
                {
                    double handleVisiblePart = Math.Max(0.0, handleSize - (Math.Abs(offset) / 100) * remainingLength);
                    startPos = (handleSize - handleVisiblePart) * -1;
                    view1Size = view2Size = length - handleVisiblePart;
                }
                else //offset > 100
                {
                    double handleVisiblePart = Math.Max(0.0, handleSize - ((offset - 100) / 100) * remainingLength);
                    view2Size = view1Size = length - handleVisiblePart;
                }
            }

            Density = 100 / remainingLength;
        }

        private void SwapViews(bool invalidateArrange)
        {
            var temp = _view1Container;
            _view1Container = _view2Container;
            _view2Container = temp;

            if (invalidateArrange)
                InvalidateArrange();
        }

        private void InvalidateFocus()
        {
            if (!IsCollapsed)
                return;

            var activeView = InternalView1;
            if (activeView != null && activeView.IsKeyboardFocusWithin)
                MoveFocusTo(activeView);
        }

        private void InvalidateActiveView()
        {
            IsView1Active = View1 != null && View1.IsKeyboardFocusWithin;
            IsView2Active = View2 != null && View2.IsKeyboardFocusWithin;
            RaiseActiveViewChangedEvent(this);
        }

        private void MoveFocusTo(FrameworkElement element)
        {
            if (element != null)
                Keyboard.Focus(element);
        }

        #endregion

        #region "Method and properties to support SD old code"

        internal int SelectedIndex
        {
            get { return IsView1Active ? 0 : 1; }
            set
            {
                if (value < 0 || value > 1)
                    throw new IndexOutOfRangeException("value");

                ActivateView(value == 0 ? "View1" : "View2");
            }
        }

        internal FrameworkElement this[int index]
        {
            get
            {
                if (index < 0 || index > 1)
                    throw new IndexOutOfRangeException("index");

                return index == 0 ? InternalView1 : InternalView2;
            }
        }

        internal void RemoveAt(int index)
        {

        }

        internal IList<FrameworkElement> Items
        {
            get
            {
                var items = new List<FrameworkElement>();
                if (View1 != null)
                    items.Add(View1);

                if (View2 != null)
                    items.Add(View2);

                return items;
            }
        }

        #endregion
    }
}
