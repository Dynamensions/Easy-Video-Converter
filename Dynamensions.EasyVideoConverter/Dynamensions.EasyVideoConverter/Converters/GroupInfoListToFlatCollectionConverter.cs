using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class GroupInfoListToFlatCollectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        List<Object> flatList = [];

        if (value is IEnumerable<Core.Models.Profiles.ClientProfileGroup> groups)
        {
            foreach (var group in groups)
            {
                flatList.Add(new ClientProfileGroup { Key = group.Key });

                foreach (var item in group.Profiles)
                {
                    flatList.Add(item);
                }
            }
        }

        return flatList.ToArray();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}



public partial class ComboBoxContainerStyleSelector : StyleSelector
{
    public Style GroupHeaderStyle { get; set; } = new();

    public Style ItemStyle { get; set; } = new();

    protected override Style SelectStyleCore(object item, DependencyObject container)
    {
        if (item is ClientProfileGroup)
        {

        }
        return item is ClientProfileGroup ? GroupHeaderStyle : ItemStyle;
    }
}

public partial class ComboBoxItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate GroupHeaderTemplate { get; set; } = new();

    public DataTemplate ItemTemplate { get; set; } = new();

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item is ClientProfileGroup ? GroupHeaderTemplate : ItemTemplate;
    }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplate(item);
    }
}