using Dynamensions.EasyVideoConverter.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dynamensions.EasyVideoConverter.Controls;

public sealed partial class DecisionSummaryControl : UserControl
{
    public DecisionSummaryControl()
    {
        InitializeComponent();
    }

    public SmartDecision Decision
    {
        get => (SmartDecision)GetValue(DecisionProperty);
        set => SetValue(DecisionProperty, value);
    }

    public static readonly DependencyProperty DecisionProperty =
        DependencyProperty.Register(
            nameof(Decision),
            typeof(SmartDecision),
            typeof(DecisionSummaryControl),
            new PropertyMetadata(null));

}
