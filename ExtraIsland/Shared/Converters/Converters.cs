using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using ClassIsland.Core.Models.Components;

namespace ExtraIsland.Shared.Converters;

[ValueConversion(typeof(bool),typeof(bool))]
public class InverseBooleanConverter : IValueConverter {
    public object Convert(object? value,Type targetType,object? parameter,
        System.Globalization.CultureInfo culture) {
        if (targetType != typeof(bool))
            throw new InvalidOperationException("The target must be a boolean");
        return !(bool)value!;
    }

    public object ConvertBack(object? value,Type targetType,object? parameter,
        System.Globalization.CultureInfo culture) {
        throw new NotSupportedException();
    }
}

public class IntToStringConverter : IValueConverter {
    public object Convert(object? value,Type targetType,object? parameter,
        System.Globalization.CultureInfo culture) {
        return System.Convert.ToString((int)value!);
    }

    public object ConvertBack(object? value,Type targetType,object? parameter,
        System.Globalization.CultureInfo culture) {
        return (string)value! == "" ? 0 : System.Convert.ToInt32((string)value!);
    }
}

public class DoubleToStringConverter : IValueConverter {
    public object Convert(object? value,Type targetType,object? parameter,
        System.Globalization.CultureInfo culture) {
        return System.Convert.ToString((double)value!,CultureInfo.InvariantCulture);
    }

    public object ConvertBack(object? value,Type targetType,object? parameter,
        System.Globalization.CultureInfo culture) {
        try {
            return System.Convert.ToDouble((string)value!);
        }
        catch {
            return 0;
        }
    }
}

public class EnumDescriptionConverter : IValueConverter {
    object IValueConverter.Convert(object? value,Type targetType,object? parameter,CultureInfo culture) {
        Enum myEnum = (Enum)value!;
        string description = GetEnumDescription(myEnum);
        return description;
    }

    object IValueConverter.ConvertBack(object? value,Type targetType,object? parameter,CultureInfo culture) {
        return string.Empty;
    }

    static string GetEnumDescription(Enum? enumObj) {
        FieldInfo? fieldInfo = enumObj!.GetType().GetField(enumObj.ToString());

        object[] attribArray = fieldInfo!.GetCustomAttributes(false);

        if (attribArray.Length == 0) {
            return enumObj.ToString();
        }
        DescriptionAttribute? attrib = attribArray[0] as DescriptionAttribute;
        return attrib!.Description;
    }
}

public class DayOfWeekEnumStringConverter : IValueConverter {
    object IValueConverter.Convert(object? value,Type targetType,object? parameter,CultureInfo culture) {
        DayOfWeek day = (DayOfWeek)value!;
        string description = day switch {

            DayOfWeek.Sunday => "日",
            DayOfWeek.Monday => "一",
            DayOfWeek.Tuesday => "二",
            DayOfWeek.Wednesday => "三",
            DayOfWeek.Thursday => "四",
            DayOfWeek.Friday => "五",
            DayOfWeek.Saturday => "六",
            _ => "???"
        };
        return description;
    }

    object IValueConverter.ConvertBack(object? value,Type targetType,object? parameter,CultureInfo culture) {
        return string.Empty;
    }
}

public class ComponentSetHalfConverter : IValueConverter {
    object IValueConverter.Convert(object? value,Type targetType,object? parameter,CultureInfo culture) {
        ObservableCollection<ClassIsland.Core.Models.Components.ComponentSettings> set = 
            (ObservableCollection<ClassIsland.Core.Models.Components.ComponentSettings>)value!;
        ObservableCollection<ClassIsland.Core.Models.Components.ComponentSettings> buffer = [];
        int t;
        if (set.Count == 0) {
            return buffer;
        }
        if (EiUtils.IsOdd(set.Count)) {
            t = (set.Count + 1)/2;
        } else {
            t = set.Count/2;
        }
        for (int i = 0; i <= t-1; i++) {
            buffer.Add(set[i]);
        }
        return buffer;
    }

    object IValueConverter.ConvertBack(object? value,Type targetType,object? parameter,CultureInfo culture) {
        throw new NotImplementedException();
    }
}