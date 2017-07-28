namespace SmartAudio
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Markup;

    [DefaultProperty("Items"), ContentProperty("Items"), TemplatePart(Name="PART_DropDown", Type=typeof(Button))]
    public class SplitButton : Button
    {
        public static readonly DependencyProperty HorizontalOffsetProperty;
        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode", typeof(SplitButtonMode), typeof(SplitButton), new FrameworkPropertyMetadata(SplitButtonMode.Split));
        public static readonly DependencyProperty PlacementProperty;
        public static readonly DependencyProperty PlacementRectangleProperty;
        public static readonly DependencyProperty VerticalOffsetProperty;

        static SplitButton()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
            PlacementProperty = ContextMenuService.PlacementProperty.AddOwner(typeof(SplitButton), new FrameworkPropertyMetadata(PlacementMode.MousePoint, new PropertyChangedCallback(SplitButton.OnPlacementChanged)));
            PlacementRectangleProperty = ContextMenuService.PlacementRectangleProperty.AddOwner(typeof(SplitButton), new FrameworkPropertyMetadata(Rect.Empty, new PropertyChangedCallback(SplitButton.OnPlacementRectangleChanged)));
            HorizontalOffsetProperty = ContextMenuService.HorizontalOffsetProperty.AddOwner(typeof(SplitButton), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(SplitButton.OnHorizontalOffsetChanged)));
            VerticalOffsetProperty = ContextMenuService.VerticalOffsetProperty.AddOwner(typeof(SplitButton), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(SplitButton.OnVerticalOffsetChanged)));
        }

        private void DoDropdownClick(object sender, RoutedEventArgs e)
        {
            if ((this.Mode != SplitButtonMode.Dropdown) && ((base.ContextMenu != null) && base.ContextMenu.HasItems))
            {
                base.ContextMenu.PlacementTarget = this;
                base.ContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        private void EnsureContextMenuIsValid()
        {
            if (base.ContextMenu == null)
            {
                base.ContextMenu = new ContextMenu();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ButtonBase base2 = base.Template.FindName("PART_DropDown", this) as ButtonBase;
            if (base2 != null)
            {
                base2.Click += new RoutedEventHandler(this.DoDropdownClick);
            }
        }

        private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SplitButton button = d as SplitButton;
            if (button != null)
            {
                button.EnsureContextMenuIsValid();
                button.ContextMenu.HorizontalOffset = (double) e.NewValue;
            }
        }

        private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SplitButton button = d as SplitButton;
            if (button != null)
            {
                button.EnsureContextMenuIsValid();
                button.ContextMenu.Placement = (PlacementMode) e.NewValue;
            }
        }

        private static void OnPlacementRectangleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SplitButton button = d as SplitButton;
            if (button != null)
            {
                button.EnsureContextMenuIsValid();
                button.ContextMenu.PlacementRectangle = (Rect) e.NewValue;
            }
        }

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SplitButton button = d as SplitButton;
            if (button != null)
            {
                button.EnsureContextMenuIsValid();
                button.ContextMenu.VerticalOffset = (double) e.NewValue;
            }
        }

        public double HorizontalOffset
        {
            get => 
                ((double) base.GetValue(HorizontalOffsetProperty));
            set
            {
                base.SetValue(HorizontalOffsetProperty, value);
            }
        }

        public ItemCollection Items
        {
            get
            {
                this.EnsureContextMenuIsValid();
                return base.ContextMenu.Items;
            }
        }

        public SplitButtonMode Mode
        {
            get => 
                ((SplitButtonMode) base.GetValue(ModeProperty));
            set
            {
                base.SetValue(ModeProperty, value);
            }
        }

        public PlacementMode Placement
        {
            get => 
                ((PlacementMode) base.GetValue(PlacementProperty));
            set
            {
                base.SetValue(PlacementProperty, value);
            }
        }

        public Rect PlacementRectangle
        {
            get => 
                ((Rect) base.GetValue(PlacementRectangleProperty));
            set
            {
                base.SetValue(PlacementRectangleProperty, value);
            }
        }

        public double VerticalOffset
        {
            get => 
                ((double) base.GetValue(VerticalOffsetProperty));
            set
            {
                base.SetValue(VerticalOffsetProperty, value);
            }
        }
    }
}

