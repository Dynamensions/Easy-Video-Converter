using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.Xaml.Interactivity;
using System;
using System.Numerics;

namespace Dynamensions.EasyVideoConverter.Behaviors;

public class HoverElevationBehavior : Behavior<UIElement>
{
    private Visual? _visual;
    private CompositionPropertySet? _props;

    public float NormalZ { get; set; } = 24f;
    public float HoverZ { get; set; } = 32f;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject == null)
            return;

        _visual = ElementCompositionPreview.GetElementVisual(AssociatedObject);
        var compositor = _visual.Compositor;

        _props = compositor.CreatePropertySet();
        _props.InsertVector3("Translation", new Vector3(0, 0, NormalZ));

        var expression = compositor.CreateExpressionAnimation("props.Translation");
        expression.SetReferenceParameter("props", _props);
        _visual.StartAnimation("Translation", expression);

        AssociatedObject.PointerEntered += OnPointerEntered;
        AssociatedObject.PointerExited += OnPointerExited;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject == null)
            return;

        AssociatedObject.PointerEntered -= OnPointerEntered;
        AssociatedObject.PointerExited -= OnPointerExited;
    }

    private void OnPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        AnimateElevation(HoverZ);
    }

    private void OnPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        AnimateElevation(NormalZ);
    }

    private void AnimateElevation(float z)
    {
        if (_props == null)
            return;

        var compositor = _props.Compositor;

        var animation = compositor.CreateVector3KeyFrameAnimation();
        animation.InsertKeyFrame(1.0f, new Vector3(0, 0, z));
        animation.Duration = TimeSpan.FromMilliseconds(150);

        _props.StartAnimation("Translation", animation);
    }
}
