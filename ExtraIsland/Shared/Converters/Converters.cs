using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

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

public class DoubleMultipleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not double v) return null;
        double m = System.Convert.ToDouble(parameter);
        return v * m;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not double v) return null;
        double m = System.Convert.ToDouble(parameter);
        return v / m;
    }
}

public class HitokotoVisibilityConverter : IValueConverter {
    public object Convert(object? value,Type targetType,object? parameter,
        System.Globalization.CultureInfo culture) {
        return (RhesisDataSource)value! switch {
            RhesisDataSource.All => Visibility.Visible,
            RhesisDataSource.Hitokoto => Visibility.Visible,
            _ => Visibility.Collapsed
        };
    }

    public object ConvertBack(object? value,Type targetType,object? parameter,
        System.Globalization.CultureInfo culture) {
        throw new NotSupportedException();
    }
}